using System;
using System.Collections.Concurrent;
using System.Linq;

using JsonLibrary.FromClient;
using JsonLibrary.FromServer;

namespace SnakeMultiplayer.Services;

public class Arena
{
    public Speed Speed { get; private set; }

    readonly Random Random = new(Guid.NewGuid().GetHashCode());
    readonly ConcurrentDictionary<string, Snake> Snakes;
    readonly ConcurrentDictionary<string, MoveDirection> PendingActions;

    ConcurrentDictionary<string, MoveDirection> PreviousActions;
    Cells[,] Board;
    int Width;
    int Height;
    bool IsWall;
    Coordinate Food;

    public Arena(ConcurrentDictionary<string, Snake> players)
    {
        Snakes = players;
        PendingActions = new ConcurrentDictionary<string, MoveDirection>();
        Food = null;
        Speed = Speed.Normal;
    }

    public ArenaStatus GenerateReport()
    {
        var report = new ArenaStatus(Food == null ? null : new XY(Food.X, Food.Y));
        foreach (var snake in Snakes)
        {
            if (snake.Value == null)
            {
                continue;
            }
            else if (!snake.Value.IsActive)
            {
                report.AddDisabledSnake(snake.Key);
            }
            else
            {
                var head = snake.Value.CloneHead().ConvertToXY();
                var tail = snake.Value.Tail?.ConvertToXY();
                var color = snake.Value.GetColorString();
                var tempSnake = new JsonLibrary.FromServer.Snake(snake.Key, color, head, tail);
                report.AddActiveSnake(tempSnake);
            }
        }
        return report;
    }

    // Inefficient. 
    // TODO: get random coordinates and use breath-first search algorithm 
    // to find nearest empty cell.
    /// <summary>
    /// If needed, generates food at random location in the board.
    /// </summary>
    /// <param name="force">If true, generates food even if its nulll</param>
    public void GenerateFood(bool force)
    {
        if (force || Food != null)
        {
            return;
        }

        Food = null;
        _ = new Coordinate(Random.Next(0, Width), Random.Next(0, Height));
        var isFoodSet = false;
        bool contains;

        while (!isFoodSet)
        {
            var newFood = new Coordinate(Random.Next(0, Width), Random.Next(0, Height));
            contains = false;
            foreach (var snake in Snakes.Values)
            {
                if (snake.Contains(newFood))
                {
                    contains = true;
                    break;
                }
            }
            if (!contains)
            {
                Food = newFood;
                Board[newFood.X, newFood.Y] = Cells.food;
                return;
            }
        }
        //TODO: Player won? Refactor to better logic.
        throw new Exception("Could not set food");
    }

    public Settings SetSettings(Settings settings)
    {
        if (settings == null)
        {
            return null;
        }

        // Consider Speed Setting
        if (settings.cellCount != 0)
        {
            Width = settings.cellCount;
            Height = settings.cellCount;
        }
        if (settings.isWall != null)
        {
            IsWall = true;

        }

        //TODO: this shouldn't be here.
        if (Enum.TryParse(settings?.speed, out Speed parsedSpeed))
        {
            Speed = parsedSpeed;
        }

        return new Settings(Width, IsWall, Speed.ToString());
    }

    public Coordinate GetInitalCoordinte(InitialPosition pos) =>
        pos.Equals(InitialPosition.UpLeft)
            ? new Coordinate(1, 1)
        : pos.Equals(InitialPosition.UpRight)
            ? new Coordinate(Width - 2, 1)
        : pos.Equals(InitialPosition.DownLeft)
            ? new Coordinate(1, Height - 2)
        : pos.Equals(InitialPosition.DownRight)
            ? new Coordinate(Width - 2, Height - 2)
        : throw new ArgumentException($" Invalid initial position {pos}");

    public bool ClearSnake(string playerName)
    {
        if (!Snakes.TryGetValue(playerName, out var snake))
        {
            return false;
        }

        if (snake == null)
        {
            return false;
        }

        foreach (var coord in snake.GetBodyList())
        {
            Board[coord.X, coord.Y] = Cells.empty;
        }

        return true;
    }

    public string PrepareForNewGame()
    {
        Board = new Cells[Width, Height];

        if (!SetInitialPositionsAndActions())
        {
            return "Could not set initial positions";
        }

        GenerateFood(true);

        return string.Empty;
    }

    private bool SetInitialPositionsAndActions()
    {
        PendingActions.Clear();
        var allPositions = Enum.GetValues(typeof(InitialPosition)).Cast<InitialPosition>().ToArray();
        var allPlayers = Snakes.Keys.ToArray();
        string player;
        InitialPosition initPos;
        Coordinate initCoord;

        for (var i = 0; i < allPlayers.Length; i++)
        {
            player = allPlayers[i];
            initPos = allPositions[i];
            initCoord = GetInitalCoordinte(initPos);
            if (!Snakes.ContainsKey(player))
            {
                return false;
            }

            Snakes[player].SetInitialPosition(initCoord);

            _ = PendingActions.TryAdd(player, GetMoveDirection(initPos));
        }

        PreviousActions = new ConcurrentDictionary<string, MoveDirection>(PendingActions);
        return true;
    }

    public void UpdateActions()
    {
        RefreshPendingActions();

        foreach (var snake in Snakes)
        {
            if (snake.Value == null || !snake.Value.IsActive)
            {
                continue;
            }

            if (!PendingActions.TryGetValue(snake.Key, out var currAction))
            {
                continue;
            }

            var newHead = snake.Value.CloneHead();
            newHead.Update(currAction);

            if (newHead.X < 0 || Width <= newHead.X || newHead.Y < 0 || Width <= newHead.Y)
            {
                snake.Value.Deactivate();
                continue;
            }

            Tuple<Coordinate, Coordinate> moveResult;
            if (Board[newHead.X, newHead.Y].Equals(Cells.empty))
            {
                moveResult = snake.Value.Move(currAction, false);
            }
            else if (Board[newHead.X, newHead.Y].Equals(Cells.food))
            {
                Food = null;
                moveResult = snake.Value.Move(currAction, true);
            }
            else
            {
                snake.Value.Deactivate();
                continue;
            }

            if (moveResult == null)
            {
                continue;
            }

            Board[moveResult.Item1.X, moveResult.Item1.Y] = Cells.snake;
            if (moveResult.Item2 != null)
            {   // snake tail must be removed
                Board[moveResult.Item2.X, moveResult.Item2.Y] = Cells.empty;
            }
        }
        GenerateFood(false);
    }

    public void SetPendingAction(string player, MoveDirection direction)
    {
        if (PendingActions.TryGetValue(player, out var currDirection))
        {
            _ = PendingActions.TryUpdate(player, direction, currDirection);
        }
    }

    /// <summary>
    /// Sets current pending actions to snakes
    /// </summary>
    private void RefreshPendingActions()
    {
        foreach (var snake in Snakes)
        {
            // check if action is valid (get head and update and check
            var currentPendingAction = PendingActions[snake.Key];
            // if current action is not valid, set to last action. The very first action is always valid.
            if (snake.Value.IsDirectionToSelf(currentPendingAction))
            {
                PendingActions[snake.Key] = PreviousActions[snake.Key];
            }
        }

        PreviousActions = new ConcurrentDictionary<string, MoveDirection>(PendingActions);
    }

    public static MoveDirection GetMoveDirection(InitialPosition pos) =>
        pos switch
        {
            InitialPosition.UpLeft => MoveDirection.Down,
            InitialPosition.UpRight => MoveDirection.Left,
            InitialPosition.DownLeft => MoveDirection.Right,
            InitialPosition.DownRight => MoveDirection.Up,
            _ => MoveDirection.None,
        };
}

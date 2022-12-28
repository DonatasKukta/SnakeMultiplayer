using System;
using System.Collections.Concurrent;
using System.Linq;

using JsonLibrary.FromClient;
using JsonLibrary.FromServer;

namespace SnakeMultiplayer.Services;

/// <summary>
/// Lobby object represents an environment of a single game arena instance
/// </summary>
public class Arena
{
    protected Cells[,] board;
    protected ConcurrentDictionary<string, Snake> snakes;
    protected ConcurrentDictionary<string, MoveDirection> pendingActions;
    protected ConcurrentDictionary<string, MoveDirection> lastActions;
    protected int width;
    protected int height;
    protected bool isWall;
    protected Coordinate food;
    protected Random rnd = new(Guid.NewGuid().GetHashCode());
    public Speed Speed { get; protected set; }

    public Arena(ConcurrentDictionary<string, Snake> players)
    {
        snakes = players;
        pendingActions = new ConcurrentDictionary<string, MoveDirection>();
        food = null;
        Speed = Speed.Normal;
    }
    public ArenaStatus GenerateReport()
    {
        var report = new ArenaStatus(food == null ? null : new XY(food.x, food.y));
        foreach (var snake in snakes)
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
                var head = snake.Value.Head().ConvertToXY();
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
    // Randomly generates food at new location
    /// <summary>
    /// If needed, generates food at random location,saves food location to board.
    /// </summary>
    /// <param name="force">If true, generates food even if its nulll</param>
    public void GenerateFood(bool force)
    {
        if (force || food != null)
        {
            return;
        }

        food = null;
        _ = new Coordinate(rnd.Next(0, width), rnd.Next(0, height));
        var isFoodSet = false;
        bool contains;

        while (!isFoodSet)
        {
            var newFood = new Coordinate(rnd.Next(0, width), rnd.Next(0, height));
            contains = false;
            foreach (var snake in snakes.Values)
            {
                if (snake.Contains(newFood))
                {
                    contains = true;
                    break;
                }
            }
            if (!contains)
            {
                food = newFood;
                board[newFood.x, newFood.y] = Cells.food;
                return;
            }
        }
        // Zaidejas laimejo??? handle this. isvengti infinite loop
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
            width = settings.cellCount;
            height = settings.cellCount;
        }
        if (settings.isWall != null)
        {
            isWall = true;

        }
        if (!string.IsNullOrEmpty(settings.speed))
        {
            if (settings.speed.Equals("NoSpeed"))
            {
                Speed = Speed.NoSpeed;
            }
            else if (settings.speed.Equals("Slow"))
            {
                Speed = Speed.Slow;
            }
            else if (settings.speed.Equals("Normal"))
            {
                Speed = Speed.Normal;
            }
            else if (settings.speed.Equals("Fast"))
            {
                Speed = Speed.Fast;
            }
        }

        return new Settings(width, isWall, Speed.ToString());
    }

    public Coordinate GetInitalCoordinte(InitialPosition pos) => pos.Equals(InitialPosition.UpLeft)
            ? new Coordinate(1, 1)
            : pos.Equals(InitialPosition.UpRight)
                ? new Coordinate(width - 2, 1)
                : pos.Equals(InitialPosition.DownLeft)
                                ? new Coordinate(1, height - 2)
                                : pos.Equals(InitialPosition.DownRight)
                                                ? new Coordinate(width - 2, height - 2)
                                                : throw new ArgumentException($" Invalid initial position {pos}");

    public bool ClearSnake(string playerName)
    {
        if (!snakes.TryGetValue(playerName, out var snake))
        {
            return false;
        }

        if (snake == null)
        {
            return false;
        }

        foreach (var coord in snake.GetBodyList())
        {
            board[coord.x, coord.y] = Cells.empty;
        }

        return true;
    }

    public string PrepareForNewGame()
    {

        // create new board of cells
        board = new Cells[width, height];
        // set initial positions for snakes and next pending positions
        if (!SetInitialPositionsAndActions())
        {
            return "Could not set initial positions";
        }
        // set food
        food = null;
        GenerateFood(true);

        return string.Empty;
    }

    private bool SetInitialPositionsAndActions()
    {

        // Delete all pending actions
        pendingActions.Clear();
        var allPositions = Enum.GetValues(typeof(InitialPosition)).Cast<InitialPosition>().ToArray();
        var allPlayers = snakes.Keys.ToArray();
        string player;
        InitialPosition initPos;
        Coordinate initCoord;

        for (var i = 0; i < allPlayers.Length; i++)
        {
            player = allPlayers[i];
            initPos = allPositions[i];
            initCoord = GetInitalCoordinte(initPos);
            if (!snakes.ContainsKey(player))
            {
                return false;
            }

            snakes[player].SetInitialPosition(initCoord);

            _ = pendingActions.TryAdd(player, GetMoveDirection(initPos));
        }
        // creates shallow copy! each lasAction.Value references relevant pendingActions.Value
        lastActions = new ConcurrentDictionary<string, MoveDirection>(pendingActions);
        return true;
    }

    public void UpdateActions()
    {
        CheckPendingActions();

        foreach (var snake in snakes)
        {
            if (snake.Value == null || !snake.Value.IsActive)
            {
                continue;
            }

            if (!pendingActions.TryGetValue(snake.Key, out var currAction))
            {
                continue;
            }

            //var currAction = pendingActions[snake.Key];
            var newHead = snake.Value.Head();
            newHead.Update(currAction);
            //TODO: implement support of no wall.
            if (newHead.x < 0 || width <= newHead.x || newHead.y < 0 || width <= newHead.y)
            {
                snake.Value.Deactivate();
                continue;
            }

            Tuple<Coordinate, Coordinate> moveResult;
            if (board[newHead.x, newHead.y].Equals(Cells.empty))
            {
                moveResult = snake.Value.Move(currAction, false);
            }
            else if (board[newHead.x, newHead.y].Equals(Cells.food))
            {
                food = null;
                moveResult = snake.Value.Move(currAction, true);
            }
            else //if (board[newHead.x, newHead.y].Equals(Cells.snake))
            {
                snake.Value.Deactivate();
                continue;
            }
            if (moveResult == null)
            {
                continue;
            }

            board[moveResult.Item1.x, moveResult.Item1.y] = Cells.snake;
            if (moveResult.Item2 != null)
            {   // snake tail must be removed
                board[moveResult.Item2.x, moveResult.Item2.y] = Cells.empty;
            }
        }
        GenerateFood(false);
    }

    /// <summary>
    ///  Error free
    /// </summary>
    /// <param name="player"></param>
    /// <param name="direction"></param>
    public void SetPendingAction(string player, MoveDirection direction)
    {
        if (pendingActions.TryGetValue(player, out var currDirection))
        {
            _ = pendingActions.TryUpdate(player, direction, currDirection);
        }
    }

    /// <summary>
    /// Sets current pending actions to snakes
    /// </summary>
    private void CheckPendingActions()
    {
        foreach (var snake in snakes)
        {
            // check if action is valid (get head and update and check
            var currentPendingAction = pendingActions[snake.Key];
            // if current action is not valid, set to last action. The very first action is alawyas valid.
            if (!snake.Value.IsDirectionNotToSelf(currentPendingAction))
            {
                pendingActions[snake.Key] = lastActions[snake.Key];
            }
        }
        // save current actions
        lastActions = new ConcurrentDictionary<string, MoveDirection>(pendingActions);
    }

    private void ClearPendingActions()
    {
        // Set last action to be next action
    }

    public static MoveDirection GetMoveDirection(InitialPosition pos)
    {
        if (pos.Equals(InitialPosition.UpLeft))
        {
            return MoveDirection.Down;
        }
        else if (pos.Equals(InitialPosition.UpRight))
        {
            return MoveDirection.Left;
        }
        else if (pos.Equals(InitialPosition.DownLeft))
        {
            return MoveDirection.Right;
        }
        else if (pos.Equals(InitialPosition.DownRight))
        {
            return MoveDirection.Up;
        }
        else
        {
            // log error
            return MoveDirection.None;
        }
    }
}
public enum Speed
{
    NoSpeed = 0,
    Fast = 1,
    Normal = 2,
    Slow = 3
}

public enum InitialPosition
{
    UpLeft,
    UpRight,
    DownLeft,
    DownRight,
}

public enum Cells
{
    empty = 0,
    food = 1,
    snake = 2
}
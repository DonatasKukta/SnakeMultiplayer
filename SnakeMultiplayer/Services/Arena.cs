using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonLibrary;

namespace SnakeMultiplayer.Services
{
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
        protected Random rnd = new Random(Guid.NewGuid().GetHashCode());
        
        public Arena(ConcurrentDictionary<string, Snake> players)
        {
            snakes = players;
            pendingActions = new ConcurrentDictionary<string, MoveDirection>();
            food = null;
        }
        public ArenaStatus GenerateReport()
        {
            var report = new ArenaStatus(this.food == null ? null : new XY(this.food.x, this.food.y));
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
                    var tail = snake.Value.tail == null ? null : snake.Value.tail.ConvertToXY();
                    var color = snake.Value.GetColorString();
                    var tempSnake = new JsonLibrary.Snake(snake.Key, color, head, tail);
                    report.AddActiveSnake(tempSnake);
                }
            }
            return report;
        }

        // Inefficient. Maybe use it in corporation with cell array: take one random empty cell
        // Randomly generates food at new location
        /// <summary>
        /// If needed, generates food at random location,saves food location to board.
        /// </summary>
        /// <param name="force">If true, generates food even if its nulll</param>
        public void GenerateFood(bool force)
        {
            if (force || food != null)
                return;

            food = null;
            var newFood = new Coordinate(rnd.Next(0, width), rnd.Next(0, height));
            var isFoodSet = false;
            bool contains;

            while (!isFoodSet)
            {
                newFood = new Coordinate(rnd.Next(0, width), rnd.Next(0, height));
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
                    this.food = newFood;
                    board[newFood.x, newFood.y] = Cells.food;
                    return;
                }
            }
            // Zaidejas laimejo??? handle this. isvengti infinite loop
            throw new Exception("Could not set food");
        }

        public void SetSettings(Settings settings)
        {
            //  ConsiderSpeedSetting
            this.width = settings.cellCount;
            this.height = settings.cellCount;
            this.isWall = settings.isWall;
        }

        public Coordinate getInitalCoordinte(InitialPosition pos)
        {
            if (pos.Equals(InitialPosition.UpLeft))
            {
                return new Coordinate(1,1);
            }
            else if (pos.Equals(InitialPosition.UpRight))
            {
                return new Coordinate(width - 1, 1);
            }
            else if (pos.Equals(InitialPosition.DownLeft))
            {
                return new Coordinate(1, height - 1);
            }
            else if (pos.Equals(InitialPosition.DownRight))
            {
                return new Coordinate(width - 1, height - 1);
            }
            else
            {
                throw new ArgumentException($" Invalid initial position {pos.ToString()}");
            }
        }

        public string PrepareForNewGame()
        {

            // create new board of cells
            this.board = new Cells[width, height];
            // set initial positions for snakes and next pending positions
            if (!SetInitialPositionsAndActions())
                return "Could not set initial positions";
            // set food
            this.food = null;
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

            for (int i = 0; i < allPlayers.Length; i++)
            {
                player = allPlayers[i];
                initPos = allPositions[i];
                initCoord = getInitalCoordinte(initPos);
                if (!snakes.ContainsKey(player))
                    return false;
                snakes[player].setInitialPosition(initCoord);

                pendingActions.TryAdd(player, GetMoveDirection(initPos));
            }
            // creates shallow copy! each lasAction.Value references relevant pendingActions.Value
            lastActions = new ConcurrentDictionary<string, MoveDirection>(pendingActions);
            return true;
        }
        
        public void UpdateActions()
        {
            CheckPendingActions();

            foreach(var snake  in snakes)
            {
                if (snake.Value == null || !snake.Value.IsActive)
                {
                    continue;
                }

                if(!pendingActions.TryGetValue(snake.Key, out MoveDirection currAction ))
                    continue;
                 
                //var currAction = pendingActions[snake.Key];
                var newHead = snake.Value.Head();
                newHead.Update(currAction);
                Tuple<Coordinate, Coordinate> moveResult = null;

                if ( ( newHead.x < 0  || width <= newHead.x )|| (newHead.y < 0 || width <= newHead.y)) // is wall???
                {
                    snake.Value.Deactivate();
                    continue;
                }

                if (board[newHead.x, newHead.y].Equals(Cells.empty)){
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
                    continue;

                board[moveResult.Item1.x, moveResult.Item1.y] = Cells.snake;
                if(moveResult.Item2 != null)
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
            if (pendingActions.TryGetValue(player, out MoveDirection currDirection))
                pendingActions.TryUpdate(player, direction, currDirection);
        }

        /// <summary>
        /// Sets current pending actions to snakes
        /// </summary>
        private void CheckPendingActions()
        {
            foreach(var snake in snakes)
            {
                // check if action is valid (get head and update and check
                var currentPendingAction = pendingActions[snake.Key];
                // if current action is not valid, set to last action. The very first action is alawyas valid.
                if (!snake.Value.IsDirectionNotToSelf(currentPendingAction))
                    pendingActions[snake.Key] = lastActions[snake.Key];
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
}

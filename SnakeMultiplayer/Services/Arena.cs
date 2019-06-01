using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonLibrary;

namespace SnakeMultiplayer.Services
{
    /// <summary>
    /// Lobby object represents an environment of a single game instance
    /// </summary>
    public class Arena
    {
        protected Cells[,] board;
        protected ConcurrentDictionary<string, Snake> snakes;
        protected ConcurrentDictionary<string, MoveDirection> pendingActions;
        protected int width;
        protected int height;
        protected bool isWall;
        
        public Arena(ConcurrentDictionary<string, Snake> players)
        {
            snakes = players;
            pendingActions = new ConcurrentDictionary<string, MoveDirection>();
        }

        public void SetSettings(Settings settings)
        {
            this.width = settings.cellCount;
            this.height = settings.cellCount;
            this.isWall = settings.isWall;
        }

        public void setPendingAction(string player, MoveDirection direction)
        {
            if (pendingActions.TryGetValue(player, out MoveDirection currDirection))
                pendingActions.TryUpdate(player, direction, currDirection);
        }

        public Coordinate getCoordinte(InitialPosition pos)
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
            // raise status update

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
                initCoord = getCoordinte(initPos);
                if (!snakes.ContainsKey(player))
                    return false;
                snakes[player].setInitialPosition(initCoord);

                pendingActions.TryAdd(player, GetMoveDirection(initPos));
            }
            return true;
        }


        /// <summary>
        /// Sets current pending actions to snakes
        /// </summary>
        private void SetPendingActions()
        {

        }

        private void ClearPendingActions()
        {
            // Set last action to be next action
        }

        /// <summary>
        /// Updates pending direction
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="direction"></param>
        public void updatePendingAction(string playerName, MoveDirection direction)
        {
            pendingActions[playerName] = direction;
        }

        /// <summary>
        /// 
        /// </summary>
        private void resetPendingActions()
        {
            foreach(var action in pendingActions)
            {
                pendingActions[action.Key] = MoveDirection.None;
            }
        }

        public bool AddSnake(string playerName, Coordinate initCord)
        {
            foreach(var item in snakes)
            {
                if (item.Key.Equals(playerName))
                    return false;
            }

            //Snake newSnake = new Snake(initCord, "green");
            //newSnake.SnakeMoved += OnSnakeMovemenent;
            //snakes.Add(playerName, newSnake);

            return true;
        }
        
        public void OnSnakeMovemenent(Coordinate head ,Coordinate tail, bool isFood)
        {
           // Snake snake = (Snake)source;
            Console.WriteLine("Snake moved!");
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

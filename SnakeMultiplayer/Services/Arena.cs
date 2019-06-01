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

        private void PrepareForNewGame()
        {
            // create new board of cells
            this.board = new Cells[width, height];
            // set initial positions for snakes 

            // set snakes next pending positions

            // raise status update
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
    }

    public enum Cells
    {
        empty = 0,
        food = 1,
        snake = 2
    }
}

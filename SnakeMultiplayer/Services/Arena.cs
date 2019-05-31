using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SnakeMultiplayer.Services
{
    /// <summary>
    /// Lobby object represents an environment of a single game instance
    /// </summary>
    public class Arena
    {
        protected Cells[,] board;
        protected Dictionary<string, Snake> snakes;
        protected Dictionary<string, MoveDirection> pendingActions;
        protected int width;
        protected int height;
        protected bool isWall;
        
        public Arena(int width, int height, bool isWall)
        {
            board = new Cells[width, height];
            this.height = height;
            this.width = width;

            pendingActions = new Dictionary<string, MoveDirection>();
            this.isWall = isWall;
        }

        /// <summary>
        /// Sets current pending actions to snakes
        /// </summary>
        private void setPendingActions()
        {

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

            Snake newSnake = new Snake(initCord, "green");
            newSnake.SnakeMoved += OnSnakeMovemenent;
            snakes.Add(playerName, newSnake);

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

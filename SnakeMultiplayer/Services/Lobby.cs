using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SnakeMultiplayer.Services
{
    public class Lobby
    {
        protected Cells[,] board;
        protected Dictionary<string, Snake> snakes;
        protected Dictionary<string, Cells> nextMoves;
        protected int width;
        protected int height;

        

        public Lobby(int width, int height, bool isWall)
        {
            board = new Cells[width, height];
            this.height = height;
            this.width = width;

            nextMoves = new Dictionary<string, Cells>();
            if (isWall)
                setWall();
        }

        public void setWall()
        {
            int i;
            int edgeIndex = width - 1;
            for (i = 0; i<width; i++)
            {
                board[0, i] = Cells.wall;
                board[edgeIndex, i] = Cells.wall;
            }

            edgeIndex = height - 1;
            for (i = 0; i < height; i++)
            {
                board[i,0] = Cells.wall;
                board[i, edgeIndex] = Cells.wall;
            }
        }

        public bool AddSnake(string playerName, Coordinate initCord)
        {
            foreach(var item in snakes)
            {
                if (item.Key.Equals(playerName))
                    return false;
            }

            Snake newSnake = new Snake(initCord);
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
        snake = 2,
        wall = 3
    }
}

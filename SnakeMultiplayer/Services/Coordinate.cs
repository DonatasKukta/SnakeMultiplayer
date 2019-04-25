using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SnakeMultiplayer.Services
{
    public class Coordinate
    {
        private int x;
        private int y;
        
        public Coordinate(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public void Update(MoveDirection direction)
        {
            switch (direction)
            {
                case MoveDirection.Up:
                    this.y -= 1;
                    break;
                case MoveDirection.Right:
                    this.x += 1;
                    break;
                case MoveDirection.Down:
                    this.y += 1;
                    break;
                case MoveDirection.Left:
                    this.x -= 1;
                    break;
                case MoveDirection.None:
                default:
                    throw new ArgumentException($"Argument value of enum CoordDirection expected, but {direction} found");
            }
        }
        public (int,int) Get()
        {
            return (x, y);
        }

        public override string ToString()
        {
            return String.Format($"({x};{y})");
        }
    }
    
    public enum MoveDirection
    {
        None = 0,
        Up = 1,
        Right = 2,
        Down = 3,
        Left = 4,
    }


}

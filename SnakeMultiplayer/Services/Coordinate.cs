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

        public void Update(CoordDirection direction)
        {
            switch (direction)
            {
                case CoordDirection.Up:
                    this.x += 1;
                    break;
                case CoordDirection.Right:
                    this.y += 1;
                    break;
                case CoordDirection.Down:
                    this.x -= 1;
                    break;
                case CoordDirection.Left:

                case CoordDirection.None:
                default:
                    throw new ArgumentException($"Argument value of enum CoordDirection expected, but {direction} found");
            }
        }
        public (int,int) Get()
        {
            return (x, y);
        }
    }
    
    public enum CoordDirection
    {
        None = 0,
        Up = 1,
        Right = 2,
        Down = 3,
        Left = 4,
    }


}

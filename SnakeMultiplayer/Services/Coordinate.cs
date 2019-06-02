using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SnakeMultiplayer.Services
{
    public class Coordinate
    {
        public int x { get; private set; }
        public int y { get; private set; }

        public Coordinate(){}

        public Coordinate(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Coordinate Clone()
        {
            return new Coordinate(this.x, this.y);
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
            return String.Format($"{x}:{y};");
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !this.GetType().Equals(obj.GetType()))
                return false;

            var other = (Coordinate) obj;
            return (x == other.x) && (y == other.y);
        }

        public override int GetHashCode()
        {
            return (x << 2) ^ y;
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SnakeMultiplayer.Services
{
    public class Snake
    {
        private LinkedList<Coordinate> body;

        //public delegate void SnakeMovementEventHandler(object source, EventArgs args);
        public delegate void SnakeMovementEventHandler(Coordinate head, Coordinate tail, bool isFood);
        public event SnakeMovementEventHandler SnakeMoved;
        public readonly PlayerColor color;
        public bool IsActive { get; private set; }
        public Coordinate tail  { get; private set; }

    public Snake(PlayerColor color)
        {
            this.color = color;
            this.body = new LinkedList<Coordinate>();
        }

        public void setInitialPosition(Coordinate coordinate)
        {
            this.IsActive = true;
            body.Clear();
            body.AddFirst(coordinate);
        }
        
        public void Deactivate()
        {
            IsActive = false;
            this.body = null;
        }
        
        public Tuple<Coordinate, Coordinate> Move(MoveDirection direction, bool isFood)
        {
            if (body == null)
                new Tuple<Coordinate, Coordinate>(null, null);

            var newPosition = body.First.Value.Clone();
            newPosition.Update(direction);
            body.AddFirst(newPosition);
            tail = null;
            
            if (!isFood)
            {
                tail = body.Last.Value.Clone();
                body.RemoveLast();
            }
            return new Tuple<Coordinate, Coordinate>(Head(), tail);
            //SnakeMoved(body.First.Value, body.Last.Value, isFood);
        }
        /// <summary>
        /// Check whether direction is valid.
        /// (Snake always can not move backwards).
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public bool IsDirectionNotToSelf(MoveDirection direction)
        {
            if (body == null || body.Count <= 1)
                return true;

            var head = Head();
            head.Update(direction);
            return !this.body.First.Next.Value.Equals(head);

        }

        public Coordinate Head()
        {
            if (body == null || body.First.Value == null)
                return null;

            return body.First.Value.Clone();
        }

        public List<Coordinate> getCoordinates()
        {
            if (body == null)
                return null;

            return body.ToList<Coordinate>();
        }

        public bool Contains(Coordinate food)
        {
            if (body == null)
                return false;

            foreach (var coord in body)
                if (coord.Equals(food))
                    return true;
            return false;
        }

        public List<string> ToStringBody()
        {
            List<string> body = new List<string>(this.body.Count);

            foreach(Coordinate coord in this.body)
            {
                body.Add(coord.ToString());
            }
            return body;
        }

        public string GetColorString()
        {
            return Enum.GetName(typeof(PlayerColor), color);
        }
    }
}
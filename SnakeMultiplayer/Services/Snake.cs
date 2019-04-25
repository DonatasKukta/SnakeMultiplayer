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

        public Snake(Coordinate initialPosition)
        {
            this.body = new LinkedList<Coordinate>();
            body.AddFirst(initialPosition);
        }
        public void Move(MoveDirection direction, bool isFood)
        {
            var newPosition = body.First.Value;
            newPosition.Update(direction);
            body.AddFirst(newPosition);

            if (!isFood)
                body.RemoveLast();

            SnakeMoved(body.First.Value, body.Last.Value, isFood);
        }

        public List<Coordinate> getCoordinates()
        {
            return body.ToList<Coordinate>();
        }

    }
}
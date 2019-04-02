using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SnakeMultiplayer.Services
{
    public class Snake
    {

        public string Name { get; private set; }
        private LinkedList<Coordinate> body;

        public Snake(string name)
        {
            this.Name = name;
            this.body = new LinkedList<Coordinate>();
        }
        public Snake(Coordinate initialPosition)
        {
            this.body = new LinkedList<Coordinate>();
            body.AddFirst(initialPosition);
        }
        public void Move(CoordDirection direction)
        {
            var newPosition = body.First.Value;
            newPosition.Update(direction);
            body.AddFirst(newPosition);
            body.RemoveLast();
        }
        public void Eat(CoordDirection foodDirection)
        {
            var newPosition = body.First.Value;
            newPosition.Update(foodDirection);
            body.AddFirst(newPosition);
        }

        public List<Coordinate> getCoordinates()
        {
            return body.ToList<Coordinate>();
        }
    }
}
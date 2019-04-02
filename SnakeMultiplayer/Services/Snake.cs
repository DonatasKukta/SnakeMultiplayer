using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SnakeMultiplayer.Services
{
    public class Snake
    {
        LinkedList<Coordinate> body;

        public Snake(Coordinate initialPosition)
        {
            this.body = new LinkedList<Coordinate>();
            body.AddFirst(initialPosition);
        }
    }
}
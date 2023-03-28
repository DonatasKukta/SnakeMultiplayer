using System;
using System.Collections.Generic;
using System.Linq;

namespace SnakeMultiplayer.Services;

public class Snake
{
    private LinkedList<Coordinate> body;
    public readonly PlayerColor color;
    public bool IsActive { get; private set; }
    public Coordinate Tail { get; private set; }

    public Snake(PlayerColor color)
    {
        this.color = color;
        body = new LinkedList<Coordinate>();
    }

    public void SetInitialPosition(Coordinate coordinate)
    {
        IsActive = true;
        body = new LinkedList<Coordinate>();
        _ = body.AddFirst(coordinate);
    }

    public void Deactivate()
    {
        IsActive = false;
        body = null;
    }

    public Tuple<Coordinate, Coordinate> Move(MoveDirection direction, bool isFood)
    {
        if (body == null)
        {
            _ = new Tuple<Coordinate, Coordinate>(null, null);
        }

        var newPosition = body.First.Value.Clone();
        newPosition.Update(direction);
        _ = body.AddFirst(newPosition);
        Tail = null;

        if (!isFood)
        {
            Tail = body.Last.Value.Clone();
            body.RemoveLast();
        }
        return new Tuple<Coordinate, Coordinate>(CloneHead(), Tail);
    }
    /// <summary>
    /// Check whether direction is valid.
    /// (Snake always can not move backwards).
    /// </summary>
    public bool IsDirectionToSelf(MoveDirection direction)
    {
        if (body == null || body.Count <= 1)
        {
            return false;
        }

        var head = CloneHead();
        head.Update(direction);
        return body.First.Next.Value.Equals(head);
    }

    public Coordinate CloneHead() =>
        body == null || body.First.Value == null
            ? null
        : body.First.Value.Clone();

    public List<Coordinate> GetCoordinates() => body?.ToList<Coordinate>();

    public bool Contains(Coordinate food)
    {
        if (body == null)
        {
            return false;
        }

        foreach (var coord in body)
        {
            if (coord.Equals(food))
            {
                return true;
            }
        }

        return false;
    }

    public List<string> ToStringBody()
    {
        var body = new List<string>(this.body.Count);

        foreach (var coord in this.body)
        {
            body.Add(coord.ToString());
        }
        return body;
    }

    public List<Coordinate> GetBodyList()
    {
        var list = new List<Coordinate>(body.Count);
        foreach (var coord in body)
        {
            list.Add(coord.Clone());
        }
        return list;
    }

    public string GetColorString() => Enum.GetName(typeof(PlayerColor), color);
}
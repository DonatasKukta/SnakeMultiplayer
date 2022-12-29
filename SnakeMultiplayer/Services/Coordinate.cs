using System;

using JsonLibrary.FromServer;

namespace SnakeMultiplayer.Services;

public class Coordinate
{
    public int X { get; private set; }
    public int Y { get; private set; }

    public Coordinate() { }

    public Coordinate(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void Update(MoveDirection direction)
    {
        _ = direction switch
        {
            MoveDirection.Up => Y -= 1,
            MoveDirection.Right => X += 1,
            MoveDirection.Down => Y += 1,
            MoveDirection.Left => X -= 1,
            _ => throw new ArgumentException($"Argument value of enum CoordDirection expected, but {direction} found")
        };
    }

    public XY ConvertToXY() => new(X, Y);

    public Coordinate Clone() => new(X, Y);

    public (int, int) Get() => (X, Y);

    public override string ToString() => string.Format($"{X}:{Y};");

    public override bool Equals(object obj)
    {
        if (obj == null || !GetType().Equals(obj.GetType()))
        {
            return false;
        }

        var other = (Coordinate)obj;
        return (X == other.X) && (Y == other.Y);
    }

    public override int GetHashCode() => (X << 2) ^ Y;
}
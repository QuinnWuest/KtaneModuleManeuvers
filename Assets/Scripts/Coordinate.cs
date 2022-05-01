using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public struct Coordinate : IEquatable<Coordinate> {

    public int x { get; private set; }
    public int y { get; private set; }

    public Coordinate(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Coordinate ApplyMovement(Dir direction)
    {
        switch (direction)
        {
            case Dir.Up: return new Coordinate(x, y + 1);
            case Dir.Right: return new Coordinate(x + 1, y);
            case Dir.Down: return new Coordinate(x, y - 1);
            case Dir.Left: return new Coordinate(x - 1, y);
        }
        throw new System.ArgumentOutOfRangeException();
    }
    public Coordinate ApplyMovement(IEnumerable<Dir> directions)
    {
        return directions.Aggregate(this, (c, dir) => c.ApplyMovement(dir));
    }
    public Coordinate ApplyMovement(ModuleInfo info)
    {
        return ApplyMovement(info.directions);
    }

    public override string ToString()
    {
        return string.Format("({0}, {1})", x, y);
    }
    public override bool Equals(object obj)
    { return obj is Coordinate && Equals((Coordinate)obj); }
    public bool Equals(Coordinate other)
    { return x == other.x && y == other.y; }

    public static bool operator ==(Coordinate a, Coordinate b) { return a.Equals(b); }
    public static bool operator !=(Coordinate a, Coordinate b) { return !a.Equals(b); }
}

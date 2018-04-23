using UnityEngine;

public struct Waypoint
{
    public readonly Vector3 Position;
    public readonly PointMode Mode;

    public Waypoint(Vector3 position, PointMode mode)
    {
        Position = position;
        Mode = mode;
    }

    public override int GetHashCode()
    {
        return Position.GetHashCode() + Mode.GetHashCode();
    }

    public override string ToString()
    {
        return "(" + Position + ", " + Mode + ")";
    }
}

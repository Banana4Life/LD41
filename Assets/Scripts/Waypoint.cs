using UnityEngine;

public struct Waypoint
{
    public readonly Vector3 Position;
    public readonly PointMode Mode;
    public readonly int PathIndex;

    public Waypoint(Vector3 position, PointMode mode, int pathIndex)
    {
        Position = position;
        Mode = mode;
        PathIndex = pathIndex;
    }
}

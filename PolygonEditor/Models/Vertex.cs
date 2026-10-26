namespace PolygonEditor.Models;

public sealed class Vertex
{
    public PointF Position { get; set; }

    public Continuity Continuity { get; set; } = Continuity.G0;

    // true if the user picked the continuity, then changing the edge kind does not overwrite it
    public bool ContinuityUserSet { get; set; }

    public Edge? Incoming { get; internal set; }
    public Edge? Outgoing { get; internal set; }

    public Vertex()
    {
    }

    public Vertex(float x, float y)
    {
        Position = new PointF(x, y);
    }

    public Vertex(PointF position)
    {
        Position = position;
    }

    // continuity matters only if at least one neighbour edge is not a line
    public bool HasAdjacentCurve =>
        (Incoming is not null && Incoming.Kind != EdgeKind.Line)
        || (Outgoing is not null && Outgoing.Kind != EdgeKind.Line);

    public bool HasAdjacentBezier =>
        (Incoming is not null && Incoming.IsBezier) || (Outgoing is not null && Outgoing.IsBezier);

    public bool HasAdjacentArc =>
        (Incoming is not null && Incoming.IsArc) || (Outgoing is not null && Outgoing.IsArc);

    // C1 only next to a Bezier and never next to an arc (an arc allows G0/G1)
    public bool AllowsC1 => HasAdjacentBezier && !HasAdjacentArc;

    public void SetContinuityByUser(Continuity continuity)
    {
        Continuity = continuity;
        ContinuityUserSet = true;
    }

    public void Translate(float dx, float dy)
    {
        Position = new PointF(Position.X + dx, Position.Y + dy);
    }
}

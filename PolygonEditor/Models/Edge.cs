namespace PolygonEditor.Models;

// Directed edge from Start to End.
public sealed class Edge
{
    public Vertex Start { get; internal set; }
    public Vertex End { get; internal set; }

    // changed through Scene.SetEdgeKind, which takes care of constraints and continuity
    public EdgeKind Kind { get; internal set; } = EdgeKind.Line;

    // P1 and P2 of the Bezier curve (P0 = Start, P3 = End)
    public ControlPoint Control1 { get; }
    public ControlPoint Control2 { get; }

    public Edge(Vertex start, Vertex end)
    {
        Start = start ?? throw new ArgumentNullException(nameof(start));
        End = end ?? throw new ArgumentNullException(nameof(end));
        Control1 = new ControlPoint(this, start, Lerp(start.Position, end.Position, 1f / 3f));
        Control2 = new ControlPoint(this, end, Lerp(start.Position, end.Position, 2f / 3f));
    }

    // which side the arc bends to (like side in InitializeControlPoints), set when the edge becomes an arc
    public float ArcSide { get; internal set; } = 1f;

    public bool IsBezier => Kind == EdgeKind.CubicBezier;
    public bool IsArc => Kind == EdgeKind.CircularArc;

    // G1 at one end: arc tangent to the neighbour edge; G0 at both ends (or tangent parallel to the chord): half circle.
    // The arc follows its neighbour by itself, so the solver does not need a continuity constraint at its vertices.
    public ArcGeometry Arc => ComputeArc(useNeighbours: true);

    // useNeighbours = false when computing the tangent for a neighbouring arc - stops the loop when two arcs
    // have G1 at the same vertex (then the neighbour is taken as a half circle)
    private ArcGeometry ComputeArc(bool useNeighbours)
    {
        PointF s = Start.Position;
        PointF e = End.Position;

        if (useNeighbours)
        {
            if (Start.Continuity != Continuity.G0 && Start.Incoming is { } before
                && ArcGeometry.TangentAtStart(s, e, before.ArrivalDirection()) is { } fromStart)
                return fromStart;

            if (End.Continuity != Continuity.G0 && End.Outgoing is { } after
                && ArcGeometry.TangentAtEnd(s, e, after.DepartureDirection()) is { } fromEnd)
                return fromEnd;
        }

        return ArcGeometry.Semicircle(s, e, ArcSide);
    }

    // direction of movement along the edge at End (arriving) and at Start (leaving); the arc takes its tangent from these
    public PointF ArrivalDirection() => Kind switch
    {
        EdgeKind.CubicBezier => Sub(End.Position, Control2.Position),
        EdgeKind.CircularArc => ComputeArc(useNeighbours: false).TangentAt(1f),
        _ => Direction
    };

    public PointF DepartureDirection() => Kind switch
    {
        EdgeKind.CubicBezier => Sub(Control1.Position, Start.Position),
        EdgeKind.CircularArc => ComputeArc(useNeighbours: false).TangentAt(0f),
        _ => Direction
    };

    private static PointF Sub(PointF a, PointF b) => new(a.X - b.X, a.Y - b.Y);

    public PointF Midpoint => new(
        (Start.Position.X + End.Position.X) * 0.5f,
        (Start.Position.Y + End.Position.Y) * 0.5f);

    public float Length
    {
        get
        {
            float dx = End.Position.X - Start.Position.X;
            float dy = End.Position.Y - Start.Position.Y;
            return MathF.Sqrt(dx * dx + dy * dy);
        }
    }

    public PointF Direction
    {
        get
        {
            float dx = End.Position.X - Start.Position.X;
            float dy = End.Position.Y - Start.Position.Y;
            return new PointF(dx, dy);
        }
    }

    // 1/3 and 2/3 of the chord, moved sideways by 1/4 of its length so the new curve is visible right away
    // side = +1 moves towards the normal (-dy, dx), side = -1 the other way
    public void InitializeControlPoints(float side = 1f)
    {
        PointF a = Start.Position;
        PointF b = End.Position;
        float offsetX = -(b.Y - a.Y) * 0.25f * side;
        float offsetY = (b.X - a.X) * 0.25f * side;

        PointF c1 = Lerp(a, b, 1f / 3f);
        PointF c2 = Lerp(a, b, 2f / 3f);
        Control1.Position = new PointF(c1.X + offsetX, c1.Y + offsetY);
        Control2.Position = new PointF(c2.X + offsetX, c2.Y + offsetY);
    }

    private static PointF Lerp(PointF a, PointF b, float t) =>
        new(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);
}

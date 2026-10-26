namespace PolygonEditor.Models;

// Krawędź skierowana od Start do End.
public sealed class Edge
{
    public Vertex Start { get; internal set; }
    public Vertex End { get; internal set; }

    // Zmieniany przez Scene.SetEdgeKind, która pilnuje ograniczeń i ciągłości.
    public EdgeKind Kind { get; internal set; } = EdgeKind.Line;

    // P1 i P2 krzywej Béziera (P0 = Start, P3 = End).
    public ControlPoint Control1 { get; }
    public ControlPoint Control2 { get; }

    public Edge(Vertex start, Vertex end)
    {
        Start = start ?? throw new ArgumentNullException(nameof(start));
        End = end ?? throw new ArgumentNullException(nameof(end));
        Control1 = new ControlPoint(this, start, Lerp(start.Position, end.Position, 1f / 3f));
        Control2 = new ControlPoint(this, end, Lerp(start.Position, end.Position, 2f / 3f));
    }

    // Strona wygięcia łuku (jak side w InitializeControlPoints); ustawiana przy zamianie na łuk.
    public float ArcSide { get; internal set; } = 1f;

    public bool IsBezier => Kind == EdgeKind.CubicBezier;
    public bool IsArc => Kind == EdgeKind.CircularArc;

    // G1 na jednym końcu: łuk styczny do sąsiedniej krawędzi; G0 na obu (lub styczna równoległa do cięciwy): półokrąg.
    // Łuk sam dopasowuje się do sąsiada, więc w jego wierzchołkach solver nie potrzebuje ograniczenia ciągłości.
    public ArcGeometry Arc => ComputeArc(useNeighbours: true);

    // useNeighbours = false przy liczeniu stycznej dla sąsiedniego łuku - przerywa cykl, gdy dwa łuki
    // mają G1 we wspólnym wierzchołku (wtedy sąsiad jest brany jako półokrąg).
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

    // Kierunek ruchu wzdłuż krawędzi w End (przy dojściu) i w Start (przy wyjściu) - to z nich łuk bierze styczną.
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

    // 1/3 i 2/3 cięciwy, odsunięte w bok o 1/4 jej długości, żeby po zamianie krzywa była od razu widoczna.
    // side = +1 odsuwa w stronę normalnej (-dy, dx), side = -1 w przeciwną.
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

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
        Control1 = new ControlPoint(this, Lerp(start.Position, end.Position, 1f / 3f));
        Control2 = new ControlPoint(this, Lerp(start.Position, end.Position, 2f / 3f));
    }

    public bool IsBezier => Kind == EdgeKind.CubicBezier;

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

    public void TranslateControlPoints(float dx, float dy)
    {
        Control1.Translate(dx, dy);
        Control2.Translate(dx, dy);
    }

    private static PointF Lerp(PointF a, PointF b, float t) =>
        new(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);
}

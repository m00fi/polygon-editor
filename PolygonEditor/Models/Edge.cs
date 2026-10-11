namespace PolygonEditor.Models;

/// <summary>Krawędź skierowana od Start do End.</summary>
public sealed class Edge
{
    public Vertex Start { get; internal set; }
    public Vertex End { get; internal set; }
    public EdgeKind Kind { get; set; } = EdgeKind.Line;

    /// <summary>Pierwszy punkt kontrolny Béziera.</summary>
    public PointF Control1 { get; set; }

    /// <summary>Drugi punkt kontrolny Béziera.</summary>
    public PointF Control2 { get; set; }

    public Edge(Vertex start, Vertex end)
    {
        Start = start ?? throw new ArgumentNullException(nameof(start));
        End = end ?? throw new ArgumentNullException(nameof(end));
        InitializeControlPointsAlongChord();
    }

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

    /// <summary>Ustawia punkty kontrolne na 1/3 i 2/3 cięciwy.</summary>
    public void InitializeControlPointsAlongChord()
    {
        PointF a = Start.Position;
        PointF b = End.Position;
        Control1 = Lerp(a, b, 1f / 3f);
        Control2 = Lerp(a, b, 2f / 3f);
    }

    public void TranslateControlPoints(float dx, float dy)
    {
        Control1 = new PointF(Control1.X + dx, Control1.Y + dy);
        Control2 = new PointF(Control2.X + dx, Control2.Y + dy);
    }

    private static PointF Lerp(PointF a, PointF b, float t) =>
        new(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);
}

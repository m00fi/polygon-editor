namespace PolygonEditor.Models;

public sealed class Vertex
{
    public PointF Position { get; set; }

    public Continuity Continuity { get; set; } = Continuity.G0;

    // True, gdy użytkownik sam wybrał ciągłość - wtedy zmiana rodzaju krawędzi jej nie nadpisuje.
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

    // Ciągłość ma znaczenie tylko wtedy, gdy co najmniej jedna sąsiednia krawędź nie jest odcinkiem.
    public bool HasAdjacentCurve =>
        (Incoming is not null && Incoming.Kind != EdgeKind.Line)
        || (Outgoing is not null && Outgoing.Kind != EdgeKind.Line);

    public void Translate(float dx, float dy)
    {
        Position = new PointF(Position.X + dx, Position.Y + dy);
    }
}

namespace PolygonEditor.Models;

// Punkt kontrolny Béziera. Klasa, a nie PointF, żeby mieć tożsamość
// (trafienie myszą, przeciąganie, kotwica solvera) tak jak Vertex.
// Pamięta przesunięcie względem swojego wierzchołka, więc każdy ruch wierzchołka
// (także wykonany przez solver) niesie go ze sobą i kształt krzywej przy wierzchołku się nie zmienia.
public sealed class ControlPoint
{
    public Edge Edge { get; }

    // Control1 należy do Edge.Start, Control2 do Edge.End.
    public Vertex Owner { get; }

    public PointF Offset { get; set; }

    public PointF Position
    {
        get => new(Owner.Position.X + Offset.X, Owner.Position.Y + Offset.Y);
        set => Offset = new PointF(value.X - Owner.Position.X, value.Y - Owner.Position.Y);
    }

    public ControlPoint(Edge edge, Vertex owner, PointF position)
    {
        Edge = edge ?? throw new ArgumentNullException(nameof(edge));
        Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        Position = position;
    }

    public void Translate(float dx, float dy)
    {
        Offset = new PointF(Offset.X + dx, Offset.Y + dy);
    }
}

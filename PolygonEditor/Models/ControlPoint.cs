namespace PolygonEditor.Models;

// Bezier control point. It is a class and not a PointF so it has an identity
// (mouse hit, dragging, solver anchor), same as Vertex.
// It stores the offset from its vertex, so every vertex move (also done by the solver)
// takes it along and the curve shape near the vertex stays the same.
public sealed class ControlPoint
{
    public Edge Edge { get; }

    // Control1 belongs to Edge.Start, Control2 to Edge.End
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

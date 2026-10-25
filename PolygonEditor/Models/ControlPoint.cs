namespace PolygonEditor.Models;

// Punkt kontrolny Béziera. Klasa, a nie PointF, żeby mieć tożsamość
// (trafienie myszą, przeciąganie, kotwica solvera) tak jak Vertex.
public sealed class ControlPoint
{
    public Edge Edge { get; }
    public PointF Position { get; set; }

    public ControlPoint(Edge edge, PointF position)
    {
        Edge = edge ?? throw new ArgumentNullException(nameof(edge));
        Position = position;
    }

    public void Translate(float dx, float dy)
    {
        Position = new PointF(Position.X + dx, Position.Y + dy);
    }
}

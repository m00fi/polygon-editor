using PolygonEditor.Models;

namespace PolygonEditor.Constraints;

/// <summary>Ograniczenie stałej długości krawędzi.</summary>
public sealed class LengthConstraint : IConstraint
{
    public Guid Id { get; } = Guid.NewGuid();
    public Edge Edge { get; }
    public float TargetLength { get; set; }

    public IReadOnlyList<Edge> InvolvedEdges => [Edge];

    public LengthConstraint(Edge edge, float targetLength)
    {
        Edge = edge ?? throw new ArgumentNullException(nameof(edge));
        TargetLength = Math.Max(1f, targetLength);
    }

    public bool InvolvesEdge(Edge edge) => edge == Edge;

    public void Satisfy(ConstraintAnchor anchor)
    {
        Vertex fixedVertex;
        Vertex freeVertex;

        if (anchor.FixedVertex == Edge.Start)
        {
            fixedVertex = Edge.Start;
            freeVertex = Edge.End;
        }
        else if (anchor.FixedVertex == Edge.End)
        {
            fixedVertex = Edge.End;
            freeVertex = Edge.Start;
        }
        else
        {
            fixedVertex = Edge.Start;
            freeVertex = Edge.End;
        }

        float dx = freeVertex.Position.X - fixedVertex.Position.X;
        float dy = freeVertex.Position.Y - fixedVertex.Position.Y;
        float currentLength = MathF.Sqrt(dx * dx + dy * dy);

        float dirX;
        float dirY;
        if (currentLength < 1e-4f)
        {
            dirX = 1f;
            dirY = 0f;
        }
        else
        {
            dirX = dx / currentLength;
            dirY = dy / currentLength;
        }

        freeVertex.Position = new PointF(
            fixedVertex.Position.X + dirX * TargetLength,
            fixedVertex.Position.Y + dirY * TargetLength);
    }
}

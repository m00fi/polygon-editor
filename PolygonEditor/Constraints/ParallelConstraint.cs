using PolygonEditor.Models;

namespace PolygonEditor.Constraints;

/// <summary>Ograniczenie równoległości dwóch krawędzi.</summary>
public sealed class ParallelConstraint : IConstraint
{
    public Guid Id { get; } = Guid.NewGuid();
    public Edge Edge1 { get; }
    public Edge Edge2 { get; }

    public IReadOnlyList<Edge> InvolvedEdges => [Edge1, Edge2];

    public ParallelConstraint(Edge edge1, Edge edge2)
    {
        Edge1 = edge1 ?? throw new ArgumentNullException(nameof(edge1));
        Edge2 = edge2 ?? throw new ArgumentNullException(nameof(edge2));
        if (ReferenceEquals(edge1, edge2))
            throw new ArgumentException("Parallel constraint requires two distinct edges.");
    }

    public bool InvolvesEdge(Edge edge) => edge == Edge1 || edge == Edge2;

    public void Satisfy(ConstraintAnchor anchor)
    {
        ResolveEdges(anchor, out Edge fixedEdge, out Edge freeEdge);
        ResolveVertices(freeEdge, anchor, out Vertex fixedVertex, out Vertex freeVertex);

        PointF fixedDir = fixedEdge.Direction;
        float fixedLen = MathF.Sqrt(fixedDir.X * fixedDir.X + fixedDir.Y * fixedDir.Y);
        if (fixedLen < 1e-4f)
            return;

        float ux = fixedDir.X / fixedLen;
        float uy = fixedDir.Y / fixedLen;

        float dx = freeVertex.Position.X - fixedVertex.Position.X;
        float dy = freeVertex.Position.Y - fixedVertex.Position.Y;
        float freeLen = MathF.Sqrt(dx * dx + dy * dy);
        if (freeLen < 1e-4f)
            freeLen = 1f;

        // Zachowaj dotychczasowy zwrot free edge, jeśli to możliwe.
        if (dx * ux + dy * uy < 0f)
        {
            ux = -ux;
            uy = -uy;
        }

        freeVertex.Position = new PointF(
            fixedVertex.Position.X + ux * freeLen,
            fixedVertex.Position.Y + uy * freeLen);
    }

    private void ResolveEdges(ConstraintAnchor anchor, out Edge fixedEdge, out Edge freeEdge)
    {
        if (anchor.FixedEdge == Edge1)
        {
            fixedEdge = Edge1;
            freeEdge = Edge2;
            return;
        }

        if (anchor.FixedEdge == Edge2)
        {
            fixedEdge = Edge2;
            freeEdge = Edge1;
            return;
        }

        Vertex? fixedVertex = anchor.FixedVertex;
        if (fixedVertex is not null)
        {
            bool onEdge1 = fixedVertex == Edge1.Start || fixedVertex == Edge1.End;
            bool onEdge2 = fixedVertex == Edge2.Start || fixedVertex == Edge2.End;

            if (onEdge1 && !onEdge2)
            {
                fixedEdge = Edge1;
                freeEdge = Edge2;
                return;
            }

            if (onEdge2 && !onEdge1)
            {
                fixedEdge = Edge2;
                freeEdge = Edge1;
                return;
            }
        }

        fixedEdge = Edge1;
        freeEdge = Edge2;
    }

    private static void ResolveVertices(
        Edge freeEdge,
        ConstraintAnchor anchor,
        out Vertex fixedVertex,
        out Vertex freeVertex)
    {
        if (anchor.FixedVertex == freeEdge.Start)
        {
            fixedVertex = freeEdge.Start;
            freeVertex = freeEdge.End;
            return;
        }

        if (anchor.FixedVertex == freeEdge.End)
        {
            fixedVertex = freeEdge.End;
            freeVertex = freeEdge.Start;
            return;
        }

        fixedVertex = freeEdge.Start;
        freeVertex = freeEdge.End;
    }
}

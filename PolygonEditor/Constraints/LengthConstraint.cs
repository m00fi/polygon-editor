using PolygonEditor.Models;

namespace PolygonEditor.Constraints;

public sealed class LengthConstraint : IConstraint
{
    // allowed length error in pixels
    public const float Tolerance = 0.5f;

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

    public bool IsSatisfied() => MathF.Abs(Edge.Length - TargetLength) <= Tolerance;

    public void Satisfy(SolveContext context)
    {
        Vertex start = Edge.Start;
        Vertex end = Edge.End;
        if (context.IsFixed(start) && context.IsFixed(end))
            return;

        float currentLength = Edge.Length;
        float dirX = 1f;
        float dirY = 0f;
        if (currentLength >= 1e-4f)
        {
            dirX = Edge.Direction.X / currentLength;
            dirY = Edge.Direction.Y / currentLength;
        }

        int startRank = context.RankOf(start);
        int endRank = context.RankOf(end);

        if (startRank < endRank)
        {
            // Start is closer to the anchor, so move End along the edge
            end.Position = new PointF(
                start.Position.X + dirX * TargetLength,
                start.Position.Y + dirY * TargetLength);
        }
        else if (endRank < startRank)
        {
            start.Position = new PointF(
                end.Position.X - dirX * TargetLength,
                end.Position.Y - dirY * TargetLength);
        }
        else
        {
            // same rank: move both ends evenly around the middle
            PointF mid = Edge.Midpoint;
            float half = TargetLength * 0.5f;
            start.Position = new PointF(mid.X - dirX * half, mid.Y - dirY * half);
            end.Position = new PointF(mid.X + dirX * half, mid.Y + dirY * half);
        }
    }
}

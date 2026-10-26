using PolygonEditor.Models;

namespace PolygonEditor.Constraints;

public sealed class ParallelConstraint : IConstraint
{
    // allowed sin of the angle between the edges (about 0.3 degrees)
    public const float Tolerance = 0.005f;

    public Guid Id { get; } = Guid.NewGuid();
    public Edge Edge1 { get; }
    public Edge Edge2 { get; }

    public IReadOnlyList<Edge> InvolvedEdges => [Edge1, Edge2];

    public ParallelConstraint(Edge edge1, Edge edge2)
    {
        Edge1 = edge1 ?? throw new ArgumentNullException(nameof(edge1));
        Edge2 = edge2 ?? throw new ArgumentNullException(nameof(edge2));
        if (!CanCreate(edge1, edge2, out string? reason))
            throw new ArgumentException(reason);
    }

    public static bool CanCreate(Edge edge1, Edge edge2, out string? reason)
    {
        if (ReferenceEquals(edge1, edge2))
        {
            reason = "Cannot make an edge parallel to itself.";
            return false;
        }

        if (edge1.Kind != EdgeKind.Line || edge2.Kind != EdgeKind.Line)
        {
            reason = "Only straight edges can be parallel.";
            return false;
        }

        if (AreAdjacent(edge1, edge2))
        {
            reason = "Adjacent edges (sharing a vertex) cannot be parallel.";
            return false;
        }

        reason = null;
        return true;
    }

    private static bool AreAdjacent(Edge a, Edge b) =>
        a.Start == b.Start || a.Start == b.End || a.End == b.Start || a.End == b.End;

    public bool InvolvesEdge(Edge edge) => edge == Edge1 || edge == Edge2;

    public bool IsSatisfied()
    {
        if (!TryGetUnit(Edge1, out float ux1, out float uy1) || !TryGetUnit(Edge2, out float ux2, out float uy2))
            return true;

        return MathF.Abs(ux1 * uy2 - uy1 * ux2) <= Tolerance;
    }

    public void Satisfy(SolveContext context)
    {
        ResolveEdges(context, out Edge fixedEdge, out Edge freeEdge);
        if (IsFullyFixed(freeEdge, context))
        {
            (fixedEdge, freeEdge) = (freeEdge, fixedEdge);
            if (IsFullyFixed(freeEdge, context))
                return;
        }

        if (!TryGetUnit(fixedEdge, out float ux, out float uy))
            return;

        PointF freeDir = freeEdge.Direction;
        float freeLen = freeEdge.Length;
        if (freeLen < 1e-4f)
            freeLen = 1f;

        // keep the current direction of the rotated edge
        if (freeDir.X * ux + freeDir.Y * uy < 0f)
        {
            ux = -ux;
            uy = -uy;
        }

        Vertex start = freeEdge.Start;
        Vertex end = freeEdge.End;
        int startRank = context.RankOf(start);
        int endRank = context.RankOf(end);

        if (startRank < endRank)
        {
            end.Position = new PointF(start.Position.X + ux * freeLen, start.Position.Y + uy * freeLen);
        }
        else if (endRank < startRank)
        {
            start.Position = new PointF(end.Position.X - ux * freeLen, end.Position.Y - uy * freeLen);
        }
        else
        {
            // same rank: rotate around the middle of the edge
            PointF mid = freeEdge.Midpoint;
            float half = freeLen * 0.5f;
            start.Position = new PointF(mid.X - ux * half, mid.Y - uy * half);
            end.Position = new PointF(mid.X + ux * half, mid.Y + uy * half);
        }
    }

    // the edge closer to the anchor stays (lower rank, then lower rank sum); if equal, Edge1 stays
    private void ResolveEdges(SolveContext context, out Edge fixedEdge, out Edge freeEdge)
    {
        int min1 = Math.Min(context.RankOf(Edge1.Start), context.RankOf(Edge1.End));
        int min2 = Math.Min(context.RankOf(Edge2.Start), context.RankOf(Edge2.End));
        long sum1 = (long)context.RankOf(Edge1.Start) + context.RankOf(Edge1.End);
        long sum2 = (long)context.RankOf(Edge2.Start) + context.RankOf(Edge2.End);

        bool edge2Closer = min2 < min1 || (min2 == min1 && sum2 < sum1);
        fixedEdge = edge2Closer ? Edge2 : Edge1;
        freeEdge = edge2Closer ? Edge1 : Edge2;
    }

    private static bool IsFullyFixed(Edge edge, SolveContext context) =>
        context.IsFixed(edge.Start) && context.IsFixed(edge.End);

    private static bool TryGetUnit(Edge edge, out float ux, out float uy)
    {
        float len = edge.Length;
        if (len < 1e-4f)
        {
            ux = uy = 0f;
            return false;
        }

        ux = edge.Direction.X / len;
        uy = edge.Direction.Y / len;
        return true;
    }
}

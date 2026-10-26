using PolygonEditor.Models;

namespace PolygonEditor.Constraints;

// Rank of a vertex = BFS distance from the anchor (0 = does not move). Constraints move
// the vertex with the higher rank, so the fixes spread out from the anchor.
public sealed class SolveContext
{
    public const int Unreachable = int.MaxValue;

    private readonly Dictionary<Vertex, int> _ranks;
    private readonly ControlPoint? _fixedControlPoint;

    private SolveContext(Dictionary<Vertex, int> ranks, ControlPoint? fixedControlPoint)
    {
        _ranks = ranks;
        _fixedControlPoint = fixedControlPoint;
    }

    public int RankOf(Vertex vertex) => _ranks.TryGetValue(vertex, out int rank) ? rank : Unreachable;

    public bool IsFixed(Vertex vertex) => RankOf(vertex) == 0;

    // a control point is fixed only when it is the anchor (being dragged), the rest are free
    public bool IsFixed(ControlPoint controlPoint) => controlPoint == _fixedControlPoint;

    public static SolveContext Build(IReadOnlyList<IConstraint> constraints, ConstraintAnchor anchor)
    {
        Dictionary<Vertex, List<Vertex>> links = BuildConstraintLinks(constraints);
        var ranks = new Dictionary<Vertex, int>();
        var queue = new Queue<Vertex>();

        // when dragging a control point both vertices of its segment stay in place
        Edge? fixedEdge = anchor.FixedEdge ?? anchor.FixedControlPoint?.Edge;
        foreach (Vertex? seed in new[] { anchor.FixedVertex, fixedEdge?.Start, fixedEdge?.End })
        {
            if (seed is not null && ranks.TryAdd(seed, 0))
                queue.Enqueue(seed);
        }

        while (queue.Count > 0)
        {
            Vertex vertex = queue.Dequeue();
            int next = ranks[vertex] + 1;
            foreach (Vertex neighbour in Neighbours(vertex, links))
            {
                if (ranks.TryAdd(neighbour, next))
                    queue.Enqueue(neighbour);
            }
        }

        return new SolveContext(ranks, anchor.FixedControlPoint);
    }

    private static IEnumerable<Vertex> Neighbours(Vertex vertex, Dictionary<Vertex, List<Vertex>> links)
    {
        if (vertex.Incoming is not null)
            yield return vertex.Incoming.Start;
        if (vertex.Outgoing is not null)
            yield return vertex.Outgoing.End;
        if (links.TryGetValue(vertex, out List<Vertex>? linked))
        {
            foreach (Vertex other in linked)
                yield return other;
        }
    }

    // links vertices of edges that share a constraint (e.g. parallel edges in different polygons)
    private static Dictionary<Vertex, List<Vertex>> BuildConstraintLinks(IReadOnlyList<IConstraint> constraints)
    {
        var links = new Dictionary<Vertex, List<Vertex>>();
        foreach (IConstraint constraint in constraints)
        {
            if (constraint.InvolvedEdges.Count < 2)
                continue;

            List<Vertex> vertices = constraint.InvolvedEdges
                .SelectMany(e => new[] { e.Start, e.End })
                .Distinct()
                .ToList();

            foreach (Vertex a in vertices)
            {
                if (!links.TryGetValue(a, out List<Vertex>? list))
                    links[a] = list = [];
                list.AddRange(vertices.Where(b => b != a));
            }
        }

        return links;
    }
}

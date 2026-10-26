namespace PolygonEditor.Models;

// Closed polygon (self intersections are allowed).
public sealed class Polygon
{
    private readonly List<Vertex> _vertices = [];
    private readonly List<Edge> _edges = [];

    public IReadOnlyList<Vertex> Vertices => _vertices;
    public IReadOnlyList<Edge> Edges => _edges;

    public int VertexCount => _vertices.Count;

    private Polygon()
    {
    }

    public static Polygon FromPoints(IReadOnlyList<PointF> points)
    {
        ArgumentNullException.ThrowIfNull(points);
        if (points.Count < 3)
            throw new ArgumentException("A polygon requires at least 3 vertices.", nameof(points));

        var polygon = new Polygon();
        foreach (PointF point in points)
            polygon._vertices.Add(new Vertex(point));

        polygon.RebuildEdges();
        return polygon;
    }

    public static Polygon FromPoints(params PointF[] points) => FromPoints((IReadOnlyList<PointF>)points);

    // control points are stored relative to the vertices, so they move with them
    public void Translate(float dx, float dy)
    {
        foreach (Vertex vertex in _vertices)
            vertex.Translate(dx, dy);
    }

    // Shoelace formula. With Y pointing down, a result > 0 means clockwise on screen.
    // For a self intersecting polygon the sign shows which direction wins.
    public float SignedArea
    {
        get
        {
            float sum = 0f;
            int n = _vertices.Count;
            for (int i = 0; i < n; i++)
            {
                PointF a = _vertices[i].Position;
                PointF b = _vertices[(i + 1) % n].Position;
                sum += a.X * b.Y - b.X * a.Y;
            }

            return sum * 0.5f;
        }
    }

    public bool ContainsVertex(Vertex vertex) => _vertices.Contains(vertex);

    public bool ContainsEdge(Edge edge) => _edges.Contains(edge);

    public int IndexOfVertex(Vertex vertex) => _vertices.IndexOf(vertex);

    public int IndexOfEdge(Edge edge) => _edges.IndexOf(edge);

    // adds a vertex in the middle of the edge (the edge is replaced by two new ones)
    public Vertex InsertVertexAtMidpoint(Edge edge)
    {
        int edgeIndex = RequireEdgeIndex(edge);
        var mid = new Vertex(edge.Midpoint);

        Vertex start = edge.Start;
        Vertex end = edge.End;

        int vertexInsertIndex = (edgeIndex + 1) % _vertices.Count;
        if (edgeIndex == _vertices.Count - 1)
            _vertices.Add(mid);
        else
            _vertices.Insert(vertexInsertIndex, mid);

        var first = new Edge(start, mid) { Kind = EdgeKind.Line };
        var second = new Edge(mid, end) { Kind = EdgeKind.Line };

        start.Outgoing = first;
        mid.Incoming = first;
        mid.Outgoing = second;
        end.Incoming = second;

        _edges[edgeIndex] = first;
        _edges.Insert(edgeIndex + 1, second);

        return mid;
    }

    public bool CanRemoveVertex(Vertex vertex) => _vertices.Count > 3 && _vertices.Contains(vertex);

    // returns false if the polygon would end up with less than 3 vertices
    public bool RemoveVertex(Vertex vertex)
    {
        int index = _vertices.IndexOf(vertex);
        if (index < 0)
            throw new ArgumentException("Vertex does not belong to this polygon.", nameof(vertex));
        if (_vertices.Count <= 3)
            return false;

        Edge? incoming = vertex.Incoming;
        Edge? outgoing = vertex.Outgoing;
        if (incoming is null || outgoing is null)
            throw new InvalidOperationException("Vertex is not fully linked in the polygon topology.");

        Vertex prev = incoming.Start;
        Vertex next = outgoing.End;

        var merged = new Edge(prev, next) { Kind = EdgeKind.Line };
        prev.Outgoing = merged;
        next.Incoming = merged;

        int incomingIndex = _edges.IndexOf(incoming);
        int outgoingIndex = _edges.IndexOf(outgoing);
        if (incomingIndex < 0 || outgoingIndex < 0)
            throw new InvalidOperationException("Adjacent edges are missing from the polygon edge list.");

        // the merged edge always replaces the incoming edge, so
        // Edges[i] still starts at Vertices[i] (also when removing vertex 0)
        _edges[incomingIndex] = merged;
        _edges.RemoveAt(outgoingIndex);

        _vertices.RemoveAt(index);
        vertex.Incoming = null;
        vertex.Outgoing = null;
        return true;
    }

    public IEnumerable<Edge> EdgesAffectedByVertexRemoval(Vertex vertex)
    {
        if (vertex.Incoming is not null)
            yield return vertex.Incoming;
        if (vertex.Outgoing is not null)
            yield return vertex.Outgoing;
    }

    private int RequireEdgeIndex(Edge edge)
    {
        int index = _edges.IndexOf(edge);
        if (index < 0)
            throw new ArgumentException("Edge does not belong to this polygon.", nameof(edge));
        return index;
    }

    private void RebuildEdges()
    {
        _edges.Clear();
        int n = _vertices.Count;
        for (int i = 0; i < n; i++)
        {
            Vertex start = _vertices[i];
            Vertex end = _vertices[(i + 1) % n];
            var edge = new Edge(start, end);
            start.Outgoing = edge;
            end.Incoming = edge;
            _edges.Add(edge);
        }
    }
}

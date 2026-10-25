using PolygonEditor.Constraints;

namespace PolygonEditor.Models;

// Stan geometrii i lista ograniczeń - pozwala wycofać operację, której solver nie umie spełnić.
public sealed class SceneSnapshot
{
    private readonly ConstraintStore _store;
    private readonly List<IConstraint> _constraints;
    private readonly List<(Vertex Vertex, PointF Position)> _vertices = [];
    private readonly List<(Edge Edge, PointF Control1, PointF Control2)> _edges = [];

    private SceneSnapshot(ConstraintStore store)
    {
        _store = store;
        _constraints = [.. store.Constraints];
    }

    public static SceneSnapshot Capture(Scene scene)
    {
        var snapshot = new SceneSnapshot(scene.Constraints);
        foreach (Polygon polygon in scene.Polygons)
        {
            foreach (Vertex vertex in polygon.Vertices)
                snapshot._vertices.Add((vertex, vertex.Position));
            foreach (Edge edge in polygon.Edges)
                snapshot._edges.Add((edge, edge.Control1, edge.Control2));
        }

        return snapshot;
    }

    public void Restore()
    {
        foreach ((Vertex vertex, PointF position) in _vertices)
            vertex.Position = position;
        foreach ((Edge edge, PointF control1, PointF control2) in _edges)
        {
            edge.Control1 = control1;
            edge.Control2 = control2;
        }

        _store.RestoreFrom(_constraints);
    }
}

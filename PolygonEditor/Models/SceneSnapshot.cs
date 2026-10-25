using PolygonEditor.Constraints;

namespace PolygonEditor.Models;

// Stan geometrii i lista ograniczeń - pozwala wycofać operację, której solver nie umie spełnić.
public sealed class SceneSnapshot
{
    private readonly ConstraintStore _store;
    private readonly List<IConstraint> _constraints;
    private readonly List<(Vertex Vertex, PointF Position)> _vertices = [];
    private readonly List<(ControlPoint ControlPoint, PointF Offset)> _controlPoints = [];

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
            {
                snapshot._controlPoints.Add((edge.Control1, edge.Control1.Offset));
                snapshot._controlPoints.Add((edge.Control2, edge.Control2.Offset));
            }
        }

        return snapshot;
    }

    public void Restore()
    {
        foreach ((Vertex vertex, PointF position) in _vertices)
            vertex.Position = position;
        foreach ((ControlPoint controlPoint, PointF offset) in _controlPoints)
            controlPoint.Offset = offset;

        _store.RestoreFrom(_constraints);
    }
}

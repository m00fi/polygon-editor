using PolygonEditor.Constraints;

namespace PolygonEditor.Models;

// Saves the geometry and the constraint list, so we can undo an operation the solver could not satisfy.
public sealed class SceneSnapshot
{
    private readonly ConstraintStore _store;
    private readonly List<IConstraint> _constraints;
    private readonly List<(Vertex Vertex, PointF Position, Continuity Continuity, bool UserSet)> _vertices = [];
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
                snapshot._vertices.Add((vertex, vertex.Position, vertex.Continuity, vertex.ContinuityUserSet));
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
        foreach ((Vertex vertex, PointF position, Continuity continuity, bool userSet) in _vertices)
        {
            vertex.Position = position;
            vertex.Continuity = continuity;
            vertex.ContinuityUserSet = userSet;
        }
        foreach ((ControlPoint controlPoint, PointF offset) in _controlPoints)
            controlPoint.Offset = offset;

        _store.RestoreFrom(_constraints);
    }
}

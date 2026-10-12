using PolygonEditor.Models;
using PolygonEditor.Constraints;

namespace PolygonEditor.Scene;

/// <summary>Buduje początkową scenę demonstracyjną.</summary>
public static class DemoSceneFactory
{
    public static Models.Scene Create()
    {
        var scene = new Models.Scene();
        var constraintStore = new ConstraintStore();

        // Prostokąt po lewej.
        scene.Add(Models.Polygon.FromPoints(
            new PointF(80, 80),
            new PointF(280, 80),
            new PointF(280, 220),
            new PointF(80, 220)));

        // Nieforemny pięciokąt po prawej (z samoprzecięciem opcjonalnie później).
        scene.Add(Models.Polygon.FromPoints(
            new PointF(380, 100),
            new PointF(560, 70),
            new PointF(620, 200),
            new PointF(500, 280),
            new PointF(360, 220)));

        constraintStore.AddConstraint(new LengthConstraint(scene.Polygons[0].Edges[0], 200.0f));
        constraintStore.AddConstraint(new ParallelConstraint(scene.Polygons[0].Edges[0], scene.Polygons[0].Edges[2]));

        scene.AddConstraintStore(constraintStore);

        return scene;
    }
}

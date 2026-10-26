using PolygonEditor.Constraints;
using PolygonEditor.Models;

namespace PolygonEditor.Scene;

// Starting scene. New polygon: AddPolygon(scene, points...), name the edges you need
// (Edges[i] goes from point i to the next one), then add constraints / curves. Settle at the end
// moves the control points so they match the continuity.
public static class DemoSceneFactory
{
    public static Models.Scene Create()
    {
        var scene = new Models.Scene();

        // Pentagon: length 220 and top-bottom parallel (pair 1), left edge parallel to the hexagon (pair 2),
        // Bezier with G1 at the start (default) and C1 at the end (next to a line: handle = 1/3 of its length).
        Models.Polygon pentagon = AddPolygon(scene, (127, 110), (347, 97), (374, 218), (293, 309), (22, 325));
        Edge pentagonTop = pentagon.Edges[0];
        Edge pentagonCurve = pentagon.Edges[1];
        Edge pentagonBottom = pentagon.Edges[3];
        Edge pentagonLeft = pentagon.Edges[4];

        scene.Constraints.AddConstraint(new LengthConstraint(pentagonTop, 220f));
        scene.Constraints.AddConstraint(new ParallelConstraint(pentagonTop, pentagonBottom));
        pentagonCurve.End.SetContinuityByUser(Continuity.C1);
        scene.SetEdgeKind(pentagonCurve, EdgeKind.CubicBezier);
        pentagonCurve.Control1.Position = new PointF(407, 92);
        pentagonCurve.Control2.Position = new PointF(401, 187);

        // Hexagon: lower left edge has two constraints at once (length 70 and parallel to the pentagon),
        // arc with G1 at the start (tangent to the upper left edge, end is G0), half circle with G0 at both ends
        // and a Bezier with G0 next to the half circle and default G1 next to the left edge.
        Models.Polygon hexagon = AddPolygon(scene, (485, 224), (516, 161), (570, 96), (800, 215), (731, 351), (571, 371));
        Edge hexagonLeft = hexagon.Edges[0];
        Edge hexagonTangentArc = hexagon.Edges[2];
        Edge hexagonSemicircle = hexagon.Edges[4];
        Edge hexagonCurve = hexagon.Edges[5];

        scene.Constraints.AddConstraint(new LengthConstraint(hexagonLeft, 70f));
        scene.Constraints.AddConstraint(new ParallelConstraint(pentagonLeft, hexagonLeft));
        scene.SetEdgeKind(hexagonTangentArc, EdgeKind.CircularArc);
        scene.SetContinuity(hexagonTangentArc.Start, Continuity.G1);
        scene.SetEdgeKind(hexagonSemicircle, EdgeKind.CircularArc);
        hexagonCurve.Start.SetContinuityByUser(Continuity.G0);
        scene.SetEdgeKind(hexagonCurve, EdgeKind.CubicBezier);
        hexagonCurve.Control1.Position = new PointF(603, 291);
        hexagonCurve.Control2.Position = new PointF(451, 293);

        Settle(scene);
        return scene;
    }

    private static Models.Polygon AddPolygon(Models.Scene scene, params (float X, float Y)[] points)
    {
        Models.Polygon polygon = Models.Polygon.FromPoints(points.Select(p => new PointF(p.X, p.Y)).ToArray());
        scene.Add(polygon);
        return polygon;
    }

    // solve once so the scene starts in a valid state
    private static void Settle(Models.Scene scene)
    {
        if (!new ConstraintSolver().Solve(scene, new ConstraintAnchor()))
            throw new InvalidOperationException("Demo scene constraints cannot be satisfied.");
    }
}

using PolygonEditor.Constraints;
using PolygonEditor.Models;

namespace PolygonEditor.Scene;

// Scena startowa. Nowy wielokąt: AddPolygon(scene, punkty...), nazwać potrzebne krawędzie
// (Edges[i] biegnie od i-tego do następnego punktu), dodać ograniczenia / krzywe. Settle na końcu
// dopasowuje punkty kontrolne do ustawionej ciągłości.
public static class DemoSceneFactory
{
    public static Models.Scene Create()
    {
        var scene = new Models.Scene();

        // Pięciokąt: długość 220 i równoległość góra-dół (para 1), lewa krawędź równoległa do sześciokąta (para 2),
        // Bézier z G1 na początku (domyślne) i C1 na końcu (obok odcinka: uchwyt = 1/3 jego długości).
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

        // Sześciokąt: lewa dolna krawędź ma dwa ograniczenia naraz (długość 70 i równoległość z pięciokątem),
        // łuk z G1 na początku (styczny do lewej górnej krawędzi, koniec G0), półokrąg z G0 na obu końcach
        // i Bézier z G0 przy półokręgu oraz domyślnym G1 przy lewej krawędzi.
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

    // Jednorazowe rozwiązanie ograniczeń, żeby scena startowała w stanie spójnym.
    private static void Settle(Models.Scene scene)
    {
        if (!new ConstraintSolver().Solve(scene, new ConstraintAnchor()))
            throw new InvalidOperationException("Demo scene constraints cannot be satisfied.");
    }
}

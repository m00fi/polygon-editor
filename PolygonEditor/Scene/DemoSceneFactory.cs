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

        // Pięciokąt bez samoprzecięć: długość 220 i równoległość góra-dół,
        // Bézier z G1 na początku (domyślne) i C1 na końcu (obok odcinka: uchwyt = 1/3 jego długości).
        Models.Polygon pentagon = AddPolygon(scene, (80, 80), (300, 80), (380, 200), (300, 320), (80, 320));
        Edge pentagonTop = pentagon.Edges[0];
        Edge pentagonCurve = pentagon.Edges[1];
        Edge pentagonBottom = pentagon.Edges[3];
        Edge pentagonLeft = pentagon.Edges[4];

        scene.Constraints.AddConstraint(new LengthConstraint(pentagonTop, 220f));
        scene.Constraints.AddConstraint(new ParallelConstraint(pentagonTop, pentagonBottom));
        pentagonCurve.End.SetContinuityByUser(Continuity.C1);
        scene.SetEdgeKind(pentagonCurve, EdgeKind.CubicBezier);

        // "Klepsydra" z jednym samoprzecięciem (przekątne się krzyżują): lewa krawędź ma dwa ograniczenia
        // naraz - długość i równoległość z pięciokątem (między wielokątami); prawa to Bézier z G0 (ostre narożniki).
        Models.Polygon hourglass = AddPolygon(scene, (520, 100), (760, 320), (760, 100), (520, 320));
        Edge hourglassCurve = hourglass.Edges[1];
        Edge hourglassLeft = hourglass.Edges[3];

        scene.Constraints.AddConstraint(new LengthConstraint(hourglassLeft, 220f));
        scene.Constraints.AddConstraint(new ParallelConstraint(pentagonLeft, hourglassLeft));
        hourglassCurve.Start.SetContinuityByUser(Continuity.G0);
        hourglassCurve.End.SetContinuityByUser(Continuity.G0);
        scene.SetEdgeKind(hourglassCurve, EdgeKind.CubicBezier);

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

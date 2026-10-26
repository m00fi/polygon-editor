using PolygonEditor.Models;

namespace PolygonEditor.Drawing;

// Punkty łuku okręgu przyrostowo: wektor promienia jest obracany o stały kąt Δ
// (cos Δ i sin Δ liczone raz), więc w pętli są tylko mnożenia i dodawania.
public static class ArcIncremental
{
    public const float PixelsPerStep = 4f;
    public const int MinSteps = 8;
    public const int MaxSteps = 1000;

    public static int StepsFor(ArcGeometry arc)
    {
        float length = MathF.Abs(arc.SweepAngle) * arc.Radius;
        return Math.Clamp((int)MathF.Ceiling(length / PixelsPerStep), MinSteps, MaxSteps);
    }

    public static PointF[] ComputePoints(Edge edge) => ComputePoints(edge.Arc, edge.End.Position);

    // end - dokładny koniec łuku (bez błędu sumowania obrotów).
    public static PointF[] ComputePoints(ArcGeometry arc, PointF end)
    {
        int steps = StepsFor(arc);
        double delta = arc.SweepAngle / (double)steps;
        double cos = Math.Cos(delta);
        double sin = Math.Sin(delta);

        double rx = arc.Radius * Math.Cos(arc.StartAngle);
        double ry = arc.Radius * Math.Sin(arc.StartAngle);

        var points = new PointF[steps + 1];
        for (int i = 0; i <= steps; i++)
        {
            points[i] = new PointF((float)(arc.Center.X + rx), (float)(arc.Center.Y + ry));
            (rx, ry) = (rx * cos - ry * sin, rx * sin + ry * cos);
        }

        points[steps] = end;
        return points;
    }
}

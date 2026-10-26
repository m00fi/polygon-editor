using PolygonEditor.Models;

namespace PolygonEditor.Drawing;

// Cubic Bezier points with the incremental method: convert to power basis
// B(t) = a*t^3 + b*t^2 + c*t + d, then use forward differences so the loop only adds.
public static class BezierIncremental
{
    public const float PixelsPerStep = 4f;
    public const int MinSteps = 8;
    public const int MaxSteps = 1000;

    public static PointF[] ComputePoints(Edge edge) =>
        ComputePoints(edge.Start.Position, edge.Control1.Position, edge.Control2.Position, edge.End.Position);

    public static PointF[] ComputePoints(PointF p0, PointF p1, PointF p2, PointF p3) =>
        ComputePoints(p0, p1, p2, p3, StepsFor(p0, p1, p2, p3));

    // the control polygon is never shorter than the curve, so the segments are at most about 4 px
    public static int StepsFor(PointF p0, PointF p1, PointF p2, PointF p3)
    {
        float length = Distance(p0, p1) + Distance(p1, p2) + Distance(p2, p3);
        return Math.Clamp((int)MathF.Ceiling(length / PixelsPerStep), MinSteps, MaxSteps);
    }

    public static PointF[] ComputePoints(PointF p0, PointF p1, PointF p2, PointF p3, int steps)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(steps, 1);

        // power basis coefficients
        double ax = -p0.X + 3.0 * p1.X - 3.0 * p2.X + p3.X;
        double ay = -p0.Y + 3.0 * p1.Y - 3.0 * p2.Y + p3.Y;
        double bx = 3.0 * p0.X - 6.0 * p1.X + 3.0 * p2.X;
        double by = 3.0 * p0.Y - 6.0 * p1.Y + 3.0 * p2.Y;
        double cx = 3.0 * (p1.X - p0.X);
        double cy = 3.0 * (p1.Y - p0.Y);

        // forward differences for step h: d1 = a*h^3 + b*h^2 + c*h, d2 = 6a*h^3 + 2b*h^2, d3 = 6a*h^3
        double h = 1.0 / steps;
        double h2 = h * h;
        double h3 = h2 * h;

        double d1x = ax * h3 + bx * h2 + cx * h;
        double d1y = ay * h3 + by * h2 + cy * h;
        double d2x = 6.0 * ax * h3 + 2.0 * bx * h2;
        double d2y = 6.0 * ay * h3 + 2.0 * by * h2;
        double d3x = 6.0 * ax * h3;
        double d3y = 6.0 * ay * h3;

        var points = new PointF[steps + 1];
        double x = p0.X;
        double y = p0.Y;
        points[0] = p0;

        for (int i = 1; i <= steps; i++)
        {
            x += d1x;
            y += d1y;
            d1x += d2x;
            d1y += d2y;
            d2x += d3x;
            d2y += d3y;
            points[i] = new PointF((float)x, (float)y);
        }

        // last point exactly at P3 (no rounding error from the sums)
        points[steps] = p3;
        return points;
    }

    private static float Distance(PointF a, PointF b)
    {
        float dx = b.X - a.X;
        float dy = b.Y - a.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }
}

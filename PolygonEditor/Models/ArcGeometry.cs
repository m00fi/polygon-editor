namespace PolygonEditor.Models;

// Circle arc: point(angle) = Center + Radius * (cos angle, sin angle), angle goes from StartAngle to StartAngle + SweepAngle.
// Angles in radians, Y axis points down (positive sweep = clockwise on screen).
public readonly record struct ArcGeometry(PointF Center, float Radius, float StartAngle, float SweepAngle)
{
    // G0 at both ends: half circle, center in the middle of the chord, radius = half of the chord length.
    // side = +1 bends the arc towards the normal (-dy, dx), side = -1 the other way (same as Edge.InitializeControlPoints).
    public static ArcGeometry Semicircle(PointF start, PointF end, float side)
    {
        var center = new PointF((start.X + end.X) * 0.5f, (start.Y + end.Y) * 0.5f);
        float dx = end.X - start.X;
        float dy = end.Y - start.Y;
        float radius = MathF.Sqrt(dx * dx + dy * dy) * 0.5f;
        float startAngle = MathF.Atan2(start.Y - center.Y, start.X - center.X);

        // turning 90 degrees from the direction to Start gives -(-dy, dx), so for side = +1 we go the other way
        float sweep = side > 0f ? -MathF.PI : MathF.PI;
        return new ArcGeometry(center, radius, startAngle, sweep);
    }

    // G1 at the start: the arc leaves start in the tangent direction. The center is on the line through start
    // perpendicular to the tangent and is equally far from both ends: C = start + lambda * n, lambda = |d|^2 / (2 * dot(n, d)).
    // returns null when the tangent is parallel to the chord (the arc would be a straight line)
    public static ArcGeometry? TangentAtStart(PointF start, PointF end, PointF tangent)
    {
        float tLen = MathF.Sqrt(tangent.X * tangent.X + tangent.Y * tangent.Y);
        float dx = end.X - start.X;
        float dy = end.Y - start.Y;
        float dLen = MathF.Sqrt(dx * dx + dy * dy);
        if (tLen < 1e-4f || dLen < 1e-4f)
            return null;

        float nx = -tangent.Y / tLen;
        float ny = tangent.X / tLen;
        float nd = nx * dx + ny * dy;
        if (MathF.Abs(nd) < 1e-3f * dLen)
            return null;

        float lambda = dLen * dLen / (2f * nd);
        var center = new PointF(start.X + lambda * nx, start.Y + lambda * ny);
        float startAngle = MathF.Atan2(start.Y - center.Y, start.X - center.X);
        float endAngle = MathF.Atan2(end.Y - center.Y, end.X - center.X);

        // with a growing angle the arc at start moves in direction (-ry, rx); if that matches the tangent, sweep > 0
        float rx = start.X - center.X;
        float ry = start.Y - center.Y;
        bool positive = -ry * tangent.X + rx * tangent.Y > 0f;

        float sweep = endAngle - startAngle;
        if (positive)
        {
            while (sweep <= 0f)
                sweep += 2f * MathF.PI;
        }
        else
        {
            while (sweep >= 0f)
                sweep -= 2f * MathF.PI;
        }

        return new ArcGeometry(center, MathF.Abs(lambda), startAngle, sweep);
    }

    // G1 at the end: compute the arc from end to start with the opposite tangent, then reverse it
    public static ArcGeometry? TangentAtEnd(PointF start, PointF end, PointF tangent)
    {
        ArcGeometry? reversed = TangentAtStart(end, start, new PointF(-tangent.X, -tangent.Y));
        if (reversed is not { } r)
            return null;

        return new ArcGeometry(r.Center, r.Radius, r.StartAngle + r.SweepAngle, -r.SweepAngle);
    }

    // direction of movement along the arc at t
    public PointF TangentAt(float t)
    {
        float angle = StartAngle + SweepAngle * t;
        float sign = MathF.Sign(SweepAngle);
        return new PointF(-MathF.Sin(angle) * sign, MathF.Cos(angle) * sign);
    }

    public PointF PointAt(float t)
    {
        float angle = StartAngle + SweepAngle * t;
        return new PointF(Center.X + Radius * MathF.Cos(angle), Center.Y + Radius * MathF.Sin(angle));
    }
}

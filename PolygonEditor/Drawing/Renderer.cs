using System.Drawing.Drawing2D;
using PolygonEditor.Constraints;
using PolygonEditor.Models;

namespace PolygonEditor.Drawing;

public sealed class Renderer
{
    public Color BackgroundColor { get; set; } = Color.White;
    public Color EdgeColor { get; set; } = Color.Black;
    public Color VertexFillColor { get; set; } = Color.LightBlue;
    public Color VertexStrokeColor { get; set; } = Color.Black;
    public float VertexRadius { get; set; } = 5f;
    public Color ControlPolygonColor { get; set; } = Color.Black;
    public Color ControlPointFillColor { get; set; } = Color.Orange;
    public float ControlPointHalfSize { get; set; } = 4f;

    private static readonly Color ContinuityLabelColor = Color.DarkGreen;
    private static readonly Color ParallelMarkColor = Color.Red;
    private static readonly Color LengthLabelColor = Color.Blue;

    public void Render(Graphics target, int width, int height, Models.Scene scene, bool useBresenham)
    {
        if (width <= 0 || height <= 0)
            return;

        using var bitmap = new Bitmap(width, height);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.Clear(BackgroundColor);
            g.SmoothingMode = SmoothingMode.None;

            foreach (Models.Polygon polygon in scene.Polygons)
            {
                foreach (Edge edge in polygon.Edges)
                    DrawEdge(bitmap, g, edge, useBresenham);

                foreach (Edge edge in polygon.Edges)
                {
                    if (edge.IsBezier)
                        DrawControlPolygon(g, edge);
                }

                foreach (Vertex vertex in polygon.Vertices)
                    DrawVertex(g, vertex.Position);
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;
            foreach (Models.Polygon polygon in scene.Polygons)
            {
                foreach (Vertex vertex in polygon.Vertices)
                {
                    if (vertex.HasAdjacentCurve)
                        DrawContinuityLabel(g, vertex);
                }
            }

            int parallelNumber = 0;
            foreach (IConstraint constraint in scene.Constraints.Constraints)
            {
                if (constraint is LengthConstraint length)
                    DrawLength(g, length);
                else if (constraint is ParallelConstraint parallel)
                    DrawParallel(g, parallel, ++parallelNumber);
            }
        }

        target.DrawImageUnscaled(bitmap, 0, 0);
    }

    // Krzywa Béziera to łamana z punktów algorytmu przyrostowego, rysowana tym samym algorytmem co odcinki.
    private void DrawEdge(Bitmap bitmap, Graphics g, Edge edge, bool useBresenham)
    {
        using var pen = new Pen(EdgeColor, 1f);

        if (!edge.IsBezier)
        {
            DrawSegment(bitmap, g, pen, edge.Start.Position, edge.End.Position, useBresenham);
            return;
        }

        PointF[] points = BezierIncremental.ComputePoints(edge);
        for (int i = 1; i < points.Length; i++)
            DrawSegment(bitmap, g, pen, points[i - 1], points[i], useBresenham);
    }

    private static void DrawSegment(Bitmap bitmap, Graphics g, Pen pen, PointF a, PointF b, bool useBresenham)
    {
        int x0 = (int)MathF.Round(a.X);
        int y0 = (int)MathF.Round(a.Y);
        int x1 = (int)MathF.Round(b.X);
        int y1 = (int)MathF.Round(b.Y);

        if (useBresenham)
            Bresenham.DrawLine(bitmap, x0, y0, x1, y1, pen.Color);
        else
            g.DrawLine(pen, x0, y0, x1, y1);
    }

    // Wielobok Béziera P0-P1-P2-P3 linią przerywaną + punkty kontrolne jako kwadraty.
    private void DrawControlPolygon(Graphics g, Edge edge)
    {
        // 6px kreski 6px przerwy
        using var dashed = new Pen(ControlPolygonColor, 1.5f) { DashPattern = [4f, 4f] };
        g.DrawLines(dashed, [edge.Start.Position, edge.Control1.Position, edge.Control2.Position, edge.End.Position]);

        DrawControlPoint(g, edge.Control1.Position);
        DrawControlPoint(g, edge.Control2.Position);
    }

    private void DrawControlPoint(Graphics g, PointF position)
    {
        float s = ControlPointHalfSize;
        var rect = new RectangleF(position.X - s, position.Y - s, s * 2f, s * 2f);
        using var fill = new SolidBrush(ControlPointFillColor);
        using var stroke = new Pen(VertexStrokeColor, 1f);
        g.FillRectangle(fill, rect);
        g.DrawRectangle(stroke, rect.X, rect.Y, rect.Width, rect.Height);
    }

    private void DrawVertex(Graphics g, PointF position)
    {
        float r = VertexRadius;
        var rect = new RectangleF(position.X - r, position.Y - r, r * 2f, r * 2f);
        using var fill = new SolidBrush(VertexFillColor);
        using var stroke = new Pen(VertexStrokeColor, 1.5f);
        g.FillEllipse(fill, rect);
        g.DrawEllipse(stroke, rect);
    }

    // Etykieta odsunięta od środka sąsiednich wierzchołków, czyli zwykle na zewnątrz kąta.
    private static void DrawContinuityLabel(Graphics g, Vertex vertex)
    {
        if (vertex.Incoming is null || vertex.Outgoing is null)
            return;

        PointF p = vertex.Position;
        PointF prev = vertex.Incoming.Start.Position;
        PointF next = vertex.Outgoing.End.Position;
        float dx = p.X - (prev.X + next.X) * 0.5f;
        float dy = p.Y - (prev.Y + next.Y) * 0.5f;
        float len = MathF.Sqrt(dx * dx + dy * dy);
        if (len < 1e-4f)
        {
            dx = 0f;
            dy = -1f;
        }
        else
        {
            dx /= len;
            dy /= len;
        }

        const float offset = 18f;
        string text = vertex.Continuity.ToString();
        using var font = new Font("Segoe UI", 8f, FontStyle.Bold);
        using var brush = new SolidBrush(ContinuityLabelColor);
        SizeF size = g.MeasureString(text, font);
        g.DrawString(
            text,
            font,
            brush,
            p.X + dx * offset - size.Width * 0.5f,
            p.Y + dy * offset - size.Height * 0.5f);
    }

    private void DrawParallel(Graphics g, ParallelConstraint parallel, int number)
    {
        DrawParallelMarks(g, parallel.Edge1, number);
        DrawParallelMarks(g, parallel.Edge2, number);
    }

    // Dwie kreski w poprzek krawędzi + numer pary (ta sama liczba na obu krawędziach).
    private static void DrawParallelMarks(Graphics g, Edge edge, int number)
    {
        if (!TryGetEdgeFrame(edge, out PointF mid, out float ux, out float uy, out float nx, out float ny))
            return;

        const float tickHalf = 7f;
        const float spacing = 2.5f;

        using var pen = new Pen(ParallelMarkColor, 1.6f);

        DrawTick(g, pen, mid, ux, uy, nx, ny, -spacing, tickHalf);
        DrawTick(g, pen, mid, ux, uy, nx, ny, spacing, tickHalf);

        // Numer dokładnie po przeciwnej stronie niż etykieta długości (DrawLength odwraca normalną, gdy ny < 0).
        if (ny >= 0f)
        {
            nx = -nx;
            ny = -ny;
        }

        const float offset = 16f;
        string text = number.ToString();
        using var font = new Font("Segoe UI", 8f, FontStyle.Bold);
        using var brush = new SolidBrush(ParallelMarkColor);
        SizeF size = g.MeasureString(text, font);
        g.DrawString(
            text,
            font,
            brush,
            mid.X + nx * offset - size.Width * 0.5f,
            mid.Y + ny * offset - size.Height * 0.5f);
    }

    private static void DrawTick(
        Graphics g,
        Pen pen,
        PointF mid,
        float ux,
        float uy,
        float nx,
        float ny,
        float along,
        float tickHalf)
    {
        float cx = mid.X + ux * along;
        float cy = mid.Y + uy * along;
        g.DrawLine(
            pen,
            cx - nx * tickHalf,
            cy - ny * tickHalf,
            cx + nx * tickHalf,
            cy + ny * tickHalf);
    }

    private void DrawLength(Graphics g, LengthConstraint length)
    {
        Edge edge = length.Edge;
        if (!TryGetEdgeFrame(edge, out PointF mid, out float ux, out float uy, out float nx, out float ny))
            return;

        if (ny < 0f)
        {
            nx = -nx;
            ny = -ny;
        }

        const float offset = 14f;
        var anchor = new PointF(mid.X + nx * offset, mid.Y + ny * offset);

        float angleDeg = MathF.Atan2(uy, ux) * (180f / MathF.PI);
        if (angleDeg > 90f || angleDeg < -90f)
            angleDeg += 180f;

        string text = length.TargetLength.ToString("0");
        using var font = new Font("Segoe UI", 10f);
        using var brush = new SolidBrush(LengthLabelColor);
        SizeF size = g.MeasureString(text, font);

        GraphicsState state = g.Save();
        g.TranslateTransform(anchor.X, anchor.Y);
        g.RotateTransform(angleDeg);
        g.DrawString(text, font, brush, -size.Width * 0.5f, -size.Height * 0.5f);
        g.Restore(state);
    }

    private static bool TryGetEdgeFrame(
        Edge edge,
        out PointF mid,
        out float ux,
        out float uy,
        out float nx,
        out float ny)
    {
        mid = edge.Midpoint;
        float dx = edge.End.Position.X - edge.Start.Position.X;
        float dy = edge.End.Position.Y - edge.Start.Position.Y;
        float len = MathF.Sqrt(dx * dx + dy * dy);
        if (len < 1e-4f)
        {
            ux = uy = nx = ny = 0f;
            return false;
        }

        ux = dx / len;
        uy = dy / len;
        nx = -uy;
        ny = ux;
        return true;
    }
}

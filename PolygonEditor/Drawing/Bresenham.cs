namespace PolygonEditor.Drawing;

/// <summary>Rysowanie odcinka algorytmem Bresenhama.</summary>
public static class Bresenham
{
    public static void DrawLine(Bitmap bitmap, int x0, int y0, int x1, int y1, Color color)
    {
        int width = bitmap.Width;
        int height = bitmap.Height;

        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        int x = x0;
        int y = y0;

        while (true)
        {
            SetPixel(bitmap, x, y, color, width, height);

            if (x == x1 && y == y1)
                break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y += sy;
            }
        }
    }

    private static void SetPixel(Bitmap bitmap, int x, int y, Color color, int width, int height)
    {
        if ((uint)x < (uint)width && (uint)y < (uint)height)
            bitmap.SetPixel(x, y, color);
    }
}

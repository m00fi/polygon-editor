namespace PolygonEditor.UI;

/// <summary>
/// Panel with double buffering enabled for scene rendering.
/// </summary>
public sealed class CanvasPanel : Panel
{
    public CanvasPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.White;
        Dock = DockStyle.Fill;
        TabStop = true;
    }
}

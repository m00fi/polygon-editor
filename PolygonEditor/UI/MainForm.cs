namespace PolygonEditor.UI;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        UseBresenhamAlgorithm = true;
        UpdateStatus("Ready. Drag with LMB • context menu with RMB • F1 for documentation.");
    }

    /// <summary>
    /// true = custom Bresenham; false = library drawing (Graphics.DrawLine).
    /// </summary>
    public bool UseBresenhamAlgorithm { get; private set; }

    private void RadioBresenham_CheckedChanged(object? sender, EventArgs e)
    {
        if (radioBresenham.Checked)
        {
            UseBresenhamAlgorithm = true;
            canvasPanel.Invalidate();
            UpdateStatus("Line algorithm: Bresenham (custom).");
        }
    }

    private void RadioLibrary_CheckedChanged(object? sender, EventArgs e)
    {
        if (radioLibrary.Checked)
        {
            UseBresenhamAlgorithm = false;
            canvasPanel.Invalidate();
            UpdateStatus("Line algorithm: library (Graphics.DrawLine).");
        }
    }

    private void MenuResetScene_Click(object? sender, EventArgs e)
    {
        // Wire up DemoSceneFactory in phase 1.
        UpdateStatus("Scene reset - to be connected in a later phase.");
        canvasPanel.Invalidate();
    }

    private void MenuHelp_Click(object? sender, EventArgs e)
    {
        // HelpForm in phase 5.
        MessageBox.Show(
            this,
            "Documentation will be available from the application (HelpForm).",
            "Help - Polygon Editor",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void CanvasPanel_Paint(object? sender, PaintEventArgs e)
    {
        // Renderer in later phases.
        e.Graphics.Clear(Color.White);
        TextRenderer.DrawText(
            e.Graphics,
            "Canvas - demo scene will appear in phase 1.",
            Font,
            new Point(16, 16),
            Color.Gray);
    }

    private void UpdateStatus(string message)
    {
        statusLabel.Text = message;
    }
}

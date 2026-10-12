using PolygonEditor.Constraints;
using PolygonEditor.Drawing;
using PolygonEditor.Interaction;
using PolygonEditor.Models;
using PolygonEditor.Scene;

namespace PolygonEditor.UI;

public partial class MainForm : Form
{
    private readonly Renderer _renderer = new();

    private Models.Scene _scene = DemoSceneFactory.Create();
    private bool _isDragging;
    private HitKind _dragKind = HitKind.None;
    private Vertex? _dragVertex;
    private Models.Polygon? _dragPolygon;
    private PointF _lastMouse;

    private ConstraintSolver _solver = new ConstraintSolver();

    public MainForm()
    {
        InitializeComponent();
        UseBresenhamAlgorithm = true;
        WireCanvasEvents();
        UpdateStatus("Ready. LMB drag • RMB on edge adds vertex • RMB on vertex deletes • F1 help.");
    }

    /// <summary>true = Bresenham, false = Graphics.DrawLine.</summary>
    public bool UseBresenhamAlgorithm { get; private set; }

    private void WireCanvasEvents()
    {
        canvasPanel.MouseDown += CanvasPanel_MouseDown;
        canvasPanel.MouseMove += CanvasPanel_MouseMove;
        canvasPanel.MouseUp += CanvasPanel_MouseUp;
        canvasPanel.Resize += (_, _) => canvasPanel.Invalidate();
    }

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
        ResetScene();
        UpdateStatus("Scene reset to demo.");
    }

    private void MenuHelp_Click(object? sender, EventArgs e)
    {
        // HelpForm - faza 5.
        MessageBox.Show(
            this,
            "LMB: drag vertex or polygon.\n" +
            "RMB on edge: add vertex at midpoint.\n" +
            "RMB on vertex: delete vertex.\n" +
            "Ctrl+R: reset scene.",
            "Help - Polygon Editor",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void CanvasPanel_Paint(object? sender, PaintEventArgs e)
    {
        _renderer.Render(
            e.Graphics,
            canvasPanel.ClientSize.Width,
            canvasPanel.ClientSize.Height,
            _scene,
            UseBresenhamAlgorithm);
    }

    private void CanvasPanel_MouseDown(object? sender, MouseEventArgs e)
    {
        var point = new PointF(e.X, e.Y);
        HitResult hit = HitTester.HitTest(_scene, point);

        if (e.Button == MouseButtons.Right)
        {
            HandleRightClick(hit);
            return;
        }

        if (e.Button != MouseButtons.Left)
            return;

        _lastMouse = point;
        _isDragging = false;
        _dragKind = HitKind.None;
        _dragVertex = null;
        _dragPolygon = null;

        if (hit.Kind == HitKind.Vertex && hit.Vertex is not null)
        {
            _isDragging = true;
            _dragKind = HitKind.Vertex;
            _dragVertex = hit.Vertex;
            _dragPolygon = hit.Polygon;
            // Wierzchołek od razu podąża za kursorem.
            _dragVertex.Position = point;
            canvasPanel.Invalidate();
            UpdateStatus("Dragging vertex.");
        }
        else if (hit.Kind == HitKind.Polygon && hit.Polygon is not null)
        {
            _isDragging = true;
            _dragKind = HitKind.Polygon;
            _dragPolygon = hit.Polygon;
            UpdateStatus("Dragging polygon.");
        }
        else if (hit.Kind == HitKind.Edge)
        {
            UpdateStatus("Edge hit - RMB adds a vertex at midpoint.");
        }
    }

    private void HandleRightClick(HitResult hit)
    {
        if (hit.Kind == HitKind.Edge && hit.Polygon is not null && hit.Edge is not null)
        {
            hit.Polygon.InsertVertexAtMidpoint(hit.Edge);
            canvasPanel.Invalidate();
            UpdateStatus("Vertex added at edge midpoint.");
            return;
        }

        if (hit.Kind == HitKind.Vertex && hit.Polygon is not null && hit.Vertex is not null)
        {
            if (hit.Polygon.RemoveVertex(hit.Vertex))
            {
                canvasPanel.Invalidate();
                UpdateStatus("Vertex deleted.");
            }
            else
            {
                UpdateStatus("Cannot delete vertex - polygon must keep at least 3 vertices.");
            }

            return;
        }

        UpdateStatus("RMB: click an edge to add a vertex, or a vertex to delete it.");
    }

    private void CanvasPanel_MouseMove(object? sender, MouseEventArgs e)
    {
        var point = new PointF(e.X, e.Y);

        if (!_isDragging)
        {
            HitResult hover = HitTester.HitTest(_scene, point);
            canvasPanel.Cursor = hover.Kind switch
            {
                HitKind.Vertex => Cursors.Hand,
                HitKind.Polygon => Cursors.SizeAll,
                HitKind.Edge => Cursors.Cross,
                _ => Cursors.Default
            };
            return;
        }

        if (_dragKind == HitKind.Vertex && _dragVertex is not null)
        {
            _dragVertex.Position = point;
            _solver.Solve(_scene.Constraints, new ConstraintAnchor { FixedVertex = _dragVertex });
            canvasPanel.Invalidate();
        }
        else if (_dragKind == HitKind.Polygon && _dragPolygon is not null)
        {
            float dx = point.X - _lastMouse.X;
            float dy = point.Y - _lastMouse.Y;
            _dragPolygon.Translate(dx, dy);
            _lastMouse = point;
            canvasPanel.Invalidate();
        }
    }

    private void CanvasPanel_MouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left || !_isDragging)
            return;

        _isDragging = false;
        _dragKind = HitKind.None;
        _dragVertex = null;
        _dragPolygon = null;
        UpdateStatus("Ready.");
    }

    private void ResetScene()
    {
        _isDragging = false;
        _dragKind = HitKind.None;
        _dragVertex = null;
        _dragPolygon = null;
        _scene = DemoSceneFactory.Create();
        canvasPanel.Invalidate();
    }

    private void UpdateStatus(string message)
    {
        statusLabel.Text = message;
    }
}

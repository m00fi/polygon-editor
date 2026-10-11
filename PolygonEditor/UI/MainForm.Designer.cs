namespace PolygonEditor.UI;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        menuStrip = new MenuStrip();
        menuScene = new ToolStripMenuItem();
        menuResetScene = new ToolStripMenuItem();
        menuHelpRoot = new ToolStripMenuItem();
        menuHelp = new ToolStripMenuItem();
        toolPanel = new Panel();
        groupDrawing = new GroupBox();
        radioBresenham = new RadioButton();
        radioLibrary = new RadioButton();
        canvasPanel = new CanvasPanel();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        menuStrip.SuspendLayout();
        toolPanel.SuspendLayout();
        groupDrawing.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // menuStrip
        // 
        menuStrip.Items.AddRange(new ToolStripItem[] { menuScene, menuHelpRoot });
        menuStrip.Location = new Point(0, 0);
        menuStrip.Name = "menuStrip";
        menuStrip.Size = new Size(1100, 24);
        menuStrip.TabIndex = 0;
        menuStrip.Text = "menuStrip";
        // 
        // menuScene
        // 
        menuScene.DropDownItems.AddRange(new ToolStripItem[] { menuResetScene });
        menuScene.Name = "menuScene";
        menuScene.Size = new Size(50, 20);
        menuScene.Text = "Scene";
        // 
        // menuResetScene
        // 
        menuResetScene.Name = "menuResetScene";
        menuResetScene.ShortcutKeys = Keys.Control | Keys.R;
        menuResetScene.Size = new Size(176, 22);
        menuResetScene.Text = "Reset scene";
        menuResetScene.Click += MenuResetScene_Click;
        // 
        // menuHelpRoot
        // 
        menuHelpRoot.DropDownItems.AddRange(new ToolStripItem[] { menuHelp });
        menuHelpRoot.Name = "menuHelpRoot";
        menuHelpRoot.Size = new Size(44, 20);
        menuHelpRoot.Text = "Help";
        // 
        // menuHelp
        // 
        menuHelp.Name = "menuHelp";
        menuHelp.ShortcutKeys = Keys.F1;
        menuHelp.Size = new Size(170, 22);
        menuHelp.Text = "Documentation";
        menuHelp.Click += MenuHelp_Click;
        // 
        // toolPanel
        // 
        toolPanel.Controls.Add(groupDrawing);
        toolPanel.Dock = DockStyle.Top;
        toolPanel.Location = new Point(0, 24);
        toolPanel.Name = "toolPanel";
        toolPanel.Padding = new Padding(8, 4, 8, 4);
        toolPanel.Size = new Size(1100, 56);
        toolPanel.TabIndex = 1;
        // 
        // groupDrawing
        // 
        groupDrawing.Controls.Add(radioBresenham);
        groupDrawing.Controls.Add(radioLibrary);
        groupDrawing.Location = new Point(8, 4);
        groupDrawing.Name = "groupDrawing";
        groupDrawing.Size = new Size(320, 48);
        groupDrawing.TabIndex = 0;
        groupDrawing.TabStop = false;
        groupDrawing.Text = "Line drawing";
        // 
        // radioBresenham
        // 
        radioBresenham.AutoSize = true;
        radioBresenham.Checked = true;
        radioBresenham.Location = new Point(12, 20);
        radioBresenham.Name = "radioBresenham";
        radioBresenham.Size = new Size(131, 19);
        radioBresenham.TabIndex = 0;
        radioBresenham.TabStop = true;
        radioBresenham.Text = "Bresenham (custom)";
        radioBresenham.UseVisualStyleBackColor = true;
        radioBresenham.CheckedChanged += RadioBresenham_CheckedChanged;
        // 
        // radioLibrary
        // 
        radioLibrary.AutoSize = true;
        radioLibrary.Location = new Point(150, 20);
        radioLibrary.Name = "radioLibrary";
        radioLibrary.Size = new Size(146, 19);
        radioLibrary.TabIndex = 1;
        radioLibrary.Text = "Library (Graphics)";
        radioLibrary.UseVisualStyleBackColor = true;
        radioLibrary.CheckedChanged += RadioLibrary_CheckedChanged;
        // 
        // canvasPanel
        // 
        canvasPanel.BackColor = Color.White;
        canvasPanel.Dock = DockStyle.Fill;
        canvasPanel.Location = new Point(0, 80);
        canvasPanel.Name = "canvasPanel";
        canvasPanel.Size = new Size(1100, 540);
        canvasPanel.TabIndex = 2;
        canvasPanel.TabStop = true;
        canvasPanel.Paint += CanvasPanel_Paint;
        // 
        // statusStrip
        // 
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
        statusStrip.Location = new Point(0, 620);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1100, 22);
        statusStrip.TabIndex = 3;
        statusStrip.Text = "statusStrip";
        // 
        // statusLabel
        // 
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(1085, 17);
        statusLabel.Spring = true;
        statusLabel.Text = "Ready";
        statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1100, 642);
        Controls.Add(canvasPanel);
        Controls.Add(statusStrip);
        Controls.Add(toolPanel);
        Controls.Add(menuStrip);
        MainMenuStrip = menuStrip;
        MinimumSize = new Size(800, 500);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Polygon Editor";
        menuStrip.ResumeLayout(false);
        menuStrip.PerformLayout();
        toolPanel.ResumeLayout(false);
        groupDrawing.ResumeLayout(false);
        groupDrawing.PerformLayout();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private MenuStrip menuStrip;
    private ToolStripMenuItem menuScene;
    private ToolStripMenuItem menuResetScene;
    private ToolStripMenuItem menuHelpRoot;
    private ToolStripMenuItem menuHelp;
    private Panel toolPanel;
    private GroupBox groupDrawing;
    private RadioButton radioBresenham;
    private RadioButton radioLibrary;
    private CanvasPanel canvasPanel;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
}

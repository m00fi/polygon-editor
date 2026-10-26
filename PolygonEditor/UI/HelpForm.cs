namespace PolygonEditor.UI;

// Help window (F1): tabs with the text from HelpContent.
public sealed class HelpForm : Form
{
    private static readonly Font BodyFont = new("Segoe UI", 10f);
    private static readonly Font HeadingFont = new("Segoe UI", 12f, FontStyle.Bold);
    private static readonly Font CodeFont = new("Consolas", 9.5f);

    public HelpForm()
    {
        Text = "Pomoc - Polygon Editor";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(900, 640);
        MinimumSize = new Size(600, 400);
        ShowInTaskbar = false;
        KeyPreview = true;

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(CreatePage("Obsługa", HelpContent.Usage));
        tabs.TabPages.Add(CreatePage("Założenia", HelpContent.Assumptions));
        tabs.TabPages.Add(CreatePage("Model danych", HelpContent.DataModel));
        tabs.TabPages.Add(CreatePage("Ograniczenia", HelpContent.Constraints));
        tabs.TabPages.Add(CreatePage("Rysowanie i trafienia", HelpContent.Drawing));
        Controls.Add(tabs);

        KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Escape)
                Close();
        };
    }

    private static TabPage CreatePage(string title, string content)
    {
        var box = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            BackColor = SystemColors.Window,
            DetectUrls = false,
            WordWrap = true
        };
        AppendFormatted(box, content);
        box.SelectionStart = 0;

        var page = new TabPage(title) { Padding = new Padding(10) };
        page.Controls.Add(box);
        return page;
    }

    // "## " heading, "- " list item (leading spaces = level), ``` code block
    private static void AppendFormatted(RichTextBox box, string content)
    {
        bool inCode = false;
        foreach (string rawLine in content.Split('\n'))
        {
            string line = rawLine.TrimEnd('\r');
            string trimmed = line.TrimStart();

            if (trimmed.StartsWith("```"))
            {
                inCode = !inCode;
                continue;
            }

            if (inCode)
            {
                Append(box, line + "\n", CodeFont, 20);
            }
            else if (trimmed.StartsWith("## "))
            {
                Append(box, trimmed[3..] + "\n", HeadingFont, 0);
            }
            else if (trimmed.StartsWith("- "))
            {
                int level = (line.Length - trimmed.Length) / 2;
                Append(box, "•  " + trimmed[2..] + "\n", BodyFont, 12 + level * 20, hanging: 14);
            }
            else
            {
                Append(box, trimmed + "\n", BodyFont, trimmed.Length > 0 && char.IsDigit(trimmed[0]) ? 12 : 0);
            }
        }
    }

    private static void Append(RichTextBox box, string text, Font font, int indent, int hanging = 0)
    {
        box.SelectionStart = box.TextLength;
        box.SelectionLength = 0;
        box.SelectionFont = font;
        box.SelectionIndent = indent;
        box.SelectionHangingIndent = hanging;
        box.AppendText(text);
    }
}

using AsmResolver;
using AsmResolver.PE;
using AsmResolver.PE.File.Headers;
using AsmResolver.PE.Win32Resources;
using AsmResolver.PE.Win32Resources.Builder;

namespace FiveMASITagger;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}

public class MainForm : Form
{
    // ── Build table ────────────────────────────────────────────────────────────
    record BuildEntry(string Build, string DLC, string Title, bool IsBeta = false);

    static readonly BuildEntry[] Builds =
    [
        new("1604", "mpchristmas2018", "Arena War"),
        new("2060", "mpsum",          "Los Santos Summer Special"),
        new("2189", "mpheist4",       "Cayo Perico Heist"),
        new("2372", "mptuner",        "Los Santos Tuners"),
        new("2545", "mpsecurity",     "The Contract"),
        new("2612", "mpg9ec",         "Expanded & Enhanced"),
        new("2699", "mpsum2",         "The Criminal Enterprise"),
        new("2802", "mpchristmas3",   "Los Santos Drug War"),
        new("2944", "mp2023_01",      "San Andreas Mercenaries"),
        new("3095", "mp2023_02",      "The Chop Shop"),
        new("3258", "mp2024_01",      "Bottom Dollar Bounties"),
        new("3407", "mp2024_02",      "Agents of Sabotage"),
        new("3570", "mp2025_01",      "Money Fronts", IsBeta: true),
    ];

    // ── UI controls ────────────────────────────────────────────────────────────
    private readonly TextBox   _txtFile;
    private readonly ComboBox  _cmbBuild;
    private readonly Button    _btnBrowse;
    private readonly Button    _btnEmbed;
    private readonly Button    _btnEmbedAll;
    private readonly Label     _lblStatus;
    private readonly Panel     _statusBar;

    // ── Colours / style ────────────────────────────────────────────────────────
    static readonly Color BgDark    = Color.FromArgb(28,  28,  36);
    static readonly Color BgCard    = Color.FromArgb(38,  38,  50);
    static readonly Color Accent    = Color.FromArgb(99,  102, 241);
    static readonly Color AccentHov = Color.FromArgb(129, 132, 255);
    static readonly Color TextMain  = Color.FromArgb(220, 220, 230);
    static readonly Color TextSub   = Color.FromArgb(140, 140, 160);
    static readonly Color BorderCol = Color.FromArgb(55,  55,  72);

    public MainForm()
    {
        SuspendLayout();

        // ── Form ──────────────────────────────────────────────────────────────
        Text            = "FiveM ASI Build Updater";
        ClientSize      = new Size(520, 280);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        StartPosition   = FormStartPosition.CenterScreen;
        BackColor       = BgDark;
        ForeColor       = TextMain;
        Font            = new Font("Segoe UI", 9.5f);

        // ── Title label ───────────────────────────────────────────────────────
        var lblTitle = MakeLabel("FiveM ASI Build Updater", 12, bold: true);
        lblTitle.Font      = new Font("Segoe UI Semibold", 13f);
        lblTitle.ForeColor = TextMain;
        lblTitle.Location  = new Point(20, 18);
        lblTitle.Size      = new Size(480, 26);

        var lblSub = MakeLabel("Embeds a FX_ASI_BUILD custom resource into a .asi (PE) file", 12);
        lblSub.ForeColor = TextSub;
        lblSub.Font      = new Font("Segoe UI", 8.5f);
        lblSub.Location  = new Point(20, 44);
        lblSub.Size      = new Size(480, 18);

        // ── Separator ─────────────────────────────────────────────────────────
        var sep = new Panel { BackColor = BorderCol, Location = new Point(0, 68), Size = new Size(520, 1) };

        // ── File row ──────────────────────────────────────────────────────────
        var lblFile = MakeLabel("ASI File", 12);
        lblFile.Location = new Point(20, 86);
        lblFile.Size     = new Size(70, 22);

        _txtFile = new TextBox
        {
            Location        = new Point(95, 84),
            Size            = new Size(300, 26),
            BackColor       = BgCard,
            ForeColor       = TextMain,
            BorderStyle     = BorderStyle.FixedSingle,
            PlaceholderText = "Select a .asi file…",
            ReadOnly        = true,
            Cursor          = Cursors.Default,
        };

        _btnBrowse = MakeButton("Browse…", new Point(403, 83), new Size(97, 28));

        // ── Build row ─────────────────────────────────────────────────────────
        var lblBuild = MakeLabel("Build", 12);
        lblBuild.Location = new Point(20, 130);
        lblBuild.Size     = new Size(70, 22);

        _cmbBuild = new ComboBox
        {
            Location      = new Point(95, 128),
            Size          = new Size(405, 26),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor     = BgCard,
            ForeColor     = TextMain,
            FlatStyle     = FlatStyle.Flat,
        };
        foreach (var b in Builds)
        {
            string betaTag = b.IsBeta ? "  [Beta/Unstable]" : "";
            _cmbBuild.Items.Add($"{b.Build}  —  {b.Title}{betaTag}");
        }
        _cmbBuild.SelectedIndex = Builds.Length - 2; // default: latest stable (3407)

        // ── Embed button ──────────────────────────────────────────────────────
        _btnEmbed        = MakeButton("Embed Resource", new Point(95, 174), new Size(170, 34));
        _btnEmbed.Font   = new Font("Segoe UI Semibold", 9.5f);
        _btnEmbed.Click += OnEmbed;

        // ── Embed All button ──────────────────────────────────────────────────
        _btnEmbedAll        = MakeButton("Embed All Builds", new Point(275, 174), new Size(170, 34));
        _btnEmbedAll.Font   = new Font("Segoe UI Semibold", 9.5f);
        _btnEmbedAll.Click += OnEmbedAll;

        // ── Status bar ────────────────────────────────────────────────────────
        _statusBar = new Panel
        {
            Dock      = DockStyle.Bottom,
            Height    = 36,
            BackColor = BgCard,
        };
        _lblStatus = new Label
        {
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = TextSub,
            Padding   = new Padding(12, 0, 0, 0),
            Font      = new Font("Segoe UI", 8.5f),
        };
        _statusBar.Controls.Add(_lblStatus);
        SetStatus("Ready.", TextSub);

        // ── Add all controls ─────────────────────────────────────────────────
        Controls.AddRange([
            lblTitle, lblSub, sep,
            lblFile, _txtFile, _btnBrowse,
            lblBuild, _cmbBuild,
            _btnEmbed, _btnEmbedAll,
            _statusBar,
        ]);

        _btnBrowse.Click += OnBrowse;

        ResumeLayout();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    static Label MakeLabel(string text, int padLeft, bool bold = false)
    {
        return new Label
        {
            Text      = text,
            ForeColor = TextMain,
            BackColor = Color.Transparent,
            Font      = bold ? new Font("Segoe UI Semibold", 9.5f) : new Font("Segoe UI", 9.5f),
            AutoSize  = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding   = new Padding(padLeft, 0, 0, 0),
        };
    }

    Button MakeButton(string text, Point loc, Size size)
    {
        var btn = new Button
        {
            Text      = text,
            Location  = loc,
            Size      = size,
            BackColor = Accent,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand,
            Font      = Font,
        };
        btn.FlatAppearance.BorderSize         = 0;
        btn.FlatAppearance.MouseOverBackColor = AccentHov;
        btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(80, 83, 210);
        return btn;
    }

    void SetStatus(string msg, Color color)
    {
        _lblStatus.Text      = msg;
        _lblStatus.ForeColor = color;
    }

    bool ValidateFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            SetStatus("Error: select a valid .asi file first.", Color.FromArgb(248, 113, 113));
            return false;
        }
        return true;
    }

    // ── Core embedding (AsmResolver) ──────────────────────────────────────────
    // Adds FX_ASI_BUILD resources for each given build entry without disturbing
    // any other resources already present in the PE file.
    static void EmbedBuilds(string filePath, IEnumerable<BuildEntry> entries)
    {
        var peFile = AsmResolver.PE.File.PEFile.FromFile(filePath);
        // EmptyErrorListener: silently skip any resource entries with invalid RVA/size
        // (e.g. 0-byte entries we previously wrote) instead of throwing.
        var image  = PEImage.FromFile(peFile, new PEReaderParameters(EmptyErrorListener.Instance));

        var root = image.Resources
            ?? throw new InvalidOperationException("This ASI file has no .rsrc section.");

        foreach (var entry in entries)
        {
            uint id = uint.Parse(entry.Build);

            // Remove existing entry for this build number so we can replace it cleanly
            for (int i = root.Entries.Count - 1; i >= 0; i--)
            {
                if (root.Entries[i] is ResourceDirectory d && d.Id == id)
                {
                    root.Entries.RemoveAt(i);
                    break;
                }
            }

            // Type (numeric build id) → Name ("FX_ASI_BUILD") → Language (1033): empty data
            var typeDir = new ResourceDirectory(id);
            var nameDir = new ResourceDirectory("FX_ASI_BUILD");
            nameDir.Entries.Add(new ResourceData(1033u, new DataSegment(Array.Empty<byte>()))); // language 1033, empty
            typeDir.Entries.Add(nameDir);
            root.Entries.Add(typeDir);
        }

        // Rebuild the .rsrc section from the modified in-memory tree
        var buffer = new ResourceDirectoryBuffer();
        buffer.AddDirectory(root);

        var rsrcRva = peFile.OptionalHeader.DataDirectories[(int)DataDirectoryIndex.ResourceDirectory].VirtualAddress;
        var rsrc    = peFile.GetSectionContainingRva(rsrcRva);
        rsrc.Contents = buffer;

        // Write to a temp file then overwrite the original atomically
        // (Write() handles UpdateOffsets/RVA calculation internally — don't call AlignSections/UpdateHeaders before it)
        string temp = Path.GetTempFileName();
        try
        {
            peFile.Write(temp);
            File.Copy(temp, filePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temp)) File.Delete(temp);
        }
    }

    // ── Event handlers ────────────────────────────────────────────────────────
    void OnBrowse(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog
        {
            Title  = "Select ASI File",
            Filter = "ASI Files (*.asi)|*.asi|All Files (*.*)|*.*",
        };
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            _txtFile.Text = ofd.FileName;
            SetStatus($"Loaded: {Path.GetFileName(ofd.FileName)}", TextSub);
        }
    }

    void OnEmbed(object? sender, EventArgs e)
    {
        string filePath = _txtFile.Text.Trim();
        if (!ValidateFile(filePath)) return;

        var entry = Builds[_cmbBuild.SelectedIndex];
        _btnEmbed.Enabled = false;
        SetStatus("Working…", TextSub);
        Application.DoEvents();

        try
        {
            EmbedBuilds(filePath, [entry]);
            SetStatus(
                $"✓  Embedded FX_ASI_BUILD  (build: {entry.Build} — {entry.Title})  →  {Path.GetFileName(filePath)}",
                Color.FromArgb(74, 222, 128));
        }
        catch (Exception ex)
        {
            SetStatus($"Error: {ex.Message}", Color.FromArgb(248, 113, 113));
        }
        finally
        {
            _btnEmbed.Enabled = true;
        }
    }

    void OnEmbedAll(object? sender, EventArgs e)
    {
        string filePath = _txtFile.Text.Trim();
        if (!ValidateFile(filePath)) return;

        _btnEmbed.Enabled    = false;
        _btnEmbedAll.Enabled = false;
        SetStatus("Working…", TextSub);
        Application.DoEvents();

        try
        {
            EmbedBuilds(filePath, Builds);
            SetStatus(
                $"✓  Embedded all {Builds.Length} builds  →  {Path.GetFileName(filePath)}",
                Color.FromArgb(74, 222, 128));
        }
        catch (Exception ex)
        {
            SetStatus($"Error: {ex.Message}", Color.FromArgb(248, 113, 113));
        }
        finally
        {
            _btnEmbed.Enabled    = true;
            _btnEmbedAll.Enabled = true;
        }
    }
}

using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using Dubeg.Sw.ExportTools.Properties;
using Serilog;
using Serilog.Core;
using Serilog.Debugging;

namespace Dubeg.Sw.ExportTools;

public partial class FrmLogs : Form {
    private static readonly object _syncRoot = new object();
    private readonly System.Windows.Controls.RichTextBox _wpfRichTextBox;
    public string LogsFolderPath { get; set; }

    public FrmLogs() {
        InitializeComponent();
        this.Icon = System.Drawing.Icon.FromHandle(Resources.logs.GetHicon());

        var frmClear = new FrmLogsButton() { 
            MainForm = this
        };
        void SetFrmClearLocation() => frmClear.Location = new System.Drawing.Point(
            this.Location.X + 122 + 20,
            this.Location.Y + (40 / 2) - (frmClear.Size.Height / 2)
        );
        this.Shown += (s, e) => {
            //frmClear.Show();
            SetFrmClearLocation();
            frmClear.Owner = this;
        };
        this.VisibleChanged += (s, e) => {
            SetFrmClearLocation();
            if (!this.Visible) frmClear.Hide();
            else frmClear.Show();
        };
        this.Move += (s, e) => SetFrmClearLocation();

        var richTextBoxHost = new ElementHost { Dock = DockStyle.Fill };
        _richTextBoxPanel.Controls.Add(richTextBoxHost);
        var wpfRichTextBox = new System.Windows.Controls.RichTextBox {
            Background = Brushes.Black,
            Foreground = Brushes.LightGray,
            FontFamily = new FontFamily("Cascadia Mono, Consolas, Courier New, monospace"),
            FontSize = 14,
            IsReadOnly = true,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            Margin = new Thickness(0),
        };
        richTextBoxHost.Child = wpfRichTextBox;
        _wpfRichTextBox = wpfRichTextBox;
        //_clearToolStripButton.Click += Clear_OnClick;
        SelfLog.Enable(message => Trace.WriteLine($"INTERNAL ERROR: {message}"));
        const string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}";
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Logger(Log.Logger)
            .WriteTo.RichTextBox(wpfRichTextBox, outputTemplate: outputTemplate, syncRoot: _syncRoot)
            .CreateLogger();

        // Handle form closing
        FormClosing += FrmLogs_FormClosing;
    }

    private void FrmLogs_FormClosing(object sender, FormClosingEventArgs e) {
        if (e.CloseReason == CloseReason.UserClosing) {
            e.Cancel = true;
            Hide();
        }
    }

    private void MainForm_Load(object sender, EventArgs e) {
        Log.Information("Starting log viewer: {Name} from thread {ThreadId}", Environment.UserName, Thread.CurrentThread.ManagedThreadId);
    }

    public void ClearTextBox() {
        lock (_syncRoot) {
            _wpfRichTextBox.Document.Blocks.Clear();
        }
    }
}
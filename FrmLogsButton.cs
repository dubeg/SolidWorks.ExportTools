using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace Dubeg.Sw.ExportTools;
public partial class FrmLogsButton : Form {
    public FrmLogs MainForm { get; set; }

    public FrmLogsButton() {
        InitializeComponent();
        this.MinimumSize = new Size(1, 1);
        this.Size = new Size(
            btnOpenLogs.Location.X + btnOpenLogs.Size.Width,
            btnOpenLogs.Height
        );
        btnClear.Click += (s, e) => {
            MainForm?.ClearTextBox();
        };
        btnOpenLogs.Click += (s, e) => {
            var path = MainForm.LogsFolderPath;
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path)) {
                Process.Start("explorer.exe", path);
            } else {
                MessageBox.Show("Logs folder path is invalid or does not exist.");
            }
        };
    }
}

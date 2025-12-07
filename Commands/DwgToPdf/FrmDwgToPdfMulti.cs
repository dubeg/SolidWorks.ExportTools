using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using BrightIdeasSoftware;
using System.Drawing;
using System.Threading.Tasks;
using Dubeg.Sw.ExportTools.Models;
using Dubeg.Sw.ExportTools.Properties;
using Dubeg.Sw.ExportTools.Utils;

namespace Dubeg.Sw.ExportTools.Commands.DwgToPdf; 

public partial class FrmDwgToPdfMulti : Form {
    private readonly List<ExportFileItem> _items;
    private readonly Dictionary<ExportStatus, Image> _statusImages;
    private DwgToPdfCommand _controller { get; set; }
    private DwgToPdfAppSettings _appSettings => _controller.AppSettings;
    private bool _cancelRequested = false;
    private bool _exportInProgress = false;

    public FrmDwgToPdfMulti(DwgToPdfCommand controller) {
        InitializeComponent();
        Icon = Icon.FromHandle(Resources.DWG.GetHicon());
        _controller = controller;

        _openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
        _openFileDialog.FilterIndex = 1;
        _openFileDialog.RestoreDirectory = true;

        _statusImages = ResourceUtils.LoadStatusImages();
        _items = new List<ExportFileItem>();
        _gridItems.SetObjects(_items);
        _gridItems.ShowGroups = false;
        _gridItems.Columns.AddRange([
            new OLVColumn {
                Text = nameof(ExportFileItem.Status),
                AspectName = nameof(ExportFileItem.Status),
                ImageGetter = (object rowObject) => {
                    if (rowObject is ExportFileItem item && _statusImages.TryGetValue(item.Status, out var image)) {
                        return image;
                    }
                    return null;
                },
                AspectGetter = x => "",
                Width = 70,
                TextAlign = HorizontalAlignment.Center
            },
            new OLVColumn {
                Text = nameof(ExportFileItem.Name),
                AspectName = nameof(ExportFileItem.Name),
                Width = 200
            },
            new OLVColumn {
                Text = nameof(ExportFileItem.FilePath),
                AspectName = nameof(ExportFileItem.FilePath),
                FillsFreeSpace = true
            }
        ]);

        btnExportToPDF.Enabled = false;
        tbOutputPath.Text = _appSettings.OutputFolderPath;
        btnOpenOutputPath.Click += (object sender, EventArgs e) => {
            System.Diagnostics.Process.Start("explorer.exe", _appSettings.OutputFolderPath);
        };

        FormClosing += (object sender, FormClosingEventArgs e) => {
            _cancelRequested = true;
            if (e.CloseReason == CloseReason.UserClosing) {
                e.Cancel = true;
                Hide();
            }
        };

        Disposed += (e, s) => {
            foreach (var image in _statusImages.Values) {
                image.Dispose();
            }
            _statusImages.Clear();
        };
    }

    private void btnPickFile_Click(object sender, EventArgs e) {
        var result = _openFileDialog.ShowDialog();
        if (result != DialogResult.OK) {
            return;
        }

        try {
            var fileLines = File.ReadAllLines(_openFileDialog.FileName);
            _items.Clear();
            foreach (var line in fileLines) {
                if (!string.IsNullOrWhiteSpace(line)) {
                    var item = new ExportFileItem(line.Trim());
                    _items.Add(item);
                }
            }
            _gridItems.SetObjects(_items);
            _gridItems.Refresh();
            btnExportToPDF.Enabled = _items.Any();
        }
        catch (Exception ex) {
            MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ToggleLockForWork(bool locked) {
        var enabled = !locked;
        Cursor = Cursors.WaitCursor;
        btnCancel.Enabled = locked;
        btnExportToPDF.Enabled = enabled;
        _btnPickFile.Enabled = enabled;
        _wipSpinner.Visible = locked;
        Cursor = Cursors.Default;
        btnCancel.Enabled = true;
    }

    private async void btnExportToPDF_Click(object sender, EventArgs e) {
        ToggleLockForWork(true);
        _exportInProgress = true;
        _cancelRequested = false;
        foreach (var item in _items) {
            item.Status = ExportStatus.Unprocessed;
        }
        _gridItems.BuildList();
        await Task.Delay(250);
        try {
            foreach (var item in _items) {
                if (_cancelRequested) {
                    break;
                }
                var result = await _controller.ShowExportForm(item.FilePath, displayNextToMultiForm: true);
                var success = result.Success;
                _controller.CloseCurrentDocument();
                item.Status = success ? ExportStatus.Processed : ExportStatus.Error;
                _gridItems.RefreshObject(item);
            }
        }
        catch (Exception ex) {
            MessageBox.Show($"Error during export: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally {
            ToggleLockForWork(false);
            _cancelRequested = false;
            _exportInProgress = false;
        }
    }

    private void btnCancel_Click(object sender, EventArgs e) {
        if (_exportInProgress) {
            _cancelRequested = true;
            btnCancel.Enabled = false;
        }
    }

    private void btnLogs_Click(object sender, EventArgs e) {
        _controller.Addin.ShowLogViewer();
    }
}

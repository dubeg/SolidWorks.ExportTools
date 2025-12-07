using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using BrightIdeasSoftware;
using System.Drawing;
using Dubeg.Sw.ExportTools.Models;
using Dubeg.Sw.ExportTools.Commands.Base;
using Dubeg.Sw.ExportTools.Properties;
using Dubeg.Sw.ExportTools.Utils;
using System.Threading.Tasks;

namespace Dubeg.Sw.ExportTools.Commands.ModelToPdf; 

public partial class FrmModelToPdfExporterMulti : Form {
    private readonly List<ExportFileItem> _items;
    private readonly Dictionary<ExportStatus, Image> _statusImages;
    private ModelToPdfCommand _controller;
    private bool _exportInProgress;
    private bool _cancelRequested;

    private AppSettings _appSettings => _controller.AppSettings;

    public FrmModelToPdfExporterMulti(ModelToPdfCommand controller) {
        InitializeComponent();
        Icon = System.Drawing.Icon.FromHandle(Resources.model.GetHicon());
        _controller = controller;

        _openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
        _openFileDialog.FilterIndex = 1;
        _openFileDialog.RestoreDirectory = true;

        _statusImages = ResourceUtils.LoadStatusImages();
        _items = new List<ExportFileItem>();
        _gridItems.ShowGroups = false;
        _gridItems.SetObjects(_items);
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
                Width = 60
            },
            new OLVColumn {
                Text = nameof(ExportFileItem.FilePath),
                AspectName = nameof(ExportFileItem.FilePath),
                FillsFreeSpace = true
            }
        ]);
        _gridItems.AutoResizeColumns();
        
        btnExportToPDF.Enabled = false;
        tbOutputPath.Text = _appSettings.OutputFolderPath;
        btnOpenOutputPath.Click += (object sender, EventArgs e) => {
            System.Diagnostics.Process.Start("explorer.exe", _appSettings.OutputFolderPath);
        };

        FormClosing += (object sender, FormClosingEventArgs e) => {
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
                var success = false;
                try {
                    await _controller.RunForFileAsync(item.FilePath);
                    success = true;
                }
                catch {
                    // TODO: catch exception & assign its message to the item (to display in grid).
                }
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
            _exportInProgress = false;
            _cancelRequested = false;
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

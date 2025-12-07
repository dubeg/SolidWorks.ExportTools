using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Interop;
using Microsoft.Extensions.Configuration;
using Dubeg.Sw.ExportTools.Properties;
using Dubeg.Sw.ExportTools.Utils;
using Serilog;
using Xarial.XCad.SolidWorks;
using static Dubeg.Sw.ExportTools.Commands.DwgToPdf.DwgToPdfCommand;

namespace Dubeg.Sw.ExportTools.Commands.DwgToPdf; 

public partial class FrmDwgToPdf : Form {
    private DwgToPdfCommand _controller;
    private DwgImportInfo _dwgImportInfo;

    private DwgToPdfAppSettings _appSettings => _controller.AppSettings;

    public class LayerGridItem {
        [Description("Layers")]
        public string Name { get; set; }
    };
    public bool Success { get; set; } = false;
    public string OutputFilePath { get; set; } = string.Empty;

    public FrmDwgToPdf(DwgToPdfCommand controller, string dwgFilePath = null) {
        InitializeComponent();
        Icon = System.Drawing.Icon.FromHandle(Resources.DWG.GetHicon());
        _controller = controller;
        tbOutputPath.Text = _appSettings.OutputFolderPath;
        btnOpenOutputPath.Click += (sender, e) => {
            System.Diagnostics.Process.Start("explorer.exe", _appSettings.OutputFolderPath);
        };

        ddlSelectMode.SelectedIndex = ddlSelectMode.Items.Count - 1;
        ddlSelectMode.SelectedIndexChanged += (object sender, EventArgs e) => {
            cbRemoveSegmentsFromOverlappingViews.Enabled = ddlSelectMode.SelectedIndex == 1;
        };

        _gridSheets.Enabled = false;
        _btnImportExport.Enabled = false;

        _openFileDialog.Filter = "DWG files (*.dwg)|*.dwg";
        _openFileDialog.FilterIndex = 1;
        _openFileDialog.RestoreDirectory = true;

        _gridSheets.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _gridSheets.MultiSelect = false;
        _gridSheets.AllowUserToAddRows = false;

        var ignoredLayers = _appSettings.IgnoredLayers.Any() ? _appSettings.IgnoredLayers : [""];
        _gridIgnoredLayers.BindDataWithTags(
            ignoredLayers.Select(x => new LayerGridItem() { Name = x })
        );

        this.Load += (s, e) => {
            if (!string.IsNullOrWhiteSpace(dwgFilePath)) {
                _btnPickFile.Enabled = false;
                LoadFileInfo(dwgFilePath, closeOnException: true);
            }
        };
    }

    private void ToggleLockForWork(bool locked) {
        var enabled = !locked;
        Cursor = Cursors.WaitCursor;
        _btnImportExport.Enabled = enabled;
        btnCopy.Enabled = enabled;
        btnCenterAndScale.Enabled = enabled;
        btnSaveAsPDF.Enabled = enabled;
        _wipSpinner.Visible = locked;
        Cursor = Cursors.Default;
    }

    private void btnPickFile_Click(object sender, EventArgs e) {
        _btnImportExport.Enabled = true;
        _gridSheets.Rows.Clear();
        _gridSheets.Columns.Clear();
        var result = _openFileDialog.ShowDialog();
        if (result != DialogResult.OK) {
            return;
        }
        LoadFileInfo(_openFileDialog.FileName);
    }

    private void LoadFileInfo(string filePath, bool closeOnException = false) {
        _tbFilePath.Text = filePath;
        _tbFilePath.ReadOnly = true;
        try {
            _dwgImportInfo = _controller.LoadFileInfo(filePath);
            var sheetInfos = _dwgImportInfo.SheetInfos;
            _gridSheets.Enabled = true;
            _gridSheets.BindDataWithTags(sheetInfos);
            var defaultSheet = sheetInfos.FirstOrDefault(x => x.HasCart && x.HasIsoView)
                ?? sheetInfos.FirstOrDefault(x => x.HasCart || x.HasIsoView)
                ?? sheetInfos.FirstOrDefault(x => x.Type.ToUpper().Contains("PAPER"))
                ;
            _gridSheets.SelectRow(defaultSheet);
            _btnImportExport.Enabled = true;
        }
        catch (DwgToPdfImportException ex) {
            var msg = ex.Message;
            Log.Fatal(ex, msg);
            if (closeOnException) {
                Close();
            }
            else {
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private async Task DoWork(ExportSteps step) {
        ToggleLockForWork(true);
        try {
            OutputFilePath = await _controller.ExportFileAsync(
                removeSegmentsFromOverlappingViews: cbRemoveSegmentsFromOverlappingViews.Checked,
                selectMode: (ddlSelectMode.SelectedItem as string) == "BoxSelect"
                    ? DwgToPdfExporter.ExportSelectMode.BoxSelect
                    : DwgToPdfExporter.ExportSelectMode.OneByOne,
                filterOnDominantLayer: cbFilterOnDominantLayer.Checked,
                ignoredLayers: _gridIgnoredLayers.Rows
                    .Cast<DataGridViewRow>()
                    .Select(x => x.Cells[0].Value as string)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList(),
                fileName: _dwgImportInfo.Filename,
                sheetName: _gridSheets.SelectedRows[0].Cells["Name"].Value as string,
                step,
                disableGraphicUpdates: cbDisableGraphicUpdates.Checked
            );
        }
        finally {
            ToggleLockForWork(false);
        }
    }

    private void btnCopy_Click(object sender, EventArgs e) => DoWork(ExportSteps.Copy);
    private void btnCenterAndScale_Click(object sender, EventArgs e) => DoWork(ExportSteps.CenterAndScale);
    private void btnSaveAsPDF_Click(object sender, EventArgs e) => DoWork(ExportSteps.SaveAsPdf);
    
    private async void btnImportExport_Click(object sender, EventArgs e) {
        if (_gridSheets.SelectedRows.Count == 0) {
            MessageBox.Show("Please select a sheet to import.");
            return;
        }
        ToggleLockForWork(true);
        try {
            OutputFilePath = await _controller.ExportFileAsync(
                removeSegmentsFromOverlappingViews: cbRemoveSegmentsFromOverlappingViews.Checked,
                selectMode: (ddlSelectMode.SelectedItem as string) == "BoxSelect"
                    ? DwgToPdfExporter.ExportSelectMode.BoxSelect
                    : DwgToPdfExporter.ExportSelectMode.OneByOne,
                filterOnDominantLayer: cbFilterOnDominantLayer.Checked,
                ignoredLayers: _gridIgnoredLayers.Rows
                    .Cast<DataGridViewRow>()
                    .Select(x => x.Cells[0].Value as string)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList(),
                fileName: _dwgImportInfo.Filename,
                sheetName: _gridSheets.SelectedRows[0].Cells["Name"].Value as string,
                disableGraphicUpdates: cbDisableGraphicUpdates.Checked
            );
            Success = true;
        }
        catch (Exception ex) {
            // MessageBox.Show($"Error exporting DWG to PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            OutputFilePath = string.Empty;
            Success = false;
        }
        finally {
            ToggleLockForWork(false);
        }
    }
}

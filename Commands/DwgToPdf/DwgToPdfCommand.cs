using System;
using System.Windows.Forms;
using Xarial.XCad.SolidWorks;
using Dubeg.Sw.ExportTools.Commands.Base;
using System.Linq;
using SolidWorks.Interop.sldworks;
using Serilog;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace Dubeg.Sw.ExportTools.Commands.DwgToPdf;

public class DwgToPdfCommand : CommandBase<DwgToPdfAppSettings> {
    private FrmDwgToPdfMulti _multiForm;
    private (string fileName, string sheetName) _lastImport = (string.Empty, string.Empty);
    public bool DisableGraphicUpdates { get; set; }

    public DwgToPdfCommand(AddinUiManager uiMgr, ISwApplication swApp, DwgToPdfAppSettings appSettings, AddIn addin) 
        : base(uiMgr, swApp, appSettings, addin) {
    }

    public string ExportCurrentDocument() {
        if (!SwApp.Documents.Any()) {
            MessageBox.Show("No documents opened.");
            return null;
        }
        if (SwApp.Documents.Active is null) {
            MessageBox.Show("No document active.");
            return null;
        }
        try {
            EnsureOutputDirectoryExists();
            var doc = SwApp.Documents.Active;
            
            var drawing = (DrawingDoc)doc.Model;
            var sheet = drawing.IGetCurrentSheet();
            if (sheet == null) {
                MessageBox.Show("No sheets found in drawing");
                return null;
            }
            var sheetName = sheet.GetName();
            var exporter = new DwgToPdfExporter(App) {
                OutputFolderPath = AppSettings.OutputFolderPath,
                InputFilePath = doc.Path
            };
            return exporter.Export();
        }
        catch (Exception ex) {
            var msg = "Error exporting the current document to PDF";
            Log.Fatal(ex, msg);
            MessageBox.Show($"{msg}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }
    }
    
    public void ShowMultiExportForm() {
        if (_multiForm == null || _multiForm.IsDisposed) {
            UiManager.InitAndShowForm(() => _multiForm = new FrmDwgToPdfMulti(this));
        }
        else {
            _multiForm.Invoke(new Action(() => {
                _multiForm.Show();
                _multiForm.WindowState = FormWindowState.Normal;
                _multiForm.Activate();
            }));
        }
    }

    public async Task<(string OutputFilePath, bool Success)> ShowExportForm(string dwgFilePath = null, bool displayNextToMultiForm = false) {
        var tcs = new TaskCompletionSource<(string OutputFilePath, bool Success)>();
        UiManager.InitAndShowForm(() => {
            var form = new FrmDwgToPdf(this, dwgFilePath);
            if (displayNextToMultiForm) {
                form.StartPosition = FormStartPosition.Manual;
                form.Location = new System.Drawing.Point(
                    _multiForm.Bounds.Right - 18,
                    _multiForm.Location.Y
                );
            }
            form.FormClosed += (s, e) => {
                tcs.SetResult((form.OutputFilePath, form.Success));
            };
            return form;
        });
        return await tcs.Task;
    }

    public DwgImportInfo LoadFileInfo(string filePath) {
        try {
            var dwgImportInfo = new DwgImportInfo(filePath);
            dwgImportInfo.Load();
            return dwgImportInfo;
        }
        catch (Exception ex) {
            var fileName = Path.GetFileName(filePath);
            throw new DwgToPdfImportException($"Error loading the file '{fileName}'", ex) {
                FilePath = filePath
            };
        }
    }

    public async Task<string> ExportFileAsync(
        bool removeSegmentsFromOverlappingViews,
        DwgToPdfExporter.ExportSelectMode selectMode,
        bool filterOnDominantLayer,
        List<string> ignoredLayers,
        string fileName,
        string sheetName,
        ExportSteps step = ExportSteps.All,
        bool disableGraphicUpdates = false
    ) {
        return await Task.Run(() => 
            ExportFile(
                removeSegmentsFromOverlappingViews,
                selectMode,
                filterOnDominantLayer,
                ignoredLayers,
                fileName,
                sheetName,
                step,
                disableGraphicUpdates
            )
        );
    }

    public enum ExportSteps { 
        All,
        Copy,
        CenterAndScale,
        SaveAsPdf,
    };

    public string ExportFile(
        bool removeSegmentsFromOverlappingViews,
        DwgToPdfExporter.ExportSelectMode selectMode,
        bool filterOnDominantLayer,
        List<string> ignoredLayers,
        string fileName,
        string sheetName,
        ExportSteps step = ExportSteps.All,
        bool disableGraphicUpdates = false
    ) {
        try {
            App.CommandInProgress = true;
            if (disableGraphicUpdates) {
                App.IActiveDoc2.SketchManager.DisplayWhenAdded = false;
                App.IActiveDoc2.SketchManager.AddToDB = true;
            }
            var exporter = new DwgToPdfExporter(App) {
                RemoveSegmentsFromOverlappingViews = removeSegmentsFromOverlappingViews,
                SelectMode = selectMode,
                FilterOnDominantLayer = filterOnDominantLayer,
                IgnoredLayers = ignoredLayers,
                OutputFolderPath = AppSettings.OutputFolderPath
            };
            if (
                App.IActiveDoc2 is null
                || _lastImport.fileName != fileName
                || _lastImport.sheetName != sheetName) {
                if (App.IActiveDoc2 is not null) {
                    App.CloseAllDocuments(true);
                }
                exporter.ImportDwg(fileName, sheetName);
                _lastImport.fileName = fileName;
                _lastImport.sheetName = sheetName;
            }
            switch (step) {
                case ExportSteps.All: return exporter.Export();
                case ExportSteps.Copy: exporter.Copy(); break;
                case ExportSteps.CenterAndScale: exporter.CenterAndScale(); break;
                case ExportSteps.SaveAsPdf: exporter.SaveAsPdf(); break;
            }
            return string.Empty;
        }
        catch (Exception ex) {
            Log.Fatal(ex, "Error exporting the current document to PDF");
            throw;
        }
        finally {
            App.CommandInProgress = false;
            if (disableGraphicUpdates) {
                App.IActiveDoc2.SketchManager.DisplayWhenAdded = true;
                App.IActiveDoc2.SketchManager.AddToDB = false;
            }
        }
    }
} 
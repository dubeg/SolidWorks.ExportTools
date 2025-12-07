using System;
using System.Windows.Forms;
using Xarial.XCad.SolidWorks;
using Dubeg.Sw.ExportTools.Commands.Base;
using System.Linq;
using Serilog;
using System.Threading.Tasks;
using System.IO;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.sldworks;
using Dubeg.Sw.ExportTools.Utils;

namespace Dubeg.Sw.ExportTools.Commands.JpgToPdf;

public class JpgToPdfCommand : CommandBase<AppSettings> {
    private FrmJpgToPdfExporterMulti _multiExportForm;
    private readonly JpgToSheetCommand _jpgToSheetCommand;
    private readonly SheetToPdfCommand _sheetToPdfCommand;

    public JpgToPdfCommand(AddinUiManager uiMgr, ISwApplication swApp, AppSettings appSettings, AddIn addin) 
        : base(uiMgr, swApp, appSettings, addin) {
        _jpgToSheetCommand = new JpgToSheetCommand(uiMgr, swApp, appSettings, addin);
        _sheetToPdfCommand = new SheetToPdfCommand(uiMgr, swApp, appSettings, addin);
    }

    public string RunForCurrentDocument() {
        if (!SwApp.Documents.Any()) {
            throw new Exception("No documents opened.");
        }
        if (SwApp.Documents.Active is null) {
            throw new Exception("No document active.");
        }
        
        EnsureOutputDirectoryExists();
        var doc = SwApp.Documents.Active;
        _jpgToSheetCommand.RunForCurrentDocument();
        return _sheetToPdfCommand.RunForCurrentSheet();
    }

    public async Task<string> RunForFileAsync(string inputFilePath) => await Task.Run(() => RunForFile(inputFilePath));

    public string RunForFile(string inputFilePath) {
        var docType = Path.GetExtension(inputFilePath).ToLower() switch {
            ".slddrw" => swDocumentTypes_e.swDocDRAWING,
            _ => throw new ArgumentException($"Unsupported file type: {inputFilePath}. Only .slddrw files are supported.")
        };
        var (success, errors) = App.OpenDocument(inputFilePath, docType);
        if (!success) {
            throw new Exception($"Failed to open file {Path.GetFileName(inputFilePath)}: {string.Join(", ", errors)}");
        }
        return RunForCurrentDocument();
    }

    public void ShowMultiExportForm() {
        if (_multiExportForm == null || _multiExportForm.IsDisposed) {
            UiManager.InitAndShowForm(() => _multiExportForm = new FrmJpgToPdfExporterMulti(this));
        }
        else {
            _multiExportForm.Invoke(new Action(() => {
                _multiExportForm.Show();
                _multiExportForm.WindowState = FormWindowState.Normal;
                _multiExportForm.Activate();
            }));
        }
    }
} 
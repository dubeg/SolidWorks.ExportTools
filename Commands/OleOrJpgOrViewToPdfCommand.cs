using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dubeg.Sw.ExportTools.Commands.Base;
using Dubeg.Sw.ExportTools.Commands.JpgToPdf;
using Dubeg.Sw.ExportTools.Utils;
using SolidWorks.Interop.swconst;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;

namespace Dubeg.Sw.ExportTools.Commands;

public class OleOrJpgOrViewToPdfCommand : CommandBase<AppSettings> {
    private readonly OleObjectToSheetCommand _oleToSheetCommand;
    private readonly JpgToSheetCommand _jpgToSheetCommand;
    private readonly ViewToSheetCommand _viewToSheetCommand;
    private readonly SheetToPdfCommand _sheetToPdfCommand;

    public OleOrJpgOrViewToPdfCommand(AddinUiManager uiMgr, ISwApplication swApp, AppSettings appSettings, AddIn addin)
        : base(uiMgr, swApp, appSettings, addin) {
        _oleToSheetCommand = new OleObjectToSheetCommand(uiMgr, swApp, appSettings, addin);
        _jpgToSheetCommand = new JpgToSheetCommand(uiMgr, swApp, appSettings, addin);
        _viewToSheetCommand = new ViewToSheetCommand(uiMgr, swApp, appSettings, addin);
        _sheetToPdfCommand = new SheetToPdfCommand(uiMgr, swApp, appSettings, addin);
    }

    public string RunForCurrentDocument(ContentType contentType = ContentType.Auto) {
        if (SwApp.Documents.Count == 0) {
            throw new InvalidOperationException("No documents opened.");
        }
        if (SwApp.Documents.Active is null) {
            throw new InvalidOperationException("No document active.");
        }
        var doc = SwApp.Documents.Active;
        if (doc is not ISwDrawing) {
            throw new InvalidOperationException("Active document is not a drawing.");
        }
        if (contentType == ContentType.Auto) {
            if (_jpgToSheetCommand.CanRunForCurrentDocument()) contentType = ContentType.Jpg;
            else if (_oleToSheetCommand.CanRunForCurrentDocument()) contentType = ContentType.OleObject;
            else contentType = ContentType.View;
        }
        switch (contentType) {
            case ContentType.OleObject: _oleToSheetCommand.RunForCurrentDocument(); break;
            case ContentType.Jpg: _jpgToSheetCommand.RunForCurrentDocument(); break;
            case ContentType.View: _viewToSheetCommand.RunForCurrentDocument(); break;
            default: throw new ArgumentException($"Unsupported content type: {contentType}");
        }
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
}

public enum ContentType {
    Auto,
    OleObject,
    Jpg,
    View
} 
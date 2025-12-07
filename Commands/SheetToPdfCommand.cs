using System;
using System.IO;
using System.Linq;
using Dubeg.Sw.ExportTools.Commands.Base;
using Dubeg.Sw.ExportTools.Utils;
using SolidWorks.Interop.sldworks;
using Xarial.XCad.SolidWorks;

namespace Dubeg.Sw.ExportTools.Commands;

/// <summary>
/// Saves the current sheet as a pdf.
/// </summary>
public class SheetToPdfCommand : CommandBase<AppSettings> {
    public SheetToPdfCommand(AddinUiManager uiMgr, ISwApplication swApp, AppSettings appSettings, AddIn addin)
        : base(uiMgr, swApp, appSettings, addin) {
    }

    public string RunForCurrentSheet() {
        var swModel = App.IActiveDoc2;
        var drawingDoc = swModel as DrawingDoc;
        if (drawingDoc == null) {
            throw new InvalidOperationException("Active document is not a drawing.");
        }
        var sheet = drawingDoc.IGetCurrentSheet();
        if (sheet is null) {
            throw new InvalidOperationException($"No current sheet.");
        }
        var sheetName = sheet.GetName();
        var sheetView = drawingDoc.GetViewBySheetName(sheetName);
        if (sheetView == null) {
            throw new InvalidOperationException($"View for sheet '{sheetName}' not found.");
        }

        EnsureOutputDirectoryExists();
        var outFilePath = GetOutputFilePath(swModel.GetPathName());
        var (success, errors) = App.SaveSheetAsPdf(sheetView, outFilePath, true, false);
        if (!success) {
            throw new ExportException($"Failed to export {Path.GetFileName(outFilePath)} to PDF: {string.Join(", ", errors)}") {
                FileName = outFilePath,
                FileSaveErrors = errors
            };
        }
        return outFilePath;
    }
}

using System;
using System.IO;
using System.Linq;
using Dubeg.Sw.ExportTools.Commands.Base;
using Dubeg.Sw.ExportTools.Enums;
using Dubeg.Sw.ExportTools.Utils;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using Xarial.XCad.SolidWorks;

namespace Dubeg.Sw.ExportTools.Commands;

/// <summary>
/// Saves the current sheet as jpg.
/// </summary>
public class SheetToJpgCommand : CommandBase<AppSettings> {
    public SheetToJpgCommand(AddinUiManager uiMgr, ISwApplication swApp, AppSettings appSettings, AddIn addin)
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
        var outFilePath = GetOutputFilePath(swModel.GetPathName(), "jpg");
        var (success, errors) = App.SaveAsJpg(
            outFilePath, 
            AllOrCurrentSheet.CurrentSheet,
            swTiffImageType_e.swTiffImageGrayScale,
            ScreenOrPrintCapture.ScreenCapture,
            SheetOrPrintSize.SheetSize,
            jpgCompressionLevel: 1
        );
        if (!success) {
            throw new ExportException($"Failed to export {Path.GetFileName(outFilePath)} to JPG: {string.Join(", ", errors)}") {
                FileName = outFilePath,
                FileSaveErrors = errors
            };
        }
        return outFilePath;
    }
}

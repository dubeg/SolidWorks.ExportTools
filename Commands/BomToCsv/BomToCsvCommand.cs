using Dubeg.Sw.ExportTools.Commands.Base;
using SolidWorks.Interop.sldworks;
using System;
using System.IO;
using Xarial.XCad.SolidWorks;

namespace Dubeg.Sw.ExportTools.Commands.BomToCsv;

/// <summary>
/// Exports the BOM table from the current drawing sheet to a CSV file.
/// </summary>
public class BomToCsvCommand : CommandBase<AppSettings> {
    public BomToCsvCommand(AddinUiManager uiMgr, ISwApplication swApp, AppSettings appSettings, AddIn addin)
        : base(uiMgr, swApp, appSettings, addin) {
    }

    /// <summary>
    /// Exports the BOM table from the current sheet to a CSV file.
    /// </summary>
    /// <returns>Path to the exported CSV file.</returns>
    public string RunForCurrentSheet() {
        var model = App.IActiveDoc2;
        var drawing = model as DrawingDoc;
        if (drawing == null) {
            throw new InvalidOperationException("Active document is not a drawing.");
        }
        var sheet = drawing.IGetCurrentSheet();
        if (sheet is null) {
            throw new InvalidOperationException("No current sheet.");
        }
        var exporter = new BomCsvExporter(App);
        var bomData = exporter.ExtractBomData();
        if (bomData.Count == 0) {
            throw new InvalidOperationException("No BOM table found on the current sheet.");
        }
        string csvContent = exporter.ConvertToCsv(bomData);
        EnsureOutputDirectoryExists();
        var outFilePath = GetOutputFilePath(model.GetPathName(), "csv");
        File.WriteAllText(outFilePath, csvContent, System.Text.Encoding.UTF8);
        return outFilePath;
    }
}


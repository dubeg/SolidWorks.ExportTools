using Dubeg.Sw.ExportTools.Commands.Base;
using SolidWorks.Interop.sldworks;
using System;
using System.IO;
using Xarial.XCad.SolidWorks;

namespace Dubeg.Sw.ExportTools.Commands.RevisionToCsv;

/// <summary>
/// Exports the Revision table from the current drawing sheet to a CSV file.
/// </summary>
public class RevisionToCsvCommand : CommandBase<AppSettings> {
    public RevisionToCsvCommand(AddinUiManager uiMgr, ISwApplication swApp, AppSettings appSettings, AddIn addin)
        : base(uiMgr, swApp, appSettings, addin) {
    }

    /// <summary>
    /// Exports the Revision table from the current sheet to a CSV file.
    /// </summary>
    /// <returns>Path to the exported CSV file, or null if no revision table exists.</returns>
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
        var exporter = new RevisionCsvExporter(App);
        var revisionData = exporter.ExtractRevisionData();
        if (revisionData.Count == 0) {
            // No revision table found - this is not an error, just return null
            return null;
        }
        EnsureOutputDirectoryExists();
        var baseName = Path.GetFileNameWithoutExtension(model.GetPathName());
        var outFilePath = Path.Combine(OutputFolderPath, $"{baseName}_revision.csv");
        var csvContent = exporter.ConvertToCsv(revisionData);
        File.WriteAllText(outFilePath, csvContent, System.Text.Encoding.UTF8);
        return outFilePath;
    }
}


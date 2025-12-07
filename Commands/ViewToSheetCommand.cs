using System;
using System.Windows.Forms;
using Xarial.XCad.SolidWorks;
using Dubeg.Sw.ExportTools.Commands.Base;
using System.Linq;
using Serilog;
using System.Threading.Tasks;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using Dubeg.Sw.ExportTools.Utils;
using System.IO;
using Xarial.XCad.SolidWorks.Documents;

namespace Dubeg.Sw.ExportTools.Commands;

/// <summary>
/// Copies a view to a new sheet and scales it accordingly.
/// </summary>
public class ViewToSheetCommand : CommandBase<AppSettings> {
    public ViewToSheetCommand(AddinUiManager uiMgr, ISwApplication swApp, AppSettings appSettings, AddIn addin)
        : base(uiMgr, swApp, appSettings, addin) {
    }

    public void RunForCurrentDocument(bool cleanup = false) {
        if (!SwApp.Documents.Any()) {
            throw new InvalidOperationException("No documents opened.");
        }
        if (SwApp.Documents.Active is null) {
            throw new InvalidOperationException("No document active.");
        }

        var doc = SwApp.Documents.Active;
        if (doc is not ISwDrawing) {
            throw new InvalidOperationException("Active document is not a drawing.");
        }
        var drawingDoc = doc.CastTo<ISwDrawing>().Drawing;

        var swModel = App.IActiveDoc2;
        var selMgr = swModel.ISelectionManager;

        // Get selected view
        if (selMgr.GetSelectedObjectCount2(-1) != 1) {
            throw new InvalidOperationException("Please select exactly one drawing view.");
        }

        var selType = (swSelectType_e)selMgr.GetSelectedObjectType3(1, -1);
        if (selType != swSelectType_e.swSelDRAWINGVIEWS) {
            throw new InvalidOperationException("Selected object is not a drawing view.");
        }

        var sourceView = (IView)selMgr.GetSelectedObject6(1, -1);
        var sourceSheet = drawingDoc.IGetCurrentSheet();
        swModel.EditCopy();

        // Create new sheet
        var newSheetName = $"ExportSheet_{DateTime.Now:yyyyMMdd_HHmmss}";
        drawingDoc.CopySheet(sourceSheet, newSheetName);
        var newSheet = drawingDoc.GetSheet(newSheetName);
        var newSheetView = drawingDoc.GetViewBySheetName(newSheetName);

        // Copy view
        drawingDoc.ActivateSheet(newSheetName);
        swModel.Paste();

        // Center & scale
        var copiedView = drawingDoc.GetViewsForSheet(newSheet).FirstOrDefault();
        newSheetView.CenterView(copiedView);
        swModel.ScaleViewInSheetToMaxSize(newSheet, copiedView);
        swModel.Extension.SelectView(copiedView);

        if (cleanup) {
            swModel.DeleteSheet(newSheetName);
        }
    }
}

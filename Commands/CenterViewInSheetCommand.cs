using System;
using System.Linq;
using Dubeg.Sw.ExportTools.Commands.Base;
using Dubeg.Sw.ExportTools.Utils;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;

namespace Dubeg.Sw.ExportTools.Commands;

/// <summary>
/// Centers a selected view in the current sheet.
/// </summary>
public class CenterViewInSheetCommand : CommandBase<AppSettings> {
    public CenterViewInSheetCommand(AddinUiManager uiMgr, ISwApplication swApp, AppSettings appSettings, AddIn addin)
        : base(uiMgr, swApp, appSettings, addin) {
    }

    public void RunForCurrentDocument() {
        if (!SwApp.Documents.Any()) {
            throw new InvalidOperationException("No documents opened.");
        }
        if (SwApp.Documents.Active is null) {
            throw new InvalidOperationException("No document active.");
        }

        var doc = SwApp.Documents.Active;
        if (!(doc is ISwDrawing)) {
            throw new InvalidOperationException("Active document is not a drawing.");
        }
        var drawingDoc = doc.As<ISwDrawing>().Drawing;

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

        var view = (IView)selMgr.GetSelectedObject6(1, -1);
        var sheet = drawingDoc.IGetCurrentSheet();
        var sheetView = drawingDoc.GetViewBySheetName(sheet.GetName());
        sheetView.CenterView(view);
        swModel.EditRebuild3();
        swModel.GraphicsRedraw2();
    }
} 
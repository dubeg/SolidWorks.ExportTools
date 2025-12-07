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
public class OleObjectToSheetCommand : CommandBase<AppSettings> {
    public OleObjectToSheetCommand(AddinUiManager uiMgr, ISwApplication swApp, AppSettings appSettings, AddIn addin)
        : base(uiMgr, swApp, appSettings, addin) {
    }

    public bool CanRunForCurrentDocument() {
        if (!SwApp.Documents.Any()) {
            throw new InvalidOperationException("No documents opened.");
        }
        if (SwApp.Documents.Active is null) {
            throw new InvalidOperationException("No document active.");
        }
        var doc = SwApp.Documents.Active;
        if (doc is not ISwDrawing) {
            return false;
        }
        var drawingDoc = doc.CastTo<ISwDrawing>().Drawing;
        var oleCount = doc.Model.Extension.GetOLEObjectCount((int)swOleObjectOptions_e.swOleObjectOptions_GetAll);
        return oleCount > 0;
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

        var oleObjectCount = swModel.Extension.GetOLEObjectCount((int)swOleObjectOptions_e.swOleObjectOptions_GetOnCurrentSheet);
        if (oleObjectCount == 0) {
            throw new InvalidOperationException("Active sheet doesn't contain an OLE object.");
        }

        SwOLEObject sourceObject;
        if (oleObjectCount == 1) {
            sourceObject = swModel.Extension.GetOleObjectsAll().FirstOrDefault();
            sourceObject.Select(false);
        }
        else {
            if (selMgr.GetSelectedObjectCount2(-1) != 1) {
                throw new InvalidOperationException("Please select exactly one OLE object.");
            }
            var selType = (swSelectType_e)selMgr.GetSelectedObjectType3(1, -1);
            if (selType != swSelectType_e.swSelOLEITEMS) {
                throw new InvalidOperationException("Selected object is not an OLE Object.");
            }
            sourceObject = (SwOLEObject)selMgr.GetSelectedObject6(1, -1);
        }

        // Copy
        var sourceSheet = drawingDoc.IGetCurrentSheet();
        swModel.EditCopy();

        // Create new sheet
        var newSheetName = $"ExportSheet_{DateTime.Now:yyyyMMdd_HHmmss}";
        drawingDoc.CopySheet(sourceSheet, newSheetName);
        var newSheet = drawingDoc.GetSheet(newSheetName);
        var newSheetView = drawingDoc.GetViewBySheetName(newSheetName);

        // Paste
        drawingDoc.ActivateSheet(newSheetName);
        swModel.Paste();

        // Center & scale
        drawingDoc.ActivateSheet(newSheetName);
        drawingDoc.ActivateView(newSheetView.GetName2());
        var copiedObject = swModel.Extension.GetOleObjectsOnCurrentSheet().First();
        newSheetView.ScaleOleObject(copiedObject);
        newSheetView.CenterOleObject(copiedObject);

        if (cleanup) {
            swModel.DeleteSheet(newSheetName);
        }
    }    
}

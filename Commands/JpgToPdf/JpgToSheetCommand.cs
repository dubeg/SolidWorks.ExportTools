using System;
using System.Linq;
using Dubeg.Sw.ExportTools.Commands.Base;
using Dubeg.Sw.ExportTools.Utils;
using SolidWorks.Interop.sldworks;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks;

namespace Dubeg.Sw.ExportTools.Commands.JpgToPdf;

public class JpgToSheetCommand : CommandBase<AppSettings> {
    public JpgToSheetCommand(AddinUiManager uiMgr, ISwApplication swApp, AppSettings appSettings, AddIn addin) 
        : base(uiMgr, swApp, appSettings, addin) {
    }

    public bool CanRunForCurrentDocument() {
        var model = App.IActiveDoc2;
        if (model == null) {
            throw new Exception($"No file currently opened");
        }
        var drawing = (DrawingDoc)model;
        var selMgr = model.ISelectionManager;
        var sketchMgr = model.SketchManager;
        var sheet = drawing.IGetCurrentSheet();
        if (sheet == null) {
            throw new Exception("No sheets found in drawing");
        }
        model.ClearSelection2(true);
        var allViews = drawing.GetViewsEx();
        var pictureInViews = allViews
            .Where(x => x.GetSketchPictureCount() > 0)
            .SelectMany(x => (object[])x.GetSketchPictures())
            .Cast<ISketchPicture>()
            .FirstOrDefault();
        var pictureInSketches = allViews
            .Select(x => (Sketch)x.GetSketch())
            .Where(x => x is not null && x.GetSketchPictureCount() > 0)
            .SelectMany(x => (object[])x.GetSketchPictures())
            .Cast<ISketchPicture>()
            .FirstOrDefault();
        var picture = pictureInViews ?? pictureInSketches;
        return picture is not null;
    }

    public void RunForCurrentDocument() {
        var model = App.IActiveDoc2;
        if (model == null) {
            throw new Exception($"No file currently opened");
        }
        var drawing = (DrawingDoc)model;
        var selMgr = model.ISelectionManager;
        var sketchMgr = model.SketchManager;
        var sheet = drawing.IGetCurrentSheet();
        if (sheet == null) {
            throw new Exception("No sheets found in drawing");
        }
        model.ClearSelection2(true);
        var allViews = drawing.GetViewsEx();
        var pictureInViews = allViews
            .Where(x => x.GetSketchPictureCount() > 0)
            .SelectMany(x => (object[])x.GetSketchPictures())
            .Cast<ISketchPicture>()
            .FirstOrDefault();
        var pictureInSketches = allViews
            .Select(x => (Sketch)x.GetSketch())
            .Where(x => x.GetSketchPictureCount() > 0)
            .SelectMany(x => (object[])x.GetSketchPictures())
            .Cast<ISketchPicture>()
            .FirstOrDefault();
        var picture = pictureInViews ?? pictureInSketches;
        if (picture is null) {
            throw new Exception("Picture not found in drawing sheet");
        }
        var newSheetName = "EXPORT_SHEET";
        drawing.ActivateSheet(sheet.GetName());
        picture.GetFeature().Select2(false, 0);
        model.EditCopy();
        drawing.CopySheet(sheet, newSheetName);
        drawing.ActivateSheet(newSheetName);
        model.Paste();
        // ----------------------------
        // Center
        // ----------------------------
        var newSheet = drawing.GetSheet(newSheetName);
        var newSheetView = drawing.GetViewBySheetName(newSheetName);
        var newSheetViewSketch = newSheetView.IGetSketch();
        var newSheetViewSketchPicture =
            newSheetView.GetSketchPicturesEx().FirstOrDefault() ??
            newSheetViewSketch.GetSketchPicturesEx().FirstOrDefault();
        if (newSheetViewSketchPicture is null) {
            throw new Exception("Picture not found in copied sheet");
        }
        newSheetView.CenterPicture(App.IGetMathUtility(), newSheetViewSketchPicture);
        // ----------------------------
        model.ViewZoomToSelection();
        model.ViewZoomtofit2();
    }
} 
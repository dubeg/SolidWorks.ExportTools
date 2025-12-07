using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Dubeg.Sw.ExportTools.Enums;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace Dubeg.Sw.ExportTools.Utils;

public record NxPoint2D {
    public double X { get; set; }
    public double Y { get; set; }
}

public record NxRectangle {
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
}

public static class SolidWorksExtensions {

    public static void SetPosition(this IView view, double x, double y) => view.Position = new double[] { x, y };

    public static (double X, double Y) GetPositionAsTuple(this IView view) {
        var viewPosition = (double[])view.Position;
        return (
            X: viewPosition[0],
            Y: viewPosition[1]
        );
    }

    public static (double MinX, double MinY, double MaxX, double MaxY) GetOutlineAsTuple(this IView view) {
        var outline = (double[])view.GetOutline();
        var xMin = outline[0];
        var yMin = outline[1];
        var xMax = outline[2];
        var yMax = outline[3];
        return (
            MinX: xMin,
            MinY: yMin,
            MaxX: xMax,
            MaxY: yMax
        );
    }

    public static NxRectangle GetOutlineAsRect(this IView view, bool meterToMM = false) {
        var outline = (double[])view.GetOutline();
        var xMin = outline[0] * (meterToMM ? 1000.0 : 1.0);
        var yMin = outline[1] * (meterToMM ? 1000.0 : 1.0);
        var xMax = outline[2] * (meterToMM ? 1000.0 : 1.0);
        var yMax = outline[3] * (meterToMM ? 1000.0 : 1.0);
        return new NxRectangle() { 
            X = xMin,
            Y = yMin,
            Width = xMax - xMin,
            Height = yMax - yMin
        };
    }

    public static (double X, double Y) GetViewCenter(this IView view) {
        var outline = (double[])view.GetOutline();
        var xMin = outline[0];
        var yMin = outline[1];
        var xMax = outline[2];
        var yMax = outline[3];
        return (
            X: (xMax - xMin) / 2.0,
            Y: (yMax - yMin) / 2.0
        );
    }

    public static ISheet GetSheet(this IDrawingDoc swDraw, string sheetName) {
        var sheetNames = (string[])swDraw.GetSheetNames();
        if (sheetNames is null) return null;
        if (sheetNames.Contains(sheetName)) {
            return swDraw.Sheet[sheetName];
        }
        return null;
    }

    public static List<ISheet> GetSheets(this IDrawingDoc swDraw) {
        var results = new List<ISheet>();
        var sheetNames = (string[])swDraw.GetSheetNames();
        if (sheetNames is null) return results;
        foreach (var name in sheetNames) {
            results.Add(swDraw.Sheet[name]);
        }
        return results;
    }

    public static IView GetViewBySheetName(this IDrawingDoc swDraw, string sheetName) {
        var viewsBySheet = (object[])swDraw.GetViews();
        if (viewsBySheet is null) return null;
        foreach (object[] views in viewsBySheet) {
            var view = views.Cast<IView>().FirstOrDefault(x => x.Name == sheetName);
            if (view is not null) {
                return view;
            }
        }
        return null;
    }

    public static List<(IView sheetView, List<IView> InnerViews)> GetViewsBySheet(this IDrawingDoc swDraw) {
        var viewsBySheet = (object[])swDraw.GetViews();
        if (viewsBySheet is null) return null;
        var results = new List<(IView sheetView, List<IView> InnerViews)>();
        foreach (object[] views in viewsBySheet) {
            var sheetViews = views.Cast<IView>().ToList();
            results.Add(
                (sheetViews.First(), sheetViews.Skip(1).ToList())
            );
        }
        return results;
    }

    public static List<IView> GetViewsEx(this IDrawingDoc swDraw) {
        var viewsBySheet = (object[])swDraw.GetViews();
        if (viewsBySheet is null) return null;
        var results = new List<IView>();
        foreach (object[] views in viewsBySheet) {
            results.AddRange(
                views.Cast<IView>()
            );
        }
        return results;
    }

    public static List<IView> GetViewsForSheet(this IDrawingDoc swDraw, ISheet sheet) => GetViewsForSheet(swDraw, sheet.GetName());

    public static List<IView> GetViewsForSheet(this IDrawingDoc swDraw, string sheetName) {
        return swDraw.GetViewsBySheet().FirstOrDefault(x => x.sheetView.GetName2() == sheetName).InnerViews;
    }

    // Note: this returns a bunch of views that are hidden in FeatureManagerTree,
    // which I never expected & introduced bugs.
    //public static List<IView> GetViewsEx(this ISheet sheet) {
    //    var results = new List<IView>();
    //    var views = (object[])sheet.GetViews();
    //    if (views is null) return results;
    //    return views.Cast<IView>().ToList();
    //}

    public static void EnableSelectionFilters(this ISldWorks app) => app.SetApplySelectionFilter(true);
    public static void DisableSelectionFilters(this ISldWorks app) => app.SetApplySelectionFilter(false);

    public static void ClearSelectionFilters(this ISldWorks app) {
        var filters = app.GetSelectionFilters();
        app.SetSelectionFilters(filters, false);
    }

    public static void SetSelectionFiltersEx(this ISldWorks app, swSelectType_e[] filters) {
        var filterTypes = filters.Select(x => (int)x).ToArray();
        app.SetSelectionFilters(filterTypes, true);
    }

    public static void DeleteSheet(this IModelDoc2 model, string sheetName) {
        var doc = (DrawingDoc)model;
        var selMgr = model.ISelectionManager;
        var swSheet = doc.Sheet[sheetName];
        if (swSheet != null) {
            selMgr.SuspendSelectionList();
            selMgr.AddSelectionListObject(swSheet, null);
            model.Extension.DeleteSelection2((int)(swDeleteSelectionOptions_e.swDelete_Absorbed | swDeleteSelectionOptions_e.swDelete_Children));
            selMgr.ResumeSelectionList();
        }
    }

    public static void CopySheet(this IDrawingDoc doc, ISheet sheet, string newSheetName) {
        var sourceSheet = sheet;
        var sourceSheetProps = (double[])sourceSheet.GetProperties2();
        var sourcePaperSize = (swDwgPaperSizes_e)sourceSheetProps[0];
        var sourceTemplate = (swDwgTemplates_e)sourceSheetProps[1];
        var sourceScale1 = sourceSheetProps[2];
        var sourceScale2 = sourceSheetProps[3];
        var sourceFirstAngle = sourceSheetProps[4];
        var sourceWidth = sourceSheetProps[5];
        var sourceHeight = sourceSheetProps[6];
        // [paperSize, templateIn, scale1, scale2, firstAngle, width, height, sameCustomProp]

        doc.NewSheet4(
            newSheetName,
            (int)sourcePaperSize,
            (int)swDwgTemplates_e.swDwgTemplateNone,
            (int)sourceScale1,
            (int)sourceScale2,
            false, 
            "", 
            sourceWidth, sourceHeight, 
            "", 0, 0, 0, 0, 1, 1
        );
    }

    public static IEnumerable<ISketchPicture> GetSketchPicturesEx(this IView view) {
        var count = view.GetSketchPictureCount();
        if (count == 0) return [];
        var pictures = (object[])view.GetSketchPictures();
        if (pictures is null) return [];
        return pictures.Cast<ISketchPicture>();
    }

    public static IEnumerable<ISketchPicture> GetSketchPicturesEx(this ISketch sketch) {
        var count = sketch.GetSketchPictureCount();
        if (count == 0) return [];
        var pictures = (object[])sketch.GetSketchPictures();
        if (pictures is null) return [];
        return pictures.Cast<ISketchPicture>();
    }

    public static bool SelectById(this IModelDocExtension modelExt, string name, string type) => modelExt.SelectByID2(name, type.ToUpper(), 0, 0, 0, false, 0, null, 0);
    public static bool SelectById(this IModelDocExtension modelExt, string name, swSelectType_e type) => modelExt.SelectByID2(name, GetTypeForSelection(type), 0, 0, 0, false, 0, null, 0);
    public static bool Select(this IModelDocExtension modelExt, IView view) => modelExt.SelectById(view.GetName2(), swSelectType_e.swSelDRAWINGVIEWS);
    public static bool Select(this IModelDocExtension modelExt, ISheet sheet) => modelExt.SelectById(sheet.GetName(), swSelectType_e.swSelSHEETS);

    public static bool SelectSheet(this IModelDocExtension modelExt, string sheetName) => modelExt.SelectById(sheetName, swSelectType_e.swSelSHEETS);
    public static bool SelectView(this IModelDocExtension modelExt, string viewName) => modelExt.SelectById(viewName, swSelectType_e.swSelDRAWINGVIEWS);
    public static bool SelectView(this IModelDocExtension modelExt, IView view) => modelExt.SelectById(view.GetName2(), swSelectType_e.swSelDRAWINGVIEWS);

    /// <summary>
    /// Get type name (uppercase) of object type to use using Model.Extension.SelectByID2(name, type, ...).
    /// </summary>
    /// <see cref="https://help.solidworks.com/2022/english/api/swconst/SolidWorks.Interop.swconst~SolidWorks.Interop.swconst.swSelectType_e.html"/>
    /// <param name="x"></param>
    /// <returns></returns>
    public static string GetTypeForSelection(swSelectType_e x) => x switch {
        swSelectType_e.swSelEDGES => "EDGE", // IEdge
        swSelectType_e.swSelFACES => "FACE", // IFace2
        swSelectType_e.swSelVERTICES => "VERTEX", // IVertex
        swSelectType_e.swSelDATUMPLANES => "PLANE", // Feature
        swSelectType_e.swSelDATUMAXES => "AXIS", // Feature
        swSelectType_e.swSelDATUMPOINTS => "DATUMPOINT", // Feature
        swSelectType_e.swSelOLEITEMS => "OLEITEM", // Not Supported
        swSelectType_e.swSelATTRIBUTES => "ATTRIBUTE", // Feature
        swSelectType_e.swSelSKETCHES => "SKETCH", // Feature
        swSelectType_e.swSelSKETCHSEGS => "SKETCHSEGMENT", // ISketchSegment
        swSelectType_e.swSelSKETCHPOINTS => "SKETCHPOINT", // ISketchPoint
        swSelectType_e.swSelDRAWINGVIEWS => "DRAWINGVIEW", // IView
        swSelectType_e.swSelGTOLS => "GTOL", // IGtol
        swSelectType_e.swSelDIMENSIONS => "DIMENSION", // IDisplayDimension
        swSelectType_e.swSelNOTES => "NOTE", // INote
        swSelectType_e.swSelSECTIONLINES => "SECTIONLINE", // Feature
        swSelectType_e.swSelDETAILCIRCLES => "DETAILCIRCLE", // Feature
        swSelectType_e.swSelSECTIONTEXT => "SECTIONTEXT", // 
        swSelectType_e.swSelSHEETS => "SHEET", // ISheet
        swSelectType_e.swSelCOMPONENTS => "COMPONENT", // IComponent2
        swSelectType_e.swSelMATES => "MATE", // Feature
        swSelectType_e.swSelBODYFEATURES => "BODYFEATURE", // 
        swSelectType_e.swSelREFCURVES => "REFCURVE", // Feature
        swSelectType_e.swSelEXTSKETCHSEGS => "EXTSKETCHSEGMENT", // ISketchSegment
        swSelectType_e.swSelEXTSKETCHPOINTS => "EXTSKETCHPOINT", // ISketchPoint
        swSelectType_e.swSelHELIX => "HELIX", // 
        //swSelectType_e.swSelREFERENCECURVES => "REFERENCECURVES", // Feature
        swSelectType_e.swSelREFSURFACES => "REFSURFACE", // 
        swSelectType_e.swSelCENTERMARKS => "CENTERMARKS", // Not supported
        swSelectType_e.swSelINCONTEXTFEAT => "INCONTEXTFEAT", // 
        swSelectType_e.swSelMATEGROUP => "MATEGROUP", // 
        swSelectType_e.swSelBREAKLINES => "BREAKLINE", // IBreakLine
        swSelectType_e.swSelINCONTEXTFEATS => "INCONTEXTFEATS", // 
        swSelectType_e.swSelMATEGROUPS => "MATEGROUPS", // 
        swSelectType_e.swSelSKETCHTEXT => "SKETCHTEXT", // 
        swSelectType_e.swSelSFSYMBOLS => "SFSYMBOL", // ISFSymbol
        swSelectType_e.swSelDATUMTAGS => "DATUMTAG", // IDatumTag
        swSelectType_e.swSelCOMPPATTERN => "COMPPATTERN", // 
        swSelectType_e.swSelWELDS => "WELD", // IWeldSymbol
        swSelectType_e.swSelDTMTARGS => "DTMTARG", // IDatumTargetSym
        swSelectType_e.swSelPOINTREFS => "POINTREF", // 
        swSelectType_e.swSelDCABINETS => "DCABINET", // 
        swSelectType_e.swSelEXPLVIEWS => "EXPLODEVIEWS", // 
        swSelectType_e.swSelEXPLSTEPS => "EXPLODESTEPS", // 
        swSelectType_e.swSelEXPLLINES => "EXPLODELINES", // 
        swSelectType_e.swSelSILHOUETTES => "SILHOUETTE", // 
        swSelectType_e.swSelCONFIGURATIONS => "CONFIGURATIONS", // Feature
        swSelectType_e.swSelARROWS => "VIEWARROW", // IProjectionArrow
        swSelectType_e.swSelZONES => "ZONES", // 
        swSelectType_e.swSelREFEDGES => "REFERENCE", // -EDGEIEdge
        swSelectType_e.swSelREFSILHOUETTE => "Feature", // 
        swSelectType_e.swSelBOMS => "BOM", // 
        swSelectType_e.swSelEQNFOLDER => "EQNFOLDER", // 
        swSelectType_e.swSelSKETCHHATCH => "SKETCHHATCH", // 
        swSelectType_e.swSelIMPORTFOLDER => "IMPORTFOLDER", // 
        swSelectType_e.swSelVIEWERHYPERLINK => "HYPERLINK", // 
        swSelectType_e.swSelCUSTOMSYMBOLS => "CUSTOMSYMBOL", // ICustomSymbol
        swSelectType_e.swSelCOORDSYS => "COORDSYS", // 
        swSelectType_e.swSelDATUMLINES => "REFLINE", // 
        swSelectType_e.swSelBOMTEMPS => "BOMTEMP", // 
        swSelectType_e.swSelROUTEPOINTS => "ROUTEPOINT", // '
        swSelectType_e.swSelCONNECTIONPOINTS => "CONNECTIONPOINT", // 
        swSelectType_e.swSelPOSGROUP => "POSGROUP", // 
        swSelectType_e.swSelBROWSERITEM => "BROWSERITEM", // 
        swSelectType_e.swSelFABRICATEDROUTE => "ROUTEFABRICATED", // 
        swSelectType_e.swSelSKETCHPOINTFEAT => "SKETCHPOINTFEAT", // 
        swSelectType_e.swSelLIGHTS => "LIGHTS", // 
        swSelectType_e.swSelSURFACEBODIES => "SURFACEBODY", // 
        swSelectType_e.swSelSOLIDBODIES => "SOLIDBODY", // 
        swSelectType_e.swSelFRAMEPOINT => "FRAMEPOINT", // 
        swSelectType_e.swSelMANIPULATORS => "MANIPULATOR", // 
        swSelectType_e.swSelPICTUREBODIES => "PICTURE", //  BODY
        swSelectType_e.swSelLEADERS => "LEADER", // /td>
        swSelectType_e.swSelSKETCHBITMAP => "SKETCHBITMAP", // /td>
        swSelectType_e.swSelDOWELSYMS => "DOWLELSYM", // IDowelSymbol
        swSelectType_e.swSelEXTSKETCHTEXT => "EXTSKETCHTEXT", // 
        swSelectType_e.swSelBLOCKINST => "BLOCKINST", // IBlockInstance
        swSelectType_e.swSelFTRFOLDER => "FTRFOLDER", // 
        swSelectType_e.swSelSKETCHREGION => "SKETCHREGION", // 
        swSelectType_e.swSelSKETCHCONTOUR => "SKETCHCONTOUR", // 
        swSelectType_e.swSelBOMFEATURES => "BOMFEATURE", // 
        swSelectType_e.swSelANNOTATIONTABLES => "ANNOTATIONTABLES", // ITableAnnotation
        swSelectType_e.swSelBLOCKDEF => "BLOCKDEF", // 
        swSelectType_e.swSelCENTERMARKSYMS => "CENTERMARKSYMS", // 
        swSelectType_e.swSelSIMULATION => "SIMULATION", // 
        swSelectType_e.swSelSIMELEMENT => "SIMULATION_ELEMENT", // 
        swSelectType_e.swSelCENTERLINES => "CENTERLINE", // 
        swSelectType_e.swSelHOLETABLEFEATS => "HOLETABLEIHoleTable", // 
        swSelectType_e.swSelHOLETABLEAXES => "HOLETABLEAXIS", // 
        swSelectType_e.swSelWELDMENT => "WELDMENT", // 
        swSelectType_e.swSelSUBWELDFOLDER => "SUBWELDMENT", // 
        swSelectType_e.swSelREVISIONTABLE => "REVISIONTABLE", // 
        swSelectType_e.swSelSUBSKETCHINST => "SUBSKETCHINST", // ISketchBlockInstance
        swSelectType_e.swSelWELDMENTTABLEFEATS => "WELDMENTTABLE", // IWeldmentCutListFeature
        swSelectType_e.swSelBODYFOLDER => "BDYFOLDERIBodyFolder", // 
        swSelectType_e.swSelREVISIONTABLEFEAT => "REVISIONTABLEFEAT", // 
        swSelectType_e.swSelWELDBEADS => "WELDBEADS", // 
        swSelectType_e.swSelEMBEDLINKDOC => "EMBEDLINKDOC", // 
        swSelectType_e.swSelJOURNAL => "JOURNAL", // 
        swSelectType_e.swSelDOCSFOLDER => "DOCSFOLDER", // 
        swSelectType_e.swSelCOMMENTSFOLDER => "COMMENTSFOLDERICommentFolder", // 
        swSelectType_e.swSelCOMMENT => "COMMENT", // IComment
        swSelectType_e.swSelCAMERAS => "CAMERAS", // Feature
        swSelectType_e.swSelMATESUPPLEMENT => "MATESUPPLEMENT", // IMateLoadReference
        swSelectType_e.swSelANNOTATIONVIEW => "ANNVIEW", // 
        swSelectType_e.swSelGENERALTABLEFEAT => "GENERALTABLEFEAT", // 
        swSelectType_e.swSelSUBSKETCHDEF => "SUBSKETCHDEFISketchBlockDefinition", //
        _ => throw new NotImplementedException($"Unsupported value: {x}")
    };

    /// <summary>
    /// In order to center a sketch element in a sheet, you must translate the center point of the sheet
    /// in sketch space (as opposed to the sheet space).
    /// In other words, the sketch might be scaled within the view, & you must apply the scale to the center point.
    /// </summary>
    /// <see cref="https://cadbooster.com/complete-overview-of-matrix-transformations-in-the-solidworks-api/"/>
    /// <param name="sheetView"></param>
    /// <param name="sketchPicture"></param>
    public static void CenterPicture(this IView sheetView, MathUtility math, ISketchPicture sketchPicture) {
        var modelToSketch = sheetView.IGetSketch().ModelToSketchTransform;
        var viewCenter = sheetView.GetViewCenter();
        var viewCenterPoint = (MathPoint)math.CreatePoint(new double[] {
            viewCenter.X,
            viewCenter.Y,
            0
        });
        var sketchCenterPoint = viewCenterPoint.IMultiplyTransform(modelToSketch);
        var sketchCenterPointData = (double[])sketchCenterPoint.ArrayData;
        var sketchCenter = (X: sketchCenterPointData[0], Y: sketchCenterPointData[1]);

        var picWidth = 0.0;
        var picHeight = 0.0;
        sketchPicture.GetSize(ref picWidth, ref picHeight);

        var x = sketchCenter.X - (picWidth / 2.0);
        var y = sketchCenter.Y - (picHeight / 2.0);
        sketchPicture.SetOrigin(x, y);
    }

    // Note: Position isn't relative to the sheet space.
    // Use SetXform instead.
    //public static void CenterView(this IView sheetView, IView subView) {
    //    var viewCenter = sheetView.GetViewCenter();
    //    subView.SetPosition(viewCenter.X, viewCenter.Y);
    //}

    public static void CenterView(this IView sheetView, IView subView) {
        var viewCenter = sheetView.GetViewCenter();
        var coordinates = (double[])subView.GetXform();
        coordinates[0] = viewCenter.X;
        coordinates[1] = viewCenter.Y;
        // coordinates[2] = 1; // Scale
        subView.SetXform(coordinates);
    }

    public static void ConvertViewModelToSketchAndScale(this IModelDoc2 doc, ISheet sheet, IView subView) {
        var modelName = subView.GetReferencedModelName();
        var drawing = doc.As<IDrawingDoc>();
        var sheetName = sheet.GetName();
        var sheetView = drawing.GetViewBySheetName(sheetName);
        var subViewName = subView.GetName2();
        drawing.ActivateSheet(sheetName);
        drawing.ActivateView(subViewName);
        doc.Extension.SelectView(subView);
        doc.Extension.SelectView(subView);
        var bRet = subView.ReplaceViewWithSketch();
        doc.EditRebuild3();
        doc.GraphicsRedraw2();
        var newView = drawing.GetViewsBySheet().First(x => x.sheetView.GetName2() == sheetName).InnerViews.First();
        var newViewName = newView.GetName2();
        ScaleViewSketchInSheetToMaxSize(doc, sheetView, newView);
        doc.ClearSelection2(true);
        doc.Extension.SelectView(newView);
        doc.ForceRebuild3(true);
        doc.GraphicsRedraw2();
        doc.GraphicsRedraw();
    }

    public static void ScaleViewInSheetToMaxSize(this IModelDoc2 doc, ISheet sheet, IView subView) {
        var modelName = subView.GetReferencedModelName();
        var drawing = doc.As<IDrawingDoc>();
        var sheetName = sheet.GetName();
        var sheetView = drawing.GetViewBySheetName(sheetName);
        var isViewModel = !string.IsNullOrWhiteSpace(modelName);
        if (isViewModel) {
            // About the triple call: a bad hack since I couldn't figure out
            // why a single call won't scale the view all the way,
            // but when I call it again twice, then the view is scaled mostly all the way.
            ScaleViewModelInSheetToMaxSize(doc, sheetView, subView);
        }
        else {
            // About the triple call: a bad hack since I couldn't figure out
            // why a single call won't scale the view all the way,
            // but when I call it again twice, then the view is scaled mostly all the way.
            ScaleViewSketchInSheetToMaxSize(doc, sheetView, subView);
        }
    }

    public static void ScaleViewSketchInSheetToMaxSize(this IModelDoc2 doc, IView sheetView, IView subView) {
        doc.ClearSelection2(true);
        var scale = CalculateScaleBetweenViews(sheetView, subView);
        var subViewName = subView.GetName2();
        doc.Extension.SelectView(subViewName);
        subView.FocusLocked = true;
        doc.Extension.SelectAll();
        doc.Extension.ScaleOrCopy(
            Copy: false,
            NumCopies: 1,
            0,
            0,
            0,
            scale
        );
        doc.EditRebuild3();
        doc.GraphicsRedraw2();
        doc.ClearSelection2(true);
        doc.Extension.SelectView(subViewName);
        subView = (IView)doc.ISelectionManager.GetSelectedObject6(1, -1);
        subView.FocusLocked = false;
    }

    public static void ScaleViewModelInSheetToMaxSize(this IModelDoc2 doc, IView sheetView, IView subView) {
        doc.ClearSelection2(true);
        var scale = CalculateScaleBetweenViews(sheetView, subView);
        subView.ScaleDecimal = subView.ScaleDecimal * scale;
        doc.EditRebuild3();
    }

    public static double CalculateScaleBetweenViews(IView viewToFitWithin, IView viewToScale) {
        if (viewToFitWithin is null) throw new ArgumentNullException(nameof(viewToFitWithin));
        if (viewToScale is null) throw new ArgumentNullException(nameof(viewToScale));

        var outlineLimit = (double[])viewToFitWithin.GetOutline();
        var outlineToScale = (double[])viewToScale.GetOutline();

        var widthLimit = Math.Abs(outlineLimit[2] - outlineLimit[0]);
        var heightLimit = Math.Abs(outlineLimit[3] - outlineLimit[1]);

        var widthToScale = Math.Abs(outlineToScale[2] - outlineToScale[0]);
        var heightToScale = Math.Abs(outlineToScale[3] - outlineToScale[1]);

        if (widthToScale == 0 || heightToScale == 0) {
            throw new InvalidOperationException("Error: Second view has zero dimensions");
        }
        var scaleX = widthLimit / widthToScale;
        var scaleY = heightLimit / heightToScale;

        var scale = Math.Min(scaleX, scaleY);
        return scale;
    }

    /// <summary>
    /// Save as JPEG
    /// </summary>
    /// <param name="jpgCompressionLevel">From 1 to 100</param>
    /// <see cref="https://help.solidworks.com/2021/English/api/swconst/FileSaveAsTIFPSDOptions.htm"/>
    public static (bool Success, IEnumerable<swFileSaveError_e> Errors) SaveAsJpg(
        this ISldWorks app,
        string filePath,
        AllOrCurrentSheet includeSheet,
        swTiffImageType_e imageType,
        ScreenOrPrintCapture captureType,
        SheetOrPrintSize size,
        int jpgCompressionLevel = 1,
        int dpi = 0,
        swDwgPaperSizes_e paperSize = swDwgPaperSizes_e.swDwgPaperA0size,
        double paperWidth = 0,
        double paperHeight = 0,
        bool scaleToFit = false,
        int scaleFactor = 0
    ) {
        var swModel = app.IActiveDoc2;
        var errors = 0;
        var warnings = 0;
        app.SetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swExportJpegCompression, jpgCompressionLevel);
        app.SetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swTiffImageType, (int)imageType);
        app.SetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swTiffScreenOrPrintCapture, (int)captureType);
        app.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swTiffPrintAllSheets, includeSheet == AllOrCurrentSheet.AllSheets);
        app.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swTiffPrintUseSheetSize, size == SheetOrPrintSize.SheetSize);
        if (captureType == ScreenOrPrintCapture.PrintCapture) {
            app.SetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swTiffPrintDPI, dpi);
            app.SetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swTiffPrintPaperSize, (int)paperSize);
            app.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swTiffPrintScaleToFit, scaleToFit);
            app.SetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swTiffPrintScaleFactor, scaleFactor);
            if (size == SheetOrPrintSize.PrintSize) { 
                app.SetUserPreferenceDoubleValue((int)swUserPreferenceDoubleValue_e.swTiffPrintDrawingPaperWidth, paperWidth);
                app.SetUserPreferenceDoubleValue((int)swUserPreferenceDoubleValue_e.swTiffPrintDrawingPaperHeight, paperHeight);
            }
        }
        var isSaved = swModel.Extension.SaveAs(
            filePath,
            (int)swSaveAsVersion_e.swSaveAsCurrentVersion,
            (int)swSaveAsOptions_e.swSaveAsOptions_Silent,
            null,
            ref errors,
            ref warnings
        );
        return (isSaved, errors.SplitFlags<swFileSaveError_e>());
    }

    public static (bool Success, IEnumerable<swFileSaveError_e> Errors) SaveAsPdf(
        this ISldWorks app, 
        string filePath,
        bool exportHighQuality = true,
        bool exportInColor = true
    ) {
        var swExpPdfData = (ExportPdfData)app.GetExportFileData((int)swExportDataFileType_e.swExportPdfData);
        swExpPdfData.ExportAs3D = false;
        swExpPdfData.ViewPdfAfterSaving = false;

        app.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swPDFExportHighQuality, exportHighQuality);
        app.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swPDFExportInColor, exportInColor);

        var swModel = app.IActiveDoc2;

        var errors = 0;
        var warnings = 0;
        var isSaved = swModel.Extension.SaveAs(
            filePath,
            (int)swSaveAsVersion_e.swSaveAsCurrentVersion,
            (int)swSaveAsOptions_e.swSaveAsOptions_Silent,
            swExpPdfData,
            ref errors,
            ref warnings
        );
        return (isSaved, errors.SplitFlags<swFileSaveError_e>());
    }

    public static (bool Success, IEnumerable<swFileSaveError_e> Errors) SaveSheetAsPdf(
        this ISldWorks app, 
        IView sheetView, 
        string filePath,
        bool exportHighQuality = true,
        bool exportInColor = true
    ) => SaveSheetAsPdf(
        app, 
        sheetView.GetName2(), 
        filePath,
        exportHighQuality,
        exportInColor
    );
    
    public static (bool Success, IEnumerable<swFileSaveError_e> Errors) SaveSheetAsPdf(
        this ISldWorks app,
        string sheetName, 
        string filePath,
        bool exportHighQuality = true,
        bool exportInColor = true
    ) {
        app.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swPDFExportHighQuality, exportHighQuality);
        app.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swPDFExportInColor, exportInColor);

        var swModel = app.IActiveDoc2;

        var swExpPdfData = (ExportPdfData)app.GetExportFileData((int)swExportDataFileType_e.swExportPdfData);
        swExpPdfData.ExportAs3D = false;
        swExpPdfData.ViewPdfAfterSaving = false;

        var expSheets = new string[] { sheetName };
        swExpPdfData.SetSheets((int)swExportDataSheetsToExport_e.swExportData_ExportSpecifiedSheets, expSheets);

        var errors = 0;
        var warnings = 0;
        var isSaved = swModel.Extension.SaveAs(
            filePath,
            (int)swSaveAsVersion_e.swSaveAsCurrentVersion,
            (int)swSaveAsOptions_e.swSaveAsOptions_Silent,
            swExpPdfData,
            ref errors,
            ref warnings
        );
        return (isSaved, errors.SplitFlags<swFileSaveError_e>());
    }

    public static (bool Success, IEnumerable<swFileLoadError_e> Errors) OpenDocument(this ISldWorks app, string filePath, swDocumentTypes_e docType) {
        var errors = 0;
        var warnings = 0;
        var model = app.OpenDoc6(
            filePath,
            (int)docType,
            (int)(
                swOpenDocOptions_e.swOpenDocOptions_Silent 
                | swOpenDocOptions_e.swOpenDocOptions_ReadOnly 
                | swOpenDocOptions_e.swOpenDocOptions_OverrideDefaultLoadLightweight
                | swOpenDocOptions_e.swOpenDocOptions_LoadLightweight
                | swOpenDocOptions_e.swOpenDocOptions_DontLoadHiddenComponents
            ),
            "",
            ref errors,
            ref warnings
        );
        // ---
        //var spec = (DocumentSpecification)app.GetOpenDocSpec(filePath);
        ////spec.LoadExternalReferencesInMemory = false;
        //spec.ViewOnly = false; // Causes my PC to bug out (requiring a machine reboot). GPU?
        //spec.ReadOnly = true;
        //spec.UseLightWeightDefault = false;
        //spec.LightWeight = true;
        //spec.Silent = true;
        //spec.DetailingMode = false;
        //spec.IgnoreHiddenComponents = true;
        //var model = app.OpenDoc7(spec);
        //var errors = spec.Error;
        //var warnings = spec.Warning;
        // --
        var warningFlags = warnings.SplitFlags<swFileLoadWarning_e>();
        return (Success: model is not null, errors.SplitFlags<swFileLoadError_e>());
    }

    public static void ShowAnnotations(this ISldWorks app) => SetDisplayAnnotations(app, true);
    public static void HideAnnotations(this ISldWorks app) => SetDisplayAnnotations(app, false);
    public static void SetDisplayAnnotations(this ISldWorks app, bool displayed) {
        app.IActiveDoc2.Extension.SetUserPreferenceToggle(
            (int)swUserPreferenceToggle_e.swDisplayAllAnnotations,
            (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified,
            displayed
        );
    }

    public static void ShowComponentAnnotations(this ISldWorks app) => SetDisplayComponentAnnotations(app, true);
    public static void HideComponentAnnotations(this ISldWorks app) => SetDisplayComponentAnnotations(app, false);
    public static void SetDisplayComponentAnnotations(this ISldWorks app, bool displayed) {
        app.IActiveDoc2.Extension.SetUserPreferenceToggle(
            (int)swUserPreferenceToggle_e.swDisplayCompAnnotations,
            (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified,
            displayed
        );
    }

    public static IEnumerable<IAnnotation> GetAnnotationsEx(this IView view) {
        var annotations = (object[])view.GetAnnotations();
        if (annotations is not null) {
            return annotations.Cast<IAnnotation>();
        }
        return Enumerable.Empty<IAnnotation>();
    }

    public static IEnumerable<IAnnotation> GetBalloonAnnotations(this IView view) {
        var results = new List<IAnnotation>();
        foreach (var annotation in view.GetAnnotationsEx()) {
            var annotationType = annotation.GetType().CastTo<swAnnotationType_e>();
            switch (annotationType) {
                case swAnnotationType_e.swNote:
                    var note = (INote)annotation.GetSpecificAnnotation();
                    if (note.HasBalloon()) {
                        results.Add(annotation);
                    }
                    break;
            }
        }
        return results;
    }

    public static void DeleteBalloonAnnotations(this IView view, IModelDoc2 model) {
        model.ISelectionManager.SuspendSelectionList();
        foreach (var annotation in view.GetBalloonAnnotations()) {
            annotation.Select3(true, null);
        }
        model.Extension.DeleteSelection2(0);
        model.ISelectionManager.ResumeSelectionList();
    }

    public static void DeleteAnnotations(this IView view, IModelDoc2 model) {
        model.ISelectionManager.SuspendSelectionList();
        foreach (var annotation in view.GetAnnotationsEx()) {
            annotation.Select3(true, null);
        }
        model.Extension.DeleteSelection2(0);
        model.ISelectionManager.ResumeSelectionList();
    }

    public static IEnumerable<SwOLEObject> GetOleObjectsOnCurrentSheet(this ModelDocExtension modelExt) => GetOleObjectsEx(modelExt, swOleObjectOptions_e.swOleObjectOptions_GetOnCurrentSheet);
    public static IEnumerable<SwOLEObject> GetOleObjectsAll(this ModelDocExtension modelExt) => GetOleObjectsEx(modelExt, swOleObjectOptions_e.swOleObjectOptions_GetAll);
    public static IEnumerable<SwOLEObject> GetOleObjectsEx(this ModelDocExtension modelExt, swOleObjectOptions_e option = swOleObjectOptions_e.swOleObjectOptions_GetAll) {
        var objectsOnSheet = (object[])modelExt.GetOLEObjects((int)option);
        if (objectsOnSheet is null) {
            yield break;
        }
        foreach (SwOLEObject obj in objectsOnSheet) {
            yield return obj;
        }
    }

    public static void CenterOleObject(this IView sheetView, SwOLEObject obj) {
        var viewCenter = sheetView.GetViewCenter();
        var objBounds = (double[])obj.Boundaries;

        var x1 = objBounds[0];
        var y1 = objBounds[1];

        var x2 = objBounds[3];
        var y2 = objBounds[4];

        var width = Math.Abs(x2 - x1);
        var height = Math.Abs(y2 - y1);

        objBounds[0] = viewCenter.X - width/2;
        objBounds[1] = viewCenter.Y + height/2;

        objBounds[3] = viewCenter.X + width / 2;
        objBounds[4] = viewCenter.Y - height / 2;

        obj.Boundaries = objBounds;
    }

    public static void ScaleOleObject(this IView sheetView, SwOLEObject obj) {
        var viewCenter = sheetView.GetViewCenter();
        var outlineLimit = (double[])sheetView.GetOutline();
        var widthLimit = Math.Abs(outlineLimit[2] - outlineLimit[0]);
        var heightLimit = Math.Abs(outlineLimit[3] - outlineLimit[1]);

        var objBounds = (double[])obj.Boundaries;
        var x1 = objBounds[0];
        var y1 = objBounds[1];
        var x2 = objBounds[3];
        var y2 = objBounds[4];
        var width = Math.Abs(x2 - x1);
        var height = Math.Abs(y2 - y1);

        var scaleX = widthLimit / width;
        var scaleY = heightLimit / height;
        var scale = Math.Min(scaleX, scaleY);

        objBounds[0] *= scale;
        objBounds[1] *= scale;
        objBounds[3] *= scale;
        objBounds[4] *= scale;

        obj.Boundaries = objBounds;
    }
}


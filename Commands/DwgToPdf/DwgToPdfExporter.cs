using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using Dubeg.Sw.ExportTools.Utils;
using Serilog;
using Xarial.XCad.SolidWorks.Sketch;

namespace Dubeg.Sw.ExportTools.Commands.DwgToPdf;

public class DwgToPdfExporter {
    public enum ExportSelectMode { 
        BoxSelect,
        OneByOne
    }

    private ISldWorks _app;

    public ExportSelectMode SelectMode { get; set; } = ExportSelectMode.BoxSelect;
    public string NewSheetName { get; set; } = "NEW_ISO";
    /// <summary>
    /// Applied only if SelectMode is OneByOne.
    /// </summary>
    public bool RemoveSegmentsFromOverlappingViews { get; set; } = false;
    public bool FilterOnDominantLayer { get; set; } = false;
    public List<string> IgnoredLayers { get; set; } = new List<string>();
    public string OutputFolderPath { get; set; } = string.Empty;
    public string InputFilePath { get; internal set; }

    public DwgToPdfExporter(ISldWorks app) => _app = app;

    public void ImportDwg(string filePath, string sheetName) {
        var importData = (ImportDxfDwgData)_app.GetImportFileData(filePath);
        importData.ImportMethod[""] = (int)swImportDxfDwg_ImportMethod_e.swImportDxfDwg_DoNotImportSheet;
        importData.ImportMethod[sheetName] = (int)swImportDxfDwg_ImportMethod_e.swImportDxfDwg_ImportToDrawing;
        var errors = 0;
        var newDoc = _app.LoadFile4(filePath, "", importData, ref errors);
        InputFilePath = filePath;
        var fileImportErrors = errors.SplitFlags<swFileLoadError_e>();
        if (fileImportErrors.Any()) {
            var fileName = Path.GetFileName(filePath);
            throw new DwgToPdfImportException($"Errors during import of '{fileName}': {string.Join(", ", fileImportErrors.Select(x => x))}") { 
                FilePath = filePath,
                FileImportErrors = fileImportErrors 
            };
        }
    }

    public string Export() {
        var swModel = _app.IActiveDoc2;
        var swDraw = (DrawingDoc)swModel;
        var newSheetName = NewSheetName;
        var outFilePath = GetOutputFilePath();
        Copy();
        CenterAndScale();
        SaveSheetAsPdf(newSheetName, outFilePath);
        return outFilePath;
    }

    public void CenterAndScale() {
        if (_app.IActiveDoc2 is null) {
            throw new DwgToPdfExportException("Aucun dessin n'est ouvert.");
        }
        var swModel = _app.IActiveDoc2;
        var swDraw = (DrawingDoc)swModel;
        var newSheetName = NewSheetName;
        
        swModel.Extension.Rebuild(
            (int)swRebuildOptions_e.swRebuildAll
        );

        var sheet = swDraw.GetSheet(newSheetName);
        var sheetView = swDraw.GetViewBySheetName(newSheetName);
        var subView = swDraw.GetViewsForSheet(sheet).FirstOrDefault();

        swDraw.ActivateSheet(sheet.GetName());
        swModel.Extension.SelectByID2(newSheetName, "SHEET", 0, 0, 0, false, 0, null, 0);

        swModel.ScaleViewInSheetToMaxSize(sheet, subView);
        sheetView.CenterView(subView);

        swModel.EditRebuild3();
        swModel.GraphicsRedraw2();
    }

    public void Copy() {
        if (_app.IActiveDoc2 is null) {
            throw new DwgToPdfExportException("Aucun dessin n'est ouvert.");
        }
        var swModel = _app.IActiveDoc2;
        var swDraw = (DrawingDoc)swModel;
        var newSheetName = NewSheetName;

        swModel.Extension.Rebuild(
            (int)swRebuildOptions_e.swRebuildAll
        );

        // Delete the "new" sheet if it exists.
        // This is for ease of debugging (ie. re-running the macro multiple times).
        swModel.DeleteSheet(newSheetName);

        var isSketchSelected = false;
        IView view = null;
        var selCount = swModel.ISelectionManager.GetSelectedObjectCount2(-1);
        if (selCount > 0) {
            var selectedType = (swSelectType_e)swModel.ISelectionManager.GetSelectedObjectType3(1, -1);
            if (selectedType == swSelectType_e.swSelDRAWINGVIEWS) {
                var selectedObject = swModel.ISelectionManager.GetSelectedObject6(1, -1);
                view = (IView)selectedObject;
            }
            else if (selectedType
                is swSelectType_e.swSelSKETCHES
                or swSelectType_e.swSelSKETCHSEGS
                or swSelectType_e.swSelSKETCHPOINTS
                or swSelectType_e.swSelEXTSKETCHSEGS
                or swSelectType_e.swSelEXTSKETCHPOINTS
                or swSelectType_e.swSelSKETCHTEXT
                or swSelectType_e.swSelSKETCHHATCH
                or swSelectType_e.swSelSKETCHPOINTFEAT
                or swSelectType_e.swSelSKETCHBITMAP
                or swSelectType_e.swSelEXTSKETCHTEXT
                or swSelectType_e.swSelSKETCHREGION
                or swSelectType_e.swSelSKETCHCONTOUR
                or swSelectType_e.swSelSUBSKETCHINST
                or swSelectType_e.swSelSUBSKETCHDEF
            ) {
                isSketchSelected = true;
            }
        }
        if (isSketchSelected) {
            swModel.EditCopy();
            var sourceSheet = swDraw.IGetCurrentSheet();
            var sourceSheetProps = (double[])sourceSheet.GetProperties2();
            var sourcePaperSize = (swDwgPaperSizes_e)sourceSheetProps[0];
            var sourceTemplate = (swDwgTemplates_e)sourceSheetProps[1];
            var sourceScale1 = sourceSheetProps[2];
            var sourceScale2 = sourceSheetProps[3];
            var sourceFirstAngle = sourceSheetProps[4];
            var sourceWidth = sourceSheetProps[5];
            var sourceHeight = sourceSheetProps[6];
            swDraw.NewSheet4(
                newSheetName,
                (int)sourcePaperSize,
                (int)sourceTemplate,
                (int)sourceScale1,
                (int)sourceScale2,
                false, "", 0, 0, "", 0, 0, 0, 0, 1, 1
            );
            swDraw.ActivateSheet(newSheetName);
            var newView = swDraw.CreateViewport3(0, 0, 0, Scale: 1);
            swDraw.ActivateSheet(newView.Sheet.GetName());
            swModel.Extension.SelectByID2(newView.GetName2(), "VIEW", 0, 0, 0, false, 0, null, 0);
            swModel.Paste();
            swModel.EditRebuild3();
            swModel.ClearSelection2(true);
        }
        else {
            // If no view is selected, try to find it using the label "VUEISO".
            // It usually is there, but not always.
            if (view is null) {
                var coordinates = GetCoordinatesOfLabelVueIsometrique();
                if (coordinates != null && coordinates.Length > 0) {
                    var labelX = coordinates[0];
                    var labelY = coordinates[1];
                    view = FindDrawingViewContainingPointOrNearest(labelX, labelY);
                }
            }

            if (view is null) {
                throw new DwgToPdfExportException("Sélectionnez une vue et réessayez la macro.");
            }

            switch (SelectMode) {
                case ExportSelectMode.OneByOne:
                    CopySketchEntitiesWithinBoundary(
                        view,
                        newSheetName,
                        checkOverlaps: RemoveSegmentsFromOverlappingViews,
                        filterOnDominantLayer: FilterOnDominantLayer
                    );
                    break;
                case ExportSelectMode.BoxSelect:
                    CopySketchEntitiesWithinBoundarySimple(
                        view,
                        newSheetName,
                        filterAfterSelect: false,
                        filterOnDominantLayer: FilterOnDominantLayer
                    );
                    break;
            }
        }
    }

    public void SaveAsPdf() {
        if (_app.IActiveDoc2 is null) {
            throw new DwgToPdfExportException("Aucun dessin n'est ouvert.");
        }
        var swModel = _app.IActiveDoc2;
        swModel.Extension.Rebuild(
            (int)swRebuildOptions_e.swRebuildAll
        );
        var outFilePath = GetOutputFilePath();
        SaveSheetAsPdf(NewSheetName, outFilePath);
        swModel.ClearSelection2(true);
    }

    // --

    private double[] GetCoordinatesOfLabelVueIsometrique() {
        var swModel = _app.IActiveDoc2;
        var swDraw = (DrawingDoc)swModel;
        var swSheet = swDraw.IGetCurrentSheet();
        var sketchManager = swModel.SketchManager;
        var vBlockDef = (object[])sketchManager.GetSketchBlockDefinitions();
        if (vBlockDef != null) {
            for (var i = 0; i < vBlockDef.Length; i++) {
                var swBlockDef = (SketchBlockDefinition)vBlockDef[i];
                var vBlockInst = (object[])swBlockDef.GetInstances();
                if (vBlockInst != null) {
                    for (var j = 0; j < vBlockInst.Length; j++) {
                        var swBlockInst = (SketchBlockInstance)vBlockInst[j];
                        if (swBlockInst.Name.Contains("VUEISO")) {
                            return (double[])swBlockInst.InstancePosition.ArrayData;
                        }
                    }
                }
            }
        }
        return null;
    }

    private IView FindDrawingViewContainingPointOrNearest(double pointX, double pointY) {
        var swDraw = (DrawingDoc)_app.ActiveDoc;
        var swSheet = swDraw.IGetCurrentSheet();
        var views = (object[])swSheet.GetViews();
        if (views == null || views.Length == 0) {
            return null;
        }

        var viewsInBounds = new List<(IView View, double Size)>();
        foreach (IView swView in views) {
            var outline = (double[])swView.GetOutline();
            var x = outline[0];
            var y = outline[1];
            var x2 = outline[2];
            var y2 = outline[3];
            var width = x2 - x;
            var height = y2 - y;
            var size = (width * height);
            if (pointX >= x && pointX <= x2 && pointY >= y && pointY <= y2) {
                viewsInBounds.Add((swView, size));
            }
        }

        if (viewsInBounds.Count > 0) {
            return viewsInBounds
                .OrderBy(x => x.Size) // TODO: should I really order by?
                .Select(x => x.View)
                .FirstOrDefault();
        }

        // If no view is found, return the nearest view.
        double minDistance = 1000000;
        IView nearestView = null;
        foreach (IView swView in views) {
            var position = (double[])swView.Position;
            var x = position[0];
            var y = position[1];
            var distance = Math.Sqrt(Math.Pow(pointX - x, 2) + Math.Pow(pointY - y, 2));
            if (distance < minDistance) {
                minDistance = distance;
                nearestView = swView;
            }
        }
        return nearestView;
    }

    private void SaveSheetAsPdf(string sheetName, string filePath) {
        var swModel = _app.IActiveDoc2;

        var swExpPdfData = (ExportPdfData)_app.GetExportFileData((int)swExportDataFileType_e.swExportPdfData);
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
        if (!isSaved) {
            var fileName = Path.GetFileName(filePath);
            var fileSaveErrors = errors.SplitFlags<swFileSaveError_e>();
            throw new DwgToPdfExportException($"Failed to export PDF to '{fileName}': {string.Join(", ", fileSaveErrors)}") {
                FileName = filePath,
                FileSaveErrors = fileSaveErrors
            };
        }
    }

    private double CalculateScaleBetweenViews(IView viewToFitWithin, IView viewToScale) {
        if (viewToFitWithin is null) throw new ArgumentNullException(nameof(viewToFitWithin));
        if (viewToScale is null) throw new ArgumentNullException(nameof(viewToScale));

        var outlineLimit = (double[])viewToFitWithin.GetOutline();
        var outlineToScale = (double[])viewToScale.GetOutline();

        var widthLimit = Math.Abs(outlineLimit[2] - outlineLimit[0]);
        var heightLimit = Math.Abs(outlineLimit[3] - outlineLimit[1]);

        var widthToScale = Math.Abs(outlineToScale[2] - outlineToScale[0]);
        var heightToScale = Math.Abs(outlineToScale[3] - outlineToScale[1]);

        if (widthToScale == 0 || heightToScale == 0) {
            throw new DwgToPdfExportException("Error: Second view has zero dimensions");
        }
        var scaleX = widthLimit / widthToScale;
        var scaleY = heightLimit / heightToScale;

        var scale = Math.Min(scaleX, scaleY);
        return scale;
    }

    private string GetPartNumberFromBlock(ModelDoc2 model, string blockName) {
        var swDraw = (DrawingDoc)model;
        var swSheet = swDraw.IGetCurrentSheet();
        var sketchManager = model.SketchManager;
        var regex = new Regex(@"\d{7,}", RegexOptions.IgnoreCase);
        var vBlockDefs = (object[])sketchManager.GetSketchBlockDefinitions();
        foreach (SketchBlockDefinition pBlock in vBlockDefs) {
            var instanceCount = pBlock.GetInstanceCount();
            if (instanceCount > 0) {
                var vInstances = (object[])pBlock.GetInstances();
                foreach (SketchBlockInstance pInstance in vInstances) {
                    if (pInstance.Name.Contains(blockName)) {
                        var attributeCount = pInstance.GetAttributeCount();
                        if (attributeCount > 0) {
                            var vAttributes = (object[])pInstance.GetAttributes();
                            foreach (Note pAttribute in vAttributes) {
                                var attrName = pAttribute.TagName;
                                var attrValue = pAttribute.GetText();
                                if (regex.IsMatch(attrValue)) {
                                    return attrValue;
                                }
                            }
                        }
                    }
                }
            }
        }
        return "";
    }

    private void CopySketchEntitiesWithinBoundarySimple(
        IView sourceView, 
        string newSheetName, 
        bool filterAfterSelect = true, 
        bool filterOnDominantLayer = false
    ) {
        var swModel = _app.IActiveDoc2;
        var swDraw = (DrawingDoc)swModel;
        var selMgr = swModel.ISelectionManager;
        var sourceViewName = sourceView.GetName2();

        var viewOutline = (double[])sourceView.GetOutline();
        var xMin = viewOutline[0];
        var yMin = viewOutline[1];
        var xMax = viewOutline[2];
        var yMax = viewOutline[3];
        var width = xMax - xMin;
        var height = yMax - yMin;

        swModel.ClearSelection2(true);

        var sourceSheet = sourceView.Sheet ?? swDraw.GetSheet(sourceView.GetName2());
        var sourceSheetProps = (double[])sourceSheet.GetProperties2();
        var sourcePaperSize = (swDwgPaperSizes_e)sourceSheetProps[0];
        var sourceTemplate = (swDwgTemplates_e)sourceSheetProps[1];
        var sourceScale1 = sourceSheetProps[2];
        var sourceScale2 = sourceSheetProps[3];
        var sourceFirstAngle = sourceSheetProps[4];
        var sourceWidth = sourceSheetProps[5];
        var sourceHeight = sourceSheetProps[6];

        swDraw.NewSheet4(
            newSheetName,
            (int)sourcePaperSize,
            (int)sourceTemplate,
            (int)sourceScale1, 
            (int)sourceScale2, 
            false, "", 0, 0, "", 0, 0, 0, 0, 1, 1
        );
        swDraw.ActivateSheet(newSheetName);
        var newView = swDraw.CreateViewport3(0, 0, 0, Scale: 1);

        swModel.ClearSelection2(true);

        void FilterAfterSelect() {
            var selCount = swModel.ISelectionManager.GetSelectedObjectCount2(-1);
            var selObjects = new List<(object Object, swSelectType_e Type)>(selCount);
            var sketchSegments = new List<ISketchSegment>(selCount);

            for (var i = 0; i < selCount; i++) {
                // API: 1 to selCount
                var selType = (swSelectType_e)selMgr.GetSelectedObjectType3(i + 1, -1);
                var selObject = selMgr.GetSelectedObject6(i + 1, -1);
                selObjects.Add((selObject, selType));
                if (selType == swSelectType_e.swSelSKETCHSEGS) {
                    sketchSegments.Add((ISketchSegment)selObject);
                }
            }

            var dominantLayer = GetDominantLayer(sketchSegments);
            foreach (var (obj, type) in selObjects) {
                switch (type) {
                    case swSelectType_e.swSelEXTSKETCHSEGS:
                    case swSelectType_e.swSelSKETCHSEGS:
                        var sketchSegment = (ISketchSegment)obj;
                        var segmentLayer = sketchSegment.Layer?.ToLower() ?? "";
                        if (IgnoredLayers.Any(x => segmentLayer.Contains(x.ToLower()))) {
                            sketchSegment.DeSelect();
                        }
                        else if (filterOnDominantLayer && sketchSegment.Layer != dominantLayer) {
                            sketchSegment.DeSelect();
                        }
                        break;
                    case swSelectType_e.swSelSKETCHHATCH:
                        var hatch = (ISketchHatch)obj;
                        hatch.DeSelect();
                        break;
                    case swSelectType_e.swSelSUBSKETCHINST:
                        var inst = (ISketchBlockInstance)obj;
                        selMgr.GetSelectByIdSpecification(
                           inst,
                           out var SelectByString,
                           out var ObjectType,
                           out var Type
                        );
                        swModel.DeSelectByID(SelectByString, ObjectType, 0, 0, 0);
                        break;
                    case swSelectType_e.swSelNOTES:
                        var note = (INote)obj;
                        note.IGetAnnotation().DeSelect();
                        break;
                    default: break;
                }
            }
        }

        swDraw.ActivateSheet(sourceView.Sheet.GetName());
        swModel.Extension.SelectByID2(sourceView.Sheet.GetName(), "SHEET", 0, 0, 0, false, 0, null, 0);
        sourceView.FocusLocked = true;
        swModel.Extension.SelectAll();
        sourceView.FocusLocked = false;
        if (filterAfterSelect) {
            FilterAfterSelect();
        }
        swModel.EditCopy();
        swDraw.ActivateSheet(newView.Sheet.GetName());
        swModel.Extension.SelectByID2(newView.Sheet.GetName(), "SHEET", 0, 0, 0, false, 0, null, 0);
        swModel.Extension.SelectByID2(newView.GetName2(), "VIEW", 0, 0, 0, false, 0, null, 0);
        swModel.Paste();
        
        swModel.ClearSelection2(true);
        swModel.EditRebuild3();

        swDraw.ActivateSheet(sourceView.Sheet.GetName());
        swModel.Extension.SelectByID2(sourceView.Sheet.GetName(), "SHEET", 0, 0, 0, false, 0, null, 0);
        sourceView.Sheet.FocusLocked = true;
        swModel.Extension.SketchBoxSelect(
            xMin, yMin, 0,  // First corner point (x,y,z)
            xMax, yMax, 0   // Second corner point (x,y,z)
        );
        sourceView.Sheet.FocusLocked = false;
        if (filterAfterSelect) {
            FilterAfterSelect();
        }
        swModel.EditCopy();
        swDraw.ActivateSheet(newView.Sheet.GetName());
        swModel.Extension.SelectByID2(newView.GetName2(), "VIEW", 0, 0, 0, false, 0, null, 0);
        swModel.Paste();
        swModel.EditRebuild3();
    }

    private void CopySketchEntitiesWithinBoundary(
        IView sourceView,
        string newSheetName,
        bool checkOverlaps = false,
        bool filterOnDominantLayer = false
    ) {
        var swModel = _app.IActiveDoc2;
        var swDraw = (DrawingDoc)swModel;
        var selMgr = swModel.ISelectionManager;

        var viewOutline = (double[])sourceView.GetOutline();
        var xMin = viewOutline[0];
        var yMin = viewOutline[1];
        var xMax = viewOutline[2];
        var yMax = viewOutline[3];

        bool IsPointInBoundary(double[] point, double xMin, double yMin, double xMax, double yMax) {
            return point[0] >= xMin && point[0] <= xMax && point[1] >= yMin && point[1] <= yMax;
        }

        var sourceSheet = sourceView.Sheet;
        var allViews = (object[])sourceSheet.GetViews();
        var otherViews = allViews.Where(v => v != sourceView).Cast<IView>().ToList();
        var otherViewBoundarys = otherViews.Select(v => (double[])v.GetOutline()).ToList();
        bool PointOverlapsOtherViews(double x, double y) {
            foreach (var otherViewBoundary in otherViewBoundarys) {
                if (x >= otherViewBoundary[0] && x <= otherViewBoundary[2] &&
                    y >= otherViewBoundary[1] && y <= otherViewBoundary[3]) {
                    return true;
                }
            }
            return false;
        }
        
        var segmentsToSelect = new List<DispatchWrapper>();
        List<string> viewNamesToSelect = [
            sourceSheet.GetName(),
            sourceView.GetName2()
        ];
        
        foreach (var viewName in viewNamesToSelect) {
            var view = swDraw.GetViewBySheetName(viewName);
            var sketch = view.IGetSketch();
            var vSketchSegments = (object[])sketch.GetSketchSegments();
            if (vSketchSegments is null) continue;
            var sketchSegments = vSketchSegments.Cast<ISketchSegment>().ToList();
            var dominantLayer = GetDominantLayer(sketchSegments); // Perhaps call this over all segments in the doc.
            foreach (var sketchSegment in sketchSegments) {
                var segmentLayer = sketchSegment.Layer?.ToLower() ?? "";
                if (filterOnDominantLayer && sketchSegment.Layer != dominantLayer) continue;
                if (IgnoredLayers.Any(x => segmentLayer.Contains(x.ToLower()))) continue;
                if (viewName == sourceView.GetName2()) {
                    segmentsToSelect.Add(new DispatchWrapper(sketchSegment));
                    continue;
                }
                var shouldSelect = false;
                switch (sketchSegment.GetType()) {
                    case (int)swSketchSegments_e.swSketchLINE:
                        var line = (ISketchLine)sketchSegment;
                        var startPoint = (double[])line.GetStartPoint();
                        var endPoint = (double[])line.GetEndPoint();
                        shouldSelect = (
                            IsPointInBoundary(startPoint, xMin, yMin, xMax, yMax)
                            || IsPointInBoundary(endPoint, xMin, yMin, xMax, yMax)
                            )
                            && (
                                !checkOverlaps || (
                                    !PointOverlapsOtherViews(startPoint[0], startPoint[1])
                                    && !PointOverlapsOtherViews(endPoint[0], endPoint[1])
                                )
                            );
                        break;

                    case (int)swSketchSegments_e.swSketchARC:
                        var arc = (ISketchArc)sketchSegment;
                        var centerPoint = (double[])arc.GetCenterPoint();
                        var arcStart = (double[])arc.GetStartPoint();
                        var arcEnd = (double[])arc.GetEndPoint();
                        shouldSelect = (
                            IsPointInBoundary(centerPoint, xMin, yMin, xMax, yMax)
                            || IsPointInBoundary(arcStart, xMin, yMin, xMax, yMax)
                            || IsPointInBoundary(arcEnd, xMin, yMin, xMax, yMax)
                            )
                            && (
                                !checkOverlaps || (
                                    !PointOverlapsOtherViews(centerPoint[0], centerPoint[1])
                                    && !PointOverlapsOtherViews(arcStart[0], arcStart[1])
                                    && !PointOverlapsOtherViews(arcEnd[0], arcEnd[1])
                                )
                            );
                        break;

                    case (int)swSketchSegments_e.swSketchELLIPSE:
                        var ellipse = (SketchEllipse)sketchSegment;
                        var ellipseCenter = (double[])ellipse.GetCenterPoint();
                        var majorPoint = (double[])ellipse.GetMajorPoint();
                        var minorPoint = (double[])ellipse.GetMinorPoint();
                        shouldSelect = (
                            IsPointInBoundary(ellipseCenter, xMin, yMin, xMax, yMax)
                            || IsPointInBoundary(majorPoint, xMin, yMin, xMax, yMax)
                            || IsPointInBoundary(minorPoint, xMin, yMin, xMax, yMax)
                            )
                            && (
                                !checkOverlaps || (
                                    !PointOverlapsOtherViews(ellipseCenter[0], ellipseCenter[1])
                                    && !PointOverlapsOtherViews(majorPoint[0], majorPoint[1])
                                    && !PointOverlapsOtherViews(minorPoint[0], minorPoint[1])
                                )
                            );
                        break;
                }
                if (shouldSelect) {
                    segmentsToSelect.Add(new DispatchWrapper(sketchSegment));
                }
            }
        }
        if (segmentsToSelect.Count == 0) {
            throw new DwgToPdfExportException($"No sketch entities found within the boundary of the selected view");
        }
        
        swModel.ClearSelection2(true);
        swModel.Extension.MultiSelect2(segmentsToSelect.ToArray(), false, null);
        
        swModel.EditCopy();
        swModel.EditRebuild3();

        var bRet = swDraw.NewSheet4(
            newSheetName,
            (int)swDwgPaperSizes_e.swDwgPaperAsize,
            (int)swDwgTemplates_e.swDwgTemplateNone,
            1, 1, false, "", 0, 0, "", 0.5, 0.5, 0.5, 0.5, 2, 2
        );
        swDraw.ActivateSheet(newSheetName);
        var newView = swDraw.CreateViewport3(0, 0, 0, Scale: 1);
        swDraw.ActivateView(newView.Name);

        bRet = swModel.Extension.SelectByID2(newView.GetName2(), "VIEW", 0, 0, 0, false, 0, null, 0);
        swDraw.ActivateView(newView.GetName2());
        swModel.Paste();
        swModel.EditRebuild3();
        swModel.ClearSelection2(false);
    }

    private string GetDominantLayer(IEnumerable<ISketchSegment> segments) {
        var layerCounts = new Dictionary<string, int>();
        var maxCount = 0;
        var dominantLayer = "";
        foreach (var swSketchSeg in segments) {
            var segmentLayer = swSketchSeg.Layer;
            if (layerCounts.ContainsKey(segmentLayer)) {
                layerCounts[segmentLayer]++;
            }
            else {
                layerCounts[segmentLayer] = 1;
            }
            if (layerCounts[segmentLayer] > maxCount) {
                maxCount = layerCounts[segmentLayer];
                dominantLayer = segmentLayer;
            }
        }
        return dominantLayer;
    }

    private string GetOutputFilePath() {
        if (_app.IActiveDoc2 is null) {
            throw new DwgToPdfExportException("No drawing opened");
        }
        var swModel = _app.IActiveDoc2;
        var outFolderPath = OutputFolderPath;
        if (string.IsNullOrWhiteSpace(outFolderPath)) {
            throw new ArgumentNullException($"{nameof(OutputFolderPath)} is null or empty.");
        }
        var outFileName = "";
        if (!string.IsNullOrWhiteSpace(InputFilePath)) {
            outFileName = Path.GetFileNameWithoutExtension(InputFilePath);
        }
        if (string.IsNullOrWhiteSpace(outFileName)) {
            outFileName = GetPartNumberFromBlock(swModel, "CART");
        }   
        if (string.IsNullOrEmpty(outFileName)) {
            outFileName = "Exported";
        }
        return System.IO.Path.Combine(outFolderPath, outFileName + ".pdf");
    }
}
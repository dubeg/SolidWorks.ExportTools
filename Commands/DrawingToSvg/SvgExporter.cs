using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using Dubeg.Sw.ExportTools.Utils;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace Dubeg.Sw.ExportTools.Commands.DrawingToSvg;

public partial class SvgExporter(ISldWorks _app) {

    public void Export(string filePath) {
        double MeterToMM(double value) => value * 1000.0;
        // --
        var model = _app.IActiveDoc2;
        if (model == null || model.GetType() != (int)swDocumentTypes_e.swDocDRAWING) {
            throw new InvalidOperationException("Active document is not a drawing.");
        }
        var drawing = (DrawingDoc)model;
        var sheet = drawing.IGetCurrentSheet();
        var sheetView = drawing.GetViewBySheetName(sheet.GetName());
        var sheetScale2 = sheetView.ScaleDecimal;

        var sheetCoords = (double[])sheetView.GetXform();
        var sheetToSketch = (MathTransform)sheetView.IGetSketch().ModelToSketchTransform;
        var arrSheetToSketch = (double[])sheetToSketch.ArrayData;

        var props = (double[])sheet.GetProperties2(); // [ paperSize, templateIn, scale1, scale2, firstAngle, width, height, ? ]
        var sheetScale = props[2] / props[3];
        var sheetWidth = MeterToMM(props[5]);
        var sheetHeight = MeterToMM(props[6]);
        var writer = new SvgWriter(sheetWidth, sheetHeight);

        // Y-flip transform matrix: flips Y-axis and translates to keep coordinates positive
        // | 1   0   0  |   | x |   | x            |
        // | 0  -1   H  | × | y | = | H - y        |
        // | 0   0   1  |   | 1 |   | 1            |
        var yFlipTransform = Matrix<double>.Build.DenseOfArray(new double[,] {
            { 1,  0,  0 },
            { 0, -1,  sheetHeight },
            { 0,  0,  1 }
        });
        (double x, double y) ApplyTransformY(double x, double y) {
            var pt = Vector<double>.Build.Dense(new[] { x, y, 1.0 });
            var result = yFlipTransform * pt;
            return (result[0], result[1]);
        }
        var views = drawing.GetViewsForSheet(sheet);
        MathTransform modelToSketch;
        MathTransform modelToView;
        foreach (var view in views) {
            var viewName = view.GetName2();
            modelToView = view.ModelToViewTransform;
            var sketch = view.IGetSketch();
            modelToSketch = sketch.ModelToSketchTransform;

            // Get the view's origin position on the sheet (where sketch origin projects to)
            var (viewOriginX, viewOriginY) = view.GetPositionAsTuple();
            viewOriginX = MeterToMM(viewOriginX);
            viewOriginY = MeterToMM(viewOriginY);
            var viewRect = view.GetOutlineAsRect(meterToMM: true);
            var viewLeft = viewOriginX - viewRect.Width / 2;
            var viewBottom = viewOriginY - viewRect.Height / 2;

            // Transform point from view/sketch space (meters) to SVG space (mm, Y-flipped)
            (double svgX, double svgY) ViewToSvg(double x, double y) {
                var mmX = MeterToMM(x) * view.ScaleDecimal + viewOriginX;
                var mmY = MeterToMM(y) * view.ScaleDecimal + viewOriginY;
                (mmX, mmY) = ApplyTransformY(mmX, mmY);
                return (mmX, mmY);
            }

            var edges = (object[])view.GetPolylines7((int)swCrossHatchFilter_e.swCrossHatchExclude, out var oPolylinesData);
            var polylinesData = (double[])oPolylinesData;
            if (polylinesData == null || polylinesData.Length == 0) continue;
            var records = ParsePolylines(polylinesData);
            foreach (var record in records) {
                if (record.Points == null || record.Points.Count < 2) continue;
                var color = ColorRefToHex(record.LineColor);
                var strokeWidth = MapLineWeight((int)record.LineWeight);
                // ---------------------
                // Polyline
                // ---------------------
                var points = record.Points;
                if (points == null || points.Count < 2) return;
                var pathBuilder = new System.Text.StringBuilder();
                for (var i = 0; i < points.Count; i++) {
                    var isFirst = i == 0;
                    var pt = points[i];
                    var (svgX, svgY) = ViewToSvg(pt.X, pt.Y);
                    pathBuilder.Append($" {(isFirst ? "M" : "L")} {Format(svgX)} {Format(svgY)}");
                }
                writer.AddPath(pathBuilder.ToString(), color, strokeWidth);
            }
            // ---------------------------------------------------------
            // Annotations
            // ---------------------------------------------------------
            (double svgX, double svgY) AnnotationToSvg(double x, double y) {
                var mmX = MeterToMM(x);
                var mmY = MeterToMM(y);
                (mmX, mmY) = ApplyTransformY(mmX, mmY);
                return (mmX, mmY);
            }
            var annotations = view.GetAnnotationsEx();
            foreach (var ann in annotations) {
                var type = (swAnnotationType_e)ann.GetType();
                switch (type) {
                    case swAnnotationType_e.swNote:
                        var pos = (double[])ann.GetPosition(); // x,y,z
                        if (pos == null) continue;
                        var (x, y) = AnnotationToSvg(pos[0], pos[1]);
                        var text = "";
                        var colorRef = ann.Color;
                        var hexColor = ColorRefToHex(colorRef);
                        var note = (INote)ann.GetSpecificAnnotation();
                        if (note != null) {
                            text = note.GetText();
                            var textCoordinates = NxPoint.FromCoords(x, y);
                            if (note.HasBalloon()) {
                                // --------------------------
                                // 1. Draw the balloon shape (circle)
                                // --------------------------
                                // [ centerX, centerY, centerZ, arcX, arcY, arcZ, radius ]
                                var balloonInfo = (double[])note.GetBalloonInfo();
                                var balloonCenter = NxPoint.FromSpan(balloonInfo.AsSpan().Slice(0, 3));
                                var balloonArc = NxPoint.FromSpan(balloonInfo.AsSpan().Slice(3, 3));
                                var balloonRadius = MeterToMM(balloonInfo[6]);
                                var style = (swBalloonStyle_e)note.GetBalloonStyle();
                                var (cx, cy) = AnnotationToSvg(balloonCenter.X, balloonCenter.Y);
                                if (style == swBalloonStyle_e.swBS_Circular) {
                                    // SOLUTION (with a caveat):
                                    // The balloon info returns coordinates in the sheet space.
                                    // However, if the balloon is attached to a drawing view, the coordinates returned for the balloon will always be
                                    // the coordinates of the balloon when the drawing was loaded (ie. file opened).
                                    // If you move the view, and its annotations along with it, the coordinates returned will be wrong
                                    // (ie. they will still be the coords of the balloon at the time the drawing was loaded).
                                    writer.AddCircle(cx, cy, balloonRadius, hexColor, 0.25);
                                    textCoordinates.X = cx; 
                                    textCoordinates.Y = cy;
                                    // WORKAROUND: (doesn't work very well however)
                                    // writer.AddCircle(x, y, balloonRadius - 0.8, hexColor, 0.25);
                                }
                                // TODO: Handle other styles (triangle, hexagon, etc.)
                                // --------------------------
                                // 2. Draw the leader line(s)
                                // --------------------------
                                var leaderCount = ann.GetLeaderCount();
                                for (var li = 0; li < leaderCount; li++) {
                                    var leaderPoints = (double[])ann.GetLeaderPointsAtIndex(li);
                                    if (leaderPoints != null && leaderPoints.Length >= 6) {
                                        // Draw line segments between consecutive points (x,y,z triplets)
                                        for (var pi = 0; pi < leaderPoints.Length - 3; pi += 3) {
                                            var (lx1, ly1) = AnnotationToSvg(leaderPoints[pi], leaderPoints[pi + 1]);
                                            var (lx2, ly2) = AnnotationToSvg(leaderPoints[pi + 3], leaderPoints[pi + 4]);
                                            writer.AddLine(lx1, ly1, lx2, ly2, hexColor, 0.25);
                                        }
                                    }
                                }
                            }
                            if (!string.IsNullOrWhiteSpace(text)) {
                                // ATTENTION: extent includes the leader (arrow), so it will be totally wrong.
                                // GetExtent: [lowerLeftX, lowerLeftY, lowerLeftZ, upperRightX, upperRightY, upperRightZ]
                                // in sheet space (meters). Use this to calculate the center of the text box.
                                // var extent = (double[])note.GetExtent();
                                // if (extent != null && extent.Length >= 6) {
                                //     var noteBottomLeft = NxPoint.FromSpan(extent.AsSpan().Slice(0, 3));
                                //     var noteUpperRight = NxPoint.FromSpan(extent.AsSpan().Slice(3, 3));
                                //     (x, y) = AnnotationToSvg(
                                //         ((noteUpperRight.X + noteBottomLeft.X) / 2),
                                //         ((noteUpperRight.Y + noteBottomLeft.Y) / 2)
                                //     );
                                // }
                                var fontSize = 8.0; // Default mm
                                var noteHeight = note.GetHeight();
                                var noteHeightPoints = note.GetHeightInPoints();
                                var fontFamily = "Arial";
                                var textFormat = (ITextFormat)ann.GetTextFormat(0);
                                if (textFormat != null) {
                                    var typeFaceName = textFormat.TypeFaceName;
                                    var lineLength = textFormat.LineLength;
                                    var lineSpacing = textFormat.LineSpacing;
                                    var charHeightInPts = textFormat.CharHeightInPts;
                                    var charSpacingFactor = textFormat.CharSpacingFactor;
                                    fontSize = textFormat.CharHeight * 1000.0; // Meters to MM
                                }
                                var lineHeight = fontSize * 1.2;
                                writer.AddText(textCoordinates.X, textCoordinates.Y, text, fontFamily, fontSize, hexColor, 0, "middle");
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        writer.Save(filePath);
    }

    /// <summary>
    /// Parses the flat double array returned by IView.GetPolylines6 into structured PolylineRecord objects.
    /// </summary>
    private List<NxPolylineRecord> ParsePolylines(double[] data) {
        var records = new List<NxPolylineRecord>();
        if (data == null || data.Length == 0) return records;
        var i = 0;
        while (i < data.Length) {
            // Check for trailing zeros or insufficient data for a valid record
            // Minimum record size: Type(1) + GeomDataSize(1) + LineColor(1) + LineStyle(1) + LineFont(1) + LineWeight(1) + LayerID(1) + LayerOverride(1) + NumPolyPoints(1) = 9
            if (i + 9 > data.Length) break;
            // Check if we've hit trailing zeros (Type would be 0 or 1, not some random value)
            var typeValue = data[i];
            if (typeValue != 0 && typeValue != 1) break;
            var record = new NxPolylineRecord();
            record.Type = (NxPolylineType)(int)data[i++];
            var geomDataSize = (int)data[i++];
            // Handle the GeomDataSize = 17 bug: treat as 0 (trailing zeros)
            if (geomDataSize == 17) geomDataSize = 0;
            // Validate we have enough data for GeomData + metadata
            if (i + geomDataSize + 7 > data.Length) break;
            // Read GeomData if present (for arcs: 12 values)
            if (geomDataSize > 0) {
                record.GeomData = new double[geomDataSize];
                Array.Copy(data, i, record.GeomData, 0, geomDataSize);
                i += geomDataSize;
            }
            // Read metadata fields
            record.LineColor = (int)data[i++];
            record.LineStyle = (int)data[i++];
            record.LineFont = (int)data[i++];
            record.LineWeight = data[i++];
            record.LayerID = (int)data[i++];
            record.LayerOverride = (int)data[i++];
            // Validate we have NumPolyPoints
            if (i >= data.Length) break;
            var numPolyPoints = (int)data[i++];
            // Validate we have enough data for all points
            var pointsDataSize = numPolyPoints * 3;
            if (i + pointsDataSize > data.Length) break;
            // Read tessellated points
            record.Points = new List<NxPoint>(numPolyPoints);
            for (var p = 0; p < numPolyPoints; p++) {
                var x = data[i++];
                var y = data[i++];
                var z = data[i++];
                record.Points.Add(new NxPoint(x, y, z));
            }
            records.Add(record);
        }
        return records;
    }

    /// <summary>
    /// Convert line weight value to stroke width in mm.
    /// The values in inches were taken from Solidworks' defaults.
    /// </summary>
    private double MapLineWeight(int value) {
        const double inchToMm = 25.4;
        switch ((swLineWeights_e)value) {
            case swLineWeights_e.swLW_THIN:    return 0.0071 * inchToMm;
            case swLineWeights_e.swLW_NORMAL:  return 0.0098 * inchToMm;
            case swLineWeights_e.swLW_THICK:   return 0.0138 * inchToMm;
            case swLineWeights_e.swLW_THICK2:  return 0.0197 * inchToMm;
            case swLineWeights_e.swLW_THICK3:  return 0.0276 * inchToMm;
            case swLineWeights_e.swLW_THICK4:  return 0.0394 * inchToMm;
            case swLineWeights_e.swLW_THICK5:  return 0.0551 * inchToMm;
            case swLineWeights_e.swLW_THICK6:  return 0.0787 * inchToMm;
            default: return 0.0098 * inchToMm; // Default to Normal
        }
    }

    /// <summary>
    /// Converts COLORREF (0x00BBGGRR) to string hex representation (eg. "#000000").
    /// </summary>
    private string ColorRefToHex(int colorRef) {
        if (colorRef == -1) return "#000000";
        int r = colorRef & 0xFF;
        int g = (colorRef >> 8) & 0xFF;
        int b = (colorRef >> 16) & 0xFF;
        return $"#{r:X2}{g:X2}{b:X2}";
    }

    private string Format(double value) => value.ToString(System.Globalization.CultureInfo.InvariantCulture);
}

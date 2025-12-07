using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dubeg.Sw.ExportTools.Commands.DrawingToSvg;

public partial class SvgExporter {
    private (double x, double y, double z) ApplyTransform(IMathTransform transform, double x, double y, double z) {
        var mathUtils = _app.IGetMathUtility();
        var point = (IMathPoint)mathUtils.CreatePoint(new double[] { x, y, z });
        point = (IMathPoint)point.MultiplyTransform(transform);
        var pointData = (double[])point.ArrayData;
        return (pointData[0], pointData[1], pointData[2]);
    }

    private NxPoint ApplyTransform(MathTransform transform, NxPoint inPoint) {
        var mathUtils = _app.IGetMathUtility();
        var point = (IMathPoint)mathUtils.CreatePoint(inPoint.ToArray());
        point = (IMathPoint)point.MultiplyTransform(transform);
        return NxPoint.FromArray((double[])point.ArrayData);
    }
}

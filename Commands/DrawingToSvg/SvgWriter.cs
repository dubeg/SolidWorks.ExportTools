using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Dubeg.Sw.ExportTools.Commands.DrawingToSvg;

public class SvgWriter {
    private readonly XElement _svg;
    private readonly XElement _contentGroup;
    private readonly XNamespace _ns = "http://www.w3.org/2000/svg";

    public double Width { get; }
    public double Height { get; }

    public SvgWriter(double width, double height) {
        Width = width;
        Height = height;
        _svg = new XElement(_ns + "svg",
            new XAttribute("width", $"{width}"),
            new XAttribute("height", $"{height}"),
            new XAttribute("viewBox", $"0 0 {width} {height}"),
            new XAttribute("style", $"border: 1px solid black"),
            new XAttribute("xmlns", _ns.NamespaceName)
        );
        // Content group for all SVG elements
        _contentGroup = new XElement(_ns + "g");
        _svg.Add(_contentGroup);
    }

    public void AddPath(string pathData, string strokeColor, double strokeWidth, string fillColor = "none") {
        var path = new XElement(_ns + "path",
            new XAttribute("d", pathData),
            new XAttribute("stroke", strokeColor),
            new XAttribute("stroke-width", strokeWidth),
            new XAttribute("fill", fillColor)
        );
        _contentGroup.Add(path);
    }
    
    public void AddLine(double x1, double y1, double x2, double y2, string strokeColor, double strokeWidth) {
        var line = new XElement(_ns + "line",
            new XAttribute("x1", Format(x1)),
            new XAttribute("y1", Format(y1)),
            new XAttribute("x2", Format(x2)),
            new XAttribute("y2", Format(y2)),
            new XAttribute("stroke", strokeColor),
            new XAttribute("stroke-width", strokeWidth)
        );
        _contentGroup.Add(line);
    }

    public void AddCircle(double cx, double cy, double r, string strokeColor, double strokeWidth, string fillColor = "none") {
        var circle = new XElement(_ns + "circle",
            new XAttribute("cx", Format(cx)),
            new XAttribute("cy", Format(cy)),
            new XAttribute("r", Format(r)),
            new XAttribute("stroke", strokeColor),
            new XAttribute("stroke-width", strokeWidth),
            new XAttribute("fill", fillColor)
        );
        _contentGroup.Add(circle);
    }

    public void AddEllipse(double cx, double cy, double rx, double ry, double rotation, string strokeColor, double strokeWidth, string fillColor = "none") {
        var ellipse = new XElement(_ns + "ellipse",
            new XAttribute("cx", Format(cx)),
            new XAttribute("cy", Format(cy)),
            new XAttribute("rx", Format(rx)),
            new XAttribute("ry", Format(ry)),
            new XAttribute("stroke", strokeColor),
            new XAttribute("stroke-width", strokeWidth),
            new XAttribute("fill", fillColor)
        );

        if (rotation != 0) {
            ellipse.Add(new XAttribute("transform", $"rotate({Format(rotation)}, {Format(cx)}, {Format(cy)})"));
        }

        _contentGroup.Add(ellipse);
    }

    public void AddText(double x, double y, string text, string fontFamily, double fontSize, string color, double rotation = 0, string textAnchor = "start") {
        var textElement = new XElement(_ns + "text",
            new XAttribute("x", Format(x)),
            new XAttribute("y", Format(y)),
            new XAttribute("font-family", fontFamily),
            new XAttribute("font-size", fontSize),
            new XAttribute("fill", color),
            new XAttribute("text-anchor", textAnchor),
            new XAttribute("dominant-baseline", "middle"),
            text
        );

        if (rotation != 0) {
            textElement.Add(new XAttribute("transform", $"rotate({Format(rotation)}, {Format(x)}, {Format(y)})"));
        }

        _contentGroup.Add(textElement);
    }

    public void SetViewBox(double minX, double minY, double width, double height) {
        var viewBoxAttr = _svg.Attribute("viewBox");
        if (viewBoxAttr != null) {
            viewBoxAttr.Value = $"{Format(minX)} {Format(minY)} {Format(width)} {Format(height)}";
        }
    }

    public void SetDimensions(double width, double height) {
        void SetAttr(string attrName, double value) {
            var attr = _svg.Attribute(attrName);
            if (attr != null) {
                attr.Value = $"{Format(value)}";
            }
        }
        SetAttr("width", width);
        SetAttr("height", height);
    }

    public void Save(string filePath) {
        var doc = new XDocument(_svg);
        doc.Save(filePath);
    }

    private string Format(double value) {
        return value.ToString("0.####", CultureInfo.InvariantCulture);
    }
}

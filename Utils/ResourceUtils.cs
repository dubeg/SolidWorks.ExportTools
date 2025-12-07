using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using Dubeg.Sw.ExportTools.Models;
using SkiaSharp;
using Svg.Skia;

namespace Dubeg.Sw.ExportTools.Utils;
internal static class ResourceUtils {
    public static Dictionary<ExportStatus, Image> LoadStatusImages() {
        var results = new Dictionary<ExportStatus, Image>();
        var assembly = Assembly.GetExecutingAssembly();
        var rootNamespace = typeof(AddIn).Namespace;
        var statusNoneIconPath = $"{rootNamespace}.Resources.status-none.svg";
        var statusOkIconPath = $"{rootNamespace}.Resources.status-ok.svg";
        var statusErrorIconPath = $"{rootNamespace}.Resources.status-error.svg";
        var resources = new (string Name, ExportStatus Status)[] {
            (statusNoneIconPath, ExportStatus.Unprocessed),
            (statusOkIconPath, ExportStatus.Processed),
            (statusErrorIconPath, ExportStatus.Error),
        };
        foreach (var resource in resources) {
            var resourceName = resource.Name;
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) continue;

            using var reader = new StreamReader(stream);
            var svgContent = reader.ReadToEnd();
            var svg = new Svg.Skia.SKSvg();
            svg.FromSvg(svgContent);

            using var bitmap = new SKBitmap(16, 16);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.Transparent);
            canvas.DrawPicture(svg.Picture, 0, 0);

            var info = new SKImageInfo(16, 16);
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var ms = new MemoryStream();
            data.SaveTo(ms);
            ms.Position = 0;

            results[resource.Status] = new Bitmap(ms);
        }
        return results;
    }
}

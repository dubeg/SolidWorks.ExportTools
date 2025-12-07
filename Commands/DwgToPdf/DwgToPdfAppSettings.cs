using System;
using System.Collections.Generic;

namespace Dubeg.Sw.ExportTools.Commands.DwgToPdf;

public class DwgToPdfAppSettings: Commands.Base.AppSettings {
    /// <summary>
    /// Layers that will be ignored during the export 
    /// if their name contains any of the keywords defined here.
    /// </summary>
    public List<string> IgnoredLayers { get; set; } = [];
}

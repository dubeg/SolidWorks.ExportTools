using System;

namespace Dubeg.Sw.ExportTools.Commands.Base;

public class AppSettings {
    private string _outputFolderPath;

    /// <summary>
    /// Output path (folder) of the exported PDFs.
    /// </summary>
    public string OutputFolderPath {
        get => _outputFolderPath;
        set => _outputFolderPath = (value ?? "")
            .Replace("{UserProfile}", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))
            .Replace("{Desktop}", Environment.GetFolderPath(Environment.SpecialFolder.Desktop))
            .Replace("{Documents}", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
    }
} 
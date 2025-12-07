using System;
using System.IO;

namespace Dubeg.Sw.ExportTools;

public class LoggingSettings {
    private string _logFilePath;
    
    /// <summary>
    /// Path to the log file. Supports placeholders:
    /// - {UserProfile}
    /// - {Desktop}
    /// - {Documents}
    /// </summary>
    public string LogFilePath {
        get => _logFilePath;
        set => _logFilePath = (value ?? "")
            .Replace("{UserProfile}", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))
            .Replace("{Desktop}", Environment.GetFolderPath(Environment.SpecialFolder.Desktop))
            .Replace("{Documents}", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
    }

    public string LogDirPath => Path.GetDirectoryName(LogFilePath);
} 
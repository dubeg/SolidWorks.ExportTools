using System.IO;

namespace Dubeg.Sw.ExportTools.Models;

public class ExportFileItem {
    public string FilePath { get; set; }
    public ExportStatus Status { get; set; }
    public string Name => Path.GetFileName(FilePath);

    public ExportFileItem(string filePath) {
        FilePath = filePath;
        Status = ExportStatus.Unprocessed;
    }
}


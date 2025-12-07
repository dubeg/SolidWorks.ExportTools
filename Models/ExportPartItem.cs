using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dubeg.Sw.ExportTools.Models;

public class ExportPartItem {
    public string PartNo { get; set; }
    public string FilePath { get; set; }
    public bool IsFileEmpty { get; set; }
    public ExportStatus Status { get; set; } = ExportStatus.Unprocessed;
    public string Message { get; set; } = string.Empty;
}

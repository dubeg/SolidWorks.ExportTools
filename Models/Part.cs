using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dubeg.Sw.ExportTools.Models;
public class Part {
    public string PartNo { get; set; }
    public string FilePath { get; set; }
    public bool IsFileEmpty { get; set; }
}

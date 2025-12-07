using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolidWorks.Interop.swconst;

namespace Dubeg.Sw.ExportTools.Commands.Base;
public class ExportException : Exception {
    public string FileName { get; set; }
    public IEnumerable<swFileSaveError_e> FileSaveErrors { get; set; }

    public ExportException() : base() { }
    public ExportException(string message) : base(message) { }
    public ExportException(string message, Exception innerException) : base(message, innerException) { }
}

using System;
using System.Collections;
using System.Collections.Generic;
using SolidWorks.Interop.swconst;

namespace Dubeg.Sw.ExportTools.Commands.DwgToPdf;

public class DwgToPdfImportException : Exception {
    public IEnumerable<swFileLoadError_e> FileImportErrors { get; set; }
    public string FilePath { get; set; }

    public DwgToPdfImportException() : base() { }
    public DwgToPdfImportException(string message) : base(message) { }
    public DwgToPdfImportException(string message, Exception innerException) : base(message, innerException) { }
}

public class DwgToPdfExportException : Exception {
    public string FileName { get; set; }
    public IEnumerable<swFileSaveError_e> FileSaveErrors { get; set; }

    public DwgToPdfExportException() : base() { }
    public DwgToPdfExportException(string message) : base(message) { }
    public DwgToPdfExportException(string message, Exception innerException) : base(message, innerException) { }
}

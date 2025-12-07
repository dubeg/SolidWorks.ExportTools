using System;
using System.Windows.Forms;
using Xarial.XCad.SolidWorks;
using Dubeg.Sw.ExportTools.Commands.Base;
using System.Linq;
using Serilog;
using System.Threading.Tasks;
using System.IO;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.sldworks;
using Dubeg.Sw.ExportTools.Utils;
using Dubeg.Sw.ExportTools.Commands.JpgToPdf;
using Dubeg.Sw.ExportTools.Commands.ModelToPdf;
using Xarial.XCad.Sketch;

namespace Dubeg.Sw.ExportTools.Commands;

public class JpgOrModelToPdfCommand : CommandBase<AppSettings> {
    private readonly JpgModelToPdfCommand _jpgModelToPdfCommand;
    private readonly ModelToPdfCommand _modelToPdfCommand;

    public JpgOrModelToPdfCommand(AddinUiManager uiMgr, ISwApplication swApp, AppSettings appSettings, AddIn addin) 
        : base(uiMgr, swApp, appSettings, addin) {
        _jpgModelToPdfCommand = new JpgModelToPdfCommand(uiMgr, swApp, appSettings, addin);
        _modelToPdfCommand = new ModelToPdfCommand(uiMgr, swApp, appSettings, addin);
    }

    public string RunForCurrentDocument(bool openPdf = false) {
        if (!SwApp.Documents.Any()) {
            throw new Exception("No documents opened.");
        }
        if (SwApp.Documents.Active is null) {
            throw new Exception("No document active.");
        }

        var model = App.IActiveDoc2;
        if (model == null) {
            throw new Exception($"No file currently opened");
        }
        var hasSketchPicture = SwApp.Documents.Active.Features.Any(x => x is IXSketchPicture);
        if (hasSketchPicture) {
            return _jpgModelToPdfCommand.RunForCurrentDocument(openPdf);
        } else {
            return _modelToPdfCommand.RunForCurrentDocument(openPdf);
        }
    }

    public async Task<string> RunForFileAsync(string inputFilePath) => await Task.Run(() => RunForFile(inputFilePath));

    public string RunForFile(string inputFilePath) {
        var docType = Path.GetExtension(inputFilePath).ToLower() switch {
            ".sldprt" => swDocumentTypes_e.swDocPART,
            ".sldasm" => swDocumentTypes_e.swDocASSEMBLY,
            _ => throw new ArgumentException($"Unsupported file type: {inputFilePath}. Only .slddrw, .sldprt, and .sldasm files are supported.")
        };
        var (success, errors) = App.OpenDocument(inputFilePath, docType);
        if (!success) {
            throw new Exception($"Failed to open file {Path.GetFileName(inputFilePath)}: {string.Join(", ", errors)}");
        }
        return RunForCurrentDocument();
    }
} 
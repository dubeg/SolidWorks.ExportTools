using System;
using System.IO;
using System.Linq;
using Dubeg.Sw.ExportTools.Commands.Base;
using Dubeg.Sw.ExportTools.Utils;
using SolidWorks.Interop.sldworks;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;

namespace Dubeg.Sw.ExportTools.Commands;

/// <summary>
/// Copies a view to a new sheet & adjust its scale before saving it as a PDF.
/// </summary>
public class ViewToPdfCommand : CommandBase<AppSettings> {
    private readonly ViewToSheetCommand _viewToSheetCommand;
    private readonly SheetToPdfCommand _sheetToPdfCommand;

    public ViewToPdfCommand(AddinUiManager uiMgr, ISwApplication swApp, AppSettings appSettings, AddIn addin)
        : base(uiMgr, swApp, appSettings, addin) {
        _viewToSheetCommand = new ViewToSheetCommand(uiMgr, swApp, appSettings, addin);
        _sheetToPdfCommand = new SheetToPdfCommand(uiMgr, swApp, appSettings, addin);
    }

    public void RunForCurrentDocument(bool cleanup = true) {
        if (!SwApp.Documents.Any()) {
            throw new InvalidOperationException("No documents opened.");
        }
        if (SwApp.Documents.Active is null) {
            throw new InvalidOperationException("No document active.");
        }

        var doc = SwApp.Documents.Active;
        if (doc is not ISwDrawing) {
            throw new InvalidOperationException("Active document is not a drawing.");
        }
        
        _viewToSheetCommand.RunForCurrentDocument(cleanup: false);
        _sheetToPdfCommand.RunForCurrentSheet();
    }
} 
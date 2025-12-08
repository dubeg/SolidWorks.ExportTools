using System.ComponentModel;
using Dubeg.Sw.ExportTools.Properties;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.UI.Commands.Attributes;
using Xarial.XCad.UI.Commands.Enums;

namespace Dubeg.Sw.ExportTools;

[Title("DUBEG Export Tools")]
[Description("")]
public enum AddinCommandTypes {
    [Title("Logs")]
    [Description("Inspecter les logs de l'add-in")]
    [CommandItemInfo(true, true, WorkspaceTypes_e.All, true)]
    [Icon(typeof(Resources), nameof(Resources.logs))]
    LogViewer,

    [Title("Parts to PDF")]
    [Icon(typeof(Resources), nameof(Resources.Unified))]
    PartsToPdf,

    [Title("JPG to PDF")]
    [Description("Exporter jpg du modèle en format pdf")]
    [CommandItemInfo(true, true, WorkspaceTypes_e.Drawing, true)]
    [Icon(typeof(Resources), nameof(Resources.jpg))]
    JpgToPdf,

    [Title("JPG to PDF (multi)")]
    [Description("Exporter jpg de plusieurs fichiers en format pdf")]
    [CommandItemInfo(true, true, WorkspaceTypes_e.All, true)]
    [Icon(typeof(Resources), nameof(Resources.jpg_multi))]
    JpgToPdfMulti,

    [Title("Model to PDF")]
    [Description("Exporter part/assembly en format pdf")]
    [CommandItemInfo(true, true, WorkspaceTypes_e.Assembly | WorkspaceTypes_e.Part, true)]
    [Icon(typeof(Resources), nameof(Resources.model))]
    ModelToPdf,

    [Title("Model to PDF (multi)")]
    [Description("Exporter part/assembly de plusieurs fichiers en format pdf")]
    [CommandItemInfo(true, true, WorkspaceTypes_e.All, true)]
    [Icon(typeof(Resources), nameof(Resources.model))]
    ModelToPdfMulti,

    [Title("DWG to PDF")]
    [Description("Exporter DWG en format pdf")]
    [CommandItemInfo(true, true, WorkspaceTypes_e.Drawing, true)]
    [Icon(typeof(Resources), nameof(Resources.DWG))]
    DwgToPdf,

    [Title("DWG to PDF (multi)")]
    [Description("Exporter plusieurs DWG en format pdf")]
    [CommandItemInfo(true, true, WorkspaceTypes_e.All, true)]
    [Icon(typeof(Resources), nameof(Resources.DWG))]
    DwgToPdfMulti,

    [Title("Sheet to PDF")]
    [Description("Exporter une feuille de dessin en format pdf")]
    [CommandItemInfo(true, true, WorkspaceTypes_e.Drawing, true)]
    [Icon(typeof(Resources), nameof(Resources.Sheet))]
    SheetToPdf,

    [Title("View to PDF")]
    [Description("Exporter une vue de dessin en format pdf")]
    [CommandItemInfo(true, true, WorkspaceTypes_e.Drawing, true)]
    [Icon(typeof(Resources), nameof(Resources.ViewModel))]
    ViewToPdf,

    [Title("View to Sheet")]
    [Description("Copier une vue de dessin vers une nouvelle feuille")]
    [CommandItemInfo(true, true, WorkspaceTypes_e.Drawing, true)]
    [Icon(typeof(Resources), nameof(Resources.ViewModel))]
    ViewToSheet,

    [Title("Center view")]
    [Icon(typeof(Resources), nameof(Resources.Center))]
    CenterViewInSheet,

    [Title("Scale view")]
    [Icon(typeof(Resources), nameof(Resources.Resize))]
    ScaleViewInSheet,

    [Title("OLE Object to Sheet")]
    [CommandItemInfo(true, true, WorkspaceTypes_e.Drawing, true)]
    OleObjectToSheet,

    [Title("OLE Object to PDF")]
    [CommandItemInfo(true, true, WorkspaceTypes_e.Drawing, false)]
    OleObjectToPdf,

    [Title("OLE/JPG/View to PDF")]
    [Description("Exporter un objet OLE, JPG ou vue en format pdf")]
    [CommandItemInfo(true, true, WorkspaceTypes_e.Drawing, true)]
    [Icon(typeof(Resources), nameof(Resources.Unified))]
    OleOrJpgOrViewToPdf,

    [Title("Jpg Model to PDF")]
    [CommandItemInfo(true, true, WorkspaceTypes_e.Part | WorkspaceTypes_e.Assembly, false)]
    JpgModelToPdf,

    [Title("JPG/Model to PDF")]
    [Description("Exporter en PDF en fonction du contenu (JPG si image, Model si pas d'image)")]
    [CommandItemInfo(true, true, WorkspaceTypes_e.Part | WorkspaceTypes_e.Assembly | WorkspaceTypes_e.Drawing, true)]
    JpgOrModelToPdf,

    [Title("Model to JPG")]
    [Description("Model -> JPG")]
    [CommandItemInfo(true, true, WorkspaceTypes_e.Part | WorkspaceTypes_e.Assembly, true)]
    ModelToJpg,

    [Title("Sheet to JPG")]
    [Description("Export la feuille active en JPG")]
    [CommandItemInfo(true, true, WorkspaceTypes_e.Drawing, true)]
    DrawingToJpg,

    [Title("Sheet to SVG")]
    [Description("Export active sheet to SVG")]
    [CommandItemInfo(true, true, WorkspaceTypes_e.Drawing, true)]
    [Icon(typeof(Resources), nameof(Resources.Drawing))]
    DrawingToSvg,

    [Title("BOM to CSV")]
    [Description("Export BOM table to CSV")]
    [CommandItemInfo(true, true, WorkspaceTypes_e.Drawing, true)]
    [Icon(typeof(Resources), nameof(Resources.Database))]
    BomToCsv,

    [Title("Test Limited Detailing")]
    [Description("Test IsInLimitedDetailingMode() detection method")]
    [CommandItemInfo(true, true, WorkspaceTypes_e.Drawing, true)]
    [Icon(typeof(Resources), nameof(Resources.logs))]
    TestLimitedDetailing,
}

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Dubeg.Sw.ExportTools.Commands.Base;
using Dubeg.Sw.ExportTools.Utils;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using Xarial.XCad.SolidWorks;

namespace Dubeg.Sw.ExportTools.Commands;

/// <summary>
/// Test command to verify the IsInLimitedDetailingMode() detection method.
/// </summary>
public class TestLimitedDetailingCommand : CommandBase<AppSettings> {
    // P/Invoke declarations for Windows API
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetWindowText(long hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowTextLength(long hWnd);

    public TestLimitedDetailingCommand(AddinUiManager uiMgr, ISwApplication swApp, AppSettings appSettings, AddIn addin)
        : base(uiMgr, swApp, appSettings, addin) {
    }

    private static string GetWindowTitle(long hWnd) {
        if (hWnd == 0) {
            return string.Empty;
        }

        try {
            int length = GetWindowTextLength(hWnd);
            if (length == 0) {
                return string.Empty;
            }

            var builder = new StringBuilder(length + 1);
            GetWindowText(hWnd, builder, builder.Capacity);
            return builder.ToString();
        }
        catch {
            return string.Empty;
        }
    }

    public void Run() {
        try {
            // Check if there's an active document
            if (App.IActiveDoc2 == null) {
                MessageBox.Show(
                    "No active document found.",
                    "Limited Detailing Mode Test",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            var activeDoc = App.IActiveDoc2;
            var docType = activeDoc.GetType();
            var docTypeName = ((swDocumentTypes_e)docType).ToString();

            // Test both detailing mode methods
            var isInDetailingMode = App.IsInDetailingMode();
            var isInLimitedDetailingMode = App.IsInLimitedDetailingMode();

            // Get additional info for debugging
            var isDrawing = docType == (int)swDocumentTypes_e.swDocDRAWING;
            var isDetailingModeRaw = false;
            var windowTitle = "";
            var featureCount = 0;
            var featureNames = new System.Collections.Generic.List<string>();

            if (isDrawing) {
                var drawingDoc = (DrawingDoc)activeDoc;
                isDetailingModeRaw = drawingDoc.IsDetailingMode();
            }

            // Get feature count and names for POC verification
            try {
                void TraverseFeatureNode(TreeControlItem featNode) {
                    var swChildFeatNode = featNode.GetFirstChild();
                    while (swChildFeatNode is not null) {
                        TraverseFeatureNode(swChildFeatNode);
                        swChildFeatNode = swChildFeatNode.GetNext();
                    }
                }
                var featureMgr = activeDoc.FeatureManager;
                var rootNode = featureMgr.GetFeatureTreeRootItem2((int)swFeatMgrPane_e.swFeatMgrPaneBottom);
                if (rootNode is null) { 
                    // Nothing is loaded! Probably in limited detailing mode.
                }
                // --
                featureCount = activeDoc.GetFeatureCount();
                if (featureCount > 0) {
                    var feature = (Feature)activeDoc.FirstFeature();
                    while (feature != null) {
                        var featureName = feature.Name;
                        var featureType = feature.GetTypeName2();
                        featureNames.Add($"{featureName} ({featureType})");
                        feature = (Feature)feature.GetNextFeature();
                    }
                }
            }
            catch (Exception ex) {
                featureCount = -1; // Error getting count
                featureNames.Add($"Error: {ex.Message}");
            }

            try {
                var frame = (Frame)App.Frame();
                if (frame != null) {
                    var hwnd = frame.GetHWndx64();
                    windowTitle = GetWindowTitle(hwnd);
                }
            }
            catch (Exception ex) {
                windowTitle = $"Error: {ex.Message}";
            }

            // Build feature list string
            var featureListStr = "";
            if (featureCount > 0) {
                featureListStr = "\n\nFeatures:\n" + string.Join("\n", featureNames.Select((name, idx) => $"  {idx + 1}. {name}"));
            }

            // Build result message
            var result = $"Detailing Mode Test Results:\n\n" +
                        $"Document Type: {docTypeName}\n" +
                        $"Is Drawing: {isDrawing}\n" +
                        $"Window Title: {windowTitle}\n" +
                        $"Feature Count: {featureCount}{featureListStr}\n\n" +
                        $"Raw DrawingDoc.IsDetailingMode(): {isDetailingModeRaw}\n\n" +
                        $"=== Extension Methods ===\n" +
                        $"IsInDetailingMode(): {isInDetailingMode}\n" +
                        $"  → Checks: drawing + DrawingDoc.IsDetailingMode()\n\n" +
                        $"IsInLimitedDetailingMode(): {isInLimitedDetailingMode}\n" +
                        $"  → Checks: drawing + window title contains '[Detailing - Limited]'\n\n" +
                        $"POC Note: Feature count typically 0 in limited detailing mode";

            MessageBox.Show(
                result,
                "Limited Detailing Mode Test",
                MessageBoxButtons.OK,
                isInLimitedDetailingMode ? MessageBoxIcon.Warning : MessageBoxIcon.Information
            );
        }
        catch (Exception ex) {
            MessageBox.Show(
                $"Error testing limited detailing mode:\n\n{ex.Message}\n\n{ex.StackTrace}",
                "Test Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}


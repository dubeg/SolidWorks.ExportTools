using Dubeg.Sw.ExportTools.Utils;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dubeg.Sw.ExportTools.Commands.BomToCsv;

/// <summary>
/// Exports BOM table data from a SolidWorks drawing to CSV format.
/// UNTESTED CODE - may require adjustments.
/// </summary>
public class BomCsvExporter {
    private readonly ISldWorks _app;

    public BomCsvExporter(ISldWorks app) {
        _app = app ?? throw new ArgumentNullException(nameof(app));
    }

    /// <summary>
    /// Extracts all data from the active drawing sheet & the first BOM table found.
    /// </summary>
    /// <returns>List of BOM rows, where each row is a list of cell values.</returns>
    public List<List<string>> ExtractBomData() {
        var bomData = new List<List<string>>();
        var model = (ModelDoc2)_app.ActiveDoc;
        if (model == null || model.GetType() != (int)swDocumentTypes_e.swDocDRAWING) {
            throw new InvalidOperationException("Active document must be a drawing");
        }
        var drawing = (DrawingDoc)model;
        Sheet sheet = drawing.IGetCurrentSheet();
        if (sheet == null) {
            throw new InvalidOperationException("No active sheet found");
        }
        var sheetView = drawing.GetViewBySheetName(sheet.GetName());
        if (sheetView == null) {
            return bomData;
        }
        var tableAnnotations = (ITableAnnotation[])sheetView.GetTableAnnotations();
        if (tableAnnotations == null || tableAnnotations.Length == 0) {
            return bomData;
        }
        foreach (var table in tableAnnotations) {
            if (table == null) continue;
            if (table.Type != (int)swTableAnnotationType_e.swTableAnnotation_BillOfMaterials) {
                continue;
            }
            var rowCount = table.RowCount;
            var columnCount = table.ColumnCount;
            for (var rowIdx = 0; rowIdx < rowCount; rowIdx++) {
                var row = new List<string>();
                for (int colIdx = 0; colIdx < columnCount; colIdx++) {
                    var cellValue = table.DisplayedText[rowIdx, colIdx] ?? "";
                    row.Add(cellValue);
                }
                bomData.Add(row);
            }
            // Only process the first BOM table found (?)
            break;
        }
        return bomData;
    }

    /// <summary>
    /// Converts BOM data to CSV format with proper escaping.
    /// </summary>
    /// <param name="bomData">List of BOM rows, where each row is a list of cell values.</param>
    /// <returns>CSV formatted string.</returns>
    public string ConvertToCsv(List<List<string>> bomData) {
        if (bomData == null || bomData.Count == 0) {
            return "";
        }
        StringBuilder csv = new StringBuilder();
        foreach (var row in bomData) {
            var csvRow = new List<string>();
            foreach (var cell in row) {
                // Escape quotes by doubling them and wrap in quotes if needed
                string escapedCell = EscapeCsvField(cell);
                csvRow.Add(escapedCell);
            }
            csv.AppendLine(string.Join(",", csvRow));
        }
        return csv.ToString();
    }

    /// <summary>
    /// Escapes a CSV field according to RFC 4180.
    /// </summary>
    private string EscapeCsvField(string field) {
        if (string.IsNullOrEmpty(field)) {
            return "\"\"";
        }
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r")) {
            // Replace quotes with double quotes
            string escaped = field.Replace("\"", "\"\"");
            return $"\"{escaped}\"";
        }
        return field;
    }
}


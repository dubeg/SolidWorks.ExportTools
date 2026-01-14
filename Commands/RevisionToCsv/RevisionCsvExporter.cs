using AngleSharp.Html.Parser;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dubeg.Sw.ExportTools.Commands.RevisionToCsv;

/// <summary>
/// Exports Revision table data from a SolidWorks drawing to CSV format.
/// </summary>
public class RevisionCsvExporter {
    private readonly ISldWorks _app;
    private readonly HtmlParser _htmlParser;

    public RevisionCsvExporter(ISldWorks app) {
        _app = app ?? throw new ArgumentNullException(nameof(app));
        _htmlParser = new HtmlParser();
    }

    /// <summary>
    /// Extracts all data from the active drawing sheet's revision table.
    /// </summary>
    /// <returns>List of revision rows, where each row is a list of cell values.</returns>
    public List<List<string>> ExtractRevisionData() {
        var revisionData = new List<List<string>>();
        var model = (ModelDoc2)_app.ActiveDoc;
        if (model == null || model.GetType() != (int)swDocumentTypes_e.swDocDRAWING) {
            throw new InvalidOperationException("Active document must be a drawing");
        }
        var drawing = (DrawingDoc)model;
        var sheet = drawing.IGetCurrentSheet();
        if (sheet == null) {
            throw new InvalidOperationException("No active sheet found");
        }
        // Get revision table directly from the sheet
        var revisionTable = sheet.RevisionTable;
        if (revisionTable == null) {
            return revisionData;
        }
        // Cast to TableAnnotation to access common table properties
        var table = (TableAnnotation)revisionTable;
        var rowCount = table.RowCount;
        var columnCount = table.ColumnCount;
        for (var rowIdx = 0; rowIdx < rowCount; rowIdx++) {
            var row = new List<string>();
            for (var colIdx = 0; colIdx < columnCount; colIdx++) {
                var cellValue = table.DisplayedText[rowIdx, colIdx] ?? "";
                cellValue = StripHtmlTags(cellValue);
                row.Add(cellValue);
            }
            revisionData.Add(row);
        }
        return revisionData;
    }

    /// <summary>
    /// Extracts all data from the active drawing sheet's revision table.
    /// </summary>
    /// <returns>List of revision rows, where each row is a list of cell values.</returns>
    public void SaveToFile(string filename) {
        var revisionData = new List<List<string>>();
        var model = (ModelDoc2)_app.ActiveDoc;
        if (model == null || model.GetType() != (int)swDocumentTypes_e.swDocDRAWING) {
            throw new InvalidOperationException("Active document must be a drawing");
        }
        var drawing = (DrawingDoc)model;
        var sheet = drawing.IGetCurrentSheet();
        if (sheet == null) {
            throw new InvalidOperationException("No active sheet found");
        }
        var revisionTable = sheet.RevisionTable;
        if (revisionTable == null) return;
        var table = (TableAnnotation)revisionTable;
        table.SaveAsText2(filename, ",", false);
    }

    /// <summary>
    /// Converts revision data to CSV format with proper escaping.
    /// </summary>
    /// <param name="revisionData">List of revision rows, where each row is a list of cell values.</param>
    /// <returns>CSV formatted string.</returns>
    public string ConvertToCsv(List<List<string>> revisionData) {
        if (revisionData == null || revisionData.Count == 0) {
            return "";
        }
        var csv = new StringBuilder();
        foreach (var row in revisionData) {
            var csvRow = new List<string>();
            foreach (var cell in row) {
                // Escape quotes by doubling them and wrap in quotes if needed
                var escapedCell = EscapeCsvField(cell);
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
            var escaped = field.Replace("\"", "\"\"");
            return $"\"{escaped}\"";
        }
        return field;
    }

    /// <summary>
    /// Strips HTML tags (like FONT tags) from cell text using AngleSharp.
    /// </summary>
    private string StripHtmlTags(string text) {
        if (string.IsNullOrEmpty(text) || !text.Contains("<")) {
            return text;
        }
        var document = _htmlParser.ParseDocument($"<body>{text}</body>");
        return document.Body?.TextContent ?? text;
    }
}


using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Dubeg.Sw.ExportTools.Utils;

public static class DataGridViewExtensions {
    public static DataGridViewRow GetRowOrDefault<T>(this DataGridView dataGridView, T item) {
        return dataGridView.Rows.Cast<DataGridViewRow>().FirstOrDefault(x => EqualityComparer<T>.Default.Equals((T)x.Tag, item));
    }

    /// <summary>
    /// Return trues if item was found & selected.
    /// </summary>
    public static bool SelectRow<T>(this DataGridView dataGridView, T item) {
        var row = dataGridView.Rows.Cast<DataGridViewRow>().FirstOrDefault(x => EqualityComparer<T>.Default.Equals((T)x.Tag, item));
        if (row is not null) {
            row.Selected = true;
            return true;
        }
        return false;
    }

    public static void BindDataWithTags<T>(this DataGridView dataGridView, IEnumerable<T> dataToBindToGrid) where T : class {
        var properties = typeof(T).GetProperties().ToList();
        if (ClassHasNestedCollections(properties)) {
            throw new InvalidOperationException("Class cannot have nested collections.");
        }
        var columns = new Dictionary<PropertyInfo, string>();
        properties.ForEach(p => columns.Add(p, GetColumnName(p)));
        columns.ToList().ForEach(column => dataGridView.Columns.Add(column.Key.Name, column.Value));
        dataGridView.Rows.Add(dataToBindToGrid.Count());
        var rowIndex = 0;
        dataToBindToGrid.ToList().ForEach(data => {
            var columnIndex = 0;
            var row = dataGridView.Rows[rowIndex];
            row.Tag = data;
            properties.ForEach(prop => {
                row.Cells[columnIndex].Value = SetCellValue(prop, data);
                columnIndex++;
            });
            rowIndex++;
        });
    }

    private static string GetColumnName(PropertyInfo propertyInfo) {
        var descriptionAttribute = propertyInfo.GetCustomAttributes(typeof(DescriptionAttribute)).FirstOrDefault();
        if (descriptionAttribute == null) {
            return propertyInfo.Name;
        }
        var description = descriptionAttribute as DescriptionAttribute;
        return description == null ? propertyInfo.Name : description.Description;
    }

    private static object SetCellValue<T>(PropertyInfo property, T data) where T : class {
        var value = property.GetValue(data, null);
        return value is Guid ? value.ToString() : value;
    }

    private static bool ClassHasNestedCollections(IEnumerable<PropertyInfo> properties) {
        return properties.ToList().Any(x => x.PropertyType != typeof(string) &&
                                             typeof(IEnumerable).IsAssignableFrom(x.PropertyType) ||
                                             typeof(IEnumerable<>).IsAssignableFrom(x.PropertyType));
    }
}

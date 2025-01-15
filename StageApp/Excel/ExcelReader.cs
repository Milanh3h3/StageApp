using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using ExcelDataReader;

public class ExcelReader
{
    public static List<string[]> GetExcel(string filePath)
    {
        List<string[]> excelRows = new List<string[]>();
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance); 
        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet();
                var table = result.Tables[0]; // voor als er meerdere tabellen zijn
                foreach (DataRow row in table.Rows)
                {
                    string[] rowValues = new string[table.Columns.Count];
                    for (int col = 0; col < table.Columns.Count; col++)
                    {
                        rowValues[col] = row[col]?.ToString();
                    }
                    excelRows.Add(rowValues);
                }
            }
        }
        return excelRows;
    }

    public static Dictionary<string, List<string[]>> GetExcelTabs(string filePath)
    {
        var excelData = new Dictionary<string, List<string[]>>();
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet();
                foreach (DataTable table in result.Tables)
                {
                    var rows = new List<string[]>();
                    foreach (DataRow row in table.Rows)
                    {
                        var rowValues = new string[table.Columns.Count];
                        for (int col = 0; col < table.Columns.Count; col++)
                        {
                            rowValues[col] = row[col]?.ToString();
                        }
                        rows.Add(rowValues);
                    }
                    excelData.Add(table.TableName, rows);
                }
            }
        }
        return excelData;
    }
}

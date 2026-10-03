using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;

namespace TalukdarSales.Web.Infrastructure
{
    public static class Excel
    {
        public const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        /// <summary>Single-sheet workbook with a bold header row. Numeric cells stay numeric.</summary>
        public static FileContentResult Sheet(string fileName, string sheetName, string[] headers, IEnumerable<object[]> rows, object[] footer = null)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            for (var c = 0; c < headers.Length; c++)
                ws.Cell(1, c + 1).Value = headers[c];
            ws.Row(1).Style.Font.Bold = true;

            var r = 2;
            foreach (var row in rows)
            {
                Fill(ws, r++, row);
            }
            if (footer != null)
            {
                Fill(ws, r, footer);
                ws.Row(r).Style.Font.Bold = true;
            }
            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return new FileContentResult(ms.ToArray(), ContentType) { FileDownloadName = fileName };
        }

        private static void Fill(IXLWorksheet ws, int row, object[] values)
        {
            for (var c = 0; c < values.Length; c++)
            {
                var cell = ws.Cell(row, c + 1);
                switch (values[c])
                {
                    case null: break;
                    case double d: cell.Value = d; cell.Style.NumberFormat.Format = "#,##0.00"; break;
                    case int i: cell.Value = i; break;
                    case DateTime dt: cell.Value = dt; cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm"; break;
                    default: cell.Value = values[c].ToString(); break;
                }
            }
        }
    }
}

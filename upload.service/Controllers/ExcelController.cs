using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Linq;
using System;

namespace upload.service.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class ExcelController : ControllerBase
    {
        [HttpPost("Import")]
        public ActionResult<dynamic> Import([FromForm]IFormFile file)
        {
            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    using (var doc = SpreadsheetDocument.Open(stream, false))
                    {
                        var wbPart = doc.WorkbookPart;
                        var sheet = doc.WorkbookPart.Workbook.Descendants<Sheet>().FirstOrDefault();
                        WorksheetPart wsPart = (WorksheetPart)(wbPart.GetPartById(sheet.Id));
                        var rows = wsPart.Worksheet.Descendants<Row>().ToList();
                        return new
                        {
                            Success = true,
                            data = rows.Select(row => row.Descendants<Cell>().Select(x => {
                                var value = x.CellValue.Text;
                                if (x.DataType != null)
                                {
                                    switch (x.DataType.Value)
                                    {
                                        case CellValues.SharedString:

                                            // For shared strings, look up the value in the
                                            // shared strings table.
                                            var stringTable =
                                                wbPart.GetPartsOfType<SharedStringTablePart>()
                                                .FirstOrDefault();

                                            // If the shared string table is missing, something 
                                            // is wrong. Return the index that is in
                                            // the cell. Otherwise, look up the correct text in 
                                            // the table.
                                            if (stringTable != null)
                                            {
                                                var t = stringTable.SharedStringTable
                                                    .ElementAt(int.Parse(value));
                                                value = t.InnerText;
                                            }
                                            break;

                                        case CellValues.Boolean:
                                            switch (value)
                                            {
                                                case "0":
                                                    value = "FALSE";
                                                    break;
                                                default:
                                                    value = "TRUE";
                                                    break;
                                            }
                                            break;
                                    }
                                }
                                if(x.StyleIndex != null && x.DataType is null)
                                {
                                    var styles = wbPart.GetPartsOfType<WorkbookStylesPart>()
                                                .FirstOrDefault();
                                    if(styles != null)
                                    {
                                        var style = (CellFormat)styles.Stylesheet.CellFormats.ToList()[(int)x.StyleIndex.Value];
                                        if(style.NumberFormatId != null)
                                        {
                                            var numFmt = styles.Stylesheet.NumberingFormats.Select(f => (NumberingFormat)f).FirstOrDefault(f => f.NumberFormatId.Value == style.NumberFormatId.Value);
                                            if(numFmt != null)
                                            {
                                                var format = numFmt.FormatCode;
                                                if (format.Value.IndexOf("yyyy") != -1) {
                                                    value = DateTime.FromOADate(double.Parse(value)).ToShortDateString();
                                                };
                                            }
                                        }
                                        
                                    }
                                }
                                if(x.StyleIndex is null && x.DataType is null)
                                {
                                    value = ((double)decimal.Round(decimal.Parse(value), 10)).ToString();
                                }
                                return value;
                            }).ToList()).ToList()
                        };
                    }
                }
            }
            return new
            {
                Success = false
            };
        }
    }
}
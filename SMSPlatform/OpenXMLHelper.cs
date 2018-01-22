using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace SMSPlatform
{
    public class OpenXMLHelp
    {

        public DataTable ReadExcel(string sheetName, Stream stream)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(stream, false))
            {//打开Stream
                IEnumerable<Sheet> sheets = document.WorkbookPart.Workbook.Descendants<Sheet>().Where(s => s.Name == sheetName);
                if (sheets.Count() == 0)
                {//找出合适前提的sheet,没有则返回
                    return null;
                }
                WorksheetPart worksheetPart = (WorksheetPart)document.WorkbookPart.GetPartById(sheets.First().Id);
                //获取Excel中共享数据
                SharedStringTable stringTable = document.WorkbookPart.SharedStringTablePart.SharedStringTable;
                IEnumerable<Row> rows = worksheetPart.Worksheet.Descendants<Row>();//获得Excel中得数据行
                DataTable dt = new DataTable("Excel");
                //因为须要将数据导入到DataTable中,所以我们假定Excel的第一行是列名,从第二行开端是行数据
                foreach (Row row in rows)
                {
                    if (row.RowIndex == 1)
                    {//Excel第一行动列名
                        dt.Columns.AddRange(row.GetDataColumn(stringTable, dt).ToArray());
                    }
                    dt.Rows.Add(row.GetDataRow(stringTable, dt));//Excel第二行同时为DataTable的第一行数据
                }
                return dt;
            }
        }

        public void WriteListToExcel<T>(List<T> list, Dictionary<string, string> headDictionary, Stream stream)
        {
            SpreadsheetDocument document;

            //            FileInfo info = new FileInfo(filePath);
            //            if (!info.Directory.Exists)
            //            {
            //                info.Directory.Create();
            //            }
            //            if (!info.Exists)
            //            {
            //                document = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook);
            //            }
            document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true);

            var wbp = document.AddWorkbookPart();
            var wb = wbp.Workbook = new Workbook();
            wb.Sheets = new Sheets();

            //add worksheetpart to document  and  get wbp uid
            WorksheetPart newWorksheetPart = wbp.AddNewPart<WorksheetPart>();
            newWorksheetPart.Worksheet = new Worksheet(new SheetData());



            var sheets = document.WorkbookPart.Workbook.GetFirstChild<Sheets>();


            string relationshipId = document.WorkbookPart.GetIdOfPart(newWorksheetPart);

            //
            // Get a unique ID for the new worksheet.  
            uint sheetId = 1;
            if (sheets.Elements<Sheet>().Count() > 0)
            {
                sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
            }

            // Give the new worksheet a name.  
            string sheetName = "Sheet" + sheetId;
            // Append the new worksheet and associate it with the workbook.  
            Sheet sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = sheetName };
            sheets.Append(sheet);

            var ws = newWorksheetPart.Worksheet;

            ws.Append(new SheetData());

            //Set Header Row
            //填充表头



            Type t = list.GetType().GenericTypeArguments[0];
            System.Reflection.PropertyInfo[] ps = t.GetProperties();
            uint headIndex = 0;
            foreach (PropertyInfo p in ps)
            {
                var dict = headDictionary.Where(m => m.Key == p.Name);
                if (dict.Any())
                {
                    string headName = dict.First().Value;
                    ws.UpdateValue(1, headIndex, headName);
                    //                    cells[0, headIndex].PutValue(headName);
                    //                    cells[0, headIndex].SetStyle(style);
                    headIndex++;
                }


            }
            //Set Values
            uint rowIndex = 2;
            foreach (var item in list)
            {

                uint colIndex = 0;
                foreach (System.Reflection.PropertyInfo p in ps)
                {
                    var dict = headDictionary.Where(m => m.Key == p.Name);
                    if (dict.Any())
                    {
                        try
                        {
                            //                                dataRow.CreateCell(colIndex).SetCellValue(p.GetValue(item, null) + "");
                            ws.UpdateValue(rowIndex, colIndex, p.GetValue(item, null) + "");
                        }
                        catch
                        {

                        }
                        colIndex++;
                    }

                }
                rowIndex++;
            }




            document.Save();
            document.Close();
        }

        public void WriteDataTableToExcel(DataTable table, Stream stream)
        {

            SpreadsheetDocument document;

            //            FileInfo info = new FileInfo(filePath);
            //            if (!info.Directory.Exists)
            //            {
            //                info.Directory.Create();
            //            }
            //            if (!info.Exists)
            //            {
            //                document = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook);
            //            }
            document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true);

            var wbp = document.AddWorkbookPart();
            var wb = wbp.Workbook = new Workbook();
            wb.Sheets = new Sheets();

            //add worksheetpart to document  and  get wbp uid
            WorksheetPart newWorksheetPart = wbp.AddNewPart<WorksheetPart>();
            newWorksheetPart.Worksheet = new Worksheet(new SheetData());



            var sheets = document.WorkbookPart.Workbook.GetFirstChild<Sheets>();


            string relationshipId = document.WorkbookPart.GetIdOfPart(newWorksheetPart);

            //
            // Get a unique ID for the new worksheet.  
            uint sheetId = 1;
            if (sheets.Elements<Sheet>().Count() > 0)
            {
                sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
            }

            // Give the new worksheet a name.  
            string sheetName = "Sheet" + sheetId;
            // Append the new worksheet and associate it with the workbook.  
            Sheet sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = sheetName };
            sheets.Append(sheet);

            var ws = newWorksheetPart.Worksheet;

            ws.Append(new SheetData());

            //Set Header Row
            //填充表头
            uint headIndex = 0;
            foreach (DataColumn p in table.Columns)
            {

                string headName = p.ColumnName;
                ws.UpdateValue(1, headIndex, headName);
                //                    cells[0, headIndex].PutValue(headName);
                //                    cells[0, headIndex].SetStyle(style);
                headIndex++;



            }
            //Set Values
            uint rowIndex = 2;
            foreach (DataRow item in table.Rows)
            {

                uint colIndex = 0;
                foreach (DataColumn p in table.Columns)
                {


                    try
                    {
                        //                                dataRow.CreateCell(colIndex).SetCellValue(p.GetValue(item, null) + "");
                        ws.UpdateValue(rowIndex, colIndex, item[p.ColumnName] + "");
                    }
                    catch
                    {

                    }
                    colIndex++;


                }
                rowIndex++;
            }




            document.Save();
            document.Close();
        }

    }


    public static class WorkBookPartExtension
    {

        public static Worksheet GetWorkSheetByName(this WorkbookPart wbPart, string sheetName)
        {

            Sheet sheet = wbPart.Workbook.Descendants<Sheet>().Where((s) => s.Name == sheetName).FirstOrDefault();
            Worksheet ws = ((WorksheetPart)(wbPart.GetPartById(sheet.Id))).Worksheet;
            return ws;
        }

        // Given a Worksheet and an address (like "AZ254"), either return a cell reference, or 
        // create the cell reference and return it.
        public static Cell InsertCellInWorksheet(this Worksheet ws, string addressName)
        {
            SheetData sheetData = ws.GetFirstChild<SheetData>();
            Cell cell = null;

            UInt32 rowNumber = GetRowIndex(addressName);
            Row row = GetRow(sheetData, rowNumber);

            // If the cell you need already exists, return it.
            // If there is not a cell with the specified column name, insert one.  
            Cell refCell = row.Elements<Cell>().
                Where(c => c.CellReference.Value == addressName).FirstOrDefault();
            if (refCell != null)
            {
                cell = refCell;
            }
            else
            {
                cell = CreateCell(row, addressName);
            }
            return cell;
        }

        public static Cell GetOrCreateCell(this Worksheet ws, UInt32 rowIndex, UInt32 cellIndex)
        {

            Cell cell = null;

            var addressName = ColumnsUtils.GetAddressName(rowIndex, cellIndex);
            Row row = ws.GetRow(rowIndex);
            Cell refCell = row.Elements<Cell>().
                Where(c => c.CellReference.Value == addressName).FirstOrDefault();
            if (refCell != null)
            {
                cell = refCell;
            }
            else
            {
                cell = CreateCell(row, addressName);
            }
            return cell;

        }

        public static Cell CreateCell(this Row row, String address)
        {
            Cell cellResult;
            Cell refCell = null;

            // Cells must be in sequential order according to CellReference. Determine where to insert the new cell.
            foreach (Cell cell in row.Elements<Cell>())
            {
                if (string.Compare(cell.CellReference.Value, address, true) > 0)
                {
                    refCell = cell;
                    break;
                }
            }

            cellResult = new Cell();
            cellResult.CellReference = address;

            row.InsertBefore(cellResult, refCell);
            return cellResult;
        }

        public static Row GetRow(this SheetData wsData, UInt32 rowIndex)
        {
            var row = wsData.Elements<Row>().
                Where(r => r.RowIndex.Value == rowIndex).FirstOrDefault();
            if (row == null)
            {
                row = new Row();
                row.RowIndex = rowIndex;
                wsData.Append(row);
            }
            return row;
        }

        public static Row GetRow(this Worksheet ws, UInt32 rowIndex)
        {
            SheetData sd = ws.GetFirstChild<SheetData>();
            return sd.GetRow(rowIndex);
        }


        public static UInt32 GetRowIndex(string address)
        {
            string rowPart;
            UInt32 l;
            UInt32 result = 0;

            for (int i = 0; i < address.Length; i++)
            {
                if (UInt32.TryParse(address.Substring(i, 1), out l))
                {
                    rowPart = address.Substring(i, address.Length - i);
                    if (UInt32.TryParse(rowPart, out l))
                    {
                        result = l;
                        break;
                    }
                }
            }
            return result;
        }

        public static bool UpdateValue(this WorkbookPart wbPart, string sheetName, string addressName, string value, UInt32Value styleIndex, bool isString)
        {
            // Assume failure.
            bool updated = false;

            Sheet sheet = wbPart.Workbook.Descendants<Sheet>().Where((s) => s.Name == sheetName).FirstOrDefault();

            if (sheet != null)
            {
                Worksheet ws = ((WorksheetPart)(wbPart.GetPartById(sheet.Id))).Worksheet;
                Cell cell = InsertCellInWorksheet(ws, addressName);

                if (isString)
                {
                    // Either retrieve the index of an existing string,
                    // or insert the string into the shared string table
                    // and get the index of the new item.
                    int stringIndex = InsertSharedStringItem(wbPart, value);

                    cell.CellValue = new CellValue(stringIndex.ToString());
                    cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
                }
                else
                {
                    cell.CellValue = new CellValue(value);
                    cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                }

                if (styleIndex > 0)
                    cell.StyleIndex = styleIndex;

                // Save the worksheet.
                ws.Save();
                updated = true;
            }

            return updated;
        }

        public static bool UpdateValue(this Worksheet ws, UInt32 rowIndex, UInt32 cellIndex, string value)
        {
            var updated = false;

            Cell cell = ws.GetOrCreateCell(rowIndex, cellIndex);
            if (ws != null)
            {
                cell.CellValue = new CellValue(value);
                cell.DataType = new EnumValue<CellValues>(CellValues.String);
                ws.Save();
                updated = true;
            }
            return updated;
        }


        public static string GetValue(this Cell cell, SharedStringTable stringTable)
        {
            //因为Excel的数据存储在SharedStringTable中,须要获取数据在SharedStringTable 中的索引
            string value = string.Empty;
            try
            {
                if (cell.ChildElements.Count == 0)
                    return value;
                value = double.Parse(cell.CellValue.InnerText).ToString();
                if ((cell.DataType != null) && (cell.DataType == CellValues.SharedString))
                {
                    value = stringTable.ChildElements[Int32.Parse(value)].InnerText;
                }
            }
            catch (Exception)
            {
                value = "N/A";
            }
            return value;
        }

        public static List<DataColumn> GetDataColumn(this Row row, SharedStringTable stringTable, DataTable dt)
        {
            List<DataColumn> cols = new List<DataColumn>();
            Dictionary<string, int> columnCount = new Dictionary<string, int>();

            foreach (Cell cell in row)
            {
                string cellVal = GetValue(cell, stringTable);
                var col = new DataColumn(cellVal);
                if (IsContainsColumn(dt, col.ColumnName))
                {
                    if (!columnCount.ContainsKey(col.ColumnName))
                        columnCount.Add(col.ColumnName, 0);
                    col.ColumnName = col.ColumnName + (columnCount[col.ColumnName]++);
                }
                cols.Add(col);
            }
            return cols;
        }

        public static DataRow GetDataRow(this Row row, SharedStringTable stringTable, DataTable dt)
        {
            // 读取算法：按行一一读取单位格,若是整行均是空数据
            // 则忽视改行(因为本人的工作内容不须要空行)-_-
            DataRow dr = dt.NewRow();
            int i = 0;
            int nullRowCount = i;
            foreach (Cell cell in row)
            {
                string cellVal = cell.GetValue(stringTable);
                if (cellVal == string.Empty)
                {
                    nullRowCount++;
                }
                dr[i] = cellVal;
                i++;
            }
            if (nullRowCount != i)
            {
                return dr;
            }
            else
            {
                return null;
            }
        }


        public static bool IsContainsColumn(DataTable dt, string columnName)
        {
            if (dt == null || columnName == null)
            {
                return false;
            }
            return dt.Columns.Contains(columnName);
        }


        // Given the main workbook part, and a text value, insert the text into the shared
        // string table. Create the table if necessary. If the value already exists, return
        // its index. If it doesn't exist, insert it and return its new index.
        private static int InsertSharedStringItem(WorkbookPart wbPart, string value)
        {
            int index = 0;
            bool found = false;
            var stringTablePart = wbPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();

            // If the shared string table is missing, something's wrong.
            // Just return the index that you found in the cell.
            // Otherwise, look up the correct text in the table.
            if (stringTablePart == null)
            {
                // Create it.
                stringTablePart = wbPart.AddNewPart<SharedStringTablePart>();
            }

            var stringTable = stringTablePart.SharedStringTable;
            if (stringTable == null)
            {
                stringTable = new SharedStringTable();
            }

            // Iterate through all the items in the SharedStringTable. If the text already exists, return its index.
            foreach (SharedStringItem item in stringTable.Elements<SharedStringItem>())
            {
                if (item.InnerText == value)
                {
                    found = true;
                    break;
                }
                index += 1;
            }

            if (!found)
            {
                stringTable.AppendChild(new SharedStringItem(new Text(value)));
                stringTable.Save();
            }

            return index;
        }

        // Used to force a recalc of cells containing formulas. The
        // CellValue has a cached value of the evaluated formula. This
        // will prevent Excel from recalculating the cell even if 
        // calculation is set to automatic.
        public static bool RemoveCellValue(this WorkbookPart wbPart, string sheetName, string addressName)
        {
            bool returnValue = false;

            Sheet sheet = wbPart.Workbook.Descendants<Sheet>().
                Where(s => s.Name == sheetName).FirstOrDefault();
            if (sheet != null)
            {
                Worksheet ws = ((WorksheetPart)(wbPart.GetPartById(sheet.Id))).Worksheet;
                Cell cell = InsertCellInWorksheet(ws, addressName);

                // If there is a cell value, remove it to force a recalc
                // on this cell.
                if (cell.CellValue != null)
                {
                    cell.CellValue.Remove();
                }

                // Save the worksheet.
                ws.Save();
                returnValue = true;
            }

            return returnValue;
        }






    }


    public class ColumnsUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rowIndex">start by 1</param>
        /// <param name="colIndex">start by 0</param>
        /// <returns></returns>
        public static string GetAddressName(UInt32 rowIndex, UInt32 colIndex)
        {
            return new Number("ABCDEFGHIJKLMNOPQRSTUVWXYZ").ToString(colIndex) + rowIndex;
        }
    }

    public class Number
    {
        public string Characters
        {
            get;
            set;
        }

        public int Length
        {
            get
            {
                if (Characters != null)
                    return Characters.Length;
                else
                    return 0;
            }

        }

        public Number()
        {
            Characters = "0123456789";
        }

        public Number(string characters)
        {
            Characters = characters;
        }

        /// <summary>  
        /// 数字转换为指定的进制形式字符串  
        /// </summary>  
        /// <param name="number"></param>  
        /// <returns></returns>  
        public string ToString(long number)
        {
            List<string> result = new List<string>();
            long t = number;

            do
            {
                var mod = t % Length;
                t = Math.Abs(t / Length);
                var character = Characters[Convert.ToInt32(mod)].ToString();
                result.Insert(0, character);
            } while (t > 0);

            return string.Join("", result.ToArray());
        }

        /// <summary>  
        /// 指定字符串转换为指定进制的数字形式  
        /// </summary>  
        /// <param name="str"></param>  
        /// <returns></returns>  
        public long FromString(string str)
        {
            long result = 0;
            int j = 0;
            foreach (var ch in new string(str.ToCharArray().Reverse().ToArray()))
            {
                if (Characters.Contains(ch))
                {
                    result += Characters.IndexOf(ch) * ((long)Math.Pow(Length, j));
                    j++;
                }
            }
            return result;
        }

    }
}

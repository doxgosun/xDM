using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Aspose.Cells;
using System.IO;
using System.Collections;

namespace xDM.xCommon.oFiles
{
    public class Excel
    {
        public enum SaveFormat
        {
            CSV = Aspose.Cells.SaveFormat.CSV,
            Xlsx = Aspose.Cells.SaveFormat.Xlsx,
            Xls = Aspose.Cells.SaveFormat.Excel97To2003,
            Unknow = Aspose.Cells.SaveFormat.Unknown
        }

        /// <summary>
        /// 从excle文件中获取datatable
        /// </summary>
        /// <param name="ExcelFileName"></param>
        /// <returns></returns>
        public static DataTable[] GetDataTables(string ExcelFileName)
        {
            DataSet ds = GetDataSet(ExcelFileName);
            if (ds != null)
            {
                DataTable[] dts = new DataTable[ds.Tables.Count];
                ds.Tables.CopyTo(dts, 0);
                return dts;
            }
            return null;
        }
        /// <summary>
        /// 从excel文件中获取DataSet
        /// </summary>
        /// <param name="ExcelFileName"></param>
        /// <returns></returns>
        public static DataSet GetDataSet(string ExcelFileName)
        {
            DataSet ds = new DataSet();
            Workbook wb = new Workbook(ExcelFileName);
            foreach (Worksheet ws in wb.Worksheets)
            {
                DataTable dt = new DataTable();
                if (ws.Cells.Count > 0)
                {
                    dt = ws.Cells.ExportDataTableAsString(0, 0, ws.Cells.MaxDataRow + 1, ws.Cells.MaxDataColumn + 1, true);
                }
                ds.Tables.Add(dt);
            }
            ds.DataSetName = System.IO.Path.GetFileName(ExcelFileName);
            return ds;
        }

        /// <summary>
        /// 从excle文件流中获取datatable
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static DataTable[] GetDataTables(Stream stream)
        {
            DataTable[] dts = null;
            DataSet ds = GetDataSet(stream);
            if (ds != null)
            {
                dts = new DataTable[ds.Tables.Count];
                ds.Tables.CopyTo(dts, 0);
            }
            return dts;
        }

        /// <summary>
        /// 从excel文件流中获取DataSet
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static DataSet GetDataSet(Stream stream)
        {
            DataSet ds = new DataSet();
            Workbook wb = new Workbook(stream);
            foreach (Worksheet ws in wb.Worksheets)
            {
                DataTable dt = new DataTable();
                dt.TableName = ws.Name;
                if (ws.Cells.Count > 0)
                {
                    dt = ws.Cells.ExportDataTableAsString(0, 0, ws.Cells.MaxDataRow + 1, ws.Cells.MaxDataColumn + 1, true);
                }
                ds.Tables.Add(dt);
            }
            return ds;
        }

        /// <summary>
        /// 将DataTable保存成07格式的文件的二进制byte[]
        /// </summary>
        /// <param name="dts"></param>
        /// <returns></returns>
        public static byte[] FromDataTable(DataTable[] dts)
        {
            return FromDataTable(dts, SaveFormat.Xlsx);
        }

        /// <summary>
        /// 将DataTable保存成指定格式的文件的二进制byte[]
        /// </summary>
        /// <param name="dts"></param>
        /// <param name="fileFormat">文件格式</param>
        /// <returns></returns>
        public static byte[] FromDataTable(DataTable[] dts, SaveFormat saveFormat)
        {
            if (dts == null) return null;
            try
            {
                Workbook wb = new Workbook();
                wb.Worksheets.Clear();
                for (int i = 0; i < dts.Length; i++)
                {
                    DataTable dt = dts[i];
                    Worksheet ws = wb.Worksheets[wb.Worksheets.Add()];
                    if (dt == null || dt.TableName == "") ws.Name = string.Format("Sheet{0}", i + 1);
                    else ws.Name = dt.TableName;
                    if (dt != null)
                    {
                        Cells cells = ws.Cells;
                        //首行字段名
                        for (int iColumn = 0; iColumn < dt.Columns.Count; iColumn++)
                        {
                            cells[0, iColumn].PutValue(dt.Columns[iColumn].ColumnName);
                        }
                        //内容
                        for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                        {
                            for (int iColumn = 0; iColumn < dt.Columns.Count; iColumn++)
                            {
                                cells[iRow + 1, iColumn].PutValue(string.Format("{0}", dt.Rows[iRow][iColumn]));
                            }
                        }

                        //表格格式
                        Aspose.Cells.Style style = wb.Styles[wb.Styles.Add()];
                        style.Font.Size = 11;
                        style.HorizontalAlignment = TextAlignmentType.Center;
                        style.VerticalAlignment = TextAlignmentType.Center;
                        style.IsTextWrapped = true;

                        style.Custom = "@";
                        style.Font.IsBold = false;
                        //表格边框
                        style.Borders[Aspose.Cells.BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                        style.Borders[Aspose.Cells.BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                        style.Borders[Aspose.Cells.BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                        style.Borders[Aspose.Cells.BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                        Aspose.Cells.Range range = cells.CreateRange(0, 0, cells.MaxRow + 1, cells.MaxColumn + 1);
                        range.SetStyle(style);
                        //表头加粗
                        style.Font.IsBold = true;
                        range = cells.CreateRange(0, 0, 1, dt.Columns.Count);
                        range.SetStyle(style);

                        //设置列宽
                        ws.AutoFitColumns(0, dt.Columns.Count);
                        for (int iColumn = 0; iColumn < dt.Columns.Count; iColumn++)
                        {
                            if (cells.GetColumnWidth(iColumn) > 40)
                            {
                                cells.SetColumnWidth(iColumn, 40);
                                //Aspose.Cells.Style style2 = wb.Styles[wb.Styles.Add()];
                                //style2.IsTextWrapped = true;
                                //Aspose.Cells.Range range2 = cells.CreateRange(0, iColumn,dt.Rows.Count, 1);
                                //range2.SetStyle(style2);
                            }
                            else
                            {
                                cells.SetColumnWidth(iColumn, cells.GetColumnWidth(iColumn) + 6);
                            }
                        }

                        //冻结第一行
                        //ws.FreezePanes(1, 1, 1, 0);

                    }

                }
                Stream sem = new MemoryStream();
                wb.Save(sem, (Aspose.Cells.SaveFormat)saveFormat);
                return sem.ToByteArray();
            }
            catch
            {
                return null;
            }
        }
        public static byte[] FromDataTable(DataTable dt)
        {
            return FromDataTable(dt, SaveFormat.Xlsx);
        }
        /// <summary>
        /// 将DataTable保存成指定格式的文件的二进制byte[]
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="saveFormat"></param>
        /// <returns></returns>
        public static byte[] FromDataTable(DataTable dt, SaveFormat saveFormat)
        {
            return FromDataTable(new DataTable[] { dt }, saveFormat);
        }
        /// <summary>
        /// 将DataSet保存成07格式的文件的二进制byte[]
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public static byte[] FromDataSet(DataSet ds)
        {
            return FromDataSet(ds, SaveFormat.Xlsx);
        }
        /// <summary>
        /// 将DataSet保存成指定格式的文件的二进制byte[]
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="saveFormat"></param>
        /// <returns></returns>
        public static byte[] FromDataSet(DataSet ds, SaveFormat saveFormat)
        {
            DataTable[] dts = new DataTable[ds.Tables.Count];
            ds.Tables.CopyTo(dts, 0);
            return FromDataTable(dts, saveFormat);
        }

        /// <summary>
        /// 通过iDataReader保存成指定格式的文件的二进制byte[]
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static byte[] FromIDataReader(IDataReader reader)
        {
            return FromIDataReader(reader, SaveFormat.Xlsx, null);
        }
        public static byte[] FromIDataReader(IDataReader reader, SaveFormat saveFormat)
        {
            return FromIDataReader(reader, saveFormat, null);
        }
        /// <summary>
        /// 通过iDataReader保存成指定格式的文件的二进制byte[]
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="report">报告当前读取数量</param>
        /// <returns></returns>
        public static byte[] FromIDataReader(IDataReader reader, Action<int> report)
        {
            return FromIDataReader(reader, SaveFormat.Xlsx, report);
        }
        /// <summary>
        /// 通过iDataReader保存成指定格式的文件的二进制byte[]
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="saveFormat"></param>
        /// <param name="report">delete 报告当前读取数量</param>
        /// <returns></returns>
        public static byte[] FromIDataReader(IDataReader reader, SaveFormat saveFormat, Action<int> report)
        {
            if (reader == null) return null;
            try
            {
                Workbook wb = new Workbook();
                wb.Worksheets.Clear();
                Worksheet ws = wb.Worksheets[wb.Worksheets.Add()];
                int count = reader.FieldCount;
                Cells cells = ws.Cells;
                //首行字段名
                for (int iColumn = 0; iColumn < count; iColumn++)
                {
                    cells[0, iColumn].PutValue(reader.GetName(iColumn));
                }
                int iRow = 0;
                //内容
                while (reader.Read())
                {
                    iRow++;
                    for (int iColumn = 0; iColumn < count; iColumn++)
                    {
                        cells[iRow, iColumn].PutValue(reader.GetValue(iColumn));
                    }
                    report?.BeginInvoke(iRow, null, null);
                }

                //表格格式
                Aspose.Cells.Style style = wb.Styles[wb.Styles.Add()];
                style.Font.Size = 11;
                style.HorizontalAlignment = TextAlignmentType.Center;
                style.VerticalAlignment = TextAlignmentType.Center;
                style.IsTextWrapped = true;

                style.Custom = "@";
                style.Font.IsBold = false;
                //表格边框
                style.Borders[Aspose.Cells.BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                style.Borders[Aspose.Cells.BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                style.Borders[Aspose.Cells.BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                style.Borders[Aspose.Cells.BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                Aspose.Cells.Range range = cells.CreateRange(0, 0, cells.MaxRow + 1, cells.MaxColumn + 1);
                range.SetStyle(style);
                //表头加粗
                style.Font.IsBold = true;
                range = cells.CreateRange(0, 0, 1, count);
                range.SetStyle(style);

                //设置列宽
                ws.AutoFitColumns(0, count);
                for (int iColumn = 0; iColumn < count; iColumn++)
                {
                    if (cells.GetColumnWidth(iColumn) > 40)
                    {
                        cells.SetColumnWidth(iColumn, 40);
                        //Aspose.Cells.Style style2 = wb.Styles[wb.Styles.Add()];
                        //style2.IsTextWrapped = true;
                        //Aspose.Cells.Range range2 = cells.CreateRange(0, iColumn,dt.Rows.Count, 1);
                        //range2.SetStyle(style2);
                    }
                    else
                    {
                        cells.SetColumnWidth(iColumn, cells.GetColumnWidth(iColumn) + 6);
                    }
                }

                //冻结第一行
                //ws.FreezePanes(1, 1, 1, 0);

                Stream sem = new MemoryStream();
                wb.Save(sem, (Aspose.Cells.SaveFormat)saveFormat);
                return sem.ToByteArray();
            }
            catch
            {
                return null;
            }

        }


        /// <summary>
        /// 合并Excel文件所有有内容的表.
        /// </summary>
        /// <param name="excelFiles">excel文件列表,带路径</param>
        /// <param name="mergeFile">合并后文件,带路径</param>
        /// <returns></returns>
        static bool MergeExcle(string[] excelFiles, string mergeFile)
        {
            if (excelFiles == null) return false;
            Workbook newWb = new Workbook();
            newWb.Worksheets.Clear();
            foreach (var file in excelFiles)
            {
                if (file == null) continue;
                string ext = Path.GetExtension(file).ToLower();
                if (ext != ".xls" && ext != ".xlsx") continue;
                if (File.Exists(file))
                {
                    try
                    {
                        Workbook wb = new Workbook(file);
                        foreach (Worksheet ws in wb.Worksheets)
                        {
                            Cells cells = ws.Cells;
                            if (cells.Rows.Count == 0 && cells.Columns.Count == 0)
                                continue;
                            Worksheet newWs = newWb.Worksheets[newWb.Worksheets.Add()];
                            newWs.Name = ws.Name;
                            newWs.Copy(ws);

                        }
                    }
                    catch { }

                }
            }
            try
            {
                newWb.Save(mergeFile);
                return true;
            }
            catch
            {
                File.Delete(mergeFile);
                return false;
            }
        }

    }
}

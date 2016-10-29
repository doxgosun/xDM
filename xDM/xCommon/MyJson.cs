using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using xDM.xCommon.xExtensions;
using System.Web.Script.Serialization;
using System.Collections;

namespace xDM.xCommon
{
    public class MyJson
    {
        /// <summary>
        /// 把C#的类进行json序列化
        /// </summary>
        /// <typeparam name="T">类名</typeparam>
        /// <param name="t">类实例</param>
        /// <returns></returns>
        public static string JsonSerializer<T>(T t)
        {
            return t.JsonSerializer();
        }

        /// <summary>
        /// json反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static T JsonDeserialize<T>(string jsonString)
        {
            return jsonString.JsonDeserialize<T>();

        }

        /// <summary>
        /// Json 字符串 转换为 DataTable数据集合
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static DataTable ToDataTable(string json)
        {
            return json.ToDataTable();
        }

        /// <summary>
        /// DataTable转换成序列化Table json字符串,第一行为字段名,第二行以后为数据
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string DT2TableJson(DataTable dt)
        {
            StringBuilder sbDT = new StringBuilder();
            sbDT.Append("[");
            try
            {
                sbDT.Append("[");
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    sbDT.Append("\"" + dt.Columns[i].ColumnName.Replace("\"", "\\\"") + "\"");
                    if (i != dt.Columns.Count - 1)
                    {
                        sbDT.Append(",");
                    }
                }
                int rowcount = dt.Rows.Count;
                if (rowcount == 0) sbDT.Append("]");
                else  sbDT.Append("],");
                for (int i = 0; i < rowcount; i++)
                {
                    sbDT.Append("[");
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        sbDT.AppendFormat("\"{0}\"",string.Format("{0}",dt.Rows[i][j]).Replace("\"", "\\\""));
                        if (j != dt.Columns.Count - 1)
                        {
                            sbDT.Append(",");
                        }
                    }
                    if (i == rowcount - 1) sbDT.Append("]");
                    else sbDT.Append("],");
                }
                sbDT.Append("]");
                return ConvertSpecialChar(sbDT.ToString());
            }
            catch
            {
                return "";
            }
        }


        /// <summary>
        /// DataTable转换成序列化json字符串
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string DT2Json(DataTable dt)
        {
            StringBuilder sbDT = new StringBuilder();
            sbDT.Append("[");
            try
            {
                int rowcount = dt.Rows.Count;
                for (int i = 0; i < rowcount; i++)
                {
                    sbDT.Append("{");
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        //sbDT.Append("\"" + dt.Columns[j].ColumnName + "\":\"" + dt.Rows[i][j].ToString() + "\"");
                        sbDT.AppendFormat("\"{0}\"", string.Format("{0}", dt.Rows[i][j]).Replace("\"", "\\\""));
                        if (j != dt.Columns.Count - 1)
                        {
                            sbDT.Append(",");
                        }
                    }
                    if (i == rowcount - 1) sbDT.Append("}");
                    else sbDT.Append("},");

                }
                sbDT.Append("]");
                return ConvertSpecialChar(sbDT.ToString());
            }
            catch
            {
                return "";
            }
        }


        #region dataTable转换成dhxgrid用的Json格式 
        /// <summary>
        /// dataTable转换成dhxgrid用的Json格式  
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="IdIndex">id列的序号</param>
        /// <returns></returns>
        public static string DataTable2DhtmlGridJson(DataTable dt, int IdIndex)
        {
            if (dt == null || IdIndex < 0 || IdIndex > dt.Columns.Count - 1) return "";
            StringBuilder sbDT = new StringBuilder();
            sbDT.Append("{");
            //总数
            sbDT.Append("\"total\":" + dt.Rows.Count.ToString());
            sbDT.Append(",");
            //表头字段名
            sbDT.Append("\"header\":\"");
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                sbDT.Append(dt.Columns[i].ColumnName.Replace("\"", "\\\""));
                if (i != dt.Columns.Count - 1)
                {
                    sbDT.Append(",");
                }
            }
            //sbDT.Remove(sbDT.Length - 1, 1);
            sbDT.Append("\",");

            //内容
            sbDT.Append("\"rows\":[");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                sbDT.Append("{\"id\":");
                sbDT.Append(dt.Rows[i][IdIndex]);
                sbDT.Append(",data:[");
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    //if (j == IdIndex) continue;
                    sbDT.Append("\"");
                    sbDT.Append(string.Format("{0}",dt.Rows[i][j]).Replace("\"", "\\\""));
                    sbDT.Append("\",");
                }
                sbDT.Remove(sbDT.Length - 1, 1);
                sbDT.Append("]");
                sbDT.Append("},");
            }
            sbDT.Remove(sbDT.Length - 1, 1);

            sbDT.Append("]");
            sbDT.Append("}");
            return ConvertSpecialChar(sbDT.ToString());
        }
        /// <summary>  
        /// dataTable转换成dhxgrid用的Json格式  
        /// </summary>  
        /// <param name="dt"></param>  
        /// <returns></returns>  
        public static string DataTable2DhtmlGridJson(DataTable dt)
        {
            StringBuilder sbDT = new StringBuilder();
            sbDT.Append("{");
            //总数
            sbDT.Append("\"total\":" + dt.Rows.Count.ToString());
            sbDT.Append(",");
            //表头字段名
            sbDT.Append("\"header\":\"");
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                sbDT.Append(dt.Columns[i].ColumnName.Replace("\"", "\\\""));
                if (i != dt.Columns.Count - 1)
                {
                    sbDT.Append(",");
                }
            }
            //sbDT.Remove(sbDT.Length - 1, 1);
            sbDT.Append("\",");

            //内容
            sbDT.Append("\"rows\":[");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                sbDT.Append("{\"id\":");
                sbDT.Append(i + 1);
                sbDT.Append(",data:[");
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    sbDT.Append("\"");
                    sbDT.Append(string.Format("{0}", dt.Rows[i][j]).Replace("\"", "\\\""));
                    sbDT.Append("\",");
                }
                sbDT.Remove(sbDT.Length - 1, 1);
                sbDT.Append("]");
                sbDT.Append("},");
            }
            sbDT.Remove(sbDT.Length - 1, 1);

            sbDT.Append("]");
            sbDT.Append("}");
            return ConvertSpecialChar(sbDT.ToString());
        }

        #endregion dataTable转换成dhxgrid用的Json格式 

        #region dhxgrid用的Json格式转换成dataTable


        #endregion dhxgrid用的Json格式转换成dataTable

        /// <summary>
        /// 转换JSON里的特殊字符  \t \r \n
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ConvertSpecialChar(string input)
        {
            if (input == null) return "";
            else return input.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
        }

    }
}

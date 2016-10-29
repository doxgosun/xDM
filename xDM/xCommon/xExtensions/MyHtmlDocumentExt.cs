using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace xDM.xCommon.xExtensions
{
    public static class MyHtmlDocumentExt
    { 
        /// <summary>
        /// 检索指定class的所有HtmlDocument对象
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        public static HtmlElement[] GetElementsByClassName(this HtmlDocument doc, string className)
        {
            HtmlElementCollection collection = doc.All;
            List<HtmlElement> elementList = new List<HtmlElement>();
            try
            {
                Regex regCls = new Regex(string.Format(@"(\s|^).*{0}(\s|$).*", className));

                foreach (HtmlElement he in collection)
                {
                    string clsName = he.GetAttribute("classname");
                    if (clsName != "" && regCls.IsMatch(clsName))
                    {
                        elementList.Add(he);
                    }
                }
            }
            catch { }
            return elementList.ToArray();

        }
    }
}

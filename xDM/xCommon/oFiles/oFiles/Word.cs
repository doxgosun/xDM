using Aspose.Words;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace xDM.xCommon.oFiles
{
    public class Word
    {
        /// <summary>
        /// 把HTML转换成word格式，HTML里各项必须用绝对路径
        /// </summary>
        /// <param name="strHtml">HTML文件内容，必须用绝对路径</param>
        /// <returns>DOC的byte[]</returns>
        public static byte[] FromHtml(string html)
        {
            try
            {
                Document doc = new Document();
                DocumentBuilder buderHtml = new DocumentBuilder(doc);
                buderHtml.InsertHtml(html);
                foreach (Section section in doc)
                {
                    foreach (Body body in section)
                    {
                        foreach (object obj in body)
                        {
                            try
                            {
                                Paragraph p = (Paragraph)obj;
                                p.ParagraphFormat.SpaceAfter = 0;
                                p.ParagraphFormat.SpaceBefore = 0;
                            }
                            catch { }
                        }
                    }
                }
                Stream sem = new MemoryStream();
                doc.Save(sem, Aspose.Words.SaveFormat.Mhtml);
                doc = new Document(sem);
                Stream semDoc = new MemoryStream();
                doc.Save(semDoc, Aspose.Words.SaveFormat.Doc);
                return semDoc.ToByteArray();
            }
            catch
            {
                return null;
            }

        }

        public static string ToHtml(string wordFileName)
        {
            Document doc = new Document(wordFileName);
            Stream sem = new MemoryStream();
            doc.Save(sem, SaveFormat.Mhtml);
            doc = new Document(sem);
            doc.Save("z:/1.html", SaveFormat.Html);
            StreamWriter sw = new StreamWriter(sem);
            string strHtml = "";
            sw.Write(strHtml);
            return "";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace xDM.xCommon
{
    public class MyImage
    {
        /// <summary>
        /// 指定文字转换图片
        /// </summary>
        /// <param name="content">文字内容</param>
        /// <param name="backgroupColor">背景色</param>
        /// <param name="fontColor">文件颜色</param>
        /// <param name="font"> 默认为 Arial, 15.5f, FontStyle.Bold</param>
        /// <param name="width">默认为文本长度 * 18</param>
        /// <param name="height">默认为30</param>
        /// <returns></returns>
        public static Bitmap CreateImage(string content,Color backgroupColor, Color fontColor, Font font,int width,int height)
        {
            //判断字符串不等于空和null
            if (content == null || content.Trim() == String.Empty)
                return null;
            if (backgroupColor == null)
            {
                backgroupColor = Color.SeaGreen;
            }
            if (fontColor == null)
            {
                fontColor = Color.Red;
            }
            if (font == null)
            {
                font = new Font("Arial", 15.5f, (FontStyle.Bold));
            }
            if (width <= 0)
            {
                width = (int)Math.Ceiling((content.Length * 18.0));
            }
            if (height <= 0)
            {
                height = 30;
            }
            //创建一个位图对象
            Bitmap image = new Bitmap(width,height);
            //创建Graphics
            using (Graphics g = Graphics.FromImage(image))
            {
                try
                {
                    //清空图片背景颜色
                    g.Clear(backgroupColor);

                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                    System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height),fontColor,fontColor, 1f, true);
                    g.DrawString(content, font, brush, -font.Size / 4, 0);
                    //画图片的边框线
                    //g.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);
                }
                finally
                {
                    g.Dispose();
                }
            }
            return image;
        }
    }
}

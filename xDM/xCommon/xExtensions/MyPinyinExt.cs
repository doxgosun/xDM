using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDM.xCommon.xExtensions
{
    public static class MyPinyinExt
    {
        #region 转换汉字为拼音
        /// <summary>
        /// 转换成拼音
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToPinyin(this string input)
        {
            return PinYin.ToPinyin(input);
        }

        /// <summary>
        /// 转换成拼音，各字之间用指定字符分割
        /// </summary>
        /// <param name="input"></param>
        /// <param name="split">指定分割的字符</param>
        /// <returns></returns>
        public static string ToPinyin(this string input, string split)
        {
            return PinYin.ToPinyin(input, split);
        }
        /// <summary>
        /// 转换成带音调的拼音
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToPin1Yin1(this string input)
        {
            return PinYin.ToPin1Yin1(input);
        }
        public static string ToPin1Yin1(this string input, string split)
        {
            return PinYin.ToPin1Yin1(input, split);
        }
        /// <summary>
        /// 转换成声母缩写
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToPinyinInitials(this string input)
        {
            return PinYin.ToPinyinInitials(input);
        }
        public static string ToPinyinInitials(this string input, string split)
        {
            return PinYin.ToPinyinInitials(input, split);
        }
        /// <summary>
        /// 转换成拼音缩写
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToShortPinyinInitials(this string input)
        {
            return PinYin.ToShortPinyinInitials(input);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="split"></param>
        /// <returns></returns>
        public static string ToShortPinyinInitials(this string input, string split)
        {
            return PinYin.ToShortPinyinInitials(input, split);
        }
        #endregion

    }
}

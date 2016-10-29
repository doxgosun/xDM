using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xDM.xCommon.xExtensions
{
    public static class MyDateTimeExt
    {
        /// <summary>
        /// UTC绝对秒数，时间假定为UTC时间
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long ToTime_t(this DateTime time)
        {
            return time.ToTime_t(0);
        }

        /// <summary>
        /// 根据时区返回UTC绝对秒数
        /// </summary>
        /// <param name="time"></param>
        /// <param name="timeZone">所在时区，-12 到 12，范围之外则为0</param>
        /// <returns></returns>
        public static long ToTime_t(this DateTime time, int timeZone)
        {
            if (Math.Abs(timeZone) > 12)
            {
                timeZone = 0;
            }
            DateTime time_19700101 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan ts = time - time_19700101;
            return long.Parse(ts.TotalSeconds.ToString("0")) + timeZone * 3600;

        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.Xml;
using System.Xml.Serialization;
using System.Management;
using System.Diagnostics;

namespace xDM.xCommon
{
    public class common
    {
        /// <summary>
        /// 关闭进程
        /// </summary>
        /// <param name="PID"></param>
        public static void CloseProcess(int PID)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = "cmd";
            proc.StartInfo.Arguments = string.Format("/C taskkill /PID {0} /F", PID);
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.Start();
            proc.WaitForExit();
        }

        public static void CloseProcess(string ProcessName)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = "cmd";
            proc.StartInfo.Arguments = string.Format("/C taskkill /IM {0} /F", ProcessName);
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.Start();
            proc.WaitForExit();
        }
    }
}

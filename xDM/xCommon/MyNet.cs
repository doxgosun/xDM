using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace xDM.xCommon
{
    public class MyNet
    {
        public class NetWorkSetting
        {
            public static Dictionary<string,string[]> GetDNS()
            {
                Dictionary<string, string[]> dns = new Dictionary<string,string[]>();
                using (ManagementClass wmi = new ManagementClass("Win32_NetworkAdapterConfiguration"))
                {
                    using (ManagementObjectCollection moc = wmi.GetInstances())
                    {
                        List<string[]> dnss = new List<string[]>();
                        foreach (var n in moc)
                        {
                            string tttt = "";
                            if (Convert.ToBoolean(n["IPEnabled"]) == true)
                                tttt = n.Properties["NetConnectionID"].Value.ToString();
                           // var ppp = n.;
                            
                        }
                    }
                }
                return dns;
            }

            public static string[] GetIPAddress()
            {
                string[] ip = null;
                using (ManagementClass wmi = new ManagementClass("Win32_NetworkAdapterConfiguration"))
                {
                    using (ManagementObjectCollection moc = wmi.GetInstances())
                    {

                    }
                }
                return ip;
            }

            public static string[] GetGateway()
            {
                string[] gateway = null;
                using (ManagementClass wmi = new ManagementClass("Win32_NetworkAdapterConfiguration"))
                {
                    using (ManagementObjectCollection moc = wmi.GetInstances())
                    {

                    }
                }
                return gateway;
            }

            /// <summary>
            /// 设置DNS
            /// </summary>
            /// <param name="dns"></param>
            public static void SetDNS(string[] dns)
            {
                SetIPAddress(null, null, null, dns);
            }
            /// <summary>
            /// 设置网关
            /// </summary>
            /// <param name="getway"></param>
            public static void SetGateway(string getway)
            {
                SetIPAddress(null, null, new string[] { getway }, null);
            }
            /// <summary>
            /// 设置网关
            /// </summary>
            /// <param name="gateway"></param>
            public static void SetGateway(string[] gateway)
            {
                SetIPAddress(null, null, gateway, null);
            }
            /// <summary>
            /// 设置IP地址和掩码
            /// </summary>
            /// <param name="ip"></param>
            /// <param name="submask"></param>
            public static void SetIPAddress(string ip, string submask)
            {
                SetIPAddress(new string[] { ip }, new string[] { submask }, null, null);
            }
            /// <summary>
            /// 设置IP地址，掩码和网关
            /// </summary>
            /// <param name="ip"></param>
            /// <param name="submask"></param>
            /// <param name="gateway"></param>
            public static void SetIPAddress(string ip, string submask, string gateway)
            {
                SetIPAddress(new string[] { ip }, new string[] { submask }, new string[] { gateway }, null);
            }
            /// <summary>
            /// 设置IP地址，掩码，网关和DNS
            /// </summary>
            /// <param name="ip"></param>
            /// <param name="submask"></param>
            /// <param name="getway"></param>
            /// <param name="dns"></param>
            public static void SetIPAddress(string[] ip, string[] submask, string[] gateway, string[] dns)
            {
                using (ManagementClass wmi = new ManagementClass("Win32_NetworkAdapterConfiguration"))
                {
                    using (ManagementObjectCollection moc = wmi.GetInstances())
                    {
                        ManagementBaseObject inPar = null;
                        ManagementBaseObject outPar = null;
                        foreach (ManagementObject mo in moc)
                        {
                            //如果没有启用IP设置的网络设备则跳过
                            if (!(bool)mo["IPEnabled"])
                                continue;

                            //设置IP地址和掩码
                            if (ip != null && submask != null)
                            {
                                inPar = mo.GetMethodParameters("EnableStatic");
                                inPar["IPAddress"] = ip;
                                inPar["SubnetMask"] = submask;
                                outPar = mo.InvokeMethod("EnableStatic", inPar, null);
                            }

                            //设置网关地址
                            if (gateway != null)
                            {
                                inPar = mo.GetMethodParameters("SetGateways");
                                inPar["DefaultIPGateway"] = gateway;
                                outPar = mo.InvokeMethod("SetGateways", inPar, null);
                            }

                            //设置DNS地址
                            if (dns != null)
                            {
                                inPar = mo.GetMethodParameters("SetDNSServerSearchOrder");
                                inPar["DNSServerSearchOrder"] = dns;
                                outPar = mo.InvokeMethod("SetDNSServerSearchOrder", inPar, null);
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// 启用DHCP服务器
            /// </summary>
            public static void EnableDHCP()
            {
                using (ManagementClass wmi = new ManagementClass("Win32_NetworkAdapterConfiguration"))
                {
                    using (ManagementObjectCollection moc = wmi.GetInstances())
                    {
                        foreach (ManagementObject mo in moc)
                        {
                            //如果没有启用IP设置的网络设备则跳过
                            if (!(bool)mo["IPEnabled"])
                                continue;
                            //重置DNS为空
                            mo.InvokeMethod("SetDNSServerSearchOrder", null);
                            //开启DHCP
                            mo.InvokeMethod("EnableDHCP", null);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 判断是否符合IP地址格式
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsIPAddress(string ip)
        {
            //将完整的IP以“.”为界限分组
            string[] arr = ip.Split('.');


            //判断IP是否为四组数组成
            if (arr.Length != 4)
                return false;


            //正则表达式，1~3位整数
            string pattern = @"\d{1,3}";
            for (int i = 0; i < arr.Length; i++)
            {
                string d = arr[i];


                //判断IP开头是否为0
                if (i == 0 && d == "0")
                    return false;


                //判断IP是否是由1~3位数组成
                if (!Regex.IsMatch(d, pattern))
                    return false;

                if (d != "0")
                {
                    //判断IP的每组数是否全为0
                    d = d.TrimStart('0');
                    if (d == "")
                        return false;

                    //判断IP每组数是否大于255
                    if (int.Parse(d) > 255)
                        return false;
                }
            } return true;
        }

        #region IP和值转换
        public static long IP2Value(string ip)
        {
            string[] sIP = ip.Split('.');
            if (sIP.Length != 4) return 0;
            try
            {
                long ip0 = 0, ip1 = 0, ip2 = 0, ip3 = 0;
                if (long.TryParse(sIP[0], out ip0) && long.TryParse(sIP[1], out ip1) && long.TryParse(sIP[2], out ip2) && long.TryParse(sIP[3], out ip3))
                {
                    return (ip0 << 24) + (ip1 << 16) + (ip2 << 8) + ip3;
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        public static string Value2IP(long IPValue)
        {
            try
            {
                long ip3 = IPValue & 255;
                IPValue = IPValue >> 8;
                long ip2 = IPValue & 255;
                IPValue = IPValue >> 8;
                long ip1 = IPValue & 255;
                IPValue = IPValue >> 8;
                long ip0 = IPValue & 255;
                return string.Format("{0}.{1}.{2}.{3}", ip0, ip1, ip2, ip3);
            }
            catch
            {
                return "";
            }
        }
        #endregion

    }
}

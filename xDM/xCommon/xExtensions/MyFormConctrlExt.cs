using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace xDM.xCommon.xExtensions
{
    public static class MyFormConctrlExt
    {
        public static T[] GetControlsByName<T>(this Control ctl) where T : Control
        {
            return ctl.GetControlsByName<T>(new Regex[] { new Regex(@".*") }, null);
        }
        /// <summary>
        /// 获取符合reg正则的集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ctl"></param>
        /// <param name="reg"></param>
        /// <returns></returns>
        public static T[] GetControlsByName<T>(this Control ctl, Regex reg) where T : Control
        {
            return ctl.GetControlsByName<T>(new Regex[] { reg },null);
        }

        /// <summary>
        /// 获取同时符合正则数组的集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ctl"></param>
        /// <param name="andRegs"></param>
        /// <returns></returns>
        public static T[] GetControlsByName<T>(this Control ctl, Regex[] andRegs) where T : Control
        {
            return ctl.GetControlsByName<T>(andRegs,null);
        }

        /// <summary>
        /// 获取同时符合正则数组,并排除符合不在正则数组的集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ctl"></param>
        /// <param name="andRegs"></param>
        /// <param name="notInRegs"></param>
        /// <returns></returns>
        public static T[] GetControlsByName<T>(this Control ctl, Regex[] andRegs,Regex[] notInRegs) where T : Control
        {
            if (ctl == null) return null;
            Regex[] _andRegs = null;
            if (andRegs == null) _andRegs = new Regex[] { new Regex(@".*") };
            else _andRegs = andRegs.ToArray();
            Regex[] _notInRegs = null;
            if (notInRegs == null) _notInRegs = new Regex[0];
            else _notInRegs = notInRegs.ToArray();
            var cList = ctl._GetControlsByName<T>(_andRegs[0]).ToList();
            for (int i = 0; i < cList.Count; i++)
            {
                var c = cList[i];
                bool _continue = false;
                for (int j = 1; j < _andRegs.Length; j++)
                {
                    if (!_andRegs[j].IsMatch(c.Name))
                    {
                        cList.RemoveAt(i--);
                        _continue = true;
                        break;
                    }
                }
                if (_continue) continue;
                for (int k = 0; k < _notInRegs.Length; k++)
                {
                    if (_notInRegs[k].IsMatch(c.Name))
                    {
                        cList.RemoveAt(i--);
                        break;
                    }
                }
            }
            return cList.ToArray();
        }


        private static IEnumerable<T> _GetControlsByName<T>(this Control ctl, Regex reg) where T : Control
        {
            if (ctl == null || reg == null) yield break;
            if (ctl.Controls.Count > 0)
                foreach (Control c in ctl.Controls)
                {
                    var cs = _GetControlsByName<T>(c, reg);
                    foreach (var item in cs)
                        yield return item;
                }
            if (ctl is T && reg.IsMatch(ctl.Name))
                yield return (T)ctl;
            yield break;
        }

        public static T GetControlByName<T>(this Control ctl, string name) where T : Control
        {
            var ts = ctl.GetControlsByName<T>(new Regex(name));
            if (ts.Length > 0)
            {
                return ts[0];
            }
            return null;
        }

        /// <summary>
        /// 获取最上层类型为T的父控件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ctl"></param>
        /// <returns></returns>
        public static Control GetBaseParent<T>(this Control ctl) where T : Control
        {
            T parent = null;
            Control c = ctl;
            while (c.Parent != null)
            {
                if (c.Parent is T)
                    parent = c.Parent as T;
                c = c.Parent;
            }
            return c;
        }

        public static Control[] GetAllParents<T>(this Control ctl) where T : Control
        {
            List<Control> list = new List<Control>();
            Control parent = ctl.Parent;
            while (parent != null)
            {
                if(parent is T)
                    list.Add(parent);
                parent = parent.Parent;
            }
            return list.ToArray();
        }
    }
}

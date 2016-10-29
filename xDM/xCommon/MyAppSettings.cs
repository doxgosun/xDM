using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace xDM.xCommon
{
    public class MyAppSettings
    {
        public static void SaveConfig(string key, object value)
        {
            if (key == null) return;
            var val = string.Format("{0}", value);
            Configuration cnf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (cnf.AppSettings.Settings[key] != null)
                cnf.AppSettings.Settings[key].Value = val;
            else cnf.AppSettings.Settings.Add(key, val);
            cnf.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        public static string GetConfig(string key)
        {
            Configuration cnf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            return cnf?.AppSettings?.Settings[key]?.Value;
        }
    }
}

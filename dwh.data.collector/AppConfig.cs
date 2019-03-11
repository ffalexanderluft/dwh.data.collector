using dwh.data.collector.Helperclasses;
using dwh.data.collector.Propertyclasses;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace dwh.data.collector
{
    static class AppConfig
    {
        static Logger Nlogger = LogManager.GetCurrentClassLogger();
        public static string GetString(string key, string default_value = "", string section = "appSettings",string file = "appConfig.json")
        {
            try
            {
                string[] _section = section.Split(',');
                IConfigurationSection _config = null;
                IConfigurationRoot config = new ConfigurationBuilder()
                    .SetBasePath(GlobalProps._configPath)
                    .AddJsonFile(file, optional: true, reloadOnChange: true)
                    .Build();

                for (int i = 0; i <= (_section.Length - 1); i++)
                {
                    if (i == 0)
                    {
                        _config = config.GetSection(_section[i]);
                    }
                    else
                    {
                        _config = _config.GetSection(_section[i]);
                    }
                }

                string result = _config.GetValue<string>(key, default_value);
                return result == "" ? default_value : result;
            }
            catch (Exception ex)
            {
                string err = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                //if (System.Diagnostics.Debugger.IsAttached == true) { Console.WriteLine(err); }
                Nlogger.Error(err);
                return default_value;
            }
        }

        public static int GetInt(string key, int default_value = 0, string section = "appSettings",string file = "appConfig.json")
        {
            try
            {
                string[] _section = section.Split(',');
                IConfigurationSection _config = null;
                IConfigurationRoot config = new ConfigurationBuilder()
                    .SetBasePath(GlobalProps._configPath)
                    .AddJsonFile(file, optional: true, reloadOnChange: true)
                    .Build();


                for (int i = 0; i <= (_section.Length - 1); i++)
                {
                    if (i == 0)
                    {
                        _config = config.GetSection(_section[i]);
                    }
                    else
                    {
                        _config = _config.GetSection(_section[i]);
                    }
                }

                int result = Convert.ToInt32(_config.GetValue<int>(key, default_value));
                return (result == 0 && default_value != 0) ? Convert.ToInt32(default_value) : result;
            }
            catch (Exception ex)
            {
                string err = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                //if (System.Diagnostics.Debugger.IsAttached == true) { Console.WriteLine(err); }
                Nlogger.Error(err);
                return default_value;
            }
        }

        public static bool GetBool(string key, bool default_value = false, string section = "appSettings",string file = "appConfig.json")
        {
            try
            {
                string[] _section = section.Split(',');
                IConfigurationSection _config = null;
                IConfigurationRoot config = new ConfigurationBuilder()
                    .SetBasePath(GlobalProps._configPath)
                    .AddJsonFile(file, optional: true, reloadOnChange: true)
                    .Build();

                for (int i = 0; i <= (_section.Length - 1); i++)
                {
                    if (i == 0)
                    {
                        _config = config.GetSection(_section[i]);
                    }
                    else
                    {
                        _config = _config.GetSection(_section[i]);
                    }
                }

                return _config.GetValue<bool>(key, default_value);
            }
            catch (Exception ex)
            {
                string err = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                //if (System.Diagnostics.Debugger.IsAttached == true) { Console.WriteLine(err); }
                Nlogger.Error(err);
                return default_value;
            }
        }

        public static List<JSONKeyValue<T>> GetValues<T>(string section = "appSettings", string file = "appConfig.json")
        {
            try
            {
                string[] _section = section.Split(',');
                List<JSONKeyValue<T>> _list = new List<JSONKeyValue<T>>();
                IConfigurationSection _config = null;
                IConfigurationRoot config = new ConfigurationBuilder()
                    .SetBasePath(GlobalProps._configPath)
                    .AddJsonFile(file, optional: true, reloadOnChange: true)
                    .Build();

                for (int i = 0; i <= (_section.Length - 1); i++)
                {
                    if (i == 0)
                    {
                        _config = config.GetSection(_section[i]);
                    }
                    else
                    {
                        _config = _config.GetSection(_section[i]);
                    }
                }

                foreach (ConfigurationSection _child in _config.GetChildren())
                {
                            
                    JSONKeyValue<T> _values = new JSONKeyValue<T>() { _key = _child.Key, _value = _config.GetValue<T>(_child.Key),_hasChildren=_child.HasChildren() };
                    _list.Add(_values);
                }

                return _list;

            }
            catch (Exception ex)
            {
                string err = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                //if (System.Diagnostics.Debugger.IsAttached == true) { Console.WriteLine(err); }
                Nlogger.Error(err);
                return null;
            }
        }

        public static List<configValues> GetValues4googleAnalytics(string file)
        {
            List<configValues> values = new List<configValues>();
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(GlobalProps._configPath)
                .AddJsonFile(file, optional: true, reloadOnChange: true)
                .Build();
            foreach (var _s in config.GetChildren())
            {
                foreach (var _cs in config.GetSection(_s.Key).GetChildren())
                {
                    configValues _cv = new configValues();
                    foreach (var _ics in _cs.GetChildren())
                    {
                        _cv._metric = _ics.Key == "metric" ? _ics.Value : _cv._metric;
                        _cv._dimension = _ics.Key == "dimensions" ? _ics.Value.Split(",") : _cv._dimension;
                        _cv._enabled = _ics.Key == "enabled" ? Convert.ToBoolean(_ics.Value) : _cv._enabled;
                        //System.Diagnostics.Debugger.Break();
                    }
                    if (_cv._enabled == true) { values.Add(_cv); }
                    //System.Diagnostics.Debugger.Break();
                }
            }
            //System.Diagnostics.Debugger.Break();
            return values;
        }

        public class configValues
        {
            public string _metric { get; set; }
            public string[] _dimension { get; set; }
            public bool _enabled { get; set; }

        }

    }
}

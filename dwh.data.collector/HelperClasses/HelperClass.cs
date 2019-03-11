using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.Odbc;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Linq;
using dwh.data.collector.Propertyclasses;
using dwh.data.collector.RedShift;
using dwh.data.collector.Config;

namespace dwh.data.collector
{
    public static class HelperClass
    {
        public static List<SqlConnection> lCn = new List<SqlConnection>();

        static Logger Nlogger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Get assembly path
        /// http://codebuckets.com/2017/10/19/getting-the-root-directory-path-for-net-core-applications/
        /// </summary>
        /// <returns></returns>
        public static string GetApplicationRoot()
        {
            try
            {
                var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
                Regex appPathMatcher = new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*");
                var appRoot = appPathMatcher.Match(exePath).Value;
                return appRoot;
            }
            catch (Exception ex)
            {
                string err = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                Nlogger.Error(err);
                return "";
            }
        }

        public static void GetConfigPath(string subfolder = "Config",string fileName = "")
        {
            try
            {
                GlobalProps._configPath = Path.Combine(Path.GetDirectoryName(typeof(AppConfig).Assembly.Location), subfolder, fileName);
            }
            catch (Exception ex)
            {
                string err = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                Nlogger.Error(err);
            }
        }

        public static string GetTagetDBServer()
        {
            string _dbserver = "";
            try
            {
                switch (AppConfig.GetString("environment"))
                {
                    case "dev":
                        _dbserver = AppConfig.GetString("DWHTEST");
                        break;
                    case "prod":
                        _dbserver = AppConfig.GetString("DWHPROD");
                        break;
                    case "local":
                        _dbserver = AppConfig.GetString("DWHLOCAL");
                        break;
                    case "rds":
                        _dbserver = AppConfig.GetString("DWHAWS");
                        break;
                    case "redshift":
                        _dbserver = AppConfig.GetString("server","","AWSRedshift");
                        GlobalProps._isRedshift = true;  
                        break;
                    default:
                        _dbserver = AppConfig.GetString("DWHTEST");
                        break;
                }
                GlobalProps._SQL_instance = _dbserver;
                return _dbserver;
            }
            catch (Exception ex)
            {
                string err = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                Nlogger.Error(err);
                return _dbserver;
            }
        }

        public static IEnumerable<DateTime> EachDay(DateTime start_date, DateTime end_date)
        {
            for (DateTime day = start_date.Date; day.Date <= end_date.Date; day = day.AddDays(1))
            yield return day;
        }

        public static void LogIt(string msg,string source)
        {
            try
            {
                string file = string.Format("{0}\\LOG_{1}.log", GlobalProps._configPath, DateTime.Now.ToShortDateString());
                StreamWriter sw = new StreamWriter(file, true);
                sw.WriteLine(string.Format("{0}: {1}", source, msg));
                sw.WriteLine(new string('-', 50));
                sw.Close();
            }
            catch (Exception ex) { }
        }

        public static void ClearCN()
        {
            foreach (SqlConnection _cn in lCn)
            {
                try
                {
                    _cn.Close();
                }
                catch (Exception ex)
                {
                    string err = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                    if (AppConfig.GetBool("Log2Disk", false) == true) { HelperClass.LogIt(err, MethodBase.GetCurrentMethod().Name); }
                }
            }
            lCn.Clear();
        }
        /// <summary>
        /// Get dataformat of the taget sql server
        /// and store the target sql instance.
        /// Msut be invoked one time at beginning at least
        /// </summary>
        public static void GetDataBaseDateFormat()
        {
            try
            {
                GetConfigPath();
                GetTagetDBServer();
                switch (GlobalProps._isRedshift)
                {
                    case true:
                        GetRedshiftDateFormat();
                        break;
                    case false:
                        GetSQLServerDateFormat();
                        break;
                }
            }
            catch (Exception ex)
            {
                string err = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                Nlogger.Error(err);
            }
        }

        static void GetSQLServerDateFormat()
        {
            try
            {
                using (SqlConnection _cn = new SqlConnection(string.Format(AppConfig.GetString("dbconnection"), HelperClass.GetTagetDBServer())))
                {
                    HelperClass.lCn.Add(_cn);
                    SqlCommand sqlcmd = new SqlCommand("select dateformat,name from syslanguages where name = @@language", _cn)
                    {
                        CommandType = CommandType.Text,
                        CommandTimeout = 180
                    };
                    _cn.Open();
                    SqlDataReader _dr = sqlcmd.ExecuteReader();
                    if (_dr.HasRows == true)
                    {
                        _dr.Read();
                        switch (_dr.GetString(1))
                        {
                            case "Deutsch":
                                GlobalProps._ci = new CultureInfo("de-De");
                                break;
                            case "us_englisch":
                            case "us_english":
                                GlobalProps._ci = new CultureInfo("en-US");
                                break;
                            default:
                                GlobalProps._ci = CultureInfo.InvariantCulture;
                                break;
                        }
                    }
                    else
                    {
                        GlobalProps._ci = CultureInfo.InvariantCulture;
                    }
                    GlobalProps._dateformat = GlobalProps._ci.DateTimeFormat;
                }
            }
            catch (Exception ex)
            {
                string err = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                Nlogger.Error(err);
            }
        }

        static void GetRedshiftDateFormat()
        {
            try
            {
                using (RedshiftWorker sql = new RedshiftWorker("PSQL_salesdashboard"))
                {
                    OdbcDataAdapter da = new OdbcDataAdapter();DataTable dt = new DataTable();
                    sql.GetDT("SHOW DATESTYLE", ref da, ref dt);
                    string[] _settings = dt.Rows[0][0].ToString().Split(',');
                    switch (_settings[1].Trim())
                    {
                        case "MDY":
                            GlobalProps._ci = new CultureInfo("de-De");
                            break;
                        default:
                            GlobalProps._ci = CultureInfo.InvariantCulture;
                            break;
                    }
                    GlobalProps._dateformat = GlobalProps._ci.DateTimeFormat;
                }
            }
            catch (Exception ex)
            {
                string err = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                Nlogger.Error(err);
                GlobalProps._ci = CultureInfo.InvariantCulture;
                GlobalProps._dateformat = GlobalProps._ci.DateTimeFormat;
            }
        }

        //public static Type GetJTokenType(JToken value)
        //{
        //    try
        //    {
        //        switch (value.Type)
        //        {
        //            case JTokenType.Integer:
        //                return Type.GetType("System.Int64");
        //            case JTokenType.Boolean:
        //                return Type.GetType("System.Boolean");
        //            case JTokenType.Date:
        //                return Type.GetType("System.DateTime");
        //            default:
        //                return Type.GetType("System.String");
        //        }
        //    }
        //    catch (Exception ex) {return Type.GetType("System.String"); }
        //}

        public static string ConsoleCaption(string addtionalInfo = "")
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string _caption = string.Format("{0} V {1}.{2} (Max. Threads: {3})", fvi.ProductName, fvi.ProductMajorPart, fvi.ProductMinorPart, (AppConfig.GetBool("singleThreading", false) == false) ? AppConfig.GetString("maxThreadsAllowed") : "- single threaded mode");
            if (addtionalInfo != "")
            {
                _caption = string.Concat(_caption, " - ", addtionalInfo);
            }
            return _caption; 
        }
        
        public static int ColumnIndex(DataRow dr,string search)
        {
            List<int> _columnIndex = dr.Table.Columns.Cast<DataColumn>()
            .Where(column => column.ColumnName == search)
            .Select(column => column.Ordinal).ToList();
            return _columnIndex[0];
        }

    }
}

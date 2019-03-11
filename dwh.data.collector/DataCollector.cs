using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Diagnostics;
using NLog;
using System.Reflection;
using dwh.data.collector.Propertyclasses;
using dwh.data.collector.SQL;

namespace dwh.data.collector.ZenDesk
{
    class DataCollector:IDisposable
    {
        private string _item { get; set; }
        private string _database { get; set; }
        readonly Logger Nlogger = LogManager.GetCurrentClassLogger();
        static cThreading _th = new cThreading(AppConfig.GetInt("maxThreadsPerCore", 2), AppConfig.GetInt("maxThreadsAllowed", 10), ProcessPriorityClass.Normal, AppConfig.GetBool("useAllCores"));
        static readonly int curThreads = _th.activeThreads();

        public DataCollector(string database,string item)
        {
            this._database = database;
            this._item = item;
        }

        public void getData<T>()
        {
            try
            {
                int _count = 0;
                int _page = 1;
                string _columns;
                StringBuilder _sb = new StringBuilder();

                List<JSONKeyValue<T>> _list = AppConfig.GetValues<T>(string.Format("data,{0}",this._item),string.Concat(this._database,".json"));
                string[] _JSONPath = new string[_list.Count];
                string[] _fields = new string[_list.Count];

                foreach (JSONKeyValue<T> _obj in _list)
                {
                    _sb.Append(_obj._key).Append(",");
                    _fields[_count] = _obj._key;
                    _JSONPath[_count] = _obj._value.ToString();
                    _count++;
                }

                _count = 0;
                _columns = _sb.ToString();

                Console.WriteLine(string.Format("writing {0} from {1} to database.........",this._item,this._database));

                do
                {
                    cJSON getData = new cJSON(AppConfig.GetString("url_domain", "", "config",string.Concat(this._database,".json")),
                    String.Format(AppConfig.GetString(this._item,"", "Config,request_url",string.Concat(this._database,".json")), _page.ToString()),
                    AppConfig.GetString("username", "", "config",string.Concat(this._database,".json")), AppConfig.GetString("password", "", "config",string.Concat(this._database,".json")));
                    DataTable dtOrganizations = getData.GetDataTable(_JSONPath, _fields);
                    //System.Diagnostics.Debugger.Break();
                    if (dtOrganizations == null) { break; }
                    _count = dtOrganizations.Rows.Count;
                    if (AppConfig.GetBool("multiThreading") == true)
                    {
                        foreach (DataRow drOrganizations in dtOrganizations.Rows)
                        {
                            int _columnIndex = HelperClass.ColumnIndex(drOrganizations, "id");
                            objSQL _params = new objSQL(drOrganizations, string.Format("Select {1}imported,lastupdate from dbo.{2} where Id = {0}", drOrganizations[_columnIndex].ToString(), _columns,this._item), _fields);
                            WaitCallback wi = new WaitCallback(new cSQL()._zendesk);
                            _th.add2queue(ref wi, _params);
                        }
                        while (_th.activeThreads() > curThreads)
                        {
                            //if (Debugger.IsAttached == true) { Console.WriteLine("{0} threads active....", (_th.activeThreads() - curThreads).ToString()); }
                        }
                    }
                    else
                    {
                        foreach (DataRow drOrganizations in dtOrganizations.Rows)
                        {
                            int _columnIndex = HelperClass.ColumnIndex(drOrganizations, "id");
                            objSQL _params = new objSQL(drOrganizations, string.Format("Select {1}imported,lastupdate from dbo.{2} where Id = {0}", drOrganizations[_columnIndex].ToString(), _columns,this._item), _fields);
                            cSQL wi = new cSQL();
                            wi._zendesk(_params);
                        }
                    }
                    _page++;
                } while (_count != 0);
            }
            catch (Exception ex)
            {
                string err = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                //if (Debugger.IsAttached == true) { Console.WriteLine(err); }
                Nlogger.Error(err);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // Dient zur Erkennung redundanter Aufrufe.

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: verwalteten Zustand (verwaltete Objekte) entsorgen.
                }

                // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten überschreiben.
                // TODO: große Felder auf Null setzen.

                disposedValue = true;
            }
        }

        // TODO: Finalizer nur überschreiben, wenn Dispose(bool disposing) weiter oben Code für die Freigabe nicht verwalteter Ressourcen enthält.
        // ~ZenDeskData() {
        //   // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
        //   Dispose(false);
        // }

        // Dieser Code wird hinzugefügt, um das Dispose-Muster richtig zu implementieren.
        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
            Dispose(true);
            // TODO: Auskommentierung der folgenden Zeile aufheben, wenn der Finalizer weiter oben überschrieben wird.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}

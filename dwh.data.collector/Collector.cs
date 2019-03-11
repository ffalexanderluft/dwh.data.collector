using Microsoft.Extensions.Hosting;
using NLog;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using dwh.data.collector.Propertyclasses;
using dwh.data.collector.ZenDesk;

namespace dwh.data.collector
{
    class Collector:IDisposable,IHostedService
    {
        Logger Nlogger = LogManager.GetCurrentClassLogger();
        private Timer _timer;
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger Nlogger = LogManager.GetCurrentClassLogger();
            try
            {
                //Detect culture of target db server
                HelperClass.GetDataBaseDateFormat();
                _timer = new Timer(
                    (e) => _work("ZenDesk"),
                    null,
                    TimeSpan.Zero,
                    TimeSpan.FromMinutes(AppConfig.GetInt("Timer", 30,"config","ZenDesk.json")));
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                string err = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                //if (Debugger.IsAttached == true) { Console.WriteLine(err); }
                Nlogger.Error(err);
                return null;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Logger Nlogger = LogManager.GetCurrentClassLogger();
            try
            {
                _timer?.Change(Timeout.Infinite, 0);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                string err = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                //if (Debugger.IsAttached == true) { Console.WriteLine(err); }
                Nlogger.Error(err);
                return null;
            }
        }

        static void _work(string database)
        {
            Logger Nlogger = LogManager.GetCurrentClassLogger();
            try
            {
                List <JSONKeyValue<object>> _list = AppConfig.GetValues<object>("data",string.Concat(database,".json"));

                foreach (JSONKeyValue<object> _obj in _list)
                {
                    if (_obj._hasChildren == true)
                    {
                        DateTime[] _dt = new DateTime[2]; _dt[0] = DateTime.Now;
                        using (DataCollector organizations = new DataCollector(database, _obj._key)) { organizations.getData<object>(); }
                        _dt[1] = DateTime.Now;
                        TimeSpan _duration = _dt[1] - _dt[0];
                        Nlogger.Info(String.Format("{0} processed - duration {1}:{2}:{3}", _obj._key, _duration.Hours.ToString("00"), _duration.Minutes.ToString("00"), _duration.Seconds.ToString("00")));
                    }
                }
                //using (ZenDeskData organizations = new ZenDeskData("organizations")) { organizations.getData<object>(); }
                //using (ZenDeskData organizations = new ZenDeskData("tickets")) { organizations.getData<object>(); }
                //using (ZenDeskData organizations = new ZenDeskData("ticket_metrics")) { organizations.getData<object>(); }
                //using (ZenDeskData organizations = new ZenDeskData("users")) { organizations.getData<object>(); }
                //using (ZenDeskData organizations = new ZenDeskData("groups")) { organizations.getData<object>(); }
                Nlogger.Info(String.Format("Waiting for next run; timer scheduled with {0} minutes", AppConfig.GetInt("Timer", 30).ToString()));
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
        // ~DataCollector() {
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

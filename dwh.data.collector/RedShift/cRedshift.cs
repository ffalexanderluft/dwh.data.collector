using System;
using NLog;
using System.Data;
using System.Data.Odbc;
using System.Reflection;
using System.Globalization;
using System.Linq;
using dwh.data.collector.Propertyclasses;

namespace dwh.data.collector.RedShift
{
    public class cRedshift:IDisposable
    {
        readonly Logger Nlogger = LogManager.GetCurrentClassLogger();

        public void _zendesk(object _p)
        {
            objSQL _params;
            int i;
            try
            {
                _params = (objSQL)_p;
                using (cRedshiftWorker sql = new cRedshiftWorker("PSQL_salesdashboard"))
                {
                    DataTable dt = new DataTable(); OdbcDataAdapter da = new OdbcDataAdapter(); DataRow dr;
                    dt.TableName = _params._tablename;
                    sql.GetDT(_params._sql, ref da, ref dt);
                    if (dt.Rows.Count == 0)
                    {
                        dr = dt.NewRow();
                        for (i = 0; i < _params._fields.Count(); i++)
                        {
                            //if (_params._dr[i].ToString().Replace("[", "").Replace("]", "").Length > 0)
                            if (_params._dr[i].ToString().Length > 0)
                            {
                                //buffer = @_params._dr[i].ToString().Replace("[", "").Replace("]", "");
                                //dr[i] = buffer.IsNumeric() == true ? (object)buffer.String2Float(2) : (object)buffer;
                                dr[i] = @_params._dr[i];
                            }
                        }
                        dr["dsn"] = Guid.NewGuid();
                        dr["imported"] = DateTime.Now;
                        dt.Rows.Add(dr);
                    }
                    else
                    {
                        dr = dt.Rows[0];
                        for (i = 0; i < _params._fields.Count(); i++)
                        {
                            if (_params._dr[i].ToString().Replace("[", "").Replace("]", "").Length > 0)
                            {
                                //buffer = @_params._dr[i].ToString().Replace("[", "").Replace("]", "");
                                //dr[i] = buffer.IsNumeric() == true ? (object)buffer.String2Float(2) : (object)buffer;
                                dr[i] = @_params._dr[i];
                            }
                        }
                        dr["lastupdate"] = DateTime.Now;
                    }
                    sql.UpdateDT(ref da, ref dt);
                }
            }
            catch (Exception ex)
            {
                string err = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                //if (Debugger.IsAttached == true) { Console.WriteLine(err); }
                Nlogger.Error(err);
            }
        }

        /// <summary>
        /// because AWS redshift doesn't support clacluetd functions dt_values are set by update
        /// </summary>
        public void _dtUpdate()
        {
            try
            {
                using (cRedshiftWorker sql = new cRedshiftWorker("PSQL_salesdashboard"))
                {
                    sql.ExcuteSQL(AppConfig.GetString("update_dt_dealstage","", "AWSRedshift_Statements"));
                    sql.ExcuteSQL(AppConfig.GetString("update_dt_created", "", "AWSRedshift_Statements"));
                }
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
        // ~cRedshift() {
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

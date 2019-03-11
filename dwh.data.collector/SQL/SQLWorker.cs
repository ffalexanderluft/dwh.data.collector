using System;
using System.Data;
using System.Data.SqlClient;
using NLog;
using System.Reflection;

namespace dwh.data.collector.SQL
{
    class SQLWorker:IDisposable
    {
        readonly Logger Nlogger = LogManager.GetCurrentClassLogger();

        string _connstr { get; set; }
        string _sqlcmd { get; set; }
        int _timeout { get; set; }
        SqlDataAdapter _da { get; set; }
        SqlCommandBuilder _cb { get; set; }

        public bool GetDT(string sqlstring,ref SqlDataAdapter da,ref DataTable dt)
        {
            this._sqlcmd = sqlstring;
            try
            {
                using (SqlConnection _cn = new SqlConnection(this._connstr))
                {
                    HelperClass.lCn.Add(_cn);
                    _cn.Open();
                    SqlCommand sqlcmd = new SqlCommand(this._sqlcmd,_cn)
                    {
                        CommandType = CommandType.Text,
                        CommandTimeout = _timeout
                    };
                    this._da = new SqlDataAdapter(sqlcmd);
                    this._cb = new SqlCommandBuilder(this._da);
                    dt.Clear();
                    this._da.Fill(dt);
                    da = this._da;
                    _cn.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                string err = string.Format("{0}: {1}{2}SQL: {3}", MethodBase.GetCurrentMethod().Name, ex.ToString(), System.Environment.NewLine, this._sqlcmd);
                //if ( Debugger.IsAttached == true ) { Console.WriteLine(err); }
                Nlogger.Error(err);
                return false;
            }
        }

        public bool UpdateDT(ref SqlDataAdapter da, ref DataTable dt,string modus = "")
        {
            try
            {
                using (SqlConnection _cn = new SqlConnection(this._connstr))
                {
                    int _counter = 0;
                    HelperClass.lCn.Add(_cn);
                    this._cb = new SqlCommandBuilder(da);
                    da.SelectCommand.Connection = _cn;
                    while (_counter <= 20)
                    {
                        try
                        {
                            da.Update(dt);
                            //Nlogger.Info(string.Format("Update table [{0}] successfull", dt.TableName));
                            break;
                        }
                        catch (System.Data.DBConcurrencyException)
                        {
                            _counter = _counter + 1;
                            Nlogger.Info(String.Format("{0} Roundtrip {1}", modus == ""?"":modus, _counter.ToString()));
                            System.Threading.Thread.Sleep(500);
                        }
                        catch (InvalidOperationException ex)
                        {
                            _counter = _counter + 1;
                            Nlogger.Info(String.Format("{0} Roundtrip {1}", modus == "" ? "" : modus, _counter.ToString()));
                            System.Threading.Thread.Sleep(500);
                        }
                    }
                    _cn.Close();
                    _cn.Dispose();
                    return true;
                }
            }
            catch (Exception ex)
            {
                string err = string.Format("{0}: {1}{2}SQL: {3}", MethodBase.GetCurrentMethod().Name, ex.ToString(), System.Environment.NewLine, this._sqlcmd);
                //if (Debugger.IsAttached == true) { Console.WriteLine(err); }
                Nlogger.Error(err);
                return false;
            }
        }

        public bool ExcuteSQL(string sqlExecute)
        {
            try
            {
                using (SqlConnection _cn = new SqlConnection(this._connstr))
                {
                    HelperClass.lCn.Add(_cn);
                    _cn.Open();
                    SqlCommand sqlcmd = new SqlCommand(sqlExecute, _cn)
                    {
                        CommandType = CommandType.Text,
                        CommandTimeout = _timeout
                    };
                    sqlcmd.ExecuteNonQuery();
                    _cn.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                string err = string.Format("{0}: {1}{2}SQL: {3}", MethodBase.GetCurrentMethod().Name, ex.ToString(), System.Environment.NewLine, this._sqlcmd);
                //if (Debugger.IsAttached == true) { Console.WriteLine(err); }
                Nlogger.Error(err);
                return false;
            }
        }

        public SQLWorker(string connstr,int timeout = 30)
        {
            this._connstr = connstr;
            //this._nlogger = LogManager.GetCurrentClassLogger();
            this._timeout = timeout;
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
        // ~Class1() {
        //   // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
        //   Dispose(false);
        // }

        // Dieser Code wird hinzugefügt, um das Dispose-Muster richtig zu implementieren.
        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
            Dispose(true);
            // TODO: Auskommentierung der folgenden Zeile aufheben, wenn der Finalizer weiter oben überschrieben wird.
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}

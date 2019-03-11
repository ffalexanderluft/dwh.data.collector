using dwh.data.collector.Config;
using NLog;
using System;
using System.Data;
using System.Data.Odbc;
using System.Reflection;

namespace dwh.data.collector.RedShift
{
    public class RedshiftWorker:IDisposable
    {
        readonly Logger Nlogger = LogManager.GetCurrentClassLogger();
        public string _database { get; set; }
        public string _connstr { get; set; }
        public string _sqlcmd { get; set; }
        public int _timeout { get; set; }
        OdbcDataAdapter _da { get; set; }
        OdbcCommandBuilder _cb { get; set; }

        public void GetDT(string sqlstring,ref OdbcDataAdapter da, ref DataTable dt)
        {
            this._sqlcmd = sqlstring;
            try
            {
                using (OdbcConnection _cn = new OdbcConnection(this._connstr))
                {
                    _cn.Open();
                    OdbcCommand sqlcmd = new OdbcCommand(this._sqlcmd, _cn)
                    {
                        CommandType = CommandType.Text,
                        CommandTimeout = _timeout
                    };
                    this._da = new OdbcDataAdapter(sqlcmd);
                    this._cb = new OdbcCommandBuilder(this._da);
                    dt.Clear();
                    this._da.Fill(dt);
                    da = this._da;
                    _cn.Close();
                    _cn.Dispose();
                }
            }
            catch (Exception ex)
            {
                string err = string.Format("{0}: {1}{2}SQL: {3}", MethodBase.GetCurrentMethod().Name, ex.ToString(), System.Environment.NewLine, this._sqlcmd);
                //if ( Debugger.IsAttached == true ) { Console.WriteLine(err); }
                Nlogger.Error(err);
            }
        }

        public bool ExcuteSQL(string sqlExecute)
        {
            try
            {
                using (OdbcConnection _cn = new OdbcConnection(this._connstr))
                {
                    _cn.Open();
                    OdbcCommand sqlcmd = new OdbcCommand(sqlExecute, _cn)
                    {
                        CommandType = CommandType.Text,
                        CommandTimeout = _timeout
                    };
                    sqlcmd.ExecuteNonQuery();
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

        public void UpdateDT(ref OdbcDataAdapter da, ref DataTable dt)
        {
            try
            {
                using (OdbcConnection _cn = new OdbcConnection(this._connstr))
                {
                    int _counter = 0;
                    this._cb = new OdbcCommandBuilder(da);
                    da.SelectCommand.Connection = _cn;
                    while (_counter <= 20)
                    {
                        try
                        {
                            da.Update(dt);
                            Nlogger.Info(string.Format("Update table [{0}] successfull",dt.TableName));
                            break;
                        }
                        catch (System.Data.DBConcurrencyException)
                        {
                            _counter = _counter + 1;
                            Nlogger.Info(String.Format("Roundtrip {0}", _counter.ToString()));
                            System.Threading.Thread.Sleep(500);
                        }
                    }
                    
                    _cn.Close();
                    _cn.Dispose();
                }
            }
            catch (Exception ex)
            {
                string err = string.Format("{0}: {1}{2}SQL: {3}", MethodBase.GetCurrentMethod().Name, ex.ToString(), System.Environment.NewLine, this._sqlcmd);
                //if (Debugger.IsAttached == true) { Console.WriteLine(err); }
                Nlogger.Error(err);
            }
        }

        #region constructor
        public RedshiftWorker(string database,int timeout = 60)
        {
            try
            {
                string driver = AppConfig.GetString("driver", "", "AWSRedshift");
                string server = AppConfig.GetString("server", "", "AWSRedshift");
                string port = AppConfig.GetString("port", "", "AWSRedshift");
                string masteruser = AppConfig.GetString("masteruser", "", "AWSRedshift");
                string password = AppConfig.GetString("password", "", "AWSRedshift");

                this._database = database;
                this._timeout = timeout;
                this._connstr = AppConfig.GetString("connstr", "", "AWSRedshift");

                this._connstr = string.Format(this._connstr,driver,server,this._database, masteruser,password,port);
            }
            catch (Exception ex)
            {
                string err = string.Format("{0}: {1}{2}SQL: {3}", MethodBase.GetCurrentMethod().Name, ex.ToString(), System.Environment.NewLine, this._sqlcmd);
                //if ( Debugger.IsAttached == true ) { Console.WriteLine(err); }
                Nlogger.Error(err);
            }
        }

        public RedshiftWorker(string dsn)
        {
            this._connstr = string.Format("DSN={0}",dsn);
        }

        #endregion

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
        // ~cRedshiftWorker() {
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

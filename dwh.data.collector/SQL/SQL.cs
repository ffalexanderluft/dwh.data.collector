﻿using dwh.data.collector.Config;
using dwh.data.collector.Helperclasses;
using dwh.data.collector.Propertyclasses;
using NLog;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace dwh.data.collector.SQL
{
    class SQL : IDisposable
    {
        static Logger Nlogger = LogManager.GetCurrentClassLogger();
        public void Dispose()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_p"></param>
        public void _work(object _p)
        {
            objSQL _params;
            int i;
            string _buffer;
            bool _update = false;

            try
            {
                _params = (objSQL)_p;
                using (SQLWorker sql = new SQLWorker(string.Format(AppConfig.GetString("dbConnection"), GlobalProps._SQL_instance)))
                {
                    DataTable _dt = new DataTable(); SqlDataAdapter _da = new SqlDataAdapter(); DataRow[] _dr = new DataRow[2];
                    if (sql.GetDT(_params._sql, ref _da, ref _dt) == false) { throw new System.Data.DataException("GetDT did not succeed!"); };
                    if (_dt.Rows.Count == 0)
                    {
                        _dr[0] = _dt.NewRow();
                        for (i = 0; i < _params._fields.Count(); i++)
                        {
                            if (_params._dr[i].ToString().Replace("[", "").Replace("]", "").Length > 0)
                            {
                                //_buffer = @_params._dr[i].ToString().Replace("[", "").Replace("]", "").Trim();
                                //_dr[0][i] = _buffer.IsNumeric() == true ? (object)_buffer.String2Float(2) : (object)_buffer;
                                _dr[0][i] = _params._dr[i];
                            }
                        }
                        _dr[0]["imported"] = DateTime.Now;
                        _dt.Rows.Add(_dr[0]);
                    }
                    else
                    {
                        _dr[0] = _dt.Rows[0];_dr[1] = _dt.Rows[0];
                        for (i = 0; i < _params._fields.Count(); i++)
                        {
                            if (_params._dr[i].ToString().Replace("[", "").Replace("]", "").Length > 0)
                            {
                                //_buffer = @_params._dr[i].ToString().Replace("[", "").Replace("]", "");
                                //_dr[0][i] = _buffer.IsNumeric() == true ? (object)_buffer.String2Float(2) : (object)_buffer;
                                _dr[0][i] = _params._dr[i];
                                if (_dr[0][i].ToString() != _dr[1][i].ToString()) { _update = true; }
                            }
                        }
                        if (_update == true) { _dr[0]["lastupdate"] = DateTime.Now; _dr[0]["s3_flag"] = false;  }
                    }
                    sql.UpdateDT(ref _da, ref _dt); 
                }
            }
            catch (Exception ex)
            {
                string err = string.Format("{0} - {1}{2}{3}", ((objSQL)_p)._sql, ex.ToString(), Environment.NewLine, MethodBase.GetCurrentMethod().Name);
                Nlogger.Error(err);
            }
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using ServiceStack;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;
using NLog;
using dwh.data.collector.Propertyclasses;

namespace dwh.data.collector
{
    class cJSON
    {

        static Logger Nlogger = LogManager.GetCurrentClassLogger();

        string _url;
        string _request;
        static IJsonServiceClient client;
        JObject _rss;
        
        /// <summary>
        /// creates a datatable from the JSON result
        /// </summary>
        /// <param name="JSONPath">specific path th tarnsalte into datatable</param>
        /// <param name="fields">desired fields</param>
        /// <param name="verbose">true if you want an output in the console</param>
        /// <returns></returns>
        
        public static T GetValue<T>(JProperty _jp)
        {
            return (T)Convert.ChangeType(_jp.Value, typeof(T));
        }

        public DataTable GetDataTable(string[] JSONPath, string[] fields, bool verbose = false)
        {
            try
            {
                var result = this._rss.SelectTokens(JSONPath[0]);
                if (result.Count() > 0)
                {
                    //define datatable
                    DataTable dt = new DataTable();
                    for (int c = 0; c < fields.Count(); c++)
                    {
                        dt.Columns.Add(fields[c]);
                    }
                    //loop through result
                    for (int i = 0; i < result.Count(); i++)
                    {
                        DataRow dr = dt.NewRow();
                        for (int j = 0; j < JSONPath.Count(); j++)
                        {
                            JToken ret = this._rss.SelectToken(JSONPath[j].Replace("*", i.ToString()));
                            try
                            {
                                JProperty _jp = ret.Parent.ToObject<JProperty>();
                                if (dt.Columns.Contains(_jp.Name) == true)
                                {
                                    dr[_jp.Name] = _jp.Value;
                                }
                            }
                            catch (Exception ex)
                            {
                                string err = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                                Nlogger.Error(err);
                            }
                        }
                        dt.Rows.Add(dr);
                    }
                    return dt;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                string err = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                Nlogger.Error(err);
                return null;
            }
        }

        /// <summary>
        /// create a JSON clientrequest object
        /// post the request
        /// and parse the result
        /// </summary>
        /// <param name="url">basic url of rest service</param>
        /// <param name="request">specific request parameter of the API</param>
        public cJSON(string url, string request)
        {
            this._url = url;
            this._request = request;
            client = new JsonServiceClient(url);
            this._rss = JObject.Parse(client.Get<String>(this._request));
        }

        public cJSON(string url, string request,string username,string password)
        {
            this._url = url;
            this._request = request;
            client = new JsonServiceClient(url)
            {
                UserName = username,
                Password = password,
            };
            this._rss = JObject.Parse(client.Get<String>(this._request));
        }

        /// <summary>
        /// get a special value of the response stream
        /// </summary>
        /// <returns>id string for net request</returns>
        public string getValue(string value)
    {
            try
            {
                string ret = (string)this._rss[value];
                return ret;
            }
            catch (Exception ex)
            {
                return "";
            }
        
    }

    }

   
}

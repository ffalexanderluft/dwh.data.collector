using System.Data;

namespace dwh.data.collector.Propertyclasses
{
    class objSQL
    {
        public string _sql { get; set; }
        public string[] _fields { get; set; }
        public DataRow _dr;
        public string _tablename { get; set; }

        public objSQL(DataRow dr,string sql,string[] fields,string tablename = "")
        {
            this._sql = sql;
            this._fields = fields;
            this._dr = dr;
            this._tablename = tablename;
        }
    }
}

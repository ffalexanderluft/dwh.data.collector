using System;
using System.Collections.Generic;
using System.Text;

namespace dwh.data.collector.Propertyclasses
{
    class JSONKeyValue<T>
    {
        public string _key { get; set; }
        public T _value { get; set; }
        public bool _hasChildren { get; set; }
    }
}

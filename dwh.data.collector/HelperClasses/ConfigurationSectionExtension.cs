using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;


namespace dwh.data.collector.Helperclasses
{
    public static class ConfigurationSectionExtension
    {
        public static bool HasChildren(this ConfigurationSection c )
        {
            try
            {
               IEnumerable<IConfigurationSection> _list = c.GetChildren();
               return _list.GetEnumerator().MoveNext();
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

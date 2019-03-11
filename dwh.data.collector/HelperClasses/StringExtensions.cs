using System;

namespace dwh.data.collector.Helperclasses
{
    public static class StringExtensions
    {
        public static bool IsNumeric(this string s)
        {
            float ret;
            return float.TryParse(s, out ret);
        }

        public static Double String2Float(this string s,int digits =0)
        {
            double ret;
            ret = Convert.ToDouble(s.Replace(".",","));
            if (digits != 0) { ret = Math.Round(ret, digits);  }
            return ret;
        }

    }
}

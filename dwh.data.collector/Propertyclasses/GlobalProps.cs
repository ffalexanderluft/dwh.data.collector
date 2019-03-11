using System.Globalization;

namespace dwh.data.collector.Propertyclasses
{
    /// <summary>
    /// static class to store global variables
    /// </summary>
    public static class GlobalProps
    {
        public static string _SQL_instance { get; set; }
        public static DateTimeFormatInfo _dateformat { get; set; }
        public static CultureInfo _ci { get; set; }
        public static bool _isRedshift { get; set; }
        public static string _configPath { get; set; }
        public static bool _showPendingThreads { get; set; }

        /// <summary>
        /// dummy constructor
        /// </summary>
        static GlobalProps()
        {
           
        }
    }


}

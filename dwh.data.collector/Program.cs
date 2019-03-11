using dwh.data.collector.ServiceHelper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

/// <summary>
/// Sample from https://www.stevejgordon.co.uk/running-net-core-generic-host-applications-as-a-windows-service
/// </summary>

namespace dwh.data.collector
{
    internal class Program
    {
        static Logger Nlogger = LogManager.GetCurrentClassLogger();
        private static async Task Main(string[] args)

        {
            try
            {
                var isService = !(Debugger.IsAttached || args.Contains("--console"));
                var builder = new HostBuilder()
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddHostedService<Collector>();
                    });

                if (isService)
                {
                    Nlogger.Info("run as service");
                    await builder.RunAsServiceAsync();
                }
                else
                {
                    Nlogger.Info("run as console app");
                    await builder.RunConsoleAsync();
                }
            }
            catch (Exception ex)
            {
                string err = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                Nlogger.Error(err);
            }
        }
    }
}

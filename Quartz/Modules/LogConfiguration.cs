using Serilog;
using Serilog.Events;

namespace Quartz.Presentation.Modules
{
    public class LogConfiguration
    {
        public static void SetConfiguration()
        {
            var path = GetLogPath();

            Log.Logger = new LoggerConfiguration()
                   .MinimumLevel.Verbose()
                   .WriteTo.Seq("http://localhost:5341", LogEventLevel.Debug)
                   .WriteTo.RollingFile(path, LogEventLevel.Verbose)
                   .CreateLogger();
        }

        private static string GetLogPath()
        {
            var path = System.AppDomain.CurrentDomain.BaseDirectory + "/App_Data/Logs/LogJobsQuartz.txt";
            return path;
        }
    }
}
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace FUParkingService.Cloudflare
{
    public static class AccessCloudflare
    {
        public static void Access()
        {
            try
            {
                IConfiguration config = new ConfigurationBuilder()
                                              .SetBasePath(Directory.GetCurrentDirectory())
                                              .AddJsonFile("appsettings.json", true, true)
                                              .Build();

                var cf = config.GetSection("AppSettings:Cloudflare").Value;
                if (cf == null)
                {
                    throw new Exception();
                }
                ExecuteCommand(cf);
            }
            catch
            {
                throw new Exception();
            }
        }

        private static void ExecuteCommand(string Command)
        {
            Process process = new();
            ProcessStartInfo startInfo = new();
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C " + Command;
            process.StartInfo = startInfo;
            process.Start();
        }
    }
}


using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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


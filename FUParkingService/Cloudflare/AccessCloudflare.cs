﻿using Microsoft.Extensions.Configuration;
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

                var cf = config.GetSection("AppSettings:Cloudflare").Value ?? throw new Exception();
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
            ProcessStartInfo startInfo = new()
            {
                WindowStyle = ProcessWindowStyle.Normal,
                FileName = "cmd.exe",
                Arguments = "/C " + Command
            };
            process.StartInfo = startInfo;
            process.Start();
        }
    }
}


using System;
using System.Collections.Generic;
using System.Timers;
using Microsoft.Extensions.Configuration;

namespace FortunesCommon
{
    public static class ConfigurationRootExtensions
    {
        private static readonly List<Timer> _timers = new List<Timer>();

        public static IConfigurationRoot AutoRefresh(this IConfigurationRoot config, TimeSpan timeSpan)
        {
            var myTimer = new Timer();
            myTimer.Elapsed += (sender, args) => config.Reload();
            myTimer.Interval = 10000;
            myTimer.Enabled = true;
            _timers.Add(myTimer);
            return config;
        }
    }

}
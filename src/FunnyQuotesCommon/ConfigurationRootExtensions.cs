using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using Microsoft.Extensions.Configuration;
// ReSharper disable CollectionNeverQueried.Local

namespace FunnyQuotesCommon
{
    public static class ConfigurationRootExtensions
    {
        private static readonly List<Timer> Timers = new List<Timer>();

        public static IConfigurationRoot AutoRefresh(this IConfigurationRoot config, TimeSpan timeSpan)
        {
            var myTimer = new Timer();
            myTimer.Elapsed += (sender, args) => config.Reload();
            myTimer.Interval = 10000;
            myTimer.Enabled = true;
            Timers.Add(myTimer);
            return config;
        }
        public static IConfigurationBuilder AddLocalConfigFiles(this IConfigurationBuilder configurationBuilder, string environment)
        {
            // config files always end up in assembly folder, but in IIS based projects assemblies are not in base directory
            var assemblyFileName = Path.GetFileName(typeof(ConfigurationRootExtensions).Assembly.Location);
            var path = File.Exists(Path.Combine(AppContext.BaseDirectory, assemblyFileName)) ? AppContext.BaseDirectory : Path.Combine(AppContext.BaseDirectory, "bin");

            configurationBuilder
                .AddYamlFile(Path.Combine(path, "global-default.yaml"), true)
                .AddYamlFile(Path.Combine(path, $"global-default.{environment}.yaml"), true)
                .AddYamlFile(Path.Combine(path, "appsettings.yaml"), false)
                .AddYamlFile(Path.Combine(path, $"appsettings.{environment}.yaml"), true);
            return configurationBuilder;
        }
    }

}
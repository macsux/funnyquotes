using System;
using System.ServiceProcess;
using Microsoft.Owin.Hosting;

namespace FunnyQuotesOwinWindowsService
{
    public partial class Service1 : ServiceBase
    {
        private IDisposable _webhostLifeline = null!;
        public Service1()
        {
            InitializeComponent();
        }

        public void Start(params string[] args)
        {
            OnStart(args);
        }
        public new void Stop()
        {
            _webhostLifeline.Dispose();
        }
        protected override void OnStart(string[] args)
        {
            if (!int.TryParse(Environment.GetEnvironmentVariable("PORT"), out var port))
                port = 8080;
            var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? $"http://localhost:{port}";
            Console.WriteLine($"Starting web server on {urls}");
            _webhostLifeline = WebApp.Start<Startup>(url: urls);
        }

        protected override void OnStop()
        {
            _webhostLifeline.Dispose();
        }
    }
}

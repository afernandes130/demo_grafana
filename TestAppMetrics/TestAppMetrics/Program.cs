using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using App.Metrics.AspNetCore;
using App.Metrics;
using App.Metrics.Formatters.Prometheus;
using App.Metrics.Filtering;
using App.Metrics.Formatters.InfluxDB;

namespace TestAppMetrics
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var Metrics = AppMetrics.CreateDefaultBuilder()
                .OutputMetrics.AsPrometheusPlainText()
                .OutputMetrics.AsPrometheusProtobuf()
                .Build();

            var filter = new MetricsFilter().WhereType(App.Metrics.MetricType.Timer);
            var metrics = new MetricsBuilder()
                .Report.ToInfluxDb(
                    options => {
                        options.InfluxDb.BaseUri = new Uri("http://127.0.0.1:8086");
                        options.InfluxDb.Database = "defaultdb";
                        options.InfluxDb.Consistenency = "admin";
                        options.InfluxDb.UserName = "admin";
                        options.InfluxDb.Password = "adminpass";
                        options.InfluxDb.RetentionPolicy = "rp";
                        options.InfluxDb.CreateDataBaseIfNotExists = true;
                        options.HttpPolicy.BackoffPeriod = TimeSpan.FromSeconds(30);
                        options.HttpPolicy.FailuresBeforeBackoff = 5;
                        options.HttpPolicy.Timeout = TimeSpan.FromSeconds(10);
                        options.MetricsOutputFormatter = new MetricsInfluxDbLineProtocolOutputFormatter();
                        options.Filter = filter;
                        options.FlushInterval = TimeSpan.FromSeconds(20);
                    })
                .Build();

           return Host.CreateDefaultBuilder(args)
                .ConfigureMetrics(Metrics)
                .UseMetrics(
                            options =>
                            {
                                options.EndpointOptions = endpointsOptions =>
                                {
                                    endpointsOptions.MetricsTextEndpointOutputFormatter = Metrics.OutputMetricsFormatters.OfType<MetricsPrometheusTextOutputFormatter>().First();
                                    endpointsOptions.MetricsEndpointOutputFormatter = Metrics.OutputMetricsFormatters.OfType<MetricsPrometheusProtobufOutputFormatter>().First();
                                };
                            })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        }
            
    }
}

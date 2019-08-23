﻿using Microsoft.AspNetCore.Builder;
using Prometheus;
using System;

namespace Hangfire.Prometheus
{
    public static class Extensions
    {
        /// <summary>
        /// Initializes Prometheus Hangfire Exporter using current Hangfire job storage and default metrics registry.
        /// </summary>
        /// <param name="app">IApplicationBuilder instance</param>
        /// <returns></returns>
        public static IApplicationBuilder UsePrometheusHangfireExporter(this IApplicationBuilder app)
        {
            JobStorage js = (JobStorage)app.ApplicationServices.GetService(typeof(JobStorage));
            if (js == null)
            {
                throw new Exception("Cannot find Hangfire JobStorage class.");
            }
            IHangfireMonitorService hangfireMonitor = new HangfireMonitorService(js);
            IPrometheusExporter exporter = new HangfirePrometheusExporter(hangfireMonitor, Metrics.DefaultRegistry);
            Metrics.DefaultRegistry.AddBeforeCollectCallback(() => exporter.ExportHangfireStatistics());
            return app;
        }
    }
}

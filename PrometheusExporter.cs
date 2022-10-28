using Prometheus;
using System;

namespace Hangfire.Prometheus
{
    public class HangfirePrometheusExporter : IPrometheusExporter
    {
        private readonly IHangfireMonitorService    _hangfireMonitorService;
        private readonly HangfirePrometheusSettings _settings;
        private readonly Gauge                      _hangfireGauge;

        private const string METRIC_NAME     = "hangfire_job_count";
        private const string METRIC_HELP     = "Number of Hangfire jobs";
        private const string STATE_LABEL_NAME = "state";

        private const string FAILED_LABEL_VALUE     = "failed";
        private const string ENQUEUED_LABEL_VALUE   = "enqueued";
        private const string SCHEDULED_LABEL_VALUE  = "scheduled";
        private const string PROCESSING_LABEL_VALUE = "processing";
        private const string SUCCEEDED_LABEL_VALUE  = "succeeded";
        private const string RETRY_LABEL_VALUE      = "retry";

        public HangfirePrometheusExporter(IHangfireMonitorService hangfireMonitorService, HangfirePrometheusSettings settings)
        {
            _hangfireMonitorService = hangfireMonitorService ?? throw new ArgumentNullException(nameof(hangfireMonitorService));
            _settings               = settings;

            CollectorRegistry collectorRegistry = settings.CollectorRegistry;

            _hangfireGauge = Metrics.WithCustomRegistry(collectorRegistry)
                                    .CreateGauge(METRIC_NAME, METRIC_HELP, STATE_LABEL_NAME);
        }

        public void ExportHangfireStatistics()
        {
            try
            {
                HangfireJobStatistics hangfireJobStatistics = _hangfireMonitorService.GetJobStatistics();
                _hangfireGauge.WithLabels(FAILED_LABEL_VALUE).Set(hangfireJobStatistics.Failed);
                _hangfireGauge.WithLabels(SCHEDULED_LABEL_VALUE).Set(hangfireJobStatistics.Scheduled);
                _hangfireGauge.WithLabels(PROCESSING_LABEL_VALUE).Set(hangfireJobStatistics.Processing);
                _hangfireGauge.WithLabels(ENQUEUED_LABEL_VALUE).Set(hangfireJobStatistics.Enqueued);
                _hangfireGauge.WithLabels(SUCCEEDED_LABEL_VALUE).Set(hangfireJobStatistics.Succeeded);
                _hangfireGauge.WithLabels(RETRY_LABEL_VALUE).Set(hangfireJobStatistics.Retry);
            }
            catch (Exception ex)
            {
                if (_settings.FailScrapeOnException)
                {
                    throw new ScrapeFailedException("Scrape failed due to exception. See InnerException for details.", ex);
                }
            }
        }
    }
}

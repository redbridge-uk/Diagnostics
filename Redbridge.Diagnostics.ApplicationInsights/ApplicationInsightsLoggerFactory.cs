using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Redbridge.Configuration;

namespace Redbridge.Diagnostics.ApplicationInsights
{
    public class ApplicationInsightsLoggerFactory : ILoggerFactory
    {
        public const string ApplicationInsightsInstrumentationConfigurationKey = "APPINSIGHTS_INSTRUMENTATIONKEY";
        public const string ApplicationInsightsRoleNameKey = "LogRoleName";
        private readonly IApplicationSettingsRepository _settings;
        private readonly LoggingSettings _loggingSettings;
        private readonly TelemetryClient _client;


        public static ApplicationInsightsLoggerFactory FromConfiguration(IApplicationSettingsRepository settings, LoggingSettings loggingSettings = null)
        {
            var logSettings = loggingSettings ?? LoggingSettings.Default;
            return new ApplicationInsightsLoggerFactory(settings, logSettings);
        }

        public static ApplicationInsightsLoggerFactory Default (LoggingSettings loggingSettings = null)
        {
            var logSettings = loggingSettings ?? LoggingSettings.Default;
            return new ApplicationInsightsLoggerFactory(logSettings);
        }

        private ApplicationInsightsLoggerFactory(LoggingSettings loggingSettings)
        {
            _loggingSettings = loggingSettings ?? throw new ArgumentNullException(nameof(loggingSettings));
            var configuration = TelemetryConfiguration.CreateDefault();
            _client = new TelemetryClient(configuration);
        }

        private ApplicationInsightsLoggerFactory (IApplicationSettingsRepository applicationSettings, LoggingSettings loggingSettings)
        {
            _settings = applicationSettings ?? throw new ArgumentNullException(nameof(applicationSettings));
            _loggingSettings = loggingSettings ?? throw new ArgumentNullException(nameof(loggingSettings));
            var key = applicationSettings.GetStringValueOrDefault(ApplicationInsightsInstrumentationConfigurationKey, string.Empty);
            var role = applicationSettings.GetStringValueOrDefault(ApplicationInsightsRoleNameKey, null);
            var configuration = TelemetryConfiguration.CreateDefault();
            configuration.InstrumentationKey = key;
            _client = new TelemetryClient(configuration) { };
            if (!string.IsNullOrEmpty(role))
            {
                _client.Context.Cloud.RoleName = role;
            }
        }

        public ILogger Create<T>()
        {
            return new ApplicationInsightsLogger<T>(_client, _loggingSettings);
        }
    }
}
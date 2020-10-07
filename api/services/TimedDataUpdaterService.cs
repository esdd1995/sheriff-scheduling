﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SS.Api.Helpers;
using SS.Api.services.JC;
using SS.Db.models;

namespace SS.Api.services
{
    internal class TimedDataUpdaterService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private Timer _timer;
        public IServiceProvider Services { get; }
        private readonly TimeSpan _jcSynchronizationDelay;
        private readonly TimeSpan _jcSynchronizationPeriod;

        public TimedDataUpdaterService(IServiceProvider services, ILogger<TimedDataUpdaterService> logger, IConfiguration configuration)
        {
            Services = services;
            _logger = logger;
            _jcSynchronizationDelay = TimeSpan.Parse(configuration.GetNonEmptyValue("JCSynchronizationDelay"));
            _jcSynchronizationPeriod = TimeSpan.Parse(configuration.GetNonEmptyValue("JCSynchronizationPeriod"));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Timed Background Service is starting with a period of {_jcSynchronizationPeriod}.");
            _logger.LogInformation($"Delaying the timed background service by {_jcSynchronizationDelay} seconds (to allow Migrations to complete).");
            _timer = new Timer(DoWork, null, _jcSynchronizationDelay, _jcSynchronizationPeriod);
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            _logger.LogInformation("Timed Background Service is working.");

            using var scope = Services.CreateScope();
            var justinDataUpdaterService =
                scope.ServiceProvider
                    .GetRequiredService<JCDataUpdaterService>();

            _logger.LogInformation("Syncing Regions.");
            await justinDataUpdaterService.SyncRegions();
            _logger.LogInformation("Syncing Locations.");
            await justinDataUpdaterService.SyncLocations();
            _logger.LogInformation("Syncing CourtRooms.");
            await justinDataUpdaterService.SyncCourtRooms();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}

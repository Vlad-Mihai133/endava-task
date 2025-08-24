using CarInsurance.Api.Data;
using CarInsurance.Api.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CarInsurance.Api.Services
{
    public class PolicyExpirationLogger : IHostedService, IDisposable
    {
        private readonly ILogger<PolicyExpirationLogger> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Timer? _timer;

        public PolicyExpirationLogger(ILogger<PolicyExpirationLogger> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(5)); // ruleaza la fiecare 5 minute
            return Task.CompletedTask;
        }
        private async void DoWork(object? state)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DoWorkAsync(db);
        }

        public async Task DoWorkAsync(AppDbContext db)
        {

            var now = DateTime.UtcNow;

            // policy-uri care au expirat in ultima ora si nu au fost inca procesate
            var expiredPolicies = await db.Policies
            .Where(p => p.EndDate <= now && p.EndDate >= now.AddHours(-1))
            .ToListAsync();

            foreach (var policy in expiredPolicies)
            {
                var alreadyProcessed = await db.ProcessedPolicyExpirations
                    .AnyAsync(x => x.PolicyId == policy.Id);

                if (alreadyProcessed) continue;

                _logger.LogInformation("Policy {PolicyId} expired at {EndDate}", policy.Id, policy.EndDate);

                db.ProcessedPolicyExpirations.Add(new ProcessedPolicyExpiration
                {
                    PolicyId = policy.Id,
                    ProcessedAt = now
                });

                await db.SaveChangesAsync();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}

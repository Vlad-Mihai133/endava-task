using CarInsurance.Api.Data;
using CarInsurance.Api.Models;
using CarInsurance.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarInsurance.Api.Tests
{
    public class PolicyExpirationLoggerTests
    {
        private AppDbContext GetInMemoryDb(out InsurancePolicy policy)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var db = new AppDbContext(options);
            db.Database.EnsureCreated();

            policy = new InsurancePolicy
            {
                CarId = 1,
                StartDate = new DateTime(2025, 1, 1),
                EndDate = DateTime.UtcNow.AddMinutes(-10) // expira azi
            };
            db.Policies.Add(policy);
            db.SaveChanges();

            return db;
        }

        [Fact]
        public async Task DoWorkAsync_LogsAndMarksPolicyProcessed()
        {
            var db = GetInMemoryDb(out var policy);

            var loggerMock = new Mock<ILogger<PolicyExpirationLogger>>();
            var service = new PolicyExpirationLogger(loggerMock.Object, null!);

            await service.DoWorkAsync(db);

            var processed = await db.ProcessedPolicyExpirations.AnyAsync(x => x.PolicyId == policy.Id);
            Assert.True(processed, $"policy enddate: {policy.EndDate}, utcnow: {DateTime.UtcNow}");

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("expired")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once
            );
        }
    }
}

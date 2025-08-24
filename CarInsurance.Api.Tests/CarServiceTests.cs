using CarInsurance.Api.Data;
using CarInsurance.Api.Models;
using CarInsurance.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarInsurance.Api.Tests
{
    public class CarServiceTests
    {
        private AppDbContext GetInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db = new AppDbContext(options);
            db.Database.EnsureCreated();

            var car = new Car { Vin = "VIN123", Make = "Dacia", Model = "Logan", YearOfManufacture = 2018 };
            db.Cars.Add(car);
            db.Policies.Add(new InsurancePolicy { CarId = car.Id, StartDate = new DateTime(2024, 1, 1), EndDate = new DateTime(2024, 12, 31), Provider = "Allianz" });
            db.Claims.Add(new InsuranceClaim { CarId = 1, ClaimDate = new DateTime(2024, 5, 20), Description = "Scratch", Amount = 300 });
            db.SaveChanges();



            return db;
        }

        [Fact]
        public async Task IsInsuranceValidAsync_ThrowsException_WhenCarWithGivenIdDoesNotExist()
        {
            var db = GetInMemoryDb();
            var service = new CarService(db);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.IsInsuranceValidAsync(999, new DateTime(2025, 1, 1))
            );
        }

        [Fact]
        public async Task IsInsuranceValidAsync_ReturnsTrue_WhenInsuranceDateIsWithinPolicy()
        {
            var db = GetInMemoryDb();
            var service = new CarService(db);

            var carId = db.Cars.First().Id;
            var result = await service.IsInsuranceValidAsync(carId, new DateTime(2024, 6, 15));

            Assert.True(result);
        }

        [Fact]
        public async Task IsInsuranceValidAsync_ReturnsFalse_WhenInsuranceDateIsOutsidePolicy()
        {
            var db = GetInMemoryDb();
            var service = new CarService(db);

            var carId = db.Cars.First().Id;
            var result = await service.IsInsuranceValidAsync(carId, new DateTime(2025, 1, 1));

            Assert.False(result);
        }

        [Fact]
        public async Task RegisterInsuranceClaimAsync_ReturnsTrue_WhenClaimIsValid()
        {
            var db = GetInMemoryDb();
            var service = new CarService(db);

            var claim = new InsuranceClaim
            {
                CarId = 1,
                ClaimDate = new DateTime(2024, 6, 1),
                Description = "Test claim",
                Amount = 100
            };

            var result = await service.RegisterInsuranceClaimAsync(claim);

            Assert.True(result);
            Assert.Contains(db.Claims, c => c.Description == "Test claim");
        }

        [Fact]
        public async Task RegisterInsuranceClaimAsync_ThrowsKeyNotFound_WhenCarWithGivenCarIdDoesNotExist()
        {
            var db = GetInMemoryDb();
            var service = new CarService(db);

            var claim = new InsuranceClaim { CarId = 999, ClaimDate = new DateTime(2024, 6, 1), Description = "X", Amount = 50 };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.RegisterInsuranceClaimAsync(claim));
        }

        [Fact]
        public async Task GetCarHistoryAsync_ReturnsHistory_WhenCarWithGivenCarIdExists()
        {
            var db = GetInMemoryDb();
            var service = new CarService(db);

            var history = await service.GetCarHistoryAsync(1);

            Assert.NotNull(history);
            Assert.Equal(1, history.CarId);
            Assert.Single(history.Policies);
            Assert.Single(history.Claims);
        }

        [Fact]
        public async Task GetCarHistoryAsync_ThrowsKeyNotFound_WhenCarWithGivenCarIdDoesNotExist()
        {
            var db = GetInMemoryDb();
            var service = new CarService(db);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetCarHistoryAsync(999));
        }

    }
}


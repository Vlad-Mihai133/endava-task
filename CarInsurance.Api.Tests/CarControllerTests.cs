using CarInsurance.Api.Controllers;
using CarInsurance.Api.Data;
using CarInsurance.Api.Dtos;
using CarInsurance.Api.Models;
using CarInsurance.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarInsurance.Api.Tests
{
    public class CarsControllerTests
    {
        private CarsController GetController()
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

            var service = new CarService(db);
            return new CarsController(service);
        }

        [Theory]
        [InlineData("2025-13-01")]
        [InlineData("2025-01-32")]
        [InlineData("1899-12-31")]
        [InlineData("2101-01-01")]
        [InlineData("invalid")]
        public async Task IsInsuranceValid_ReturnsBadRequest_WhenDateIsInvalid(string invalidDate)
        {
            var controller = GetController();
            var result = await controller.IsInsuranceValid(1, invalidDate);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task IsInsuranceValid_ReturnsNotFound_WhenCarWithGivenIdDoesNotExist()
        {
            var controller = GetController();
            var result = await controller.IsInsuranceValid(999, "2025-01-01");

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task IsInsuranceValid_ReturnsOk_WhenBothCarIdAndDateAreValid()
        {
            var controller = GetController();
            var result = await controller.IsInsuranceValid(1, "2024-06-01");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<InsuranceValidityResponse>(okResult.Value);
            Assert.True(response.Valid);
        }

        [Fact]
        public async Task RegisterInsuranceClaim_ReturnsCreated_WhenClaimIsValid()
        {
            var controller = GetController();

            var request = new CreateInsuranceClaimRequest("2024-06-01", "Test Claim", 100);

            var result = await controller.RegisterInsuranceClaim(1, request);

            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Theory]
        [InlineData("2025-13-01")]
        [InlineData("2025-01-32")]
        [InlineData("1899-12-31")]
        [InlineData("2101-01-01")]
        [InlineData("invalid")]
        public async Task RegisterInsuranceClaim_ReturnsBadRequest_WhenDateIsNotValid(string date)
        {
            var controller = GetController();
            var request = new CreateInsuranceClaimRequest(date, "X", 50);

            var result = await controller.RegisterInsuranceClaim(1, request);

            Assert.IsType<BadRequestObjectResult>(result);
        }
        
        [Fact]
        public async Task RegisterInsuranceClaim_ReturnsNotFound_WhenCarWithGivenCarIdDoesNotExist()
        {
            var controller = GetController();
            var request = new CreateInsuranceClaimRequest("2024-06-01", "X", 50);

            var result = await controller.RegisterInsuranceClaim(999, request);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetCarHistory_ReturnsOk_WhenCarWithGivenCarIdExists()
        {
            var controller = GetController();

            var result = await controller.GetCarHistory(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var history = Assert.IsType<CarHistoryDto>(okResult.Value);

            Assert.Equal(1, history.CarId);
            Assert.Single(history.Policies);
            Assert.Single(history.Claims);
        }

        [Fact]
        public async Task GetCarHistory_ReturnsNotFound_WhenCarWithGivenCarIdDoesNotExist()
        {
            var controller = GetController();

            var result = await controller.GetCarHistory(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}

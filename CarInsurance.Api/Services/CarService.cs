using CarInsurance.Api.Data;
using CarInsurance.Api.Dtos;
using CarInsurance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Services;

public class CarService(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task<List<CarDto>> ListCarsAsync()
    {
        return await _db.Cars.Include(c => c.Owner)
            .Select(c => new CarDto(c.Id, c.Vin, c.Make, c.Model, c.YearOfManufacture,
                                    c.OwnerId, c.Owner.Name, c.Owner.Email))
            .ToListAsync();
    }

    public async Task<bool> IsInsuranceValidAsync(long carId, DateTime date)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");

        return await _db.Policies.AnyAsync(p =>
            p.CarId == carId &&
            p.StartDate.Date <= date &&
            p.EndDate.Date >= date
        );
    }

    public async Task<bool> RegisterInsuranceClaimAsync(InsuranceClaim insuranceClaim)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == insuranceClaim.CarId);
        if (!carExists) throw new KeyNotFoundException($"Car {insuranceClaim.CarId} not found");

        await _db.Claims.AddAsync(insuranceClaim);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<CarHistoryDto?> GetCarHistoryAsync(long carId)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");

        var car = await _db.Cars
            .Where(c => c.Id == carId)
            .Select(c => new CarHistoryDto(
                c.Id,
                c.Vin,
                c.Make ?? string.Empty,
                c.Model ?? string.Empty,
                c.YearOfManufacture,
                c.Policies
                .OrderBy(p => p.StartDate)
                .Select(p => new InsurancePolicyDto(p.Id, p.StartDate, p.EndDate, p.Provider ?? string.Empty)).ToList(),
                c.Claims
                .OrderBy(c => c.ClaimDate)
                .Select(cl => new InsuranceClaimDto(cl.Id, cl.ClaimDate, cl.Description ?? string.Empty, cl.Amount)).ToList()
            )).FirstOrDefaultAsync();
        return car;
    }

}

using CarInsurance.Api.Dtos;
using CarInsurance.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarInsurance.Api.Controllers;

[ApiController]
[Route("api")]
public class CarsController(CarService service) : ControllerBase
{
    private readonly CarService _service = service;

    [HttpGet("cars")]
    public async Task<ActionResult<List<CarDto>>> GetCars()
        => Ok(await _service.ListCarsAsync());

    [HttpGet("cars/{carId:long}/insurance-valid")]
    public async Task<ActionResult<InsuranceValidityResponse>> IsInsuranceValid(long carId, [FromQuery] string date)
    {
        if (!DateTime.TryParse(date, out var parsed))
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");

        if (parsed.Year < 1900 || parsed.Year > 2100)
            return BadRequest("Date is out of allowed range (1900-2100).");

        try
        {
            var valid = await _service.IsInsuranceValidAsync(carId, parsed);
            return Ok(new InsuranceValidityResponse(carId, parsed.ToString("yyyy-MM-dd"), valid));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("cars/{carId:long}/claims")]
    public async Task<ActionResult> RegisterInsuranceClaim(long carId, [FromBody] CreateInsuranceClaimRequest insuranceClaimRequestDto)
    {
        if(!DateTime.TryParse(insuranceClaimRequestDto.ClaimDate, out var claimDate))
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");

        if (claimDate.Year < 1900 || claimDate.Year > 2100)
            return BadRequest("Date is out of allowed range (1900-2100).");

        try
        {
            var claimEntity = new Models.InsuranceClaim
            {
                CarId = carId,
                ClaimDate = claimDate,
                Description = insuranceClaimRequestDto.Description,
                Amount = insuranceClaimRequestDto.Amount
            };
            var status = await _service.RegisterInsuranceClaimAsync(claimEntity);
            if(!status)
                return BadRequest("Could not register the insurance claim.");

            return CreatedAtAction(nameof(RegisterInsuranceClaim), new { carId }, null);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("cars/{carId:long}/history")]
    public async Task<ActionResult<CarHistoryDto>> GetCarHistory(long carId)
    {
        try
        {
            var history = await _service.GetCarHistoryAsync(carId);
            return Ok(history);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}

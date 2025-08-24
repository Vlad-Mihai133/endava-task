namespace CarInsurance.Api.Dtos;

public record CarDto(long Id, string Vin, string? Make, string? Model, int Year, long OwnerId, string OwnerName, string? OwnerEmail);
public record InsuranceValidityResponse(long CarId, string Date, bool Valid);
public record CreateInsuranceClaimRequest(string ClaimDate, string Description, int Amount);
public record InsurancePolicyDto(long PolicyId, DateTime StartDate, DateTime EndDate, string Provider);

public record InsuranceClaimDto(long ClaimId, DateTime ClaimDate, string Description, long Amount);
public record CarHistoryDto(long CarId, string Vin, string Make, string Model, int Year, List<InsurancePolicyDto> Policies, List<InsuranceClaimDto> Claims);
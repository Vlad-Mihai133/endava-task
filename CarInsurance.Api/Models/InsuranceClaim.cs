namespace CarInsurance.Api.Models;

public class InsuranceClaim
{
    public long Id { get; set; }
    public long CarId { get; set; }
    public Car Car { get; set; } = default!;
    public DateTime ClaimDate { get; set; }
    public string Description { get; set; } = default!;
    public int Amount { get; set; }
}

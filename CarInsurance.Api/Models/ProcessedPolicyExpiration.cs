namespace CarInsurance.Api.Models
{
    public class ProcessedPolicyExpiration
    {
        public long Id { get; set; }
        public long PolicyId { get; set; }
        public InsurancePolicy Policy { get; set; } = default!;
        public DateTime ProcessedAt { get; set; }
    }
}

namespace EBallotApi.Dto
{
    public class CandidateDto
    {
        public int CandidateId { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string Party { get; set; }
        public string? Symbol { get; set; }
        public string? Manifesto { get; set; }
        public string AadharEnc { get; set; }
        public string PhoneNumberEnc { get; set; }
        public string? Photo { get; set; }
        public int ElectionId { get; set; }
        public string ElectionName { get; set; }
        public int ConstituencyId { get; set; }
        public string ConstituencyName { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; }
    }
}

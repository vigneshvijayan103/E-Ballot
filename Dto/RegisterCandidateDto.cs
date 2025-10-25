namespace EBallotApi.Dto
{
    public class RegisterCandidateDto
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string PartyName { get; set; } = string.Empty;
        public IFormFile? Symbol { get; set; }
        public string? Manifesto { get; set; }
        public string AadharEnc { get; set; } = string.Empty;       
        public string PhoneNumberEnc { get; set; } = string.Empty;
        public IFormFile? Photo { get; set; }
        public int ElectionId { get; set; }
        public bool IsActive { get; set; } = false;
    }
}


namespace EBallotApi.Models
{
    public class Voters
    {

        public int VoterId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty; 
        public string AadhaarQuickHash { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int ConstituencyId { get; set; }
        public string AadhaarEnc { get; set; } = string.Empty; 
        public string Status { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }  

    }
}

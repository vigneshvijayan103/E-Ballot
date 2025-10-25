namespace EBallotApi.Dto
{
    public class VoterDbDto
    {
        public int VoterId { get; set; }
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string AadhaarEnc { get; set; }
        public string Status { get; set; }
        public string ConstituencyName { get; set; }

        public string rejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int ConstituencyId { get; set; }
        public int Age { get; set; }
    }
}

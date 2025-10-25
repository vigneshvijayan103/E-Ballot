namespace EBallotApi.Dto
{
    public class VoterResponseDto
    {

        public int VoterId { get; set; }
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }

        public string Aadhaar { get; set; }
        public string Status { get; set; }
        public string ConstituencyName { get; set; }

        public string RejectionReason { get; set; }

        public int ConstituencyId { get; set; }
        public int Age { get; set; }
    }
}

namespace EBallotApi.Dto
{
    public class UpdateElectionOfficerDto
    {
        public int OfficerId { get; set; }
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Gender { get; set; }
        public string? EmployeeId { get; set; }
        public bool? IsActive { get; set; }
        public string? Email { get; set; } 
    }
}

namespace EBallotApi.Dto
{
    public class ElectionOfficerResponseDto
    {
        public int OfficerId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public string EmployeeId { get; set; }
        public bool IsActive { get; set; }
        public int? ConstituencyId { get; set; }

        public string ConstituencyName { get; set; }
        public int? AssignedByAdminId { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

namespace EBallotApi.Models
{
    public class ElectionOfficerDetails
    {
        public int OfficerDetailId { get; set; }
        public int OfficerId { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public char Gender { get; set; }
        public string EmployeeId { get; set; }
        public int? ConstituencyId { get; set; }
        public bool IsActive { get; set; } = true;
        public int? AssignedByAdminId { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}

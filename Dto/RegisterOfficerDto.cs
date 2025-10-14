namespace EBallotApi.Dto
{
    public class RegisterOfficerDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; } 
        public string EmployeeId { get; set; }
    }
}

namespace EBallotApi.Dto
{
    public class VoterRegisterDto
    {
        public string Name { get; set; }
        public string DateOfBirthString { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string AadhaarNumber { get; set; }
        public string Password { get; set; }
    }
}

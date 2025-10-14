namespace EBallotApi.Dto
{
    public class VoterRegisterVerifyDto
    {
        public string Name { get; set; }      
        public string Dob { get; set; }      
        public string Gender { get; set; }     
        public string Phone { get; set; }      
        public string Aadhaar { get; set; }    
        public string Password { get; set; }
        public int ConstituencyId { get; set; }
        public string otp { get; set; }        
        public string sessionId { get; set; }
    }
}

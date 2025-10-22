namespace EBallotApi.Models
{
    public class Voters
    {

        public int VoterId { get; set; }         
        public string Name { get; set; }           
        public DateTime DateOfBirth { get; set; }  
        public char Gender { get; set; }            
        public string PhoneNumber { get; set; }


        // Aadhaar security
        public string AadhaarHash { get; set; }

        // Password security
        public string PasswordHash { get; set; }     

        public int ConstituencyId { get; set; }



        public bool IsVerified { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

    }
}

namespace EBallotApi.Models
{
    public class Constituency
    {
        public int ConstituencyId { get; set; }   
        public string Name { get; set; }          
        public string District { get; set; }     
        public string State { get; set; }       
        public bool IsActive { get; set; } = true; 
        public DateTime CreatedAt { get; set; }   
        public DateTime UpdatedAt { get; set; }  
    }
}

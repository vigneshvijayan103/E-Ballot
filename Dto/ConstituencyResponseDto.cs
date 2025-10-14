namespace EBallotApi.Dto
{
    public class ConstituencyResponseDto
    {
        public int ConstituencyId { get; set; }     
        public string Name { get; set; }              
        public string District { get; set; }          
        public string State { get; set; }            
        public int RegisteredVoters { get; set; }   
        public int AssignedOfficers { get; set; }     
        public List<OfficerDto> Officers { get; set; } 
    }

    public class OfficerDto
    {
        public int Id { get; set; }                
        public string Name { get; set; }             
        public string Email { get; set; }            
        public bool IsActive { get; set; }          
    }
}

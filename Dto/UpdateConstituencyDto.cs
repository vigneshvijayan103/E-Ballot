namespace EBallotApi.Dto
{
    public class UpdateConstituencyDto
    {
        public int ConstituencyId { get; set; } 

        public string? Name { get; set; }        
        public string? District { get; set; }    
        public string? State { get; set; }
    }
}

namespace EBallotApi.Dto
{
    public class CandidateByElectionDto
    {
        public int CandidateId { get; set; }
        public string CandidateName { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string PartyName { get; set; }
        public string? Symbol { get; set; }        
        public string? Photo { get; set; }        
        public string? Manifesto { get; set; }
        public int ConstituencyId { get; set; }
        public string ConstituencyName { get; set; }
        public int ElectionId { get; set; }
        public string ElectionTitle { get; set; }
    }
}

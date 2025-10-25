namespace EBallotApi.Dto
{
    public class CandidateCountDto
    {
        public int CandidateId { get; set; }
        public int Votes { get; set; }
        public int? ElectionConstituencyId { get; set; } 
    }
}

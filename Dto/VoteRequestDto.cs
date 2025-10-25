namespace EBallotApi.Dto
{
    public class VoteRequestDto
    {
        public int ElectionId { get; set; }
        public int ElectionConstituencyId { get; set; }
        public int CandidateId { get; set; }
        public int VoterId { get; set; }
    }
}

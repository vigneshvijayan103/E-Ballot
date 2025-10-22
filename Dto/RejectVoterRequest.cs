namespace EBallotApi.Dto
{
    public class RejectVoterRequest
    {
        public int VoterId { get; set; }
        public string Reason { get; set; }
    }
}

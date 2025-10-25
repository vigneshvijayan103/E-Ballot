namespace EBallotApi.Dto
{
    public class OfficerDashBoardDto
    {
        public int totalVoters { get; set; }
        public int approvedVoters { get; set; }
        public int pendingVoters { get; set; }

        public int rejectedVoters { get; set; }

        public int candidates { get; set; }

        public int votesCast { get; set; }
    }
}

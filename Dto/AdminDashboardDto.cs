namespace EBallotApi.Dto
{
    public class AdminDashboardDto
    {
        public int TotalOfficers { get; set; }
        public int ActiveElections { get; set; }
        public int TotalConstituencies { get; set; }

        public int TotalRegisteredVoters { get; set; }

        public int TotalApprovedVoters { get; set; }
    }
}

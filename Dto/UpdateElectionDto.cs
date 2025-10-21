namespace EBallotApi.Dto
{
    public class UpdateElectionDto
    {
        public int ElectionId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public bool? IsActive { get; set; }
    }
}

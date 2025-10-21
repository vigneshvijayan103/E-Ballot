namespace EBallotApi.Dto
{
    public class CreateElectionDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Upcoming"; // Optional default
        public bool IsActive { get; set; } = true;       // Optional default
    }
}

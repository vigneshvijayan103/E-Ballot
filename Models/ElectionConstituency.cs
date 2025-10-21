namespace EBallotApi.Models
{
    public class ElectionConstituency
    {
        public int ElectionConstituencyId { get; set; }
        public int ElectionId { get; set; }
        public int ConstituencyId { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

using EBallotApi.Dto;

namespace EBallotApi.Services
{
    public interface IVotingService
    {
        Task SubmitVoteAsync(VoteRequestDto vote);
        Task<IEnumerable<CandidateCountDto>> GetTallyAsync(int electionId, int? electionConstituencyId = null);
        Task<Dictionary<int, int>> GetTallyByDecryptionAsync(int electionConstituencyId);
    }
}

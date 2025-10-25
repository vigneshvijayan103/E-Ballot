using EBallotApi.Dto;

namespace EBallotApi.Services
{
    public interface ICandidateService
    {
         Task<int> RegisterCandidateAsync(RegisterCandidateDto candidateDto, int CreatedByOfficerId);
        Task<IEnumerable<CandidateDto>> GetCandidatesByOfficerAsync(int officerId);

        Task<CandidateViewDto?> GetCandidateByIdAsync(int candidateId);
        Task<IEnumerable<CandidateDto>> GetCandidatesByElectionIdAsync(int electionId);

    }

}

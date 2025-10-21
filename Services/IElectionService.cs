
using EBallotApi.Dto;
using EBallotApi.Models;

namespace EBallotApi.Services
{
    public interface IElectionService
    {
        Task<int> CreateElectionAsync(CreateElectionDto dto, int CreatedById);
        Task<int> UpdateElectionAsync(UpdateElectionDto dto, int updatedById);
        Task<IEnumerable<ElectionDto>> GetAllElectionsAsync();

        Task<ElectionDto> GetElectionByIdAsync(int electionId);
        Task<ElectionConstituency?> AssignConstituencyToElectionAsync(int electionId, int constituencyId, int userId);
        Task<bool> UnassignConstituencyFromElectionAsync(int electionId, int constituencyId, int userId);
        Task<IEnumerable<ElectionDto>> GetElectionsByConstituencyAsync(int constituencyId);

    }

}

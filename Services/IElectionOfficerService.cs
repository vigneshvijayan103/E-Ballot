using EBallotApi.Dto;

namespace EBallotApi.Services
{
    public interface IElectionOfficerService
    {
        Task<IEnumerable<MyElectionDto>> GetAssignedElectionsAsync(int officerId);
    }
}

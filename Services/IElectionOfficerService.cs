using EBallotApi.Dto;

namespace EBallotApi.Services
{
    public interface IElectionOfficerService
    {
        Task<OfficerDashBoardDto> GetDashboardMetrics(int officerID);

        Task<IEnumerable<MyElectionDto>> GetAssignedElectionsAsync(int officerId);
    }
}

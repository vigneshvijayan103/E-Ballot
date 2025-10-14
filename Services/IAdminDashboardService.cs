using EBallotApi.Dto;
namespace EBallotApi.Services
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardDto> GetDashboardStatsAsync();
    }
}

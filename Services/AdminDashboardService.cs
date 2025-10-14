using Dapper;
using EBallotApi.Dto;
using System.Data;

namespace EBallotApi.Services
{
    public class AdminDashboardService:IAdminDashboardService
    {
        private readonly IDbConnection _connection;

        public AdminDashboardService(IDbConnection connection)
        {
            _connection = connection;
        }

       public async Task<AdminDashboardDto> GetDashboardStatsAsync()
        {

            var metrics = _connection.QuerySingle<AdminDashboardDto>(
               "sp_GetDashboardMetrics",
               commandType: CommandType.StoredProcedure
           );
            return metrics ?? new AdminDashboardDto();
        }
    }
}

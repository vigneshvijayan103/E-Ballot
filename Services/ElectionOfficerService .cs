using Dapper;
using EBallotApi.Dto;
using Microsoft.IdentityModel.Tokens;
using System.Data;

namespace EBallotApi.Services
{
    public class ElectionOfficerService:IElectionOfficerService
    {
        private readonly IDbConnection _connection;

        public ElectionOfficerService(IDbConnection connection)
        {
            _connection = connection;
        }


        public async Task<OfficerDashBoardDto>GetDashboardMetrics(int officerID)
        {

            var metrics = await _connection.QuerySingleAsync<OfficerDashBoardDto>(
               "sp_GetOfficerDashboardMetrics",
               new { OfficerId = officerID },
               commandType: CommandType.StoredProcedure
               );

            return metrics ?? new OfficerDashBoardDto();

        }

        public async Task<IEnumerable<MyElectionDto>> GetAssignedElectionsAsync(int officerId)
        {

            var result = await _connection.QueryAsync<MyElectionDto>(
          "GetElectionsByOfficerId",
          new { OfficerId = officerId },
          commandType: CommandType.StoredProcedure
                 );

            return result;
        }
    }
}

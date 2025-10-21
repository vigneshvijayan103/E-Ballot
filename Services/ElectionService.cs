using Dapper;
using EBallotApi.Dto;
using EBallotApi.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;


namespace EBallotApi.Services
{
    public class ElectionService:IElectionService
    {

        private readonly IDbConnection _connection;


        public  ElectionService(IDbConnection connection)
        {
           _connection=connection;
        }

       public async  Task<int> CreateElectionAsync(CreateElectionDto dto, int CreatedById)
        {

            var parameters = new DynamicParameters();
            parameters.Add("@Title", dto.Title);
            parameters.Add("@Description", dto.Description);
            parameters.Add("@StartDate", dto.StartDate);
            parameters.Add("@EndDate", dto.EndDate);
            parameters.Add("@CreatedBy", CreatedById);
            parameters.Add("@Status", dto.Status);
            parameters.Add("@IsActive", dto.IsActive);

            int newElectionId = await _connection.QuerySingleAsync<int>(
            "sp_AddElection",
            parameters,
            commandType: CommandType.StoredProcedure
                 );

            return newElectionId;

        }

        public async Task<int> UpdateElectionAsync(UpdateElectionDto dto, int updatedById)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ElectionId", dto.ElectionId);
            parameters.Add("@Title", dto.Title);
            parameters.Add("@Description", dto.Description);
            parameters.Add("@StartDate", dto.StartDate);
            parameters.Add("@EndDate", dto.EndDate);
            parameters.Add("@Status", dto.Status);
            parameters.Add("@IsActive", dto.IsActive);
            parameters.Add("@UpdatedBy", updatedById);

            int updatedElectionId = await _connection.QuerySingleAsync<int>(
                "sp_UpdateElection",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return updatedElectionId;
        }


        public async Task<IEnumerable<ElectionDto>> GetAllElectionsAsync()
        {

                var result = await _connection.QueryAsync<ElectionDto>(
                    "sp_GetAllElections",
                    commandType: CommandType.StoredProcedure
                );

                return result;
        }


        public async Task<ElectionDto> GetElectionByIdAsync(int electionId)
        {

            var parameters = new DynamicParameters();
            parameters.Add("@ElectionId", electionId);

            var election = await _connection.QueryFirstOrDefaultAsync<ElectionDto>(
                "sp_GetElectionById",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return election;
        }


        public async Task<ElectionConstituency?> AssignConstituencyToElectionAsync(int electionId, int constituencyId, int userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ElectionId", electionId, DbType.Int32);
            parameters.Add("@ConstituencyId", constituencyId, DbType.Int32);
            parameters.Add("@UserId", userId, DbType.Int32);

            // This will return either the new record or the updated one
            var result = await _connection.QueryFirstOrDefaultAsync<ElectionConstituency>(
                "sp_AssignConstituencyToElection",
                parameters,
                commandType: CommandType.StoredProcedure
            );
            return result;

        }


        public async Task<bool> UnassignConstituencyFromElectionAsync(int electionId, int constituencyId, int userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ElectionId", electionId, DbType.Int32);
            parameters.Add("@ConstituencyId", constituencyId, DbType.Int32);
            parameters.Add("@UserId", userId, DbType.Int32);

            var rowsAffected = await _connection.ExecuteAsync(
                "sp_UnassignConstituencyFromElection",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return rowsAffected > 0; 
        }

        public async Task<IEnumerable<ElectionDto>> GetElectionsByConstituencyAsync(int constituencyId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ConstituencyId", constituencyId, DbType.Int32);

            var result = await _connection.QueryAsync<ElectionDto>(
                "sp_GetElectionsByConstituency",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }


    }
}

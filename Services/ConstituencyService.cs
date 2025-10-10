using Azure.Core;
using Dapper;
using EBallotApi.Dto;
using EBallotApi.Models;
using System.Data;

namespace EBallotApi.Services
{
    public class ConstituencyService:IConstituencyService
    {
        private readonly IDbConnection _connection;

        public ConstituencyService(IDbConnection connection)
        {
            _connection = connection;
        }

        // Add Constituency
        public async Task<int> AddConstituencyAsync(ConstituencyDto dto, int updatedByAdminId)
        {
            var existing = await _connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Constituencies WHERE Name = @Name", new { dto.Name });
            if (existing > 0)
                throw new ArgumentException("Constituency with the same name already exists.");


            var parameters = new DynamicParameters();
            parameters.Add("@Name", dto.Name, DbType.String);
            parameters.Add("@District", dto.District, DbType.String);
            parameters.Add("@State", dto.State, DbType.String);
            parameters.Add("@CreatedByAdminId", updatedByAdminId, DbType.Int32);

            var result = await _connection.ExecuteAsync(
                "sp_AddConstituency",
                parameters,
                commandType: CommandType.StoredProcedure
            );
            return result;

        }
        //Get all Constituency
        public async Task<IEnumerable<Constituency>> GetAllConstituenciesAsync()
        {
            var constituencies = await _connection.QueryAsync<Constituency>(
                "sp_GetConstituencies",
                commandType: CommandType.StoredProcedure
            );
            return constituencies;
        }

        //Get constituency ById
        public async Task<Constituency> GetConstituencyByIdAsync(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ConstituencyId", id, DbType.Int32);

            var constituency = await _connection.QueryFirstOrDefaultAsync<Constituency>(
                "sp_GetConstituencyById",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return constituency;
        }

        //update constituency
        public async Task<bool> UpdateConstituencyAsync(UpdateConstituencyDto dto, int updatedByAdminId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ConstituencyId", dto.ConstituencyId, DbType.Int32);
            parameters.Add("@Name", dto.Name, DbType.String);
            parameters.Add("@District", dto.District, DbType.String);
            parameters.Add("@State", dto.State, DbType.String);
            parameters.Add("@UpdatedByAdminId", updatedByAdminId, DbType.Int32);

            var rowsAffected = await _connection.ExecuteAsync(
                "sp_UpdateConstituency",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return true;
        }




    }
}

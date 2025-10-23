using Dapper;
using EBallotApi.Dto;
using EBallotApi.Helper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EBallotApi.Services
{
    public class CandidateService : ICandidateService
    {
        private readonly IDbConnection _connection;
        public CandidateService(IDbConnection connection)
        {
            _connection = connection;
        }

        //register candidates by officer
        public async Task<int> RegisterCandidateAsync(RegisterCandidateDto candidateDto, int CreatedByOfficerId)
        {

            string adhaarEncrypt = AesEncryptionHelper.Encrypt(candidateDto.AadharEnc);

            string encryptedPhone=AesEncryptionHelper.Encrypt(candidateDto.PhoneNumberEnc);

            var constituencyId = await _connection.ExecuteScalarAsync<int?>(
                            @"SELECT ConstituencyId 
                              FROM ElectionOfficerDetails
                              WHERE OfficerId = @OfficerId",
                            new { OfficerId = CreatedByOfficerId }
                        );
            

            var parameters = new DynamicParameters();
            parameters.Add("@Name", candidateDto.Name);
            parameters.Add("@Age", candidateDto.Age);
            parameters.Add("@Gender", candidateDto.Gender);
            parameters.Add("@PartyName", candidateDto.PartyName);
            parameters.Add("@Symbol", candidateDto.Symbol);
            parameters.Add("@Manifesto", candidateDto.Manifesto);
            parameters.Add("@AadharEnc", adhaarEncrypt);
            parameters.Add("@PhoneNumberEnc", encryptedPhone);
            parameters.Add("@Photo", candidateDto.Photo);
            parameters.Add("@ConstituencyId", constituencyId);
            parameters.Add("@ElectionId", candidateDto.ElectionId);
            parameters.Add("@CreatedByOfficerId", CreatedByOfficerId);
            parameters.Add("@IsActive", candidateDto.IsActive);


            var candidateId = await _connection.ExecuteScalarAsync<int>(
                        "dbo.sp_RegisterCandidate",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );
            return candidateId; 
        }

        //Get candidates assigned by officer
        public async Task<IEnumerable<CandidateDto>> GetCandidatesByOfficerAsync(int officerId)
        {
            
            var candidates = await _connection.QueryAsync<CandidateDto>(
                "sp_GetCandidatesByOfficer",
                new { OfficerId = officerId },
                commandType: CommandType.StoredProcedure
            );
            return candidates;
        }

    }
}


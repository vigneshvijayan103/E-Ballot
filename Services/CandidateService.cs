using Dapper;
using EBallotApi.Dto;
using EBallotApi.Helper;
using EBallotApi.Helpers;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EBallotApi.Services
{
    public class CandidateService : ICandidateService
    {
        private readonly IDbConnection _connection;
        private readonly IWebHostEnvironment _env;
        public CandidateService(IDbConnection connection, IWebHostEnvironment environment)
        {
            _connection = connection;
            _env = environment;
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

            string? photoPath = candidateDto.Photo != null
                                  ? await FileHelper.SaveFileAsync(candidateDto.Photo, "candidates", _env)
                                  : null;

            string? symbolPath = candidateDto.Symbol != null
                                    ? await FileHelper.SaveFileAsync(candidateDto.Symbol, "symbols", _env)
                                    : null;


            var parameters = new DynamicParameters();
            parameters.Add("@Name", candidateDto.Name);
            parameters.Add("@Age", candidateDto.Age);
            parameters.Add("@Gender", candidateDto.Gender);
            parameters.Add("@PartyName", candidateDto.PartyName);
            parameters.Add("@Symbol", symbolPath);
            parameters.Add("@Manifesto", candidateDto.Manifesto);
            parameters.Add("@AadharEnc", adhaarEncrypt);
            parameters.Add("@PhoneNumberEnc", encryptedPhone);
            parameters.Add("@Photo", photoPath);
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

        //Get candidate By ID

        public async Task<CandidateViewDto?> GetCandidateByIdAsync(int candidateId)
        {
          
            
                var result = await _connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_GetCandidateById",
                    new { CandidateId = candidateId },
                    commandType: CommandType.StoredProcedure
                );

                if (result == null)
                    return null;

                return new CandidateViewDto
                {
                    CandidateId = result.CandidateId,
                    Name = result.Name,
                    Age = result.Age,
                    Gender = result.Gender,
                    PartyName = result.PartyName,
                    Symbol = result.Symbol,
                    Manifesto = result.Manifesto,
                    Aadhar = AesEncryptionHelper.Decrypt(result.AadharEnc),
                    PhoneNumber = AesEncryptionHelper.Decrypt(result.PhoneNumberEnc),
                    Photo = result.Photo,
                    ConstituencyName = result.ConstituencyName,
                    ElectionName = result.ElectionName
                };
            
        }

        //get candidate by electionId
        public async Task<IEnumerable<CandidateDto>> GetCandidatesByElectionIdAsync(int electionId)
        {

            var candidates = await _connection.QueryAsync<CandidateDto>(
            "GetCandidatesByElectionId",
            new { ElectionId = electionId },
            commandType: CommandType.StoredProcedure
        );

            return candidates;

        }

    }
}


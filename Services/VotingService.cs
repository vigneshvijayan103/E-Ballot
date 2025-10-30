using Dapper;
using EBallotApi.Dto;
using EBallotApi.Helper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EBallotApi.Services
{
    public class VotingService : IVotingService
    {
        private readonly IDbConnection _connection;
        public VotingService(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task SubmitVoteAsync(VoteRequestDto vote, int voterId)
        {

            var (cipherBytes, ivBytes) = VoteAesHelper.EncryptVote(vote.CandidateId.ToString());

            try
            {
                await _connection.ExecuteAsync(
                    "sp_SubmitVote",
                    new
                    {
                        ElectionId = vote.ElectionId,
                        ElectionConstituencyId = vote.ElectionConstituencyId,
                        CandidateId = vote.CandidateId,
                        VoterId = voterId,
                        EncryptedVote = cipherBytes,
                        IV = ivBytes
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("Voter has already voted"))
                    throw new InvalidOperationException("This voter has already cast a vote.");

                throw;
            }
        }

        public async Task<IEnumerable<CandidateCountDto>> GetTallyAsync(int electionId, int? electionConstituencyId = null)
        {
            var result = await _connection.QueryAsync<CandidateCountDto>(
                "sp_GetCandidateCounts",
                new { ElectionId = electionId, ElectionConstituencyId = electionConstituencyId },
                commandType: CommandType.StoredProcedure
            );

            return result;
        }


        public async Task<Dictionary<int, int>> GetTallyByDecryptionAsync(int electionConstituencyId)
        {
            var votes = await _connection.QueryAsync<(byte[] EncryptedVote, byte[] IV)>(
                @"SELECT EncryptedVote, IV 
                  FROM AESBallots 
                  WHERE ElectionConstituencyId = @ecid",
                new { ecid = electionConstituencyId }
            );

            var tally = new Dictionary<int, int>();
            foreach (var vote in votes)
            {
                int candidateId = int.Parse(VoteAesHelper.DecryptVote(vote.EncryptedVote, vote.IV));
                if (tally.ContainsKey(candidateId))
                    tally[candidateId]++;
                else
                    tally[candidateId] = 1;
            }

            return tally;
        }
    }

}

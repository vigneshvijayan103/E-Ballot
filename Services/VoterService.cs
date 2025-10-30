using Dapper;
using EBallotApi.Dto;
using EBallotApi.Helper;
using EBallotApi.Models;
using Microsoft.Data.SqlClient;
using System.Collections.Concurrent;
using System.Data;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace EBallotApi.Services
{
    public class VoterService : IVoterService
    {
        private readonly IDbConnection _connection;
        private readonly JwtTokenService _jwtTokenService;
        

        private static readonly Dictionary<string, (string Aadhaar, string Otp, DateTime Expiry)> _store = new();


        public VoterService(IDbConnection connection, JwtTokenService jwtTokenService)
        {
            _connection = connection;
            _jwtTokenService = jwtTokenService;
        }



        public async Task<int> RegisterVoterAsync(VoterRegisterDto dto)
        {
            if (dto == null)
                throw new ArgumentException("Invalid request.");

            // Validate and parse DOB
            if (!DateTime.TryParseExact(dto.DateOfBirthString, "yyyy-MM-dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDob))
            {
                throw new ArgumentException("Invalid Date of Birth format. Expected YYYY-MM-DD.");
            }

            // Encrypt Phone Number
            string encryptedPhone;
            try
            {
                encryptedPhone = AesEncryptionHelper.Encrypt(dto.PhoneNumber);
            }
            catch (Exception ex)
            {
                throw new Exception("Phone number encryption failed: " + ex.Message);
            }

            // Encrypt Aadhaar (reversible encryption)
            string encryptedAadhaar;
            try
            {
                encryptedAadhaar = AesEncryptionHelper.Encrypt(dto.AadhaarNumber); // AES-GCM encryption
            }
            catch (Exception ex)
            {
                throw new Exception("Aadhaar encryption failed: " + ex.Message);
            }

            // Compute quick hash for uniqueness check
            var aadhaarQuickHash = AadhaarHelper.ComputeQuickHash(dto.AadhaarNumber);

            // Check if voter already exists
            var existing = await _connection.ExecuteScalarAsync<int>(
                @"SELECT COUNT(1) FROM Voters 
                 WHERE PhoneNumber = @PhoneNumber 
                 OR AadhaarQuickHash = @AadhaarQuickHash",
                new
                {
                    PhoneNumber = encryptedPhone,
                    AadhaarQuickHash = aadhaarQuickHash
                }
            );

            if (existing > 0)
                throw new ArgumentException("Phone number or Aadhaar is already registered.");

            // Hash password
            var hashedPassword = PasswordHelper.HashPassword(dto.Password);

            // Insert voter
            try
            {
                var voterId = await _connection.ExecuteScalarAsync<int>(
                    "sp_InsertVoter",
                    new
                    {
                        dto.Name,
                        DateOfBirth = parsedDob,
                        dto.Gender,
                        PhoneNumber = encryptedPhone,
                        AadhaarEnc = encryptedAadhaar, // store encrypted
                        AadhaarQuickHash = aadhaarQuickHash,
                        PasswordHash = hashedPassword,
                        dto.ConstituencyId,
                        Status = "Pending"
                    },
                    commandType: CommandType.StoredProcedure
                );

                return voterId;
            }
            catch (SqlException sqlEx) when (sqlEx.Number == 2627)
            {
                throw new ArgumentException("Phone number or Aadhaar is already registered.");
            }
            catch (Exception ex)
            {
                throw new Exception("Database error: " + ex.Message);
            }
        }





        //otp generation and sending


        public string SendOtp(string aadhaar, string phone)
        {
            var otp = new Random().Next(100000, 999999).ToString();
            var sessionId = Guid.NewGuid().ToString();

            _store[sessionId] = (aadhaar, otp, DateTime.Now.AddMinutes(5));

            Console.WriteLine($"[MOCK OTP] Aadhaar: {aadhaar}, OTP: {otp}");

            return sessionId;
        }



        //verifying otp
        public bool VerifyOtp(string aadhaar, string otp, string sessionId)
        {
            if (!_store.ContainsKey(sessionId)) return false;

            var record = _store[sessionId];
            if (record.Expiry < DateTime.Now) return false;
            if (record.Aadhaar != aadhaar) return false;
            if (record.Otp != otp) return false;

            _store.Remove(sessionId);
            return true;
        }


        //login of voter
        public async Task<VoterLoginResponseDto> LoginAsync(VoterLoginDto dto)
        {
            if (dto == null)
                throw new ArgumentException("Invalid login request."); 

            try
            {
                // Compute quick hash from input Aadhaar
                var quickHash = AadhaarHelper.ComputeQuickHash(dto.AadhaarNumber);

                // Fetch voter using AadhaarQuickHash
                var voter = await _connection.QuerySingleOrDefaultAsync<dynamic>(
                    @"SELECT VoterId, Name, AadhaarEnc, PasswordHash, DateOfBirth
                          FROM Voters
                          WHERE AadhaarQuickHash = @QuickHash",
                    new { QuickHash = quickHash }
                );

                if (voter == null)
                    return null; 

                // Decrypt stored Aadhaar
                string decryptedAadhaar = AesEncryptionHelper.Decrypt(voter.AadhaarEnc);


                // Verify Aadhaar matches input
                if (decryptedAadhaar != dto.AadhaarNumber)
                    return null; // invalid login

                // Verify Date of Birth
                if (Convert.ToDateTime(voter.DateOfBirth).Date != dto.DateOfBirth.Date)
                    return null; // invalid login

                // Verify password
                bool isPasswordValid = PasswordHelper.VerifyPassword(dto.Password, voter.PasswordHash);
                if (!isPasswordValid)
                    return null; // invalid login

                // Generate JWT token
                string token = _jwtTokenService.GenerateToken(voter.VoterId,null,"Voter" );

               

                // Return response
                return new VoterLoginResponseDto
                {
                    VoterId = voter.VoterId,
                    Name = voter.Name,
                    Token = token,
                    Aadhaar = AesEncryptionHelper.MaskAadhaar(decryptedAadhaar)
                };
            }
            catch (Exception ex)
            {
               
                Console.WriteLine("Unexpected error during login: " + ex.Message);
                throw; // let middleware handle 500
            }
        }



        //get all voters
        public async Task<IEnumerable<VoterResponseDto>> GetAllVotersAsync(string role, int userId)
        {
            var voters = (await _connection.QueryAsync<VoterDbDto>(
                                "sp_GetVoters",
                                commandType: CommandType.StoredProcedure
                            )).ToList();

            // Role-based filtering
            if (role == "ElectionOfficer")
            {
                int constituencyId = await _connection.ExecuteScalarAsync<int>(
                    "SELECT ConstituencyId FROM ElectionOfficerDetails WHERE OfficerId = @UserId",
                    new { UserId = userId }
                );

                voters = voters.Where(v => v.ConstituencyId == constituencyId).ToList();
            }

          

            var responseList = voters.Select(voter => new VoterResponseDto
            {
                VoterId = voter.VoterId,
                Name = voter.Name,
                DateOfBirth = voter.DateOfBirth,
                Gender = voter.Gender,
                PhoneNumber = SafeDecryptHelper.SafeDecrypt(voter.PhoneNumber),
                Aadhaar = SafeDecryptHelper.SafeDecryptAndMaskAadhaar(voter.AadhaarEnc),
                Status = voter.Status,
                ConstituencyName = voter.ConstituencyName,
                ConstituencyId = voter.ConstituencyId,
                Age = voter.Age,
                RejectionReason=voter.rejectionReason
            }).ToList();

            return responseList;


        }

        //getvoterByid

        public async Task<VoterResponseDto> GetVoterByIdAsync(int voterId)
        {
            var voter = await _connection.QueryFirstOrDefaultAsync<VoterDbDto>(
                "sp_GetVoterById",
                new { VoterId = voterId },
                commandType: CommandType.StoredProcedure
            );

            if (voter == null)
                return null; 

            var response = new VoterResponseDto
            {
                VoterId = voter.VoterId,
                Name = voter.Name,
                DateOfBirth = voter.DateOfBirth,
                Gender = voter.Gender,
                PhoneNumber = SafeDecryptHelper.SafeDecrypt(voter.PhoneNumber),
                Aadhaar = SafeDecryptHelper.SafeDecryptAndMaskAadhaar(voter.AadhaarEnc),
                Status = voter.Status,
                ConstituencyName = voter.ConstituencyName,
                ConstituencyId = voter.ConstituencyId,
                Age = voter.Age,
                RejectionReason=voter.rejectionReason
               
            };

            return response;
        }






        //approve voters for election officer
        public async Task<string> ApproveVoterAsync(int voterId, int officerId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@VoterId", voterId, DbType.Int32);
            parameters.Add("@OfficerId", officerId, DbType.Int32);
            parameters.Add("@ResultMessage", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

            await _connection.ExecuteAsync("sp_ApproveVoter", parameters, commandType: CommandType.StoredProcedure);

            return parameters.Get<string>("@ResultMessage");
        }



        //reject voters for election officer
        public async Task<string> RejectVoterAsync(int voterId, int officerId, string reason)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@VoterId", voterId, DbType.Int32);
            parameters.Add("@OfficerId", officerId, DbType.Int32);
            parameters.Add("@Reason", reason, DbType.String);
            parameters.Add("@ResultMessage", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

            await _connection.ExecuteAsync("sp_RejectVoter", parameters, commandType: CommandType.StoredProcedure);

            return parameters.Get<string>("@ResultMessage");
        }


        //check whether the voter is voter for specified election
        public async Task<bool> HasVoterVotedAsync(int voterId, int electionConstituencyId)
        {
            const string sql = @"
                                    SELECT CASE 
                                             WHEN EXISTS (
                                                 SELECT 1 
                                                 FROM VoterParticipation
                                                 WHERE VoterId = @VoterId 
                                                   AND ElectionConstituencyId = @ElectionConstituencyId
                                             ) 
                                             THEN 1 
                                             ELSE 0 
                                           END";

            bool hasVoted = await _connection.ExecuteScalarAsync<bool>(sql, new
            {
                VoterId = voterId,
                ElectionConstituencyId = electionConstituencyId
            });

            return hasVoted;
        }















    }
}







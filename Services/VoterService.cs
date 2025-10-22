using Dapper;
using EBallotApi.Dto;
using EBallotApi.Helper;
using Microsoft.Data.SqlClient;
using System.Collections.Concurrent;
using System.Data;
using System.Globalization;
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
                throw new ArgumentException("Invalid login request."); // Can keep this as pre-check

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
                string token = _jwtTokenService.GenerateToken(voter.VoterId, null, "Voter");

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
                // Log the exception
                Console.WriteLine("Unexpected error during login: " + ex.Message);
                throw; // let middleware handle 500
            }
        }



        //get all voters
        public async Task<IEnumerable<VoterDto>> GetAllVotersAsync(string role, int userId)
        {
            var voters = (await _connection.QueryAsync<VoterDto>(
                "sp_GetVoters",
                commandType: CommandType.StoredProcedure
            )).ToList();


            foreach (var voter in voters)
            {
                try
                {
                    if (!string.IsNullOrEmpty(voter.PhoneNumber))
                        voter.PhoneNumber = AesEncryptionHelper.Decrypt(voter.PhoneNumber);
                }
                catch
                {
                    voter.PhoneNumber = "[Decryption Error]";
                }

                try
                {
                    if (!string.IsNullOrEmpty(voter.AadhaarEnc))
                    {
                        var decryptedAadhaar = AesEncryptionHelper.Decrypt(voter.AadhaarEnc);
                        voter.Aadhaar = AesEncryptionHelper.MaskAadhaar(decryptedAadhaar);
                    }
                }
                catch
                {
                    voter.Aadhaar = "[Decryption Error]";
                }
            }


                //Role-based filtering
                if (role == "ElectionOfficer")
            {
                int constituencyId = await _connection.ExecuteScalarAsync<int>(
                    "SELECT ConstituencyId FROM ElectionOfficerDetails WHERE OfficerId = @UserId",
                    new { UserId = userId }
                );

                voters = voters.Where(v => v.ConstituencyId == constituencyId).ToList();
            }

            return voters;
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
















    }
}







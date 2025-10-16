using Dapper;
using EBallotApi.Dto;
using EBallotApi.Helper;
using Microsoft.Data.SqlClient;
using System.Collections.Concurrent;
using System.Data;
using System.Globalization;

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

            //Validate and parse DOB =====
            if (!DateTime.TryParseExact(dto.DateOfBirthString, "yyyy-MM-dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDob))
            {
                throw new ArgumentException("Invalid Date of Birth format. Expected YYYY-MM-DD.");
            }

            //  Encrypt Phone Number
            string encryptedPhone;
            try
            {
                encryptedPhone = AesEncryptionHelper.Encrypt(dto.PhoneNumber);
            }
            catch (Exception ex)
            {
                throw new Exception("Phone number encryption failed: " + ex.Message);
            }

            // hashing Aadhaar
            var aadhaarSalt = AadhaarHelper.GenerateSalt();
            var aadhaarHash = AadhaarHelper.ComputeHash(dto.AadhaarNumber, aadhaarSalt);
            var aadhaarQuickHash = AadhaarHelper.ComputeQuickHash(dto.AadhaarNumber);

           
            var existing = await _connection.ExecuteScalarAsync<int>(
                @"SELECT COUNT(1) FROM Voters 
          WHERE PhoneNumber = @PhoneNumber 
             OR AadhaarHash = @AadhaarHash 
             OR AadhaarQuickHash = @AadhaarQuickHash",
                new
                {
                    PhoneNumber = encryptedPhone,
                    AadhaarHash = aadhaarHash,
                    AadhaarQuickHash = aadhaarQuickHash
                }
            );

            if (existing > 0)
                throw new ArgumentException("Phone number or Aadhaar is already registered.");

            // hashing password 
            var hashedPassword = PasswordHelper.HashPassword(dto.Password);

           
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
                        AadhaarHash = aadhaarHash,
                        AadhaarSalt = aadhaarSalt,
                        AadhaarQuickHash = aadhaarQuickHash,
                        PasswordHash = hashedPassword,
                        dto.ConstituencyId,
                        IsVerified = 0
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
            var quickHash = AadhaarHelper.ComputeQuickHash(dto.AadhaarNumber);

           
            var voter = await _connection.QuerySingleOrDefaultAsync<dynamic>(
                @"SELECT VoterId, Name, AadhaarHash, AadhaarSalt, PasswordHash, DateOfBirth
          FROM Voters
          WHERE AadhaarQuickHash = @QuickHash",
                new { QuickHash = quickHash }
            );

            if (voter == null)
                throw new ArgumentException("Voter not found with this Aadhaar");


            string computedHash = AadhaarHelper.ComputeHash(dto.AadhaarNumber, voter.AadhaarSalt);
            if (computedHash != voter.AadhaarHash)
                throw new ArgumentException("Voter not found with this Aadhaar");


          
            if (Convert.ToDateTime(voter.DateOfBirth).Date != dto.DateOfBirth.Date)
                throw new ArgumentException("Invalid Date of Birth.");


            bool isPasswordValid = PasswordHelper.VerifyPassword(dto.Password, voter.PasswordHash);
            if (!isPasswordValid)
                throw new ArgumentException("Invalid password.");

            string token = _jwtTokenService.GenerateToken(voter.VoterId, null, "Voter");


            return new VoterLoginResponseDto
            {
                VoterId = voter.VoterId,
                Name = voter.Name,
                Token = token,
                Aadhaar = null
                
            };
        }




    }
}







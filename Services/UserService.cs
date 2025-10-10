using Azure.Core;
using Dapper;
using EBallotApi.Dto;
using EBallotApi.Helper;
using EBallotApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;

namespace EBallotApi.Services
{
    public class UserService : IUserService
    {
        private readonly IDbConnection _connection;
        private readonly JwtTokenService _jwtTokenService;

        public UserService(IDbConnection connection, JwtTokenService jwtTokenService)
        {
            _connection = connection;
            _jwtTokenService = jwtTokenService;
        }

        //Register Officer By Admin
        public async Task<bool> RegisterElectionOfficerAsync(RegisterOfficerDto dto, int createdByAdminId)
        {
            string PasswordHash = PasswordHelper.HashPassword(dto.PasswordHash);


                    var query = @"
                                    SELECT COUNT(1)
                                    FROM Users u
                                    INNER JOIN ElectionOfficerDetails eod ON u.UserId = eod.OfficerId
                                    WHERE u.Email = @Email OR eod.PhoneNumber = @PhoneNumber;
                                ";

                    var existing = await _connection.ExecuteScalarAsync<int>(
                        query,
                        new { dto.Email, dto.PhoneNumber }
                    );

                    if (existing > 0)
                    {
                        throw new ApplicationException("An officer with this email or phone number already exists.");
                    }



            try
            {
                var officerId = await _connection.ExecuteAsync(
                        "sp_RegisterElectionOfficer",
                        new
                        {
                            dto.Name,
                            dto.Email,
                            PasswordHash = PasswordHash,
                            dto.PhoneNumber,
                            dto.Address,
                            dto.Gender,
                            dto.EmployeeId,
                            CreatedByAdminId = createdByAdminId
                        },
                        commandType: CommandType.StoredProcedure
                    );

                return true;

            }
            catch (SqlException ex)
            {
               
                throw new ApplicationException($"Database error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Unexpected error: {ex.Message}", ex);
            }
        }

        //Login for Admin & Officer

       public async  Task<UserLoginResponseDto> LoginAsync(UserLoginDto dto)
        {
                var query = @"
                        SELECT UserId, Name, Email, PasswordHash, Role
                        FROM Users
                        WHERE Email = @Email;
                    ";

            var user = await _connection.QueryFirstOrDefaultAsync<dynamic>(
                query,
                new { dto.Email }
            );

            if (user == null)
                throw new ApplicationException("Invalid email or password.");

            

            bool isPasswordValid = PasswordHelper.VerifyPassword(dto.Password, user.PasswordHash);

            

            if (!isPasswordValid)
                throw new ApplicationException("Invalid  password.");

            var token = _jwtTokenService.GenerateToken((int)user.UserId,(string)user.Email, (string)user.Role);

            return new UserLoginResponseDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Role = user.Role,
                Token = token
            };
        }

        //admin register
        public async Task<bool> RegisterUserAsync(RegisterUserDto dto)
        {
            // Check if user already exists
            var existingUser = await _connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Email = @Email", new { dto.Email });

            if (existingUser != null)
                throw new Exception("Email already registered.");

            // Hash the password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // Insert new user
            string sql = @"INSERT INTO Users (Name, Email, PasswordHash, Role, CreatedAt)
                       VALUES (@Name, @Email, @PasswordHash, @Role, GETDATE())";

            int rows = await _connection.ExecuteAsync(sql, new
            {
                dto.Name,
                dto.Email,
                PasswordHash = hashedPassword,
                dto.Role
            });

            return rows > 0;
        }

        //Get all ElectionOfficers
        public async Task<IEnumerable<ElectionOfficerResponseDto>> GetAllOfficersAsync()
        {
            var officers = await _connection.QueryAsync<ElectionOfficerResponseDto>(
                "sp_GetAllElectionOfficers",
                commandType: CommandType.StoredProcedure
            );
            return officers;
        }

        //Get ElectionOfficer By Id
        public async Task<ElectionOfficerResponseDto?> GetOfficerByIdAsync(int officerId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@OfficerId", officerId, DbType.Int32);

            var officer = await _connection.QueryFirstOrDefaultAsync<ElectionOfficerResponseDto>(
                "sp_GetElectionOfficerById",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return officer;
        }



        //Update ElectionOfficer Details by Admin

        public async Task<bool> UpdateElectionOfficerAsync(UpdateElectionOfficerDto dto, int updatedByAdminId)
        {


            var existingOfficer = await _connection.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT u.UserId FROM Users u INNER JOIN ElectionOfficerDetails eod ON u.UserId = eod.OfficerId WHERE u.UserId = @OfficerId",
                new { dto.OfficerId }
            );
            if (existingOfficer == null)
                throw new ApplicationException("Election officer not found.");


            var parameters = new DynamicParameters();
            parameters.Add("@OfficerId", dto.OfficerId, DbType.Int32);
            parameters.Add("@PhoneNumber", dto.PhoneNumber, DbType.String);
            parameters.Add("@Address", dto.Address, DbType.String);
            parameters.Add("@Gender", dto.Gender, DbType.String);
            parameters.Add("@EmployeeId", dto.EmployeeId, DbType.String);
            parameters.Add("@IsActive", dto.IsActive, DbType.Boolean);
            parameters.Add("@Email", dto.Email, DbType.String);
            parameters.Add("@UpdatedByAdminId", updatedByAdminId, DbType.Int32);

            var rowsAffected = await _connection.ExecuteAsync(
                "sp_UpdateElectionOfficer",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return rowsAffected > 0;
        }


        //assign constituency to office By admin
        public async Task<bool> AssignConstituencyAsync(AssignConstituencyDto dto, int adminId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@OfficerId", dto.OfficerId, DbType.Int32);
            parameters.Add("@ConstituencyId", dto.ConstituencyId, DbType.Int32);
            parameters.Add("@AssignedByAdminId", adminId, DbType.Int32);

            var rowsAffected = await _connection.ExecuteAsync(
                "sp_AssignConstituencyToOfficer",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return true;
        }

    }


}





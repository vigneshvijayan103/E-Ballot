using Dapper;
using EBallotApi.Dto;
using EBallotApi.Helper;
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



    }

}


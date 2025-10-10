using EBallotApi.Dto;
using Microsoft.AspNetCore.Mvc;

namespace EBallotApi.Services
{
    public interface IUserService
    {
        Task<bool> RegisterElectionOfficerAsync(RegisterOfficerDto dto, int createdByAdminId);
        Task<UserLoginResponseDto> LoginAsync(UserLoginDto dto);

        Task<bool> RegisterUserAsync(RegisterUserDto dto);

        Task<IEnumerable<ElectionOfficerResponseDto>> GetAllOfficersAsync();
        Task<ElectionOfficerResponseDto?> GetOfficerByIdAsync(int officerId);


        Task<bool> UpdateElectionOfficerAsync(UpdateElectionOfficerDto dto, int updatedByAdminId);


        Task<bool> AssignConstituencyAsync(AssignConstituencyDto dto, int adminId);

    }
}

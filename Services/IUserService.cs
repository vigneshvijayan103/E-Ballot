using EBallotApi.Dto;

namespace EBallotApi.Services
{
    public interface IUserService
    {
        Task<bool> RegisterElectionOfficerAsync(RegisterOfficerDto dto, int createdByAdminId);
        Task<UserLoginResponseDto> LoginAsync(UserLoginDto dto);

        Task<bool> RegisterUserAsync(RegisterUserDto dto);
        Task<bool> UpdateElectionOfficerAsync(UpdateElectionOfficerDto dto, int updatedByAdminId);

    }
}

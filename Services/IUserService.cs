using EBallotApi.Dto;

namespace EBallotApi.Services
{
    public interface IUserService
    {
        Task<bool> RegisterElectionOfficerAsync(RegisterOfficerDto dto, int createdByAdminId);
        Task<UserLoginResponseDto> LoginAsync(UserLoginDto dto);

    }
}

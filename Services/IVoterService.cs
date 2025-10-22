using EBallotApi.Dto;

namespace EBallotApi.Services
{
    public interface IVoterService
    {
        Task<int> RegisterVoterAsync(VoterRegisterDto dto);
        public string SendOtp(string aadhaar, string phone);

        public bool VerifyOtp(string aadhaar, string otp, string sessionId);

        Task<VoterLoginResponseDto> LoginAsync(VoterLoginDto dto);

        Task<IEnumerable<VoterDto>> GetAllVotersAsync(string role, int userId);
        Task<string> ApproveVoterAsync(int voterId, int officerId);
        Task<string> RejectVoterAsync(int voterId, int officerId, string reason);


    }
}

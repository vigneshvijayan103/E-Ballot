using EBallotApi.Dto;

namespace EBallotApi.Services
{
    public interface IVoterService
    {
        Task<int> RegisterVoterAsync(VoterRegisterDto dto);
        public string SendOtp(string aadhaar, string phone);

        public bool VerifyOtp(string aadhaar, string otp, string sessionId);

        Task<VoterLoginResponseDto> LoginAsync(VoterLoginDto dto);



    }
}

using EBallotApi.Dto;
using EBallotApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EBallotApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IVoterService _voterService;

        public AuthController(IVoterService voterService)
        {
            _voterService = voterService;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] VoterRegisterDto dto)
        {
            if (dto == null) return BadRequest("Invalid input");


            try
            {
                var voterId = await _voterService.RegisterVoterAsync(dto);
                return Ok(new { success = true, message = "Voter registered successfully", voterId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }


        }


        [HttpPost("send-otp")]
        public IActionResult SendOtp([FromBody] OtpRequestDto dto)
        {
            var sessionId = _voterService.SendOtp(dto.Aadhaar, dto.PhoneNumber);
            return Ok(new { success = true, sessionId });
        }



        [HttpPost("register-verify")]
        public async Task<IActionResult> RegisterVerify([FromBody] VoterRegisterVerifyDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { success = false, message = "Invalid input data" });



                bool isOtpValid = _voterService.VerifyOtp(dto.Aadhaar, dto.otp, dto.sessionId);
                if (!isOtpValid)
                    return BadRequest(new { success = false, message = "Invalid or expired OTP" });


                if (!DateTime.TryParseExact(dto.Dob, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime dob))
                    return BadRequest(new { success = false, message = "Invalid Date of Birth format. Use YYYY-MM-DD." });


                var voterDto = new VoterRegisterDto
                {
                    Name = dto.Name,
                    DateOfBirthString = dto.Dob,
                    Gender = dto.Gender,
                    PhoneNumber = dto.Phone,
                    AadhaarNumber = dto.Aadhaar,
                    Password = dto.Password
                };

                var voterId = await _voterService.RegisterVoterAsync(voterDto);

                return Ok(new { success = true, message = "Voter registered successfully", voterId });
            }
            catch (ArgumentException ex)
            {

                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] VoterLoginDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.AadhaarNumber) || string.IsNullOrEmpty(dto.Password))
            {
                return BadRequest(new { success = false, message = "Aadhaar and password are required." });


            }

            try
            {
                var voter = await _voterService.LoginAsync(dto);

                return Ok(new
                {
                    success = true,
                    message = "Login successful",
                    voter = new
                    {
                        voter.VoterId,
                        voter.Name,
                        voter.Aadhaar
                    }
                });
            }
            catch (ArgumentException ex)
            {

                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }


    }





}




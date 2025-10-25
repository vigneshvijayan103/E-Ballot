using EBallotApi.Dto;
using EBallotApi.Helper;
using EBallotApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Text.Json;

namespace EBallotApi.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IVoterService _voterService;
        private readonly IUserService _userService;

        public AuthController(IVoterService voterService, IUserService userService)
        {
            _voterService = voterService;
            _userService = userService;
        }

        //Register voter
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

        

        //Register ElectionOfficer By Admin
        [HttpPost("register-officer")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> RegisterElectionOfficer([FromBody] RegisterOfficerDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { Message = "Invalid input data." });
            }


            var createdByAdminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));




            var success = await _userService.RegisterElectionOfficerAsync(dto, createdByAdminId);
            if (success)
            {
                return Ok(new { Message = "Election officer registered successfully." });
            }
            else
            {
                return StatusCode(500, new { Message = "Failed to register election officer." });
            }

        }



        //sending otp while  voter Registration

        [HttpPost("send-otp")]
        public IActionResult SendOtp([FromBody] OtpRequestDto dto)
        {
            var sessionId = _voterService.SendOtp(dto.Aadhaar, dto.PhoneNumber);
            return Ok(new { success = true, sessionId });
        }


        //voter verification after otp 

        [HttpPost("register-verify")]
        public async Task<IActionResult> RegisterVerify([FromBody] VoterRegisterVerifyDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { success = false, message = "Invalid input data." });

                bool isOtpValid = _voterService.VerifyOtp(dto.Aadhaar, dto.otp, dto.sessionId);
                if (!isOtpValid)
                    return BadRequest(new { success = false, message = "Invalid or expired OTP." });

                if (!DateTime.TryParseExact(dto.Dob, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime dob))
                    return BadRequest(new { success = false, message = "Invalid Date of Birth format. Use YYYY-MM-DD." });

                var voterDto = new VoterRegisterDto
                {
                    Name = dto.Name,
                    DateOfBirthString = dto.Dob,
                    Gender = dto.Gender,
                    PhoneNumber = dto.Phone,
                    AadhaarNumber = dto.Aadhaar,
                    Password = dto.Password,
                    ConstituencyId = dto.ConstituencyId
                };

                int voterId = await _voterService.RegisterVoterAsync(voterDto);

                return Ok(new { success = true, message = "Voter registered successfully.", voterId });
            }
            catch (ArgumentException ex)
            {
             
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
              
                return StatusCode(500, new { success = false, message = "Internal server error.", error = ex.Message });
            }
        }






        //Login Voter
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


                if (voter==null)
                    return Unauthorized(new { success = false, message = "Invalid password or Adhaar" });

                return Ok(new
                {
                    success = true,
                    message = "Login successful",
                    voter = new
                    {
                        voter.VoterId,
                        voter.Name,
                        voter.Token
                    }
                });
            }
            catch (ArgumentException ex)
            {

                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message , stack = ex.StackTrace });
            }
        }


        //login for officer and admin
        [HttpPost("login-user")]
        public async Task<IActionResult> LoginUser([FromBody] UserLoginDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { Message = "Invalid login request." });
            }
            var result = await _userService.LoginAsync(dto);
            return Ok(new
            {
                Message = "Login successful",
                UserId = result.UserId,
                Name = result.Name,
                Role = result.Role,
                Token = result.Token
            });

        }

        //admin register

        [HttpPost("register-admin")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            try
            {
                bool result = await _userService.RegisterUserAsync(dto);

                if (result)
                    return Ok(new { message = "User registered successfully." });
                else
                    return BadRequest(new { message = "User registration failed." });
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, new { message = ex.Message });
            }
        }



    }


}




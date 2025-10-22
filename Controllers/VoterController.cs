using Dapper;
using EBallotApi.Dto;
using EBallotApi.Helper;
using EBallotApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EBallotApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoterController : ControllerBase
    {
        private readonly IVoterService _voterService;
        private readonly IDbConnection _connection;

        public VoterController(IVoterService voterService,IDbConnection connection)
        {
            _voterService = voterService;
            _connection = connection;

        }


        //Get all voters
        [Authorize(Roles = "Admin,ElectionOfficer")]
        [HttpGet("voters")]
        public async Task<IActionResult> GetVoters()
        {
            try
            {
                // Get user role and user info from token
                var role = User.FindFirst(ClaimTypes.Role)?.Value; 
                var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userId = string.IsNullOrEmpty(userIdClaim) ? 0 : Convert.ToInt32(userIdClaim);
             

                var voters = await _voterService.GetAllVotersAsync(role, userId);
                return Ok(new { success = true, data = voters });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        //approve voter by election officer
        [Authorize(Roles = "ElectionOfficer")]
        [HttpPost("approve")]
        public async Task<IActionResult> ApproveVoter([FromBody] ApproveVoterRequest request)
        {
            if (request == null || request.VoterId <= 0)
                return BadRequest(new { message = "Invalid voter ID." });

            // Get officerId from JWT claims
            var officerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            
            var message = await _voterService.ApproveVoterAsync(request.VoterId, officerId);

           
            return Ok(new { message });
        }


        //reject voter by election officer
        [Authorize(Roles = "ElectionOfficer")]
        [HttpPost("reject")]
        public async Task<IActionResult> RejectVoter([FromBody] RejectVoterRequest request)
        {
            if (request == null || request.VoterId <= 0)
                return BadRequest(new { message = "Invalid voter ID." });

            
            var officerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

           
            var message = await _voterService.RejectVoterAsync(request.VoterId, officerId, request.Reason);

           
            return Ok(new { message });
        }


    }
}


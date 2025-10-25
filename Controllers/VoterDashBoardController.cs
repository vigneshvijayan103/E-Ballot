using EBallotApi.Models;
using EBallotApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EBallotApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoterDashBoardController : ControllerBase
    {
        private readonly IVoterService _voterService;

        public VoterDashBoardController(IVoterService voterService)
        {
            _voterService = voterService;
        }

        [Authorize(Roles = "Voter")]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
           
            var voterId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));


            var voter = await _voterService.GetVoterByIdAsync(voterId);
            if (voter == null)
                return NotFound(new { success = false, message = "Voter not found." });

            return Ok(new { success = true, data = voter });
        }

    }
}


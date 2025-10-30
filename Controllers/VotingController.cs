using EBallotApi.Dto;
using EBallotApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EBallotApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VotingController : ControllerBase
    {
        private readonly IVotingService _service;
        public VotingController(IVotingService service)
        {
            _service = service;
        }



        [Authorize(Roles = "Voter")]
        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] VoteRequestDto vote)
        {
            // Extract voterId from JWT claim
            var voterIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(voterIdClaim))
                return Unauthorized(new { message = "Invalid token or voter not found." });

            int voterId = int.Parse(voterIdClaim);

            // Pass voterId explicitly to the service
            await _service.SubmitVoteAsync(vote, voterId);

            return Ok(new { message = "Vote submitted successfully." });
        }


        // Get real-time Updates
        [HttpGet("tally/{electionId}")]
        public async Task<IActionResult> Tally(int electionId, [FromQuery] int? electionConstituencyId = null)
        {
            var result = await _service.GetTallyAsync(electionId, electionConstituencyId);
            return Ok(result);
        }


        [HttpGet("audit/{electionConstituencyId}")]
        public async Task<IActionResult> Audit(int electionConstituencyId)
        {
            var result = await _service.GetTallyByDecryptionAsync(electionConstituencyId);
            return Ok(result);
        }









    }
}

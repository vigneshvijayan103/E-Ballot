using EBallotApi.Dto;
using EBallotApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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



        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] VoteRequestDto vote)
        {
            await _service.SubmitVoteAsync(vote);
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

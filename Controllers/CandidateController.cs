using EBallotApi.Dto;
using EBallotApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EBallotApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateController : ControllerBase
    {
        private readonly ICandidateService _candidateService;

        public CandidateController(ICandidateService candidateService)
        {
            _candidateService = candidateService;
        }


        //Register Candidate by election officer
        [HttpPost("register")]


        public async Task<IActionResult> RegisterCandidate([FromBody] RegisterCandidateDto dto)
        {
            var CreatedByOfficerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var candidateId = _candidateService.RegisterCandidateAsync(dto, CreatedByOfficerId);

            return Ok(new { CandidateId = candidateId, Message = "Candidate registered successfully" });

        }


        //get candidates created by electionOfficer

        [HttpGet("by-officer")]
        public async Task<IActionResult> GetCandidatesByOfficer()
        {


            int officerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var candidates = await _candidateService.GetCandidatesByOfficerAsync(officerId);

            if (candidates == null || !candidates.Any())
                return NotFound("No candidates found for your assigned constituencies.");

            return Ok(candidates);


        }
    }
}

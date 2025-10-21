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
    public class ElectionController : ControllerBase
    {   
        private readonly IElectionService _electionService;

        public ElectionController(IElectionService electionService)
        {
            _electionService = electionService;
        }


        //create election
        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateElection([FromBody] CreateElectionDto dto, int CreatedById)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new { Message = "Election title is required." });

            // Extract Admin ID from JWT claims
            int createdById = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            

            int newElectionId = await _electionService.CreateElectionAsync(dto, createdById);

            if (newElectionId <= 0)
                return BadRequest(new { Message = "Failed to create election." });

            return Ok(new
            {
                Message = "Election created successfully.",
                ElectionId = newElectionId
            });
        }


        //update election
        [HttpPut("update")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateElection([FromBody] UpdateElectionDto dto)
        {
            if (dto == null || dto.ElectionId <= 0)
                return BadRequest(new { Message = "Invalid election data." });

            int updatedById = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var updatedElectionId = await _electionService.UpdateElectionAsync(dto, updatedById);

            if (updatedElectionId <= 0)
                return BadRequest(new { Message = "Failed to update election." });

            return Ok(new { Message = "Election updated successfully.", ElectionId = updatedElectionId });
        }


        //get all elections
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllElections()
        {
            var elections = await _electionService.GetAllElectionsAsync();

            if (elections == null || !elections.Any())
            {
                return NotFound(new { Message = "No elections found." });
            }

            return Ok(new { Message = "Elections fetched successfully.", Data = elections });
        }

        //get election by id
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetElectionById(int id)
        {
            var election = await _electionService.GetElectionByIdAsync(id);

            if (election == null)
                return NotFound(new { Message = "Election not found." });

            return Ok(new { Message = "Election fetched successfully.", Data = election });
        }


        //assign election to constituency
        [HttpPost("AssignConstituency")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignConstituency([FromBody] AssignConstituencyRequest request)
        {
            if (request == null || request.ElectionId <= 0 || request.ConstituencyId <= 0)
                return BadRequest("Invalid request data.");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            

            var result = await _electionService.AssignConstituencyToElectionAsync(
                request.ElectionId,
                request.ConstituencyId,
                userId
            );

            return Ok(result);
        }

        //unassign election from constituency

        [HttpPost("UnassignConstituency")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnassignConstituency([FromBody] AssignConstituencyRequest request)
        {
            if (request == null || request.ElectionId <= 0 || request.ConstituencyId <= 0)
                return BadRequest("Invalid request data.");

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            bool success = await _electionService.UnassignConstituencyFromElectionAsync(
                request.ElectionId,
                request.ConstituencyId,
                userId
            );

            if (!success)
                return NotFound("The assignment was not found or could not be removed.");

            return Ok(new { Message = "Constituency unassigned successfully." });
        }

        //get elections by constituency
        [HttpGet("ByConstituency/{constituencyId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetElectionsByConstituency(int constituencyId)
        {
            if (constituencyId <= 0)
                return BadRequest("Invalid constituency ID.");

            var elections = await _electionService.GetElectionsByConstituencyAsync(constituencyId);

            return Ok(new { message = "Elections fetched successfully.", data = elections });
        }



    }
}

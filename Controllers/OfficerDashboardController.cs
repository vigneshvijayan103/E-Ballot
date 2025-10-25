using EBallotApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;
using System.Security.Claims;

namespace EBallotApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ElectionOfficer")]
    public class OfficerDashboardController : ControllerBase
    {
        private readonly IElectionOfficerService _electionOfficerService;
        public OfficerDashboardController(IElectionOfficerService electionOfficerService)
        {
            _electionOfficerService = electionOfficerService;
        }

        //officerDashboard metrics
        [HttpGet("metrics")]
        public async Task<IActionResult> GetDashboardMetrics()
        {
            int officerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var result = await _electionOfficerService.GetDashboardMetrics(officerId);
            return Ok(result);
        }

        //get elections assigned to officer
        [HttpGet("Myelections")]
        public async Task<IActionResult> GetElectionsByOfficer()
        {

              int officerId= int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var elections = await _electionOfficerService.GetAssignedElectionsAsync(officerId);

                if (elections == null || !elections.Any())
                    return NotFound($"No elections found for officer ID {officerId}.");

                return Ok(elections);
            


        }

    }
}



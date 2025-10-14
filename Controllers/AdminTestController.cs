
using Microsoft.AspNetCore.Http;    
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace EBallotApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Only Admins can access dashboard metrics
    public class AdminTestController : ControllerBase
    {
        [HttpGet("dashboard/metrics")]
        public IActionResult GetDashboardMetrics()
        {
            var result = new
            {
                totalOfficers = 6,
                totalElections = 3,
                totalConstituencies = 4
            };
            return Ok(result);
        }

        [HttpGet("officers")]
        public IActionResult GetOfficers()
        {
            var officers = new List<object>
            {
                new { id = 1, name = "John Doe", email = "john@eballot.com", constituencyName = "North Zone" },
                new { id = 2, name = "Mary Smith", email = "mary@eballot.com", constituencyName = "South Zone" },
            };
            return Ok(officers);
        }

        [HttpPost("officers")]
        public IActionResult AddOfficer([FromBody] dynamic officer)
        {
            return Ok(new { message = "Officer added successfully", officer });
        }

        [HttpGet("constituencies")]
        public IActionResult GetConstituencies()
        {
            var data = new List<object>
            {
                new { id = 1, name = "North Zone", state = "Kerala" },
                new { id = 2, name = "South Zone", state = "Kerala" }
            };
            return Ok(data);
        }

        [HttpGet("elections")]
        public IActionResult GetElections()
        {
            var elections = new List<object>
            {
                new { id = 1, name = "General Election 2025", date = "2025-12-15" },
                new { id = 2, name = "By-Election 2025", date = "2025-11-20" },
                  new { id = 3, name = "By-Election 2025", date = "2025-11-20" }
            };
            return Ok(elections);
        }

        [HttpGet("elections/{id}/results")]
        public IActionResult GetElectionResults(int id)
        {
            var results = new List<object>
            {
                new { candidateName = "Alice", votes = 1050 },
                new { candidateName = "Bob", votes = 900 }
            };
            return Ok(results);
        }
    }
}

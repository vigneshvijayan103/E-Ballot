using Microsoft.AspNetCore.Mvc;
using EBallotApi.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace EBallotApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] 
    public class DashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _dashboardService;

        public DashboardController(IAdminDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("metrics")]
        public async Task<IActionResult> GetDashboardMetrics()
        {
            var result = await _dashboardService.GetDashboardStatsAsync();
            return Ok(result);
        }
    }
}

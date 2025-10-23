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
    [Authorize(Roles ="Admin")]
    public class ElectionOfficerController : ControllerBase
    {
        private readonly IUserService _userService;
        
        public ElectionOfficerController(IUserService userService)
        {
            _userService = userService;
        }

        //Get all ElectionOfficers
        [HttpGet("all")]
        public async Task<IActionResult> GetAllOfficers()
        {
            var officers = await _userService.GetAllOfficersAsync();
            return Ok(new
            {
                Message = "Officers fetched successfully",
                Data = officers
            });

        }

        //Get ElectionOfficer By Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOfficerById(int id)
        {
            var officer = await _userService.GetOfficerByIdAsync(id);
            if (officer == null)
                return NotFound(new { Message = "Officer not found" });

            return Ok(officer);
        }




        // Update Officer Details By Admin

        [HttpPatch("update")]

          public async Task<IActionResult> UpdateElectionOfficer([FromBody] UpdateElectionOfficerDto dto)
          {
            if (dto == null || dto.OfficerId <= 0)
                return BadRequest(new { Message = "Invalid officer data" });

            // Get AdminId from token
            var updatedByAdminId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            //var updatedByAdminId = 1;

            var updatedOfficer = await _userService.UpdateElectionOfficerAsync(dto, updatedByAdminId);

            if (updatedOfficer == null)
                return NotFound(new { Message = "Officer not found or no changes applied" });

            return Ok(new
            {
                Message = "Officer updated successfully",
            });
          }




      
   


    }
}

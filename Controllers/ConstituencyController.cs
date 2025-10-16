using EBallotApi.Dto;
using EBallotApi.Models;
using EBallotApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;

namespace EBallotApi.Controllers
{
   
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin")]

    public class ConstituencyController : ControllerBase
    {
        private readonly IConstituencyService _constituencyService;
        public ConstituencyController(IConstituencyService constituencyService)
        {
            _constituencyService = constituencyService;
        }

        // Add Constituency
        [HttpPost("add")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddConstituency([FromBody] ConstituencyDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { Message = "Constituency name is required." });

            var createdByAdminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));


            var rowsAffected = await _constituencyService.AddConstituencyAsync(dto,createdByAdminId);

            if (rowsAffected == 0)
            {
                return BadRequest(new { Message = "Failed to add constituency." });
            }

            return Ok(new { Message = "Constituency added successfully." });
           
        }

        //Get Constituency
        [HttpGet("all")]

        public async Task<IActionResult> GetAll()
        {
            var list = await _constituencyService.GetAllConstituenciesAsync();
            return Ok(list);
        }


        
        //Get constituency ById
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var constituency = await _constituencyService.GetConstituencyByIdAsync(id);

            if (constituency == null)
                return NotFound(new { Message = "Constituency not found" });

            return Ok(constituency);
        }



        // Update Constituency By Admin
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateConstituencyDto dto)
        {
            if (id != dto.ConstituencyId)
                return BadRequest(new { Message = "Id mismatch" });

            var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            //var adminId = 1;
            var result = await _constituencyService.UpdateConstituencyAsync(dto, adminId);

            if (!result)
                return NotFound(new { Message = "Constituency not found or not updated" });

            return Ok(new { Message = "Constituency updated successfully" });
        }

    }

}

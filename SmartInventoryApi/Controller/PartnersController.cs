using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventoryApi.DTOs;
using SmartInventoryApi.Services;

namespace SmartInventoryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PartnersController : ControllerBase
    {
        private readonly IPartnerService _partnerService;

        public PartnersController(IPartnerService partnerService)
        {
            _partnerService = partnerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPartners([FromQuery] string? type)
        {
            var partners = await _partnerService.GetAllPartnersAsync(type);
            return Ok(partners);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPartnerById(int id)
        {
            var partner = await _partnerService.GetPartnerByIdAsync(id);
            if (partner == null)
            {
                return NotFound("Partner not found.");
            }
            return Ok(partner);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreatePartner([FromBody] CreatePartnerDto createPartnerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var newPartner = await _partnerService.CreatePartnerAsync(createPartnerDto);
                return CreatedAtAction(nameof(GetPartnerById), new { id = newPartner.Id }, newPartner);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdatePartner(int id, [FromBody] UpdatePartnerDto updatePartnerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                await _partnerService.UpdatePartnerAsync(id, updatePartnerDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePartner(int id)
        {
            try
            {
                await _partnerService.DeletePartnerAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
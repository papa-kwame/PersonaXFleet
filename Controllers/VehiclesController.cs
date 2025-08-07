using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonaXFleet.Data;
using PersonaXFleet.DTOs;
using PersonaXFleet.Models.Enums;
using PersonaXFleet.Models;
using PersonaXFleet.Services;
using System.Security.Claims;

namespace PersonaXFleet.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        private readonly AuthDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserActivityService _activityService;
        public VehiclesController(IVehicleService vehicleService, AuthDbContext context,
             UserManager<ApplicationUser> userManager, IUserActivityService activityService)
        {
            _context = context; 
            _userManager = userManager; 
            _vehicleService = vehicleService;
            _activityService = activityService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehicleDto>>> GetVehicles()
        {
            var vehicles = await _vehicleService.GetAllVehiclesAsync();
            return Ok(vehicles);
        }



        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleDto>> GetVehicle(string id)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);

            if (vehicle == null)
            {
                return NotFound();
            }

            return vehicle;
        }

        [HttpPost]
        public async Task<ActionResult<VehicleDto>> PostVehicle(VehicleDto vehicleDto, [FromQuery] string userId)
        {
            try
            {
                var createdVehicle = await _vehicleService.CreateVehicleAsync(vehicleDto);
                
                // Log activity
                if (!string.IsNullOrEmpty(userId))
                {
                    await _activityService.LogActivityAsync(userId, "Create", "Vehicles", 
                        $"Created vehicle {createdVehicle.LicensePlate}", 
                        "Vehicle", createdVehicle.Id, 
                        new { make = createdVehicle.Make, model = createdVehicle.Model, licensePlate = createdVehicle.LicensePlate });
                }
                
                return CreatedAtAction(nameof(GetVehicle), new { id = createdVehicle.Id }, createdVehicle);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("due-dates")]
        public async Task<IActionResult> GetVehiclesWithDueDates([FromQuery] int days = 30)
        {
            var vehicles = await _vehicleService.GetVehiclesWithDueDatesAsync(days);
            return Ok(vehicles);
        }

        [HttpGet("document-expiry")]
        public async Task<IActionResult> GetDocumentExpiryDetails([FromQuery] int days = 30)
        {
            var expiryDetails = await _vehicleService.GetDocumentExpiryDetailsAsync(days);
            return Ok(expiryDetails);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutVehicle(string id, VehicleDto vehicleDto, [FromQuery] string userId)
        {
            if (id != vehicleDto.Id)
            {
                return BadRequest("ID mismatch");
            }

            try
            {
                await _vehicleService.UpdateVehicleAsync(id, vehicleDto);
                
                // Log activity
                if (!string.IsNullOrEmpty(userId))
                {
                    await _activityService.LogActivityAsync(userId, "Update", "Vehicles", 
                        $"Updated vehicle {vehicleDto.LicensePlate}", 
                        "Vehicle", id, 
                        new { make = vehicleDto.Make, model = vehicleDto.Model, licensePlate = vehicleDto.LicensePlate });
                }
                
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(string id, [FromQuery] string userId)
        {
            try
            {
                await _vehicleService.DeleteVehicleAsync(id);
                
                // Log activity
                if (!string.IsNullOrEmpty(userId))
                {
                    await _activityService.LogActivityAsync(userId, "Delete", "Vehicles", 
                        $"Deleted vehicle", 
                        "Vehicle", id, 
                        new { vehicleId = id });
                }
                
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("CheckAssignment")]
        public async Task<ActionResult<bool>> CheckVehicleAssignment(CheckAssignmentDto dto,AuthDbContext _context)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.Id == dto.VehicleId);

            if (vehicle == null) return NotFound("Vehicle not found");

            var isAssigned = vehicle.UserId == dto.UserId;
            return Ok(isAssigned);
        }


      


        [HttpGet("Available")]
        [Produces("application/json")] 
        public async Task<IActionResult> GetAvailableVehicles()
        {
            try
            {
                var vehicles = await _context.Vehicles
                    .Where(v => v.UserId == null) 
                    .Select(v => new {
                        v.Id,
                        v.Make,
                        v.Model,
                        v.LicensePlate
                    })
                    .ToListAsync();

                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error retrieving vehicles");
            }
        }
    }
}

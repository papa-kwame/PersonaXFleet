using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonaXFleet.Data;
using PersonaXFleet.DTOs;
using PersonaXFleet.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonaXFleet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FuelLogsController : ControllerBase
    {
        private readonly AuthDbContext _context;

        public FuelLogsController(AuthDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FuelLog>>> GetFuelLogs()
        {
            var fuelLogs = await _context.FuelLogs
                .Include(fl => fl.Vehicle)
                .ToListAsync();
            return fuelLogs;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<FuelLog>>> GetFuelLogsByUserId(string userId)
        {
            var fuelLogs = await _context.FuelLogs
                .Include(fl => fl.Vehicle) 
                .Where(fl => fl.UserId == userId)
                .ToListAsync();

            if (fuelLogs == null || !fuelLogs.Any())
            {
                return Ok("You don't have any fuel logs.");
            }

            return Ok(fuelLogs);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<FuelLog>> GetFuelLog(string id)
        {
            var fuelLog = await _context.FuelLogs
                .Include(fl => fl.Vehicle)
                .FirstOrDefaultAsync(fl => fl.Id == id);

            if (fuelLog == null)
            {
                return NotFound();
            }

            return fuelLog;
        }

        [HttpPost("{userId}")]
        public async Task<ActionResult<FuelLog>> PostFuelLog(string userId, FuelLogDto fuelLogDto)
        {
            var assignedVehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (assignedVehicle == null)
            {
                return BadRequest("No vehicle assigned to the specified user.");
            }

            var fuelLog = new FuelLog
            {
                UserId = userId,
                VehicleId = assignedVehicle.Id,
                Vehicle = assignedVehicle,
                FuelAmount = fuelLogDto.FuelAmount,
                Cost = fuelLogDto.Cost,
                FuelStation = fuelLogDto.FuelStation,
                Date = DateTime.UtcNow
            };

            _context.FuelLogs.Add(fuelLog);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFuelLog", new { id = fuelLog.Id }, fuelLog);
        }

        [HttpGet("vehicle/{vehicleId}")]
        public async Task<IActionResult> GetFuelLogsByVehicle(string vehicleId)
        {
            var logs = await _context.FuelLogs
                .Where(l => l.VehicleId == vehicleId)
                .Include(l => l.User) // Eager load the User
                .ToListAsync();

            // Optionally, map to a DTO to avoid circular references or overexposing user data
            var result = logs.Select(log => new
            {
                log.Id,
                log.VehicleId,
                log.UserId,
                User = log.User == null ? null : new
                {
                    log.User.Id,
                    log.User.UserName,
                },
                log.FuelAmount,
                log.Cost,
                log.Date,
                log.FuelStation
            });

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutFuelLog(string id, FuelLogDto fuelLogDto)
        {
            var fuelLog = await _context.FuelLogs.FindAsync(id);
            if (fuelLog == null)
            {
                return NotFound();
            }

            fuelLog.FuelAmount = fuelLogDto.FuelAmount;
            fuelLog.Cost = fuelLogDto.Cost;
            fuelLog.FuelStation = fuelLogDto.FuelStation;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("user/{userId}/stats")]
        public async Task<ActionResult<object>> GetUserFuelStats(string userId)
        {
            var logs = await _context.FuelLogs
                .Where(fl => fl.UserId == userId)
                .ToListAsync();

            if (!logs.Any())
                return NotFound();

            var totalFuel = logs.Sum(fl => fl.FuelAmount);
            var totalCost = logs.Sum(fl => fl.Cost);
            var averageCostPerLitre = totalFuel > 0 ? totalCost / totalFuel : 0;

            return Ok(new
            {
                TotalFuel = totalFuel,
                TotalCost = totalCost,
                AverageCostPerLitre = averageCostPerLitre
            });
        }

        [HttpGet("user/{userId}/filter")]
        public async Task<ActionResult<IEnumerable<FuelLog>>> GetFuelLogsByUserWithDateRange(
            string userId, DateTime startDate, DateTime endDate)
        {
            var logs = await _context.FuelLogs
                .Where(fl => fl.UserId == userId && fl.Date >= startDate && fl.Date <= endDate)
                .ToListAsync();

            if (!logs.Any())
                return NotFound();

            return logs;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFuelLog(string id)
        {
            var fuelLog = await _context.FuelLogs.FindAsync(id);
            if (fuelLog == null)
            {
                return NotFound();
            }

            _context.FuelLogs.Remove(fuelLog);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("stats/monthly-summary")]
        public async Task<ActionResult<object>> GetMonthlyFuelSummary()
        {
            var now = DateTime.UtcNow;
            var thirtyDaysAgo = now.AddDays(-30); // Calculate the date 30 days ago
            var today = now.Date;

            var logsLastThirtyDays = await _context.FuelLogs
                .Where(fl => fl.Date >= thirtyDaysAgo && fl.Date <= now)
                .ToListAsync();

            if (!logsLastThirtyDays.Any())
                return Ok(new
                {
                    totalMonthlyFuelCost = 0,
                    totalTodayFuelCost = 0,
                    topSpender = (object)null,
                    dailyExpenses = new object[0]
                });

            var totalMonthlyFuelCost = logsLastThirtyDays.Sum(fl => fl.Cost);

            var totalTodayFuelCost = logsLastThirtyDays
                .Where(fl => fl.Date.Date == today)
                .Sum(fl => fl.Cost);

            var topSpender = logsLastThirtyDays
                .GroupBy(fl => fl.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalSpent = g.Sum(fl => fl.Cost)
                })
                .OrderByDescending(x => x.TotalSpent)
                .FirstOrDefault();

            var topSpenderUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == topSpender.UserId);

            var dailyExpenses = logsLastThirtyDays
                .GroupBy(fl => fl.Date.Date)
                .Select(g => new
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Cost = g.Sum(fl => fl.Cost)
                })
                .OrderBy(x => x.Date)
                .ToList();

            return Ok(new
            {
                totalMonthlyFuelCost,
                totalTodayFuelCost,
                topSpender = new
                {
                    userId = topSpender.UserId,
                    name = topSpenderUser?.UserName ?? "Unknown",
                    totalSpent = topSpender.TotalSpent
                },
                dailyExpenses
            });
        }


        [HttpGet("stats/date-range")]
        public async Task<IActionResult> GetFuelStatsByDateRange(DateTime startDate, DateTime endDate)
        {
            var logs = await _context.FuelLogs
                .Where(fl => fl.Date >= startDate && fl.Date <= endDate)
                .ToListAsync();

            if (!logs.Any())
            {
                return NotFound("No fuel logs found within the specified date range.");
            }

            var totalFuel = logs.Sum(fl => fl.FuelAmount);
            var totalCost = logs.Sum(fl => fl.Cost);
            var averageCostPerLitre = totalFuel > 0 ? totalCost / totalFuel : 0;

            return Ok(new
            {
                StartDate = startDate.ToString("yyyy-MM-dd"),
                EndDate = endDate.ToString("yyyy-MM-dd"),
                TotalFuel = totalFuel,
                TotalCost = totalCost,
                AverageCostPerLitre = averageCostPerLitre
            });
        }


        [HttpGet("stats/user/{userId}/date-range")]
        public async Task<IActionResult> GetUserFuelStatsByDateRange(string userId, DateTime startDate, DateTime endDate)
        {
            var logs = await _context.FuelLogs
                .Where(fl => fl.UserId == userId && fl.Date >= startDate && fl.Date <= endDate)
                .ToListAsync();

            if (!logs.Any())
            {
                return NotFound("No fuel logs found for this user within the specified date range.");
            }

            var totalFuel = logs.Sum(fl => fl.FuelAmount);
            var totalCost = logs.Sum(fl => fl.Cost);
            var averageCostPerLitre = totalFuel > 0 ? totalCost / totalFuel : 0;

            var user = await _context.Users.FindAsync(userId);

            return Ok(new
            {
                UserId = userId,
                UserName = user?.UserName ?? "Unknown",
                StartDate = startDate.ToString("yyyy-MM-dd"),
                EndDate = endDate.ToString("yyyy-MM-dd"),
                TotalFuel = totalFuel,
                TotalCost = totalCost,
                AverageCostPerLitre = averageCostPerLitre
            });
        }


        [HttpGet("stats/{vehicleId}")]
        public async Task<IActionResult> GetVehicleFuelStats(
            string vehicleId,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.FuelLogs)
                .FirstOrDefaultAsync(v => v.Id == vehicleId);

            if (vehicle == null)
            {
                return NotFound("Vehicle not found.");
            }

            var fuelLogs = vehicle.FuelLogs.AsQueryable();

            if (startDate.HasValue)
                fuelLogs = fuelLogs.Where(log => log.Date >= startDate.Value);

            if (endDate.HasValue)
                fuelLogs = fuelLogs.Where(log => log.Date <= endDate.Value);

            if (!fuelLogs.Any())
            {
                return NotFound("No fuel logs found for this vehicle in the specified period.");
            }

            var totalFuel = fuelLogs.Sum(log => log.FuelAmount);
            var totalCost = fuelLogs.Sum(log => log.Cost);

            return Ok(new
            {
                VehicleId = vehicle.Id,
                Vehicle = $"{vehicle.Make} {vehicle.Model} {vehicle.Year}",
                LicensePlate = vehicle.LicensePlate,
                TotalFuelAmount = totalFuel,
                TotalFuelCost = totalCost,
                Transactions = fuelLogs
                    .OrderByDescending(log => log.Date)
                    .Select(log => new
                    {
                        log.Id,
                        log.Date,
                        log.FuelStation,
                        log.FuelAmount,
                        log.Cost
                    })
            });
        }
    }
}

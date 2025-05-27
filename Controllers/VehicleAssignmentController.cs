using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonaXFleet.Data;
using PersonaXFleet.DTOs;
using PersonaXFleet.Models;
using PersonaXFleet.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace PersonaXFleet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleAssignmentController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public VehicleAssignmentController(
            AuthDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("AllAssignments")]
        public async Task<IActionResult> GetAllAssignments()
        {
            var assignments = await _context.Vehicles
                .Where(v => v.UserId != null)
                .Include(v => v.User)
                .Select(v => new VehicleAssignmentDto
                {
                    AssignmentId = $"{v.Id}-{v.UserId}",
                    VehicleId = v.Id,
                    VehicleMake = v.Make,
                    VehicleModel = v.Model,
                    LicensePlate = v.LicensePlate,
                    UserId = v.UserId,
                    UserName = v.User.UserName,
                    UserEmail = v.User.Email,
                    AssignmentDate = null
                })
                .ToListAsync();

            return Ok(assignments);
        }

       

        [HttpGet("AssignmentHistory/{vehicleId}")]
        public async Task<IActionResult> GetAssignmentHistory(string vehicleId)
        {
            var history = await _context.VehicleAssignmentHistories
                .Where(h => h.VehicleId == vehicleId)
                .OrderByDescending(h => h.AssignmentDate)
                .Select(h => new
                {
                    h.Id,
                    h.UserId,
                    UserName = h.User.UserName,
                    UserEmail = h.User.Email,
                    h.AssignmentDate,
                    h.UnassignmentDate,
                    Duration = h.UnassignmentDate.HasValue
                        ? (h.UnassignmentDate.Value - h.AssignmentDate).ToString(@"dd\.hh\:mm\:ss")
                        : "Current"
                })
                .ToListAsync();

            return Ok(history);
        }

        [HttpGet("ByUser/{userId}")]
        public async Task<IActionResult> GetVehiclesByUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var vehicles = _context.Vehicles
                .Where(v => v.UserId == userId)
                .Select(v => new VehicleDto
                {
                    Id = v.Id,
                    Make = v.Make,
                    Model = v.Model,
                    Year = v.Year,
                    LicensePlate = v.LicensePlate,
                    VehicleType = v.VehicleType,
                    Status = v.Status.ToString()
                })
                .ToList();

            return Ok(vehicles);
        }

        [HttpGet("ByVehicle/{vehicleId}")]
        public async Task<IActionResult> GetUserByVehicle(string vehicleId)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.Id == vehicleId);

            if (vehicle == null)
            {
                return NotFound("Vehicle not found");
            }

            if (vehicle.User == null)
            {
                return Ok(new { Message = "No user assigned to this vehicle" });
            }

            return Ok(new UserDto
            {
                Id = vehicle.User.Id,
                UserName = vehicle.User.UserName,
                Email = vehicle.User.Email
            });
        }

        [HttpPost("Unassign")]
        public async Task<IActionResult> UnassignVehicle([FromBody] string vehicleId)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.Id == vehicleId);

            if (vehicle == null)
            {
                return NotFound("Vehicle not found");
            }

            if (string.IsNullOrEmpty(vehicle.UserId))
            {
                return BadRequest("This vehicle is not currently assigned to any user.");
            }

            var currentHistory = await _context.VehicleAssignmentHistories
                .FirstOrDefaultAsync(h => h.VehicleId == vehicleId && h.UnassignmentDate == null);

            if (currentHistory != null)
            {
                currentHistory.UnassignmentDate = DateTime.UtcNow;
            }

            vehicle.UserId = null;
            vehicle.User = null;

            try
            {
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Vehicle unassigned successfully",
                    VehicleId = vehicle.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error unassigning vehicle: {ex.Message}");
            }
        }

        [HttpPost("RequestVehicle")]
        public async Task<IActionResult> RequestVehicle([FromBody] VehicleRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrEmpty(dto.UserId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                return NotFound("User not found");

            var vehicle = await _context.Vehicles.FindAsync(dto.VehicleId);
            if (vehicle == null)
                return NotFound("Vehicle not found");

            if (!string.IsNullOrEmpty(vehicle.UserId))
                return BadRequest("Vehicle is already assigned to another user");

            var department = user.Department;

            var route = await _context.Routes
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.Department == department);

            if (route == null)
                return BadRequest("No approval route found for department");

            var request = new VehicleAssignmentRequest
            {
                UserId = dto.UserId,
                VehicleId = dto.VehicleId,
                RequestReason = dto.RequestReason,
                RequestDate = DateTime.UtcNow,
                CurrentStage = "Comment",
                Status = VehicleRequestStatus.Pending,
                CurrentRouteId = route.Id
            };

            _context.VehicleAssignmentRequests.Add(request);
            await _context.SaveChangesAsync();

            var creationTransaction = new VehicleAssignmentTransaction
            {
                VehicleAssignmentRequestId = request.Id,
                UserId = dto.UserId,
                Stage = "Create",
                Comments = "Vehicle assignment request created",
                Timestamp = DateTime.UtcNow,
                IsCompleted = true
            };

            _context.VehicleAssignmentTransactions.Add(creationTransaction);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Vehicle assignment request submitted successfully",
                RequestId = request.Id,
                RouteId = route.Id,
                CurrentStage = request.CurrentStage
            });
        }

        [HttpPost("vehicle-requests/{id}/process-stage")]
        public async Task<IActionResult> ProcessVehicleRequestStage(string id, [FromBody] ProcessStageDto dto, [FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var request = await _context.VehicleAssignmentRequests
                .Include(r => r.CurrentRoute)
                    .ThenInclude(r => r.UserRoles)
                .Include(r => r.Transactions)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            bool isRequestorInCurrentStage = request.UserId == userId;

            if (isRequestorInCurrentStage)
            {
                var existingTransaction = request.Transactions
                    .FirstOrDefault(t => t.UserId == userId && t.Stage == request.CurrentStage);

                if (existingTransaction == null)
                {
                    var skipTransaction = new VehicleAssignmentTransaction
                    {
                        VehicleAssignmentRequestId = request.Id,
                        UserId = userId,
                        Stage = request.CurrentStage,
                        Comments = "Automatically skipped for requestor",
                        Timestamp = DateTime.UtcNow,
                        IsCompleted = true
                    };

                    _context.VehicleAssignmentTransactions.Add(skipTransaction);
                    await _context.SaveChangesAsync();
                    await _context.Entry(request).Collection(r => r.Transactions).LoadAsync();
                }

                var requiredUsers = request.CurrentRoute.UserRoles
                    .Where(ur => ur.Role.Equals(request.CurrentStage, StringComparison.OrdinalIgnoreCase))
                    .Select(ur => ur.UserId).Distinct().ToList();

                var completedUsers = request.Transactions
                    .Where(t => t.Stage == request.CurrentStage && t.IsCompleted)
                    .Select(t => t.UserId).Distinct().ToList();

                if (completedUsers.Count >= requiredUsers.Count)
                {
                    var nextStage = GetNextStage(request.CurrentStage);

                    if (nextStage == "Complete")
                    {
                        request.Status = VehicleRequestStatus.Approved;
                        request.CurrentStage = nextStage;
                        await _context.SaveChangesAsync();

                        // Assign the vehicle to the user
                        var vehicle = await _context.Vehicles.FindAsync(request.VehicleId);
                        if (vehicle == null)
                        {
                            return NotFound("Vehicle not found");
                        }

                        var user = await _userManager.FindByIdAsync(request.UserId);
                        if (user == null)
                        {
                            return NotFound("User not found");
                        }

                        if (!string.IsNullOrEmpty(vehicle.UserId))
                        {
                            var previousAssignment = await _context.VehicleAssignmentHistories
                                .FirstOrDefaultAsync(h => h.VehicleId == vehicle.Id && h.UnassignmentDate == null);

                            if (previousAssignment != null)
                            {
                                previousAssignment.UnassignmentDate = DateTime.UtcNow;
                            }
                        }

                        vehicle.UserId = user.Id;
                        vehicle.User = user;

                        _context.VehicleAssignmentHistories.Add(new VehicleAssignmentHistory
                        {
                            VehicleId = vehicle.Id,
                            UserId = user.Id,
                            AssignmentDate = DateTime.UtcNow
                        });

                        await _context.SaveChangesAsync();

                        return Ok(new
                        {
                            Message = "Vehicle request approved and completed. Vehicle assigned successfully",
                            RequestId = request.Id,
                            Vehicle = new VehicleDto
                            {
                                Id = vehicle.Id,
                                Make = vehicle.Make,
                                Model = vehicle.Model,
                                Year = vehicle.Year,
                                LicensePlate = vehicle.LicensePlate,
                                VehicleType = vehicle.VehicleType,
                                Status = vehicle.Status.ToString()
                            },
                            User = new UserDto
                            {
                                Id = user.Id,
                                UserName = user.UserName,
                                Email = user.Email
                            }
                        });
                    }

                    request.CurrentStage = nextStage;
                    await _context.SaveChangesAsync();
                }

                return NoContent();
            }

            var userRole = request.CurrentRoute.UserRoles
                .FirstOrDefault(ur => ur.UserId == userId && ur.Role.Equals(request.CurrentStage, StringComparison.OrdinalIgnoreCase));

            if (userRole == null)
                return Forbid("You don't have permission for this action");

            var existingNormalTransaction = request.Transactions
                .FirstOrDefault(t => t.UserId == userId && t.Stage == request.CurrentStage);

            if (existingNormalTransaction != null)
                return Conflict("You've already processed this stage");

            var transaction = new VehicleAssignmentTransaction
            {
                VehicleAssignmentRequestId = request.Id,
                UserId = userId,
                Stage = request.CurrentStage,
                Comments = dto.Comments,
                Timestamp = DateTime.UtcNow,
                IsCompleted = true
            };

            _context.VehicleAssignmentTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            await _context.Entry(request).Collection(r => r.Transactions).LoadAsync();

            var requiredAfter = request.CurrentRoute.UserRoles
                .Where(ur => ur.Role.Equals(request.CurrentStage, StringComparison.OrdinalIgnoreCase))
                .Select(ur => ur.UserId).Distinct().ToList();

            var completedAfter = request.Transactions
                .Where(t => t.Stage == request.CurrentStage && t.IsCompleted)
                .Select(t => t.UserId).Distinct().ToList();

            if (completedAfter.Count >= requiredAfter.Count)
            {
                var nextStage = GetNextStage(request.CurrentStage);

                if (nextStage == "Complete")
                {
                    request.Status = VehicleRequestStatus.Approved;
                    request.CurrentStage = nextStage;
                    await _context.SaveChangesAsync();

                    var vehicle = await _context.Vehicles.FindAsync(request.VehicleId);
                    if (vehicle == null)
                    {
                        return NotFound("Vehicle not found");
                    }

                    var user = await _userManager.FindByIdAsync(request.UserId);
                    if (user == null)
                    {
                        return NotFound("User not found");
                    }

                    if (!string.IsNullOrEmpty(vehicle.UserId))
                    {
                        var previousAssignment = await _context.VehicleAssignmentHistories
                            .FirstOrDefaultAsync(h => h.VehicleId == vehicle.Id && h.UnassignmentDate == null);

                        if (previousAssignment != null)
                        {
                            previousAssignment.UnassignmentDate = DateTime.UtcNow;
                        }
                    }

                    vehicle.UserId = user.Id;
                    vehicle.User = user;

                    _context.VehicleAssignmentHistories.Add(new VehicleAssignmentHistory
                    {
                        VehicleId = vehicle.Id,
                        UserId = user.Id,
                        AssignmentDate = DateTime.UtcNow
                    });

                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        Message = "Vehicle request approved and completed. Vehicle assigned successfully",
                        RequestId = request.Id,
                        Vehicle = new VehicleDto
                        {
                            Id = vehicle.Id,
                            Make = vehicle.Make,
                            Model = vehicle.Model,
                            Year = vehicle.Year,
                            LicensePlate = vehicle.LicensePlate,
                            VehicleType = vehicle.VehicleType,
                            Status = vehicle.Status.ToString()
                        },
                        User = new UserDto
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            Email = user.Email
                        }
                    });
                }

                request.CurrentStage = nextStage;
                await _context.SaveChangesAsync();
            }

            var stageComments = await _context.VehicleAssignmentTransactions
                .Where(t => t.VehicleAssignmentRequestId == request.Id && !string.IsNullOrEmpty(t.Comments))
                .GroupBy(t => t.Stage)
                .Select(g => new
                {
                    Stage = g.Key,
                    Comments = g.Select(t => new
                    {
                        UserId = t.UserId,
                        Comment = t.Comments,
                        Timestamp = t.Timestamp
                    }).ToList()
                }).ToListAsync();

            return Ok(new
            {
                Message = "Stage processed successfully",
                RequestId = request.Id,
                CurrentStage = request.CurrentStage,
                Status = request.Status.ToString(),
                StageComments = stageComments
            });
        }


        [HttpGet("vehicle-requests/{id}/workflow-status")]
        public async Task<IActionResult> GetVehicleWorkflowStatus(string id)
        {
            var request = await _context.VehicleAssignmentRequests
                .Include(r => r.CurrentRoute)
                    .ThenInclude(r => r.UserRoles)
                    .ThenInclude(ur => ur.User)
                .Include(r => r.Transactions)
                    .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound("Vehicle assignment request not found");

            var status = new
            {
                CurrentStage = request.CurrentStage,
                CompletedActions = request.Transactions
                    .Where(t => t.IsCompleted)
                    .GroupBy(t => t.Stage)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(t => new
                        {
                            UserName = t.User?.UserName ?? "(unknown)",
                            Comments = t.Comments,
                            Timestamp = t.Timestamp
                        }).ToList()
                    ),
                PendingActions = request.CurrentRoute.UserRoles
                    .Where(ur => ur.Role == request.CurrentStage)
                    .Select(ur => new
                    {
                        Role = ur.Role,
                        UserName = ur.User?.UserName ?? "(unknown user)",
                        UserId = ur.UserId,
                        IsPending = !request.Transactions.Any(t =>
                            t.Stage == ur.Role &&
                            t.UserId == ur.UserId &&
                            t.IsCompleted)
                    }).ToList()
            };

            return Ok(status);
        }


        [HttpGet("vehicle-requests/{id}/comments")]
        public async Task<IActionResult> GetAllVehicleRequestComments(string id)
        {
            var request = await _context.VehicleAssignmentRequests
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound("Vehicle assignment request not found");

            var comments = await _context.VehicleAssignmentTransactions
                .Where(t => t.VehicleAssignmentRequestId == id && !string.IsNullOrEmpty(t.Comments))
                .Select(t => new
                {
                    Stage = t.Stage,
                    Comment = t.Comments,
                    UserId = t.UserId,
                    Timestamp = t.Timestamp
                })
                .OrderBy(t => t.Timestamp)
                .ToListAsync();

            return Ok(new
            {
                RequestId = id,
                Comments = comments
            });
        }


        [HttpGet("my-pending-actions")]
        public async Task<IActionResult> GetMyPendingVehicleActions([FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var requests = await (from ur in _context.UserRouteRoles
                                  where ur.UserId == userId
                                  join r in _context.VehicleAssignmentRequests
                                      .Include(r => r.User)
                                      .Include(r => r.Vehicle)
                                      .Include(r => r.CurrentRoute)
                                  on ur.RouteId equals r.CurrentRouteId
                                  where ur.Role == r.CurrentStage
                                  where !_context.VehicleAssignmentTransactions.Any(t =>
                                      t.VehicleAssignmentRequestId == r.Id &&
                                      t.Stage == r.CurrentStage &&
                                      t.UserId == userId &&
                                      t.IsCompleted)
                                  select new
                                  {
                                      r.Id,
                                      r.UserId,
                                      RequestorName = r.User.UserName,
                                      r.RequestReason,
                                      r.RequestDate,
                                      r.CurrentStage,
                                      r.Status,
                                      RouteName = r.CurrentRoute.Name,
                                      Vehicle = new
                                      {
                                          r.Vehicle.Id,
                                          r.Vehicle.Make,
                                          r.Vehicle.Model,
                                          r.Vehicle.LicensePlate
                                      }
                                  }).ToListAsync();

            return Ok(requests);
        }

        [HttpGet("{id}/comments")]
        public async Task<IActionResult> GetAllRequestComments(string id)
        {
            var request = await _context.MaintenanceRequests
                .FirstOrDefaultAsync(r => r.MaintenanceId == id);

            if (request == null)
                return NotFound("Maintenance request not found");

            var comments = await _context.MaintenanceTransactions
                .Where(t => t.MaintenanceRequestId == id && !string.IsNullOrEmpty(t.Comments))
                .Select(t => new
                {
                    Stage = t.Action,
                    Comment = t.Comments,
                    UserId = t.UserId,
                    Timestamp = t.Timestamp
                })
                .OrderBy(t => t.Timestamp)
                .ToListAsync();

            return Ok(new
            {
                RequestId = id,
                Comments = comments
            });
        }

        private string GetNextStage(string currentStage)
        {
            return currentStage switch
            {
                "Comment" => "Review",
                "Review" => "Commit",
                "Commit" => "Approve",
                "Approve" => "Complete",
                _ => "Complete"
            };
        }

        [HttpGet("UserRequests/{userId}")]
        public async Task<IActionResult> GetUserRequests(string userId)
        {
            var requests = await _context.VehicleAssignmentRequests
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.RequestDate)
                .Select(a => new
                {
                    a.Id,
                    a.Status,
                    a.RequestDate
                })
                .ToListAsync();
             
            return Ok(requests);
        }

        [HttpGet("AllRequests")]
        public async Task<IActionResult> GetAllRequests()
        {
            var requests = await _context.VehicleAssignmentRequests
                .Include(r => r.User)
                .Include(r => r.Vehicle)
                .Include(r => r.CurrentRoute) // include route
                .OrderByDescending(r => r.RequestDate)
                .Select(r => new
                {
                    r.Id,
                    r.UserId,
                    r.User.UserName,
                    r.User.Email,
                    r.RequestReason,
                    r.RequestDate,
                    r.Status,
                    r.CurrentStage,
                    RouteName = r.CurrentRoute.Name,
                    Vehicle = new
                    {
                        r.Vehicle.Id,
                        r.Vehicle.Make,
                        r.Vehicle.Model,
                        r.Vehicle.LicensePlate
                    }
                }).ToListAsync();

            return Ok(requests);
        }

        [HttpGet("MyVehicleRequests/{userId}")]
        public async Task<IActionResult> GetMyVehicleRequests(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var requests = await _context.VehicleAssignmentRequests
                .Where(r => r.UserId == userId)
                .Include(r => r.Vehicle) // Include Vehicle details
                .OrderByDescending(r => r.RequestDate)
                .Select(r => new
                {
                    r.Id,
                    r.RequestReason,
                    r.RequestDate,
                    r.Status,
                    r.CurrentStage,
                    Vehicle = r.Vehicle == null ? null : new
                    {
                        r.Vehicle.Id,
                        r.Vehicle.Make,
                        r.Vehicle.Model,
                        r.Vehicle.LicensePlate
                    }
                })
                .ToListAsync();

            return Ok(requests);
        }
    }
}

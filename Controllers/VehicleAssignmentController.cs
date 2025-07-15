using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PersonaXFleet.Data;
using PersonaXFleet.DTOs;
using PersonaXFleet.Models;
using PersonaXFleet.Models.Enums;
using PersonaXFleet.Services;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;

namespace PersonaXFleet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleAssignmentController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _env;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public VehicleAssignmentController(
            AuthDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IWebHostEnvironment env,
            INotificationService notificationService,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _env = env;
            _notificationService = notificationService;
            _hubContext = hubContext;
        }

        private string LoadTemplate(string relativePath, Dictionary<string, string> placeholders)
        {
            string path = Path.Combine(_env.ContentRootPath, relativePath);
            if (!System.IO.File.Exists(path))
            {
                Console.WriteLine($"Template not found at: {path}");
                return "<p>Email template not found.</p>";
            }
            string template = System.IO.File.ReadAllText(path, Encoding.UTF8);
            foreach (var placeholder in placeholders)
            {
                template = template.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
            }
            return template;
        }

        private async Task SendEmailAsync(EmailMessage message)
        {
            var placeholders = new Dictionary<string, string>

                {
                    { "firstName", message.firstName ?? "Failed to Fetch Name"},
                    { "Subject", message.Subject },
                    { "BodyContent", message.Body },
                    { "Year", DateTime.Now.Year.ToString() }
                };

            string template = LoadTemplate("Templates/EmailTemplate.html", placeholders);
            message.Body = template;

            await _emailService.SendEmailAsync(message);
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

            var hasAssignedVehicle = await _context.Vehicles.AnyAsync(v => v.UserId == dto.UserId);
            if (hasAssignedVehicle)
                return BadRequest("User already has a vehicle assigned");

            var vehicle = await _context.Vehicles.FindAsync(dto.VehicleId);
            if (vehicle == null)
                return NotFound("Vehicle not found");

            if (!string.IsNullOrEmpty(vehicle.UserId))
                return BadRequest("Vehicle is already assigned to another user");

            var existingPendingRequest = await _context.VehicleAssignmentRequests
                .AnyAsync(r => r.VehicleId == dto.VehicleId && r.Status == VehicleRequestStatus.Pending);
            if (existingPendingRequest)
                return Conflict("There is already a pending request for this vehicle.");

            var department = user.Department;
            var route = await _context.Routes.Include(r => r.UserRoles).FirstOrDefaultAsync(r => r.Department == department);
            if (route == null)
            {
                route = await _context.Routes.Include(r => r.UserRoles).FirstOrDefaultAsync(r => r.Department == "Default");
                if (route == null)
                    return BadRequest("No default approval route found");
            }

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

            var requesterEmail = new EmailMessage
            {   firstName = user.UserName,
                ToAddresses = new List<string> { "ofosupapa@gmail.com" },
                Subject = "Vehicle Request Created",
                Body = $"Your vehicle request for {vehicle.LicensePlate} has been created and is pending approval."
            };
            await SendEmailAsync(requesterEmail);
            await _notificationService.CreateNotificationAsync(
                user.Id,
                "Vehicle Request Created",
                $"Your vehicle request for {vehicle.LicensePlate} has been created and is pending approval.",
                NotificationType.System,
                request.Id,
                null
            );
            await _hubContext.Clients.Group(user.Id).SendAsync("NewNotification");

            await NotifyUsersForStageAsync(request, request.CurrentStage);

            return Ok(new
            {
                Message = "Vehicle assignment request submitted successfully",
                RequestId = request.Id,
                RouteId = route.Id,
                CurrentStage = request.CurrentStage,
                VehicleLicensePlate = vehicle.LicensePlate
            });
        }

        private async Task NotifyUsersForStageAsync(VehicleAssignmentRequest request, string stageName)
        {
            var route = await _context.Routes
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.Id == request.CurrentRouteId);

            if (route == null) return;

            var users = route.UserRoles
                .Where(ur => ur.Role.Equals(stageName, StringComparison.OrdinalIgnoreCase))
                .Select(ur => ur.UserId)
                .Distinct();

            foreach (var userId in users)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    var email = new EmailMessage
                 
                    { 
                        firstName = user.UserName,
                        ToAddresses = new List<string> { "ofosupapa@gmail.com" },
                        Subject = $"Action Required: Vehicle Assignment Request for {request.User.UserName}",
                        Body = $"You are required to take action on vehicle assignment request by  {request.User.UserName} at the '{stageName}' stage."
                    };
                    await SendEmailAsync(email);
                }
            }
        }
        [HttpPost("vehicle-requests/{id}/process-stage")]
        public async Task<IActionResult> ProcessVehicleRequestStage(string id, [FromBody] ProcessStageDto dto, [FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var request = await _context.VehicleAssignmentRequests
                .Include(r => r.CurrentRoute)
                    .ThenInclude(r => r.UserRoles)
                        .ThenInclude(ur => ur.User)
                .Include(r => r.Transactions)
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound("Vehicle assignment request not found");

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
                        Comments = "",
                        Timestamp = DateTime.UtcNow,
                        IsCompleted = true
                    };
                    _context.VehicleAssignmentTransactions.Add(skipTransaction);
                    await _context.SaveChangesAsync();
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

                        var vehicle = await _context.Vehicles.FindAsync(request.VehicleId);
                        if (vehicle == null)
                            return NotFound("Vehicle not found");

                        var user = await _userManager.FindByIdAsync(request.UserId);
                        if (user == null)
                            return NotFound("User not found");

                        if (!string.IsNullOrEmpty(vehicle.UserId))
                        {
                            var previousAssignment = await _context.VehicleAssignmentHistories
                                .FirstOrDefaultAsync(h => h.VehicleId == vehicle.Id && h.UnassignmentDate == null);

                            if (previousAssignment != null)
                                previousAssignment.UnassignmentDate = DateTime.UtcNow;
                        }

                        vehicle.UserId = user.Id;

                        _context.VehicleAssignmentHistories.Add(new VehicleAssignmentHistory
                        {
                            VehicleId = vehicle.Id,
                            UserId = user.Id,
                            AssignmentDate = DateTime.UtcNow
                        });

                        await _context.SaveChangesAsync();

                        var emailMessage = new EmailMessage
                        {
                            ToAddresses = new List<string> { "ofosupapa@gmail.com" },
                            Subject = "Vehicle Request Approved",
                            Body = $"Your vehicle request for {vehicle.LicensePlate} has been approved.",
                            firstName = user.UserName
                        };

                        await SendEmailAsync(emailMessage);
                        await _notificationService.CreateNotificationAsync(
                            user.Id,
                            "Vehicle Request Approved",
                            $"Your vehicle request for {vehicle.LicensePlate} has been approved.",
                            NotificationType.System,
                            request.Id,
                            null
                        );
                        await _hubContext.Clients.Group(user.Id).SendAsync("NewNotification");

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

                    var nextStageApprover = request.CurrentRoute.UserRoles
                        .Where(ur => ur.Role.Equals(nextStage, StringComparison.OrdinalIgnoreCase))
                        .Select(ur => new { Email = ur.User.Email, Name = ur.User.UserName ,Id = ur.Id })
                        .FirstOrDefault();

                    if (nextStageApprover != null)
                    {
                        var emailMessage = new EmailMessage
                        {
                            ToAddresses = new List<string> { "ofosupapa@gmail.com" },
                            Subject = $"Action Required: Vehicle Request in {nextStage} Stage",
                            Body = $"The vehicle request for {request.Vehicle.LicensePlate} has moved to the {nextStage} stage and requires your approval.",
                            firstName = nextStageApprover.Name
                        };

                        await SendEmailAsync(emailMessage);

                        await _notificationService.CreateNotificationAsync(
                        nextStageApprover.Id,
                        "Requires Approval",
                        $" A vehicle request for {request.Vehicle.LicensePlate} requires your approval.",
                        NotificationType.Vehicle,
                        request.Id,
                        null
                    );

                        await _hubContext.Clients.Group(nextStageApprover.Id).SendAsync("NewNotification");
                    }
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
                        return NotFound("Vehicle not found");

                    var user = await _userManager.FindByIdAsync(request.UserId);
                    if (user == null)
                        return NotFound("User not found");

                    if (!string.IsNullOrEmpty(vehicle.UserId))
                    {
                        var previousAssignment = await _context.VehicleAssignmentHistories
                            .FirstOrDefaultAsync(h => h.VehicleId == vehicle.Id && h.UnassignmentDate == null);

                        if (previousAssignment != null)
                            previousAssignment.UnassignmentDate = DateTime.UtcNow;
                    }

                    vehicle.UserId = user.Id;

                    _context.VehicleAssignmentHistories.Add(new VehicleAssignmentHistory
                    {
                        VehicleId = vehicle.Id,
                        UserId = user.Id,
                        AssignmentDate = DateTime.UtcNow
                    });

                    await _context.SaveChangesAsync();

                    var emailMessage = new EmailMessage
                    {
                        ToAddresses = new List<string> { "ofosupapa@gmail.com" },
                        Subject = "Vehicle Request Approved",
                        Body = $"Your vehicle request for {vehicle.LicensePlate} has been approved.",
                        firstName = user.UserName
                    };

                    await SendEmailAsync(emailMessage);
                    await _notificationService.CreateNotificationAsync(
                        user.Id,
                        "Vehicle Request Approved",
                        $"Your vehicle request for {vehicle.LicensePlate} has been approved.",
                        NotificationType.System,
                        request.Id,
                        null
                    );
                    await _hubContext.Clients.Group(user.Id).SendAsync("NewNotification");

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

                var nextStageApprover = request.CurrentRoute.UserRoles
                    .Where(ur => ur.Role.Equals(nextStage, StringComparison.OrdinalIgnoreCase))
                    .Select(ur => new { Email = ur.User.Email, Name = ur.User.UserName ,Id = ur.User.Id})
                    .FirstOrDefault();

                if (nextStageApprover != null)
                {
                    var emailMessage = new EmailMessage
                    {
                        ToAddresses = new List<string> { "ofosupapa@gmail.com" },
                        Subject = $"Action Required: Vehicle Request in {nextStage} Stage",
                        Body = $"The vehicle request for {request.Vehicle.LicensePlate} has moved to the {nextStage} stage and requires your approval.",
                        firstName = nextStageApprover.Name
                    };

                    await SendEmailAsync(emailMessage);

                     await _notificationService.CreateNotificationAsync(
                        nextStageApprover.Id,
                        "Action Required",
                        $"The vehicle request for {request.Vehicle.LicensePlate} has moved to the {nextStage} stage and requires your approval.",
                        NotificationType.System,
                        request.Id,
                        null
                    );
                    await _hubContext.Clients.Group(nextStageApprover.Id).SendAsync("NewNotification");

                   await _notificationService.CreateNotificationAsync(
                       request.UserId,
                       "Request Update ",
                       $"The vehicle request you made has moved to the {nextStage} stage ",
                       NotificationType.System,
                       request.Id,
                       null
                    );
                    await _hubContext.Clients.Group(request.UserId).SendAsync("NewNotification");
                }
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


        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectVehicleRequest(string id, [FromQuery] string userId, [FromBody] string rejectionReason)
        {
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var request = await _context.VehicleAssignmentRequests
                .Include(r => r.CurrentRoute)
                    .ThenInclude(r => r.UserRoles)
                .Include(r => r.Transactions)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound("Vehicle assignment request not found");

            if (request.Status == VehicleRequestStatus.Approved || request.Status == VehicleRequestStatus.Rejected)
                return BadRequest("Request is already finalized");

            var userRole = request.CurrentRoute.UserRoles
                .FirstOrDefault(ur => ur.UserId == userId && ur.Role.Equals(request.CurrentStage, StringComparison.OrdinalIgnoreCase));

            if (userRole == null)
                return Forbid("You don't have permission for this action");

            var existingTransaction = request.Transactions
                .FirstOrDefault(t => t.UserId == userId && t.Stage == request.CurrentStage);

            if (existingTransaction != null)
                return Conflict("You've already processed this stage");

            var transaction = new VehicleAssignmentTransaction
            {
                VehicleAssignmentRequestId = request.Id,
                UserId = userId,
                Stage = request.CurrentStage,
                Comments = $"",
                Timestamp = DateTime.UtcNow,
                IsCompleted = true
            };

            _context.VehicleAssignmentTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            var requiredUsers = request.CurrentRoute.UserRoles
                .Where(ur => ur.Role.Equals(request.CurrentStage, StringComparison.OrdinalIgnoreCase))
                .Select(ur => ur.UserId).Distinct().ToList();

            var completedUsers = request.Transactions
                .Where(t => t.Stage == request.CurrentStage && t.IsCompleted)
                .Select(t => t.UserId).Distinct().ToList();

            if (completedUsers.Count >= requiredUsers.Count)
            {
                request.Status = VehicleRequestStatus.Rejected;
                request.CurrentStage = "Complete";
                await _context.SaveChangesAsync();

                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user != null)
                {
                    var emailMessage = new EmailMessage
                    {
                        firstName = user.UserName,
                        ToAddresses = new List<string> { "ofosupapa@gmail.com" },
                        Subject = "Vehicle Request Rejected",
                        Body = $"Your vehicle request has been rejected. Reason: {rejectionReason}.Apologies for any inconvenience."
                    };
                    await SendEmailAsync(emailMessage);
                    await _notificationService.CreateNotificationAsync(
                        user.Id,
                        "Vehicle Request Rejected",
                        $"Your vehicle request has been rejected. Reason: {rejectionReason}.Apologies for any inconvenience.",
                        NotificationType.System,
                        request.Id,
                        null
                    );
                    await _hubContext.Clients.Group(user.Id).SendAsync("NewNotification");
                }

                return Ok(new
                {
                    Message = "Request rejected",
                    RequestId = request.Id
                });
            }

            return Ok(new
            {
                Message = "Rejection recorded",
                RequestId = request.Id,
                CurrentStage = request.CurrentStage,
                Status = request.Status.ToString()
            });
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignVehicleDirectly([FromBody] DirectAssignmentDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var vehicle = await _context.Vehicles.FindAsync(dto.VehicleId);
            if (vehicle == null)
            {
                return NotFound("Vehicle not found");
            }

            if (!string.IsNullOrEmpty(vehicle.UserId))
            {
                return BadRequest("Vehicle is already assigned to another user");
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

            var emailMessage = new EmailMessage
            {
                firstName = user.UserName,
                ToAddresses = new List<string> { "ofosupapa@gmail.com"},
                Subject = "Vehicle Assigned",
                Body = $"You have been assigned the vehicle with license plate {vehicle.LicensePlate} report to front desk to receive the keys."
            };
            await SendEmailAsync(emailMessage);
            await _notificationService.CreateNotificationAsync(
                user.Id,
                "Vehicle Assigned",
                $"You have been assigned the vehicle with license plate {vehicle.LicensePlate} report to front desk to receive the keys.",
                NotificationType.System,
                vehicle.Id,
                null
            );
            await _hubContext.Clients.Group(user.Id).SendAsync("NewNotification");

            return Ok(new
            {
                Message = "Vehicle assigned successfully",
                VehicleId = vehicle.Id,
                UserId = user.Id,
                UserName = user.UserName,
                VehicleLicensePlate = vehicle.LicensePlate
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

            var user = await _userManager.FindByIdAsync(vehicle.UserId);
            vehicle.UserId = null;
            vehicle.User = null;

            try
            {
                await _context.SaveChangesAsync();

                if (user != null)
                {
                    var emailMessage = new EmailMessage
                    {
                        firstName = user.UserName,
                        ToAddresses = new List<string> { "ofosupapa@gmail.com" },
                        Subject = "Vehicle Unassigned",
                        Body = $"The vehicle with license plate {vehicle.LicensePlate} has been unassigned from you. Please return it to the office premises within 3 days and submit the keys to the front desk."
                    };

                    await SendEmailAsync(emailMessage);
                    await _notificationService.CreateNotificationAsync(
                        user.Id,
                        "Vehicle Unassigned",
                        $"The vehicle with license plate {vehicle.LicensePlate} has been unassigned from you. Please return it to the office premises within 3 days and submit the keys to the front desk.",
                        NotificationType.System,
                        vehicle.Id,
                        null
                    );
                    await _hubContext.Clients.Group(user.Id).SendAsync("NewNotification");
                }

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
                .Include(r => r.CurrentRoute)
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

        [HttpGet("RequestsBeforeApproval")]
        public async Task<IActionResult> GetRequestsBeforeApproval()
        {
            var requests = await _context.VehicleAssignmentRequests
                .Include(r => r.User)
                .Include(r => r.Vehicle)
                .Include(r => r.CurrentRoute)
                .Where(r => r.Status != VehicleRequestStatus.Approved && r.Status != VehicleRequestStatus.Rejected)
                .OrderByDescending(r => r.RequestDate)
                .Select(r => new
                {
                    r.Id,
                    r.UserId,
                    UserName = r.User.UserName,
                    Email = r.User.Email,
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
                })
                .ToListAsync();
            return Ok(requests);
        }

        [HttpGet("ApprovedRequests")]
        public async Task<IActionResult> GetApprovedRequests()
        {
            var requests = await _context.VehicleAssignmentRequests
                .Include(r => r.User)
                .Include(r => r.Vehicle)
                .Include(r => r.CurrentRoute)
                .Where(r => r.Status == VehicleRequestStatus.Approved || r.Status == VehicleRequestStatus.Rejected)
                .OrderByDescending(r => r.RequestDate)
                .Select(r => new
                {
                    r.Id,
                    r.UserId,
                    UserName = r.User.UserName,
                    Email = r.User.Email,
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
                })
                .ToListAsync();
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
                .Where(r => r.UserId == userId )
                .Include(r => r.Vehicle)
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

        [HttpGet("RecentAssignments")]
        public async Task<IActionResult> GetRecentAssignments()
        {
            var recentAssignments = await _context.VehicleAssignmentHistories
                .Include(va => va.Vehicle)
                .Include(va => va.User)
                .OrderByDescending(va => va.AssignmentDate)
                .Take(2)
                .Select(va => new VehicleAssignmentTrackingDto
                {
                    VehicleId = va.Vehicle.Id,
                    VehicleMake = va.Vehicle.Make,
                    VehicleModel = va.Vehicle.Model,
                    LicensePlate = va.Vehicle.LicensePlate,
                    UserId = va.User.Id,
                    UserName = va.User.UserName,
                    UserEmail = va.User.Email,
                    AssignmentDate = va.AssignmentDate
                })
                .ToListAsync();
            return Ok(recentAssignments);
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
                    .Where(t => t.IsCompleted && t.Stage != "Create")
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
                    .Where(ur => ur.Role == request.CurrentStage && ur.Role != "Create")
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
                .Where(t => t.VehicleAssignmentRequestId == id &&
                             !string.IsNullOrEmpty(t.Comments) &&
                             t.Stage != "Create")
                .Select(t => new
                {
                    User = t.User.UserName,
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

        [HttpGet("latest-comments")]
        public async Task<IActionResult> GetMostRecentCommentsForAllRequests()
        {
            var data = await (
                from t in _context.VehicleAssignmentTransactions
                join r in _context.VehicleAssignmentRequests on t.VehicleAssignmentRequestId equals r.Id
                join owner in _context.Users on r.UserId equals owner.Id into ownerJoin
                from ownerUser in ownerJoin.DefaultIfEmpty()
                join commenter in _context.Users on t.UserId equals commenter.Id into commenterJoin
                from commenterUser in commenterJoin.DefaultIfEmpty()
                where t.Stage != "create" && !string.IsNullOrEmpty(t.Comments)
                orderby t.Timestamp descending
                select new
                {
                    RequestId = r.Id,
                    RequestOwnerName = ownerUser.UserName ?? "Unknown Owner",
                    Stage = t.Stage,
                    Comment = t.Comments,
                    CommenterUserName = commenterUser.UserName ?? "Unknown User",
                    Timestamp = t.Timestamp
                }
            ).ToListAsync();
            var latestComments = data
                .GroupBy(d => new { d.RequestId, d.Stage })
                .Select(g => g.First())
                .OrderBy(d => d.RequestId)
                .ThenBy(d => d.Stage)
                .ToList();
            return Ok(new
            {
                LatestComments = latestComments
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
    }

    public class DirectAssignmentDto
    {
        public string UserId { get; set; }
        public string VehicleId { get; set; }
    }
}

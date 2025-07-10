using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonaXFleet.Data;
using PersonaXFleet.DTOs;
using PersonaXFleet.Models;
using PersonaXFleet.Models.Enums;
using PersonaXFleet.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;

namespace PersonaXFleet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceRequestController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _env;

        public MaintenanceRequestController(AuthDbContext context, UserManager<ApplicationUser> userManager, IEmailService emailService, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _env = env;
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
        { "firstName", message.firstName }, 
        { "Subject", message.Subject },
        { "BodyContent", message.Body },
        { "Year", DateTime.Now.Year.ToString() }
    };

            string template = LoadTemplate("Templates/EmailTemplate.html", placeholders);
            message.Body = template;

            await _emailService.SendEmailAsync(message);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMaintenanceRequests()
        {
            var requests = await _context.MaintenanceRequests
                .Include(r => r.Vehicle)
                .Include(r => r.RequestedByUser)
                .Include(r => r.CurrentRoute)
                .Include(r => r.Transactions)
                .OrderByDescending(r => r.RequestDate)
                .Select(r => new MaintenanceRequestDto
                {
                    Id = r.MaintenanceId,
                    VehicleId = r.VehicleId,
                    VehicleMake = r.Vehicle.Make,
                    VehicleModel = r.Vehicle.Model,
                    LicensePlate = r.Vehicle.LicensePlate,
                    RequestType = r.RequestType.ToString(),
                    Description = r.Description,
                    RequestDate = r.RequestDate,
                    Status = r.Status.ToString(),
                    Priority = r.Priority.ToString(),
                    EstimatedCost = r.EstimatedCost,
                    AdminComments = r.AdminComments,
                    RequestedByUserId = r.RequestedByUserId,
                    RequestedByUserName = r.RequestedByUser.UserName,
                    Department = r.Department,
                    CurrentStage = r.CurrentStage,
                    RouteName = r.CurrentRoute.Name
                })
                .ToListAsync();
            return Ok(requests);
        }

        [HttpGet("approved-rejected")]
        public async Task<IActionResult> GetApprovedAndRejectedRequests()
        {
            var requests = await _context.MaintenanceRequests
                .Where(r => r.Status == MaintenanceRequestStatus.Approved || r.Status == MaintenanceRequestStatus.Rejected)
                .Include(r => r.Vehicle)
                .Include(r => r.RequestedByUser)
                .Include(r => r.CurrentRoute)
                .Include(r => r.Transactions)
                .OrderByDescending(r => r.RequestDate)
                .Select(r => new MaintenanceRequestDto
                {
                    Id = r.MaintenanceId,
                    VehicleId = r.VehicleId,
                    VehicleMake = r.Vehicle.Make,
                    VehicleModel = r.Vehicle.Model,
                    LicensePlate = r.Vehicle.LicensePlate,
                    RequestType = r.RequestType.ToString(),
                    Description = r.Description,
                    RequestDate = r.RequestDate,
                    Status = r.Status.ToString(),
                    Priority = r.Priority.ToString(),
                    EstimatedCost = r.EstimatedCost,
                    AdminComments = r.AdminComments,
                    RequestedByUserId = r.RequestedByUserId,
                    RequestedByUserName = r.RequestedByUser.UserName,
                    Department = r.Department,
                    CurrentStage = r.CurrentStage,
                    RouteName = r.CurrentRoute.Name
                })
                .ToListAsync();
            return Ok(requests);
        }

        [HttpGet("active-requests")]
        public async Task<IActionResult> GetActiveRequests()
        {
            var requests = await _context.MaintenanceRequests
                .Where(r => r.Status != MaintenanceRequestStatus.Approved && r.Status != MaintenanceRequestStatus.Rejected)
                .Include(r => r.Vehicle)
                .Include(r => r.RequestedByUser)
                .Include(r => r.CurrentRoute)
                .Include(r => r.Transactions)
                .OrderByDescending(r => r.RequestDate)
                .Select(r => new MaintenanceRequestDto
                {
                    Id = r.MaintenanceId,
                    VehicleId = r.VehicleId,
                    VehicleMake = r.Vehicle.Make,
                    VehicleModel = r.Vehicle.Model,
                    LicensePlate = r.Vehicle.LicensePlate,
                    RequestType = r.RequestType.ToString(),
                    Description = r.Description,
                    RequestDate = r.RequestDate,
                    Status = r.Status.ToString(),
                    Priority = r.Priority.ToString(),
                    EstimatedCost = r.EstimatedCost,
                    AdminComments = r.AdminComments,
                    RequestedByUserId = r.RequestedByUserId,
                    RequestedByUserName = r.RequestedByUser.UserName,
                    Department = r.Department,
                    CurrentStage = r.CurrentStage,
                    RouteName = r.CurrentRoute.Name
                })
                .ToListAsync();
            return Ok(requests);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMaintenanceRequest(string id)
        {
            var request = await _context.MaintenanceRequests
                .Include(r => r.Vehicle)
                .Include(r => r.RequestedByUser)
                .Include(r => r.CurrentRoute)
                .ThenInclude(r => r.UserRoles)
                .ThenInclude(ur => ur.User)
                .Include(r => r.Transactions)
                .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(r => r.MaintenanceId == id);
            if (request == null)
            {
                return NotFound();
            }
            var requestDto = new MaintenanceRequestDetailDto
            {
                Id = request.MaintenanceId,
                VehicleId = request.VehicleId,
                VehicleMake = request.Vehicle.Make,
                VehicleModel = request.Vehicle.Model,
                LicensePlate = request.Vehicle.LicensePlate,
                RequestType = request.RequestType.ToString(),
                Description = request.Description,
                RequestDate = request.RequestDate,
                Status = request.Status.ToString(),
                Priority = request.Priority.ToString(),
                EstimatedCost = request.EstimatedCost,
                AdminComments = request.AdminComments,
                RequestedByUserId = request.RequestedByUserId,
                RequestedByUserName = request.RequestedByUser.UserName,
                Department = request.Department,
                CurrentStage = request.CurrentStage,
                RouteName = request.CurrentRoute.Name,
                RouteUsers = request.CurrentRoute.UserRoles.Select(ur => new UserRoleDto
                {
                    UserId = ur.UserId,
                    Role = ur.Role
                }).ToList(),
                Transactions = request.Transactions.Select(t => new TransactionDto
                {
                    Id = t.Id,
                    UserName = t.User.UserName,
                    Action = t.Action,
                    Comments = t.Comments,
                    Timestamp = t.Timestamp,
                    IsCompleted = t.IsCompleted
                }).OrderBy(t => t.Timestamp).ToList()
            };
            return Ok(requestDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMaintenanceRequest([FromBody] CreateMaintenanceRequestDto requestDto, string userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");
            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.UserId == userId);
            if (vehicle == null)
            {
                return BadRequest("No vehicle found assigned to the specified user");
            }
            var route = await _context.Routes
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.Department == user.Department);
            if (route == null)
            {
                return BadRequest("No workflow route defined for this department");
            }
            var request = new MaintenanceRequest
            {
                VehicleId = vehicle.Id,
                RequestedByUserId = userId,
                Department = user.Department,
                RequestType = requestDto.RequestType,
                Description = requestDto.Description,
                RequestDate = DateTime.UtcNow,
                Status = MaintenanceRequestStatus.Pending,
                Priority = requestDto.Priority,
                EstimatedCost = requestDto.EstimatedCost,
                AdminComments = requestDto.AdminComments ?? string.Empty,
                CurrentRouteId = route.Id,
                CurrentStage = "Comment"
            };
            _context.MaintenanceRequests.Add(request);
            await _context.SaveChangesAsync();
            var creationTransaction = new MaintenanceTransaction
            {
                MaintenanceRequestId = request.MaintenanceId,
                UserId = userId,
                Action = "Create",
                Comments = "Request created",
                Timestamp = DateTime.UtcNow,
                IsCompleted = true
            };
            _context.MaintenanceTransactions.Add(creationTransaction);
            await _context.SaveChangesAsync();
            await ProcessRequestorSkipsAsync(request, route, userId);
            var emailMessage = new EmailMessage
            {
                firstName = user.UserName,
                ToAddresses = new List<string> { "ofosupapa@gmail.com" },
                Subject = "Maintenance Request Created",
                Body = $"Your maintenance request for a {request.RequestType} for {vehicle.LicensePlate} has been created and is pending approval."
            };
            await SendEmailAsync(emailMessage);
            return Ok(new
            {
                Message = "Maintenance request submitted successfully",
                RequestId = request.MaintenanceId,
                RouteId = route.Id,
                CurrentStage = request.CurrentStage
            });
        }

        [HttpPost("personal")]
        public async Task<IActionResult> CreatePersonalMaintenanceRequest([FromBody] CreatePersonalMaintenanceRequestDto requestDto, [FromQuery] string userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.UserId == userId);
            if (vehicle == null)
                return BadRequest("No vehicle assigned to this user");

            var route = await _context.Routes
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.Department == user.Department);
            if (route == null)
            {
                route = await _context.Routes
                    .Include(r => r.UserRoles)
                    .FirstOrDefaultAsync(r => r.Department == "Default");
                if (route == null)
                    return BadRequest("No default workflow route defined");
            }

            var request = new MaintenanceRequest
            {
                VehicleId = vehicle.Id,
                RequestedByUserId = userId,
                Department = user.Department,
                RequestType = requestDto.RequestType,
                Description = requestDto.Description,
                RequestDate = DateTime.UtcNow,
                Status = MaintenanceRequestStatus.Pending,
                Priority = requestDto.Priority,
                CurrentRouteId = route.Id,
                CurrentStage = "Comment" // Initial stage
            };

            _context.MaintenanceRequests.Add(request);
            await _context.SaveChangesAsync();

            var creationTransaction = new MaintenanceTransaction
            {
                MaintenanceRequestId = request.MaintenanceId,
                UserId = userId,
                Action = "Create",
                Comments = "Personal request created",
                Timestamp = DateTime.UtcNow,
                IsCompleted = true
            };

            _context.MaintenanceTransactions.Add(creationTransaction);
            await _context.SaveChangesAsync();

            await ProcessRequestorSkipsAsync(request, route, userId);

            // Notify the requester
            var requesterEmail = new EmailMessage
            {
                firstName= user.UserName,
                ToAddresses = new List<string> { "ofosupapa@gmail.com" },
                Subject = "Maintenance Request Created",
                Body = $"Your maintenance request for a {request.RequestType} for {vehicle.LicensePlate} has been created and is pending approval."
            };
            await SendEmailAsync(requesterEmail);


            await NotifyUsersForStageAsync(request, request.CurrentStage);

            return Ok(new
            {
                Message = "Personal maintenance request submitted successfully",
                RequestId = request.MaintenanceId,
                RouteId = route.Id,
                CurrentStage = request.CurrentStage,
                Vehicle = new
                {
                    vehicle.Id,
                    vehicle.Make,
                    vehicle.Model,
                    vehicle.LicensePlate
                }
            });
        }

        private async Task NotifyUsersForStageAsync(MaintenanceRequest request, string stageName)
        {
            // Get the current route with user roles
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
                        Subject = $"Action Required: Maintenance Request for {request.RequestedByUser.UserName}",
                        Body = $"You are required to take action on maintenance request by {request.RequestedByUser.UserName} at the '{stageName}' stage."
                    };
                    await SendEmailAsync(email);
                }
            }
        }

        private async Task ProcessRequestorSkipsAsync(MaintenanceRequest request, Router route, string userId)
        {
            bool advanced;

            do
            {
                advanced = false;

                var currentStageUsers = route.UserRoles
                    .Where(ur => ur.Role.Equals(request.CurrentStage, StringComparison.OrdinalIgnoreCase))
                    .Select(ur => ur.UserId)
                    .Distinct()
                    .ToList();

                if (currentStageUsers.Contains(userId))
                {
                    var existingTransaction = await _context.MaintenanceTransactions
                        .FirstOrDefaultAsync(t =>
                            t.MaintenanceRequestId == request.MaintenanceId &&
                            t.UserId == userId &&
                            t.Action == request.CurrentStage);

                    if (existingTransaction == null)
                    {
                        var skipTransaction = new MaintenanceTransaction
                        {
                            MaintenanceRequestId = request.MaintenanceId,
                            UserId = userId,
                            Action = request.CurrentStage,
                            Comments = "(Skipped as Requestor)",
                            Timestamp = DateTime.UtcNow,
                            IsCompleted = true
                        };
                        _context.MaintenanceTransactions.Add(skipTransaction);
                        await _context.SaveChangesAsync();
                    }
                }

                await _context.Entry(request).Collection(r => r.Transactions).LoadAsync();

                var completedUsers = request.Transactions
                    .Where(t => t.Action == request.CurrentStage && t.IsCompleted)
                    .Select(t => t.UserId)
                    .Distinct()
                    .ToList();

                if (completedUsers.Count >= currentStageUsers.Count)
                {
                    var nextStage = GetNextStage(request.CurrentStage);
                    request.CurrentStage = nextStage;

                    if (nextStage == "Complete")
                    {
                        request.Status = MaintenanceRequestStatus.Approved;
                    }

                    await _context.SaveChangesAsync();
                    advanced = true;
                }

            } while (advanced);
        }
        [HttpPost("{id}/process-stage")]
        public async Task<IActionResult> ProcessRequestStage(string id, [FromBody] ProcessStageDto dto, [FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var request = await _context.MaintenanceRequests
                .Include(r => r.CurrentRoute)
                    .ThenInclude(r => r.UserRoles)
                .Include(r => r.Transactions)
                .Include(r => r.RequestedByUser) 
                .FirstOrDefaultAsync(r => r.MaintenanceId == id);

            if (request == null)
            {
                return NotFound();
            }

            if (request.RequestedByUserId == userId)
            {
                await ProcessRequestorSkipsAsync(request, request.CurrentRoute, userId);

                var nextStageUsers = request.CurrentRoute.UserRoles
                    .Where(ur => ur.Role.Equals(request.CurrentStage, StringComparison.OrdinalIgnoreCase))
                    .Select(ur => ur.UserId)
                    .Distinct()
                    .ToList();

                foreach (var nextUserId in nextStageUsers)
                {
                    var nextUser = await _userManager.FindByIdAsync(nextUserId);
                    if (nextUser != null && !string.IsNullOrEmpty(nextUser.Email))
                    {
                        var emailMessage = new EmailMessage
                        {
                            firstName = nextUser.UserName,
                            ToAddresses = new List<string> { "ofosupapa@gmail.com" },
                            Subject = $"Action Required: Maintenance Request ",
                            Body = $"A maintenance request is awaiting your action at the '{request.CurrentStage}' stage."
                        };
                        await SendEmailAsync(emailMessage);
                    }
                }

                return Ok(new
                {
                    Message = "",
                    RequestId = request.MaintenanceId,
                    CurrentStage = request.CurrentStage,
                    Status = request.Status.ToString()
                });
            }

            var userRole = request.CurrentRoute.UserRoles
                .FirstOrDefault(ur => ur.UserId == userId && ur.Role.Equals(request.CurrentStage, StringComparison.OrdinalIgnoreCase));

            if (userRole == null)
            {
                return Forbid("You are not authorized for this stage.");
            }

            var existingTransaction = request.Transactions
                .FirstOrDefault(t => t.UserId == userId && t.Action == request.CurrentStage);

            if (existingTransaction != null)
            {
                return Conflict("You've already processed this stage.");
            }

            var transaction = new MaintenanceTransaction
            {
                MaintenanceRequestId = request.MaintenanceId,
                UserId = userId,
                Action = request.CurrentStage,
                Comments = dto.Comments,
                Timestamp = DateTime.UtcNow,
                IsCompleted = true
            };

            _context.MaintenanceTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            await _context.Entry(request).Collection(r => r.Transactions).LoadAsync();

            var requiredUsers = request.CurrentRoute.UserRoles
                .Where(ur => ur.Role.Equals(request.CurrentStage, StringComparison.OrdinalIgnoreCase))
                .Select(ur => ur.UserId)
                .Distinct()
                .ToList();

            var completedUsers = request.Transactions
                .Where(t => t.Action == request.CurrentStage && t.IsCompleted)
                .Select(t => t.UserId)
                .Distinct()
                .ToList();

            if (completedUsers.Count >= requiredUsers.Count)
            {
                var nextStage = GetNextStage(request.CurrentStage);
                request.CurrentStage = nextStage;
                if (nextStage == "Complete")
                {
                    request.Status = MaintenanceRequestStatus.Approved;

                    var emailMessage1 = new EmailMessage
                    {
                        firstName = request.RequestedByUser?.UserName ?? "User",
                        ToAddresses = new List<string> { "ofosupapa@gmail.com" },
                        Subject = "Maintenance Request Approved",
                        Body = $"Your maintenance request has been approved and marked as complete."
                    };

                    var emailMessage2 = new EmailMessage
                    {
                        firstName = "Papa-Kwame",
                        ToAddresses = new List<string> { "ofosupapa@gmail.com" }, 
                        Subject = "Notification: Maintenance Request Completed",
                        Body = $"The maintenance request by {request.RequestedByUser?.UserName ?? "a user"} has been approved and completed and a schedule request has been created ."
                    };

                    await SendEmailAsync(emailMessage1);
                    await SendEmailAsync(emailMessage2);
                }


                if (dto.EstimatedCost.HasValue && nextStage == "Commit")
                {
                    request.EstimatedCost = dto.EstimatedCost.Value;
                }

                await _context.SaveChangesAsync();

                if (nextStage != "Complete")
                {
                    var nextStageUsers = request.CurrentRoute.UserRoles
                        .Where(ur => ur.Role.Equals(nextStage, StringComparison.OrdinalIgnoreCase))
                        .Select(ur => ur.UserId)
                        .Distinct()
                        .ToList();

                    foreach (var nextUserId in nextStageUsers)
                    {
                        var nextUser = await _userManager.FindByIdAsync(nextUserId);
                        if (nextUser != null && !string.IsNullOrEmpty(nextUser.Email))
                        {
                            var emailMessage = new EmailMessage
                            {
                                firstName = nextUser.UserName,
                                ToAddresses = new List<string> { "ofosupapa@gmail.com" },
                                Subject = $"Action Required: Maintenance Request ",
                                Body = $"A maintenance request is awaiting your action at the '{nextStage}' stage."
                            };
                            await SendEmailAsync(emailMessage);
                        }
                    }
                }
            }

            var stageComments = await _context.MaintenanceTransactions
                .Where(t => t.MaintenanceRequestId == request.MaintenanceId && !string.IsNullOrEmpty(t.Comments))
                .GroupBy(t => t.Action)
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
                RequestId = request.MaintenanceId,
                CurrentStage = request.CurrentStage,
                Status = request.Status.ToString(),
                StageComments = stageComments
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

        [HttpGet("{id}/workflow-status")]
        public async Task<IActionResult> GetWorkflowStatus(string id)
        {
            var request = await _context.MaintenanceRequests
                .Include(r => r.CurrentRoute)
                .ThenInclude(r => r.UserRoles)
                .ThenInclude(ur => ur.User)
                .Include(r => r.Transactions)
                .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(r => r.MaintenanceId == id);
            if (request == null)
            {
                return NotFound();
            }
            var status = new
            {
                CurrentStage = request.CurrentStage,
                CompletedActions = request.Transactions
                    .Where(t => t.IsCompleted)
                    .GroupBy(t => t.Action)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(t => new
                        {
                            UserName = t.User.UserName,
                            t.Comments,
                            t.Timestamp
                        }).ToList()
                    ),
                PendingActions = request.CurrentRoute.UserRoles
                    .Where(ur => ur.Role == request.CurrentStage)
                    .Select(ur => new
                    {
                        Role = ur.Role,
                        UserName = ur.User != null ? ur.User.UserName : "(unknown user)",
                        UserId = ur.UserId,
                        IsPending = !request.Transactions.Any(t =>
                            t.Action == ur.Role &&
                            t.UserId == ur.UserId &&
                            t.IsCompleted)
                    })
                    .ToList()
            };
            return Ok(status);
        }

        [HttpGet("{id}/comments")]
        public async Task<IActionResult> GetAllRequestComments(string id)
        {
            var request = await _context.MaintenanceRequests
                .FirstOrDefaultAsync(r => r.MaintenanceId == id);
            if (request == null)
                return NotFound("Maintenance request not found");
            var comments = await _context.MaintenanceTransactions
                .Where(t => t.MaintenanceRequestId == id &&
                             !string.IsNullOrEmpty(t.Comments) &&
                             t.Action != "Create")
                .Include(t => t.User)
                .Select(t => new
                {
                    Stage = t.Action,
                    Comment = t.Comments,
                    UserName = t.User != null ? t.User.UserName : "Unknown",
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
        public async Task<IActionResult> GetMostRecentMaintenanceComments()
        {
            var data = await (
                from t in _context.MaintenanceTransactions
                join r in _context.MaintenanceRequests on t.MaintenanceRequestId equals r.MaintenanceId
                join owner in _context.Users on r.RequestedByUserId equals owner.Id into ownerJoin
                from ownerUser in ownerJoin.DefaultIfEmpty()
                join commenter in _context.Users on t.UserId equals commenter.Id into commenterJoin
                from commenterUser in commenterJoin.DefaultIfEmpty()
                where t.Action != "create" && !string.IsNullOrEmpty(t.Comments)
                orderby t.Timestamp descending
                select new
                {
                    RequestId = r.MaintenanceId,
                    RequestOwnerName = ownerUser.UserName ?? "Unknown Owner",
                    Stage = t.Action,
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
        public async Task<IActionResult> GetMyPendingActions(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var requests = await (from ur in _context.UserRouteRoles
                                  where ur.UserId == userId
                                  join r in _context.MaintenanceRequests.Include(r => r.Vehicle)
                                      on ur.RouteId equals r.CurrentRouteId
                                  where ur.Role == r.CurrentStage
                                  where !_context.MaintenanceTransactions.Any(t =>
                                      t.MaintenanceRequestId == r.MaintenanceId &&
                                      t.Action == r.CurrentStage &&
                                      t.UserId == ur.UserId &&
                                      t.IsCompleted)
                                  select new MaintenanceRequestDto
                                  {
                                      Id = r.MaintenanceId,
                                      VehicleId = r.VehicleId,
                                      VehicleMake = r.Vehicle.Make,
                                      VehicleModel = r.Vehicle.Model,
                                      LicensePlate = r.Vehicle.LicensePlate,
                                      RequestType = r.RequestType.ToString(),
                                      Description = r.Description,
                                      RequestDate = r.RequestDate,
                                      Status = r.Status.ToString(),
                                      Priority = r.Priority.ToString(),
                                      EstimatedCost = r.EstimatedCost,
                                      AdminComments = r.AdminComments,
                                      RequestedByUserId = r.RequestedByUserId,
                                      RequestedByUserName = r.RequestedByUser.UserName,
                                      Department = r.Department,
                                      CurrentStage = r.CurrentStage,
                                      RouteName = r.CurrentRoute.Name
                                  })
                                .ToListAsync();
            return Ok(requests);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetMaintenanceHistory()
        {
            var history = await _context.MaintenanceHistories
                .Include(h => h.Vehicle)
                .Include(h => h.RequestedByUser)
                .OrderByDescending(h => h.ApprovedDate)
                .Select(h => new MaintenanceHistoryDto
                {
                    HistoryId = h.HistoryId,
                    OriginalRequestId = h.OriginalRequestId,
                    VehicleId = h.VehicleId,
                    VehicleMake = h.Vehicle.Make,
                    VehicleModel = h.Vehicle.Model,
                    LicensePlate = h.Vehicle.LicensePlate,
                    RequestType = h.RequestType.ToString(),
                    Description = h.Description,
                    RequestDate = h.RequestDate,
                    CompletionDate = h.CompletionDate,
                    Status = h.Status.ToString(),
                    Priority = h.Priority.ToString(),
                    EstimatedCost = h.EstimatedCost,
                    AdminComments = h.AdminComments,
                    RequestedByUserId = h.RequestedByUserId,
                    RequestedByUserName = h.RequestedByUser.UserName,
                    ApprovedDate = h.ApprovedDate,
                    ApprovalComments = h.ApprovalComments
                })
                .ToListAsync();
            return Ok(history);
        }

        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectMaintenanceRequest(string id, [FromQuery] string userId, [FromBody] string rejectionReason)
        {
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var request = await _context.MaintenanceRequests
                .Include(r => r.CurrentRoute)
                    .ThenInclude(r => r.UserRoles)
                .Include(r => r.Transactions)
                .FirstOrDefaultAsync(r => r.MaintenanceId == id);
            if (request == null)
                return NotFound();
            if (request.Status == MaintenanceRequestStatus.Approved || request.Status == MaintenanceRequestStatus.Rejected)
                return BadRequest("Request is already finalized.");
            bool isRequestorInCurrentStage = request.RequestedByUserId == userId;
            if (isRequestorInCurrentStage)
            {
                var existingTransaction = request.Transactions
                    .FirstOrDefault(t => t.UserId == userId && t.Action == request.CurrentStage);
                if (existingTransaction == null)
                {
                    var skipTransaction = new MaintenanceTransaction
                    {
                        MaintenanceRequestId = request.MaintenanceId,
                        UserId = userId,
                        Action = request.CurrentStage,
                        Comments = "Automatically skipped for requestor",
                        Timestamp = DateTime.UtcNow,
                        IsCompleted = true
                    };
                    _context.MaintenanceTransactions.Add(skipTransaction);
                    await _context.SaveChangesAsync();
                    await _context.Entry(request).Collection(r => r.Transactions).LoadAsync();
                }
                var requiredUsers = request.CurrentRoute.UserRoles
                    .Where(ur => ur.Role.Equals(request.CurrentStage, StringComparison.OrdinalIgnoreCase))
                    .Select(ur => ur.UserId).Distinct().ToList();
                var completedUsers = request.Transactions
                    .Where(t => t.Action == request.CurrentStage && t.IsCompleted)
                    .Select(t => t.UserId).Distinct().ToList();
                if (completedUsers.Count >= requiredUsers.Count)
                {
                    var nextStage = GetNextStage(request.CurrentStage);
                    if (nextStage == "Complete")
                    {
                        request.Status = MaintenanceRequestStatus.Rejected;
                        request.AdminComments = $"Rejected: {rejectionReason}";
                        request.CurrentStage = nextStage;
                        await _context.SaveChangesAsync();
                        var user = await _userManager.FindByIdAsync(userId);
                        if (user != null && !string.IsNullOrEmpty(user.Email))
                        {
                            var emailMessage = new EmailMessage
                            { firstName = user.UserName,
                                ToAddresses = new List<string> { "ofosupapa@gmail.com"},
                                Subject = "Maintenance Request Rejected",
                                Body = $"Your maintenance request has been rejected. Reason: {rejectionReason}"
                            };
                            await SendEmailAsync(emailMessage);
                        }
                        return Ok(new
                        {
                            Message = "Request rejected.",
                            RequestId = request.MaintenanceId
                        });
                    }
                    request.CurrentStage = nextStage;
                    await _context.SaveChangesAsync();
                }
                return NoContent();
            }
            var userRole = request.CurrentRoute.UserRoles
                .FirstOrDefault(ur => ur.UserId == userId && ur.Role.Equals(request.CurrentStage, StringComparison.OrdinalIgnoreCase));
            var existingNormalTransaction = request.Transactions
                .FirstOrDefault(t => t.UserId == userId && t.Action == request.CurrentStage);
            if (existingNormalTransaction != null)
                return Conflict("You've already processed this stage");
            var transaction = new MaintenanceTransaction
            {
                MaintenanceRequestId = request.MaintenanceId,
                UserId = userId,
                Action = request.CurrentStage,
                Comments = $"Rejected: {rejectionReason}",
                Timestamp = DateTime.UtcNow,
                IsCompleted = true
            };
            _context.MaintenanceTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            await _context.Entry(request).Collection(r => r.Transactions).LoadAsync();
            var requiredAfter = request.CurrentRoute.UserRoles
                .Where(ur => ur.Role.Equals(request.CurrentStage, StringComparison.OrdinalIgnoreCase))
                .Select(ur => ur.UserId).Distinct().ToList();
            var completedAfter = request.Transactions
                .Where(t => t.Action == request.CurrentStage && t.IsCompleted)
                .Select(t => t.UserId).Distinct().ToList();
            if (completedAfter.Count >= requiredAfter.Count)
            {
                var nextStage = GetNextStage(request.CurrentStage);
                if (nextStage == "Complete")
                {
                    request.Status = MaintenanceRequestStatus.Rejected;
                    request.AdminComments = $"Rejected: {rejectionReason}";
                    request.CurrentStage = nextStage;
                    await _context.SaveChangesAsync();
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        var emailMessage = new EmailMessage
                        {   firstName = user.UserName,
                            ToAddresses = new List<string> { "ofosupapa@gmail.com" },
                            Subject = "Maintenance Request Rejected",
                            Body = $"Your maintenance request has been rejected. Reason: {rejectionReason}"
                        };
                        await SendEmailAsync(emailMessage);
                    }
                    return Ok(new
                    {
                        Message = "Request rejected.",
                        RequestId = request.MaintenanceId
                    });
                }
                request.CurrentStage = nextStage;
                await _context.SaveChangesAsync();
            }
            var stageComments = await _context.MaintenanceTransactions
                .Where(t => t.MaintenanceRequestId == request.MaintenanceId && !string.IsNullOrEmpty(t.Comments))
                .GroupBy(t => t.Action)
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
                Message = "Request rejected.",
                RequestId = request.MaintenanceId,
                CurrentStage = request.CurrentStage,
                Status = request.Status.ToString(),
                StageComments = stageComments
            });
        }

        [HttpPost("{id}/invalidate")]
        public async Task<IActionResult> InvalidateCompletedRequest(string id, [FromQuery] string userId, [FromBody] string comment)
        {
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var request = await _context.MaintenanceRequests.FirstOrDefaultAsync(r => r.MaintenanceId == id);
            if (request == null)
                return NotFound();
            if (request.CurrentStage != "Complete")
                return BadRequest("Request is not at the final stage.");
            request.Status = MaintenanceRequestStatus.Invalid;
            request.AdminComments += $" | Invalidated: {comment}";
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                var emailMessage = new EmailMessage
                {
                    ToAddresses = new List<string> { user.Email },
                    Subject = "Maintenance Request Invalidated",
                    Body = $"Your maintenance request has been invalidated. Reason: {comment}"
                };
                await SendEmailAsync(emailMessage);
            }
            return Ok(new { Message = "Completed request invalidated and moved to history." });
        }

        [HttpPost("{id}/upload-document")]
        public async Task<IActionResult> UploadDocument(string id, IFormFile file, [FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var request = await _context.MaintenanceRequests.FindAsync(id);
            if (request == null)
                return NotFound();
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");
            var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            if (!Directory.Exists(uploadsDirectory))
            {
                Directory.CreateDirectory(uploadsDirectory);
            }
            var filePath = Path.Combine(uploadsDirectory, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            var document = new MaintenanceDocument
            {
                MaintenanceRequestId = id,
                FileName = file.FileName,
                FilePath = filePath,
                UploadDate = DateTime.UtcNow,
                UploadedByUserId = userId
            };
            _context.MaintenanceDocuments.Add(document);
            await _context.SaveChangesAsync();
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                var emailMessage = new EmailMessage
                {   firstName = user.UserName,
                    ToAddresses = new List<string> { "ofosupapa@gmail.com" },
                    Subject = "Document Uploaded",
                    Body = $"A document has been uploaded for your maintenance request."
                };
                await SendEmailAsync(emailMessage);
            }
            return Ok(new { Message = "Document uploaded successfully.", DocumentId = document.Id });
        }

        [HttpGet("{id}/documents")]
        public async Task<IActionResult> GetRequestDocuments(string id)
        {
            var documents = await _context.MaintenanceDocuments
                .Where(d => d.MaintenanceRequestId == id)
                .Select(d => new
                {
                    DocumentId = d.Id,
                    FileName = d.FileName,
                    FilePath = d.FilePath,
                    UploadDate = d.UploadDate,
                    UploadedByUserId = d.UploadedByUserId
                })
                .ToListAsync();
            return Ok(new
            {
                RequestId = id,
                Documents = documents
            });
        }

        [HttpGet("documents/{documentId}")]
        public async Task<IActionResult> GetDocument(int documentId)
        {
            var document = await _context.MaintenanceDocuments.FindAsync(documentId);
            if (document == null)
                return NotFound();
            var filePath = document.FilePath;
            if (!System.IO.File.Exists(filePath))
                return NotFound();
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return File(fileStream, "application/octet-stream", document.FileName);
        }

        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyRequests(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");
            var requests = await _context.MaintenanceRequests
                .Include(r => r.Vehicle)
                .Include(r => r.RequestedByUser)
                .Include(r => r.CurrentRoute)
                .Where(r => r.RequestedByUserId == userId && r.Status != MaintenanceRequestStatus.Approved)
                .OrderByDescending(r => r.RequestDate)
                .Select(r => new MaintenanceRequestDto
                {
                    Id = r.MaintenanceId,
                    VehicleId = r.VehicleId,
                    VehicleMake = r.Vehicle.Make,
                    VehicleModel = r.Vehicle.Model,
                    LicensePlate = r.Vehicle.LicensePlate,
                    RequestType = r.RequestType.ToString(),
                    Description = r.Description,
                    RequestDate = r.RequestDate,
                    Status = r.Status.ToString(),
                    Priority = r.Priority.ToString(),
                    EstimatedCost = r.EstimatedCost,
                    AdminComments = r.AdminComments,
                    RequestedByUserId = r.RequestedByUserId,
                    RequestedByUserName = r.RequestedByUser.UserName,
                    Department = r.Department,
                    CurrentStage = r.CurrentStage,
                    RouteName = r.CurrentRoute.Name
                })
                .ToListAsync();
            return Ok(requests);
        }

        [HttpGet("approved-requests")]
        public async Task<IActionResult> GetApprovedMaintenanceRequests()
        {
            var scheduledRequestIds = await _context.MaintenanceSchedules
                .Select(s => s.MaintenanceRequestId)
                .ToListAsync();
            var approvedRequests = await _context.MaintenanceRequests
                .Include(r => r.Vehicle)
                .Include(r => r.RequestedByUser)
                .Where(r => r.Status == MaintenanceRequestStatus.Approved && !scheduledRequestIds.Contains(r.MaintenanceId))
                .Select(r => new MaintenanceRequestDto
                {
                    Id = r.MaintenanceId,
                    VehicleId = r.VehicleId,
                    VehicleMake = r.Vehicle.Make,
                    VehicleModel = r.Vehicle.Model,
                    LicensePlate = r.Vehicle.LicensePlate,
                    RequestType = r.RequestType.ToString(),
                    Description = r.Description,
                    RequestDate = r.RequestDate,
                    Status = r.Status.ToString(),
                    Priority = r.Priority.ToString(),
                    EstimatedCost = r.EstimatedCost,
                    AdminComments = r.AdminComments,
                    RequestedByUserId = r.RequestedByUserId,
                    RequestedByUserName = r.RequestedByUser.UserName,
                    Department = r.Department,
                    CurrentStage = r.CurrentStage,
                    RouteName = r.CurrentRoute.Name
                })
                .ToListAsync();
            return Ok(approvedRequests);
        }

        [HttpGet("schedules")]
        public async Task<IActionResult> GetAllMaintenanceSchedules()
        {
            var schedules = await _context.MaintenanceSchedules
                .Include(s => s.MaintenanceRequest)
                    .ThenInclude(r => r.Vehicle)
                .Include(s => s.AssignedMechanic)
                .Select(s => new MaintenanceScheduledDto
                {
                    Id = s.Id,
                    MaintenanceRequestId = s.MaintenanceRequestId,
                    ScheduledDate = s.ScheduledDate,
                    Reason = s.Reason,
                    AssignedMechanicId = s.AssignedMechanicId,
                    AssignedMechanicName = s.AssignedMechanic.UserName,
                    Status = s.Status,
                    Comments = s.Comments,
                    VehicleId = s.MaintenanceRequest.VehicleId,
                    VehicleMake = s.MaintenanceRequest.Vehicle.Make,
                    VehicleModel = s.MaintenanceRequest.Vehicle.Model,
                    LicensePlate = s.MaintenanceRequest.Vehicle.LicensePlate,
                    RepairType = s.MaintenanceRequest.RequestType.ToString(),
                    CompletedDate =s.CompletedDate
                })
                .ToListAsync();
            return Ok(schedules);
        }

        [HttpGet("scheduled/{id}")]
        public async Task<IActionResult> GetMaintenanceSchedule(string id)
        {
            var schedule = await _context.MaintenanceSchedules
                .Include(s => s.MaintenanceRequest)
                .Include(s => s.AssignedMechanic)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (schedule == null)
            {
                return NotFound();
            }
            var scheduleDto = new MaintenanceScheduledDto
            {
                Id = schedule.Id,
                MaintenanceRequestId = schedule.MaintenanceRequestId,
                ScheduledDate = schedule.ScheduledDate,
                Reason = schedule.Reason,
                AssignedMechanicId = schedule.AssignedMechanicId,
                AssignedMechanicName = schedule.AssignedMechanic.UserName,
                Status = schedule.Status,
                Comments = schedule.Comments
            };
            return Ok(scheduleDto);
        }

        [HttpPost("{id}/schedule")]
        public async Task<IActionResult> ScheduleMaintenanceRequest(string id, [FromBody] MaintenanceScheduleDto scheduleDto)
        {
            var request = await _context.MaintenanceRequests.FindAsync(id);
            if (request == null || request.Status != MaintenanceRequestStatus.Approved)
                return NotFound("Approved maintenance request not found");
            var existingSchedule = await _context.MaintenanceSchedules
                .FirstOrDefaultAsync(s => s.MaintenanceRequestId == id);
            if (existingSchedule != null)
            {
                existingSchedule.ScheduledDate = scheduleDto.ScheduledDate;
                existingSchedule.Reason = scheduleDto.Reason;
                existingSchedule.AssignedMechanicId = scheduleDto.AssignedMechanicId;
                existingSchedule.Comments = scheduleDto.Comments;
                await _context.SaveChangesAsync();
                return Ok(new { Message = "Maintenance request already scheduled; updated existing schedule", ScheduleId = existingSchedule.Id });
            }
            var schedule = new MaintenanceSchedule
            {
                MaintenanceRequestId = id,
                ScheduledDate = null,
                DateCreated = DateTime.UtcNow,
                Reason = scheduleDto.Reason,
                AssignedMechanicId = scheduleDto.AssignedMechanicId,
                Status = "Scheduled",
                Comments = scheduleDto.Comments
            };
            _context.MaintenanceSchedules.Add(schedule);
            await _context.SaveChangesAsync();
            var user = await _userManager.FindByIdAsync(schedule.AssignedMechanicId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                var emailMessage = new EmailMessage
                {   firstName = user.UserName,
                    ToAddresses = new List<string> { "ofosupapa@gmail.com" },
                    Subject = "Maintenance Schedule Request ",
                    Body = $"A schedule request has been assigned to you make an update for when its possible for the vehicle to be returned ."
                };
                await SendEmailAsync(emailMessage);
            }
            return Ok(new { Message = "Maintenance request scheduled successfully", ScheduleId = schedule.Id });
        }

        [HttpPut("{id}/schedule")]
        public async Task<IActionResult> UpdateMaintenanceSchedule(string id, [FromBody] MaintenanceScheduleDto scheduleDto)
        {
            var schedule = await _context.MaintenanceSchedules.FindAsync(id);
            if (schedule == null)
                return NotFound("Schedule not found");
            schedule.ScheduledDate = scheduleDto.ScheduledDate;
            schedule.Reason = scheduleDto.Reason;
            schedule.AssignedMechanicId = scheduleDto.AssignedMechanicId;
            schedule.Comments = scheduleDto.Comments;
            await _context.SaveChangesAsync();
            var user = await _userManager.FindByIdAsync(schedule.AssignedMechanicId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                var emailMessage = new EmailMessage
                {   firstName = user.UserName,
                    ToAddresses = new List<string> { "ofosupapa@gmail.com" },
                    Subject = "Maintenance Schedule Updated",
                    Body = $"The maintenance schedule has been updated by {user.UserName}."
                };
                await SendEmailAsync(emailMessage);
            }
            return Ok(new { Message = "Maintenance schedule updated successfully", ScheduleId = schedule.Id });
        }

        [HttpGet("user/{userId}/schedules")]
        public async Task<IActionResult> GetUserSchedules(string userId)
        {
            var schedules = await _context.MaintenanceSchedules
                .Include(s => s.MaintenanceRequest)
                    .ThenInclude(r => r.Vehicle)
                .Include(s => s.AssignedMechanic)
                .Where(s => s.AssignedMechanicId == userId || s.MaintenanceRequest.Vehicle.UserId == userId)
                .Select(s => new MaintenanceScheduledDto
                {
                    Id = s.Id,
                    MaintenanceRequestId = s.MaintenanceRequestId,
                    ScheduledDate = s.ScheduledDate,
                    Reason = s.Reason,
                    AssignedMechanicId = s.AssignedMechanicId,
                    AssignedMechanicName = s.AssignedMechanic.UserName,
                    Status = s.Status,
                    Comments = s.Comments,
                    VehicleId = s.MaintenanceRequest.VehicleId,
                    VehicleMake = s.MaintenanceRequest.Vehicle.Make,
                    VehicleModel = s.MaintenanceRequest.Vehicle.Model,
                    LicensePlate = s.MaintenanceRequest.Vehicle.LicensePlate,
                    RepairType = s.MaintenanceRequest.RequestType.ToString()
                })
                .ToListAsync();
            return Ok(schedules);
        }


        [HttpPost("{id}/complete-with-invoice")]
        public async Task<IActionResult> CompleteWithInvoice(string id, [FromBody] CompleteWithInvoiceDto data, string user)
        {
            var schedule = await _context.MaintenanceSchedules
                .Include(x => x.MaintenanceRequest)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (schedule == null)
                return NotFound("Schedule not found");
            var userId = user;
            if (schedule.AssignedMechanicId != userId)
                return Forbid("Not your schedule!");
            schedule.Status = MaintenanceRequestStatus.Completed.ToString();
            schedule.CompletedDate = DateTime.UtcNow;
            var invoice = new Invoice
            {
                Id = Guid.NewGuid().ToString(),
                MaintenanceScheduleId = id,
                LaborHours = data.Invoice.LaborHours,
                TotalCost = data.Invoice.TotalCost,
                SubmittedBy = userId,
                SubmittedAt = DateTime.UtcNow,
            };
            _context.Invoices.Add(invoice);
            if (data.Invoice.PartsUsed != null)
            {
                foreach (var part in data.Invoice.PartsUsed)
                {
                    _context.PartsUsed.Add(new PartUsed
                    {
                        Id = Guid.NewGuid().ToString(),
                        InvoiceId = invoice.Id,
                        PartName = part.PartName,
                        Quantity = part.Quantity,
                        UnitPrice = part.UnitPrice
                    });
                }
            }
            await _context.SaveChangesAsync();
            var userObj = await _userManager.FindByIdAsync(userId);
            if (userObj != null && !string.IsNullOrEmpty(userObj.Email))
            {
                var emailMessage = new EmailMessage
                {   firstName = userObj.UserName,
                    ToAddresses = new List<string> { "ofosupapa@gmail.com"},
                    Subject = "Maintenance Request Completed ",
                    Body = $"The maintenance request has been completed and has been submitted."
                };
                await SendEmailAsync(emailMessage);
            }
            return Ok(new { Message = "Completed & Invoice submitted", InvoiceId = invoice.Id });
        }


        [HttpPost("{id}/progress-update")]
        public async Task<IActionResult> SubmitProgressUpdate(string id, [FromBody] MaintenanceProgressUpdateDto dto, [FromQuery] string user)
        {
            var schedule = await _context.MaintenanceSchedules
                .Include(s => s.AssignedMechanic)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (schedule == null)
                return NotFound("Schedule not found");
            if (schedule.AssignedMechanicId != user)
                return Forbid("Not your schedule!");
            schedule.ScheduledDate = dto.ExpectedCompletionDate;
            var update = new MaintenanceProgressUpdate
            {
                MaintenanceScheduleId = id,
                MechanicId = user,
                ExpectedCompletionDate = dto.ExpectedCompletionDate,
                Comment = dto.Comment
            };
            _context.MaintenanceProgressUpdates.Add(update);
            await _context.SaveChangesAsync();
            var userObj = await _userManager.FindByIdAsync(user);
            if (userObj != null && !string.IsNullOrEmpty(userObj.Email))
            {
                var emailMessage = new EmailMessage
                {   firstName = userObj.UserName,
                    ToAddresses = new List<string> { "ofosupapa@gmail.com"},
                    Subject = "Maintenance Progress Update",
                    Body = $"A progress update has been submitted for the maintenance request."
                };
                await SendEmailAsync(emailMessage);
            }
            return Ok(new { Message = "Progress update submitted", UpdateId = update.Id });
        }

        [HttpGet("{id}/progress-updates")]
        public async Task<IActionResult> GetProgressUpdates(string id)
        {
            var updates = await _context.MaintenanceProgressUpdates
                .Where(p => p.MaintenanceScheduleId == id)
                .OrderByDescending(p => p.Timestamp)
                .Select(p => new
                {
                    p.ExpectedCompletionDate,
                    p.Comment,
                    p.Timestamp,
                    Mechanic = p.Mechanic.UserName
                })
                .ToListAsync();
            return Ok(updates);
        }

        [HttpGet("progress-updates")]
        public async Task<IActionResult> GetAllProgressUpdates()
        {
            var updates = await _context.MaintenanceProgressUpdates
                .Include(p => p.MaintenanceSchedule)
                    .ThenInclude(s => s.MaintenanceRequest)
                        .ThenInclude(r => r.Vehicle)
                .Include(p => p.Mechanic)
                .OrderByDescending(p => p.Timestamp)
                .Select(p => new
                {
                    ScheduleId = p.MaintenanceScheduleId,
                    Vehicle = new
                    {
                        Id = p.MaintenanceSchedule.MaintenanceRequest.Vehicle.Id,
                        Make = p.MaintenanceSchedule.MaintenanceRequest.Vehicle.Make,
                        Model = p.MaintenanceSchedule.MaintenanceRequest.Vehicle.Model,
                        Plate = p.MaintenanceSchedule.MaintenanceRequest.Vehicle.LicensePlate

                    },
                    Mechanic = p.Mechanic.UserName,
                    ExpectedCompletionDate = p.ExpectedCompletionDate,
                    Comment = p.Comment,
                    Timestamp = p.Timestamp
                })
                .ToListAsync();
            return Ok(updates);
        }

        [HttpGet("progress-updates/user/{userId}")]
        public async Task<IActionResult> GetProgressUpdatesByUser(string userId)
        {
            var updates = await _context.MaintenanceProgressUpdates
                .Where(p => p.MechanicId == userId)
                .Include(p => p.MaintenanceSchedule)
                    .ThenInclude(s => s.MaintenanceRequest)
                        .ThenInclude(r => r.Vehicle)
                .Include(p => p.Mechanic)
                .OrderByDescending(p => p.Timestamp)
                .Select(p => new
                {
                    ScheduleId = p.MaintenanceScheduleId,
                    Vehicle = new
                    {
                        Id = p.MaintenanceSchedule.MaintenanceRequest.Vehicle.Id,
                        Make = p.MaintenanceSchedule.MaintenanceRequest.Vehicle.Make,
                        Model = p.MaintenanceSchedule.MaintenanceRequest.Vehicle.Model,
                        Plate = p.MaintenanceSchedule.MaintenanceRequest.Vehicle.LicensePlate
                    },
                    Mechanic = p.Mechanic.UserName,
                    ExpectedCompletionDate = p.ExpectedCompletionDate,
                    Comment = p.Comment,
                    Timestamp = p.Timestamp
                })
                .ToListAsync();
            return Ok(updates);
        }

        [HttpGet("progress-updates/vehicle/{vehicleId}")]
        public async Task<IActionResult> GetProgressUpdatesByVehicle(string vehicleId)
        {
            var updates = await _context.MaintenanceProgressUpdates
                .Include(p => p.MaintenanceSchedule)
                    .ThenInclude(s => s.MaintenanceRequest)
                        .ThenInclude(r => r.Vehicle)
                .Include(p => p.Mechanic)
                .Where(p => p.MaintenanceSchedule.MaintenanceRequest.VehicleId == vehicleId)
                .OrderByDescending(p => p.Timestamp)
                .Select(p => new
                {
                    ScheduleId = p.MaintenanceScheduleId,
                    Vehicle = new
                    {
                        Id = p.MaintenanceSchedule.MaintenanceRequest.Vehicle.Id,
                        Make = p.MaintenanceSchedule.MaintenanceRequest.Vehicle.Make,
                        Model = p.MaintenanceSchedule.MaintenanceRequest.Vehicle.Model,
                        Plate = p.MaintenanceSchedule.MaintenanceRequest.Vehicle.LicensePlate
                    },
                    Mechanic = p.Mechanic.UserName,
                    ExpectedCompletionDate = p.ExpectedCompletionDate,
                    Comment = p.Comment,
                    Timestamp = p.Timestamp
                })
                .ToListAsync();
            return Ok(updates);
        }

        [HttpGet("progress-updates/request/{requestId}")]
        public async Task<IActionResult> GetProgressUpdatesByRequest(string requestId)
        {
            var scheduleIds = await _context.MaintenanceSchedules
                .Where(s => s.MaintenanceRequestId == requestId)
                .Select(s => s.Id)
                .ToListAsync();
            if (!scheduleIds.Any())
                return NotFound("No schedules found for this maintenance request.");
            var updates = await _context.MaintenanceProgressUpdates
                .Where(p => scheduleIds.Contains(p.MaintenanceScheduleId))
                .Include(p => p.MaintenanceSchedule)
                    .ThenInclude(s => s.MaintenanceRequest)
                        .ThenInclude(r => r.Vehicle)
                .Include(p => p.Mechanic)
                .OrderByDescending(p => p.Timestamp)
                .Select(p => new
                {
                    ScheduleId = p.MaintenanceScheduleId,
                    Vehicle = new
                    {
                        Id = p.MaintenanceSchedule.MaintenanceRequest.Vehicle.Id,
                        Make = p.MaintenanceSchedule.MaintenanceRequest.Vehicle.Make,
                        Model = p.MaintenanceSchedule.MaintenanceRequest.Vehicle.Model,
                        Plate = p.MaintenanceSchedule.MaintenanceRequest.Vehicle.LicensePlate
                    },
                    Mechanic = p.Mechanic.UserName,
                    ExpectedCompletionDate = p.ExpectedCompletionDate,
                    Comment = p.Comment,
                    Timestamp = p.Timestamp
                })
                .ToListAsync();
            return Ok(updates);
        }
    }

    public class ProcessStageDto
    {
        public string Comments { get; set; }
        public decimal? EstimatedCost { get; set; }
    }

    public class MaintenanceRequestDetailDto : MaintenanceRequestDto
    {
        public List<UserRoleDto> RouteUsers { get; set; }
        public List<TransactionDto> Transactions { get; set; }
    }

    public class TransactionDto
    {
        public string? Id { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string Comments { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class CompleteWithInvoiceDto
    {
        public InvoiceDto Invoice { get; set; }
    }

    public class InvoiceDto
    {
        public decimal LaborHours { get; set; }
        public decimal TotalCost { get; set; }
        public List<PartUsedDto> PartsUsed { get; set; }
    }

    public class PartUsedDto
    {
        public string PartName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}

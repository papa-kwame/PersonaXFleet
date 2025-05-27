using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonaXFleet.Data;
using PersonaXFleet.DTOs;
using PersonaXFleet.Models;
using PersonaXFleet.Models.Enums;
using System.Security.Claims;

namespace PersonaXFleet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceRequestController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MaintenanceRequestController(AuthDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (vehicle == null)
                return BadRequest("No vehicle assigned to this user");

            var route = await _context.Routes
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.Department == requestDto.Department);

            if (route == null)
                return BadRequest("No workflow route defined for this department");

            var user = await _userManager.FindByIdAsync(userId);

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
                Comments = "Personal request created",
                Timestamp = DateTime.UtcNow,
                IsCompleted = true
            };
            _context.MaintenanceTransactions.Add(creationTransaction);
            await _context.SaveChangesAsync();

            await ProcessRequestorSkipsAsync(request, route, userId);

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
                            Comments = "Automatically skipped for requestor",
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

                    await _context.Entry(request).Collection(r => r.Transactions).LoadAsync();

                    advanced = true;
                }

            } while (advanced);
        }




        [HttpPost("{id}/process-stage")]
        public async Task<IActionResult> ProcessRequestStage(string id, [FromBody] ProcessStageDto dto, [FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var request = await _context.MaintenanceRequests
                .Include(r => r.CurrentRoute)
                    .ThenInclude(r => r.UserRoles)
                .Include(r => r.Transactions)
                .FirstOrDefaultAsync(r => r.MaintenanceId == id);

            if (request == null)
                return NotFound();

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
                        request.Status = MaintenanceRequestStatus.Approved;
                        request.CurrentStage = nextStage;
                        await _context.SaveChangesAsync();
                        await MoveToHistory(request, dto.Comments);
                        return Ok(new
                        {
                            Message = "Request approved and moved to history",
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

            if (userRole == null)
                return Forbid("You don't have permission for this action");

            var existingNormalTransaction = request.Transactions
                .FirstOrDefault(t => t.UserId == userId && t.Action == request.CurrentStage);

            if (existingNormalTransaction != null)
                return Conflict("You've already processed this stage");

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
                    request.Status = MaintenanceRequestStatus.Approved;
                    request.CurrentStage = nextStage;
                    await _context.SaveChangesAsync();
                    await MoveToHistory(request, dto.Comments);
                    return Ok(new
                    {
                        Message = "Request approved and moved to history",
                        RequestId = request.MaintenanceId
                    });
                }

                request.CurrentStage = nextStage;
                await _context.SaveChangesAsync();
            }

            if (dto.EstimatedCost.HasValue && request.CurrentStage == "Commit")
            {
                request.EstimatedCost = dto.EstimatedCost.Value;
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

        private async Task MoveToHistory(MaintenanceRequest request, string approvalComments)
        {
            var transactions = _context.MaintenanceTransactions
                .Where(t => t.MaintenanceRequestId == request.MaintenanceId);
            _context.MaintenanceTransactions.RemoveRange(transactions);

            var historyRecord = new MaintenanceHistory
            {
                OriginalRequestId = request.MaintenanceId,
                VehicleId = request.VehicleId,
                RequestedByUserId = request.RequestedByUserId,
                RequestType = request.RequestType,
                Description = request.Description,
                RequestDate = request.RequestDate,
                CompletionDate = DateTime.UtcNow,
                Status = request.Status,
                Priority = request.Priority,
                EstimatedCost = request.EstimatedCost,
                AdminComments = request.AdminComments,
                ApprovalComments = approvalComments,
                ApprovedDate = DateTime.UtcNow
            };

            _context.MaintenanceHistories.Add(historyRecord);
            _context.MaintenanceRequests.Remove(request);
            await _context.SaveChangesAsync();
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
                            UserName = t.User,
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
                .Where(t => t.MaintenanceRequestId == id && !string.IsNullOrEmpty(t.Comments))
                .Include(t => t.User)  // Include the User data
                .Select(t => new
                {
                    Stage = t.Action,
                    Comment = t.Comments,
                    UserName = t.User != null ? t.User.UserName : "Unknown", // Adjust based on your User property
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
                                      t.UserId == userId &&
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

            var request = await _context.MaintenanceRequests.FirstOrDefaultAsync(r => r.MaintenanceId == id);

            if (request == null)
                return NotFound();

            if (request.Status == MaintenanceRequestStatus.Approved || request.Status == MaintenanceRequestStatus.Rejected)
                return BadRequest("Request is already finalized.");

            request.Status = MaintenanceRequestStatus.Rejected;
            request.AdminComments = $"Rejected: {rejectionReason}";
            await MoveToHistory(request, rejectionReason);

            return Ok(new { Message = "Request rejected and moved to history." });
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
            await MoveToHistory(request, comment);

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

            // Ensure the Uploads directory exists
            var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            if (!Directory.Exists(uploadsDirectory))
            {
                Directory.CreateDirectory(uploadsDirectory);
            }

            // Save the file to the Uploads directory
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
                .Where(r => r.RequestedByUserId == userId)
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
                    Department = user.Department,
                    CurrentStage = r.CurrentStage,
                    RouteName = r.CurrentRoute.Name
                })
                .ToListAsync();

            return Ok(requests);
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
}

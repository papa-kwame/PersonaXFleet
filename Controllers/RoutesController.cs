﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonaXFleet.Data;
using PersonaXFleet.DTOs;
using PersonaXFleet.Models;
using System.Security.Claims;

namespace PersonaXFleet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoutesController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly string[] _validDepartments = {
    "Admin",
    "Audit",
    "Business Development",
    "Consulting",
    "Customer Support",
    "Finance",
    "HR",
    "Legal",
    "Management",
    "Operations",
    "Software",
    "Support",
    "Systems Integration"
};

        private readonly string[] _requiredRolesInOrder = { "Comment", "Review", "Commit", "Approve" };

        public RoutesController(AuthDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RouteDto>>> GetRoutes()
        {
            var routes = await _context.Routes
                .Include(r => r.UserRoles)
                .ThenInclude(ur => ur.User)
                .ToListAsync();

            var routeDtos = routes.Select(r => new RouteDto
            {
                Id = r.Id,
                Name = r.Name,
                Department = r.Department,
                Description = r.Description,
                Users = r.UserRoles
                    .OrderBy(ur => Array.IndexOf(_requiredRolesInOrder, ur.Role))
                    .Select(ur => new UserRoleDto
                    {
                        UserId = ur.UserId,
                        Role = ur.Role,
                        UserEmail = ur.User?.Email
                    }).ToList()
            }).ToList();

            return Ok(routeDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RouteDto>> GetRoute(string id)
        {
            var route = await _context.Routes
                .Include(r => r.UserRoles)
                .ThenInclude(ur => ur.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (route == null) return NotFound();

            var routeDto = new RouteDto
            {
                Id = route.Id,
                Name = route.Name,
                Department = route.Department,
                Description = route.Description,
                Users = route.UserRoles
                    .OrderBy(ur => Array.IndexOf(_requiredRolesInOrder, ur.Role))
                    .Select(ur => new UserRoleDto
                    {
                        UserId = ur.UserId,
                        Role = ur.Role,
                        UserEmail = ur.User?.Email
                    }).ToList()
            };

            return Ok(routeDto);
        }

        [HttpPost]
        public async Task<ActionResult<RouteDto>> CreateRoute(CreateRouteDto dto)
        {
            if (!_validDepartments.Contains(dto.Department))
                return BadRequest("Invalid department");

            var existingRouteWithSameName = await _context.Routes
                .FirstOrDefaultAsync(r => r.Name == dto.Name);

            if (existingRouteWithSameName != null)
                return Conflict("A route with the same name already exists");

            var existingRouteWithSameDepartment = await _context.Routes
                .FirstOrDefaultAsync(r => r.Department == dto.Department);

            if (existingRouteWithSameDepartment != null)
                return Conflict("A route with the same department already exists");

            var validationResult = ValidateRoleAssignment(dto.Users);
            if (validationResult != null) return validationResult;

            foreach (var userRole in dto.Users)
            {
                if (await _userManager.FindByIdAsync(userRole.UserId) == null)
                    return BadRequest($"User {userRole.UserId} not found");
            }

            var route = new Router
            {
                Name = dto.Name,
                Department = dto.Department,
                Description = dto.Description,
                UserRoles = dto.Users.Select(ur => new UserRouteRole
                {
                    UserId = ur.UserId,
                    Role = ur.Role
                }).ToList()
            };

            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRoute), new { id = route.Id }, route);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoute(string id, CreateRouteDto dto)
        {
            var route = await _context.Routes
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (route == null) return NotFound();

            if (!_validDepartments.Contains(dto.Department))
                return BadRequest("Invalid department");

            var validationResult = ValidateRoleAssignment(dto.Users);
            if (validationResult != null) return validationResult;

            route.Name = dto.Name;
            route.Department = dto.Department;
            route.Description = dto.Description;

            // Update user roles
            var existingRoles = route.UserRoles.ToList();
            var newRoles = dto.Users.ToList();

            // Remove roles not in new list
            foreach (var existing in existingRoles)
            {
                if (!newRoles.Any(nr => nr.UserId == existing.UserId && nr.Role == existing.Role))
                    _context.UserRouteRoles.Remove(existing);
            }

            // Add new roles
            foreach (var newRole in newRoles)
            {
                if (!existingRoles.Any(er => er.UserId == newRole.UserId && er.Role == newRole.Role))
                {
                    route.UserRoles.Add(new UserRouteRole
                    {
                        UserId = newRole.UserId,
                        Role = newRole.Role
                    });
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoute(string id)
        {
            var route = await _context.Routes.FindAsync(id);
            if (route == null) return NotFound();

            _context.Routes.Remove(route);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("departments")]
        public IActionResult GetDepartments()
        {
            return Ok(_validDepartments);
        }

        [HttpGet("roles")]
        public IActionResult GetRoles()
        {
            return Ok(_requiredRolesInOrder);
        }

        [HttpGet("user-route-roles/{userId}")]
        public async Task<ActionResult<IEnumerable<UserRouteAccessDto>>> GetUserRouteRoles(string userId)
        {
            // Check if the user exists
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            // Retrieve the user's route roles
            var userRouteRoles = await _context.UserRouteRoles
                .Include(ur => ur.Route)
                .Where(ur => ur.UserId == userId)
                .ToListAsync();

            if (userRouteRoles == null || !userRouteRoles.Any())
            {
                return NotFound($"No route roles found for user with ID {userId}.");
            }

            // Map the user's route roles to a DTO
            var userRouteAccessDtos = userRouteRoles.Select(ur => new UserRouteAccessDto
            {
                RouteId = ur.RouteId,
                RouteName = ur.Route.Name,
                Department = ur.Route.Department,
                Role = ur.Role
            }).ToList();

            return Ok(userRouteAccessDtos);
        }

        [HttpGet("my-route-details/{routeId}")]
        public async Task<ActionResult<UserRouteDetailDto>> GetMyRouteDetails(string routeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var userRole = await _context.UserRouteRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RouteId == routeId);

            if (userRole == null) return Forbid();

            var route = await _context.Routes
                .Include(r => r.UserRoles)
                .ThenInclude(ur => ur.User)
                .FirstOrDefaultAsync(r => r.Id == routeId);

            if (route == null) return NotFound();

            var routeDto = new UserRouteDetailDto
            {
                RouteId = route.Id,
                RouteName = route.Name,
                Department = route.Department,
                Description = route.Description,
                MyRole = userRole.Role,
                OtherUsers = route.UserRoles
                    .Where(ur => ur.UserId != userId)
                    .OrderBy(ur => Array.IndexOf(_requiredRolesInOrder, ur.Role))
                    .Select(ur => new UserRoleDto
                    {
                        UserId = ur.UserId,
                        Role = ur.Role,
                        UserEmail = ur.User?.Email
                    }).ToList()
            };

            return Ok(routeDto);
        }

        private BadRequestObjectResult ValidateRoleAssignment(List<UserRoleDto> userRoles)
        {
            var providedRoles = userRoles.Select(ur => ur.Role).Distinct().ToList();

            // Check if all required roles are present
            if (!_requiredRolesInOrder.All(r => providedRoles.Contains(r)))
                return BadRequest("All roles (Comment, Review, Commit, Approve) must be assigned");

            // Check if exactly one user per role
            var roleCounts = userRoles.GroupBy(ur => ur.Role)
                .ToDictionary(g => g.Key, g => g.Count());

            if (roleCounts.Values.Any(count => count != 1))
                return BadRequest("Each role must have exactly one user assigned");

            // Check role order
            var rolePositions = userRoles
                .GroupBy(ur => ur.Role)
                .ToDictionary(g => g.Key, g => g.Min(ur => userRoles.IndexOf(ur)));

            for (int i = 0; i < _requiredRolesInOrder.Length - 1; i++)
            {
                if (rolePositions[_requiredRolesInOrder[i]] > rolePositions[_requiredRolesInOrder[i + 1]])
                {
                    return BadRequest($"Roles must be in order: Comment → Review → Commit → Approve");
                }
            }

            return null;
        }
    }
}
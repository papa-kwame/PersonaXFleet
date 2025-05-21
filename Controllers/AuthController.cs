﻿    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Identity;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using Microsoft.IdentityModel.Tokens;
    using System.Text;
    using FleetIdentityServer.Helpers;
    using PersonaXFleet.Models;
    using Microsoft.AspNetCore.Authorization;
    using PersonaXFleet.DTOs;
using Microsoft.EntityFrameworkCore;
using PersonaXFleet.Data;

namespace FleetIdentityServer.Controllers;

    [ApiController]
    [Route("api/[controller]")]
    public class Auth : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;
    private readonly AuthDbContext _context;

    public Auth(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration config, AuthDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
             _config = config;
             _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            Department = model.Department
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _userManager.AddToRoleAsync(user, "User");

        return Ok(new
        {
            Message = "User registered successfully",
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        });
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null) return Unauthorized();

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded) return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);

        var routePermissions = await _context.UserRouteRoles
            .Where(ur => ur.UserId == user.Id)
            .Include(ur => ur.Route)
            .Select(ur => new
            {
                RouteId = ur.Route.Id,
                RouteName = ur.Route.Name,
                Role = ur.Role
            })
            .ToListAsync();

        var token = await JwtHelper.GenerateJwtToken(user, _userManager, _config);
        var rolesList = routePermissions.Select(rp => rp.Role).ToList();

        return Ok(new
        {
            token,
            roles,
            routeRoles = rolesList,
            userId = user.Id
        });
    }

    [HttpGet("verify")]
        public IActionResult Verify()
        {
            return Ok(new { isValid = true });
        }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = _userManager.Users.ToList();
        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Roles = roles.ToList(),
                IsLocked = await _userManager.IsLockedOutAsync(user)
            });
        }

        return Ok(userDtos);
    }


    [HttpPost("users/{userId}/roles")]
        public async Task<IActionResult> UpdateUserRoles(string userId, [FromBody] UpdateRolesDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);

            var rolesToAdd = dto.Roles.Except(currentRoles);
            var rolesToRemove = currentRoles.Except(dto.Roles);

            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded) return BadRequest(removeResult.Errors);
            }

            if (rolesToAdd.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded) return BadRequest(addResult.Errors);
            }

            return Ok();
        }
        [HttpPost("users/{userId}/lock")]
        public async Task<IActionResult> ToggleUserLock(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var isLocked = await _userManager.IsLockedOutAsync(user);

            if (isLocked)
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            }

            return Ok();
        }


        [HttpDelete("users/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Prevent deleting own account
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == userId) return BadRequest("Cannot delete your own account");

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return NoContent();
        }
        // Add this to your Auth controller
        [HttpGet("roles")]
        public IActionResult GetAllRoles()
        {
            // Get all roles from Identity
            var roles = new List<string>
        {
            "Admin",
            "Mechanic",
            "User"
        };

            return Ok(roles);
        }

    }

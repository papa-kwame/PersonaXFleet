﻿using FleetIdentityServer.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PersonaXFleet.Data;
using PersonaXFleet.DTOs;
using PersonaXFleet.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FleetIdentityServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Auth : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;
        private readonly AuthDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public Auth(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration config, AuthDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
            _context = context;
            _roleManager = roleManager;
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

            if (user.MustChangePassword)
            {
                return Ok(new
                {
                    token,
                    roles,
                    routeRoles = rolesList,
                    userId = user.Id,
                    mustChangePassword = true
                });
            }

            return Ok(new
            {
                token,
                roles,
                routeRoles = rolesList,
                userId = user.Id,
                mustChangePassword = false
            });
        }

        [HttpPost("change-password-on-first-login")]
        public async Task<IActionResult> ChangePasswordOnFirstLogin([FromBody] ChangePasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.CurrentPassword, false);
            if (!result.Succeeded)
            {
                return BadRequest("Invalid current password");
            }

            if (model.CurrentPassword == model.NewPassword)
            {
                return BadRequest("New password must be different from the current password.");
            }

            var changeResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!changeResult.Succeeded)
            {
                return BadRequest(changeResult.Errors);
            }

            user.MustChangePassword = false;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "Password changed successfully." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPassword model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                return Ok();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Auth", new { userId = user.Id, token = token }, protocol: HttpContext.Request.Scheme);

            // Log the token to the console or debug output
            Console.WriteLine($"Password Reset Token for {user.Email}: {token}");

            // Send email with the callbackUrl
            // await _emailService.SendPasswordResetEmailAsync(user.Email, callbackUrl);

            return Ok(new { Token = token });
        }

        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var roles = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Roles = roles.ToList(),
                IsLocked = await _userManager.IsLockedOutAsync(user),
                Department = user.Department
            };

            return Ok(userDto);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPassword model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { message = "Password has been reset successfully" });
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
                    IsLocked = await _userManager.IsLockedOutAsync(user),
                    Department = user.Department
                });
            }

            return Ok(userDtos);
        }
        [HttpPut("users/{userId}")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Update user properties
            user.UserName = model.UserName ?? user.UserName;
            user.Email = model.Email ?? user.Email;
            user.PhoneNumber = model.PhoneNumber ?? user.PhoneNumber;
            user.Department = model.Department ?? user.Department;

            // Check if the email is valid
            if (!IsValidEmail(user.Email))
            {
                return BadRequest(new { code = "InvalidEmail", description = "Email is invalid." });
            }

            // Update the user
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new
            {
                Message = "User updated successfully",
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Department = user.Department
            });
        }

        public class UpdateUserDto
        {
            public string UserName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string Department { get; set; }
        }

        [HttpGet("mechanics")]
        public async Task<IActionResult> GetMechanics()
        {
            if (!await _roleManager.RoleExistsAsync("Mechanic"))
            {
                return NotFound("Mechanic role does not exist.");
            }

            var usersInRole = await _userManager.GetUsersInRoleAsync("Mechanic");
            return Ok(usersInRole);
        }

        [HttpPost("users/{userId}/roles")]
        public async Task<IActionResult> UpdateUserRoles(string userId, [FromBody] UpdateRolesDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if (!IsValidEmail(user.Email))
            {
                return BadRequest(new { code = "InvalidEmail", description = "Email is invalid." });
            }

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

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
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

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == userId) return BadRequest("Cannot delete your own account");

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return NoContent();
        }

        [HttpGet("roles")]
        public IActionResult GetAllRoles()
        {
            var roles = _roleManager.Roles.ToList();
            return Ok(roles);
        }
    }
    public class ChangePasswordDto
    {
        public string Email { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }

}

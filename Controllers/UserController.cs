using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Blog.Models;
using Blog.DTOs;
using System;
using System.Threading.Tasks;
using Blog.DTOs.Blog.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Blog.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;

		public UserController(UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
		}

		// GET: api/User
		[HttpGet]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<IEnumerable<ApplicationUserReadDTO>>> GetAllUsers()
		{
			var users = _userManager.Users.ToList();

			if (users == null || !users.Any())
			{
				return NotFound("No users found.");
			}

			var userDTOs = users.Select(user => new ApplicationUserReadDTO
			{
				Id = user.Id,
				FirstName = user.FirstName,
				LastName = user.LastName,
				BirthDate = user.BirthDate,
				Email = user.Email!,
				UserName = user.UserName!,
				PhotoUrl = user.PhotoUrl,
				RegisterDate = user.RegisterDate,
				Status = user.Status
			}).ToList();

			return userDTOs;
		}

		// POST: api/User
		[HttpPost]
		public async Task<ActionResult<ApplicationUserReadDTO>> PostUser(ApplicationUserCreateDTO userDto)
		{
			if (userDto.Password != userDto.ConfirmPassword)
			{
				return BadRequest("Passwords do not match.");
			}

			var user = new ApplicationUser
			{
				FirstName = userDto.FirstName,
				LastName = userDto.LastName,
				BirthDate = userDto.BirthDate,
				Email = userDto.Email,
				UserName = userDto.UserName,
				PhotoUrl = userDto.PhotoUrl,
				RegisterDate = DateTime.Now,
				Status = true
			};

			var result = await _userManager.CreateAsync(user, userDto.Password);

			if (!result.Succeeded)
			{
				return BadRequest(result.Errors);
			}

			var userReadDto = new ApplicationUserReadDTO
			{
				Id = user.Id,
				FirstName = user.FirstName,
				LastName = user.LastName,
				BirthDate = user.BirthDate,
				Email = user.Email,
				UserName = user.UserName,
				PhotoUrl = user.PhotoUrl,
				RegisterDate = user.RegisterDate,
				Status = user.Status
			};

			return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userReadDto);
		}

		// GET: api/User/5
		[HttpGet("{id}")]
		public async Task<ActionResult<ApplicationUserReadDTO>> GetUser(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			var currentUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var roles = await _userManager.GetRolesAsync(user!);

			if (user == null)
			{
				return NotFound();
			}

			if(currentUserID!=id && !roles.Contains("Admin"))
			{
				return StatusCode(403);
			}

			var userReadDto = new ApplicationUserReadDTO
			{
				Id = user.Id,
				FirstName = user.FirstName,
				LastName = user.LastName,
				BirthDate = user.BirthDate,
				Email = user.Email!,
				UserName = user.UserName!,
				PhotoUrl = user.PhotoUrl,
				RegisterDate = user.RegisterDate,
				Status = user.Status
			};

			return userReadDto;
		}




		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteUser(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			var currentUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var roles = await _userManager.GetRolesAsync(user!);

			if (user == null)
			{
				return NotFound();
			}

			if (currentUserID != id && !roles.Contains("Admin"))
			{
				return StatusCode(403);
			}

			if (user.UserName == "admin")
			{
				return StatusCode(403);
			}

			var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var role = User.FindFirstValue(ClaimTypes.Role);

			// Prevent users from deactivating other users unless they are administrators
			if (loggedInUserId != id && role != "Admin")
			{
				return StatusCode(403, "Unauthorized to deactivate this user.");
			}

			// Assuming 'Status' is a boolean property on ApplicationUser that indicates if the user is active or not
			user.Status = false;

			var result = await _userManager.UpdateAsync(user);
			if (!result.Succeeded)
			{
				return StatusCode(500, "Failed to update user status.");
			}

			return NoContent();
		}


		[HttpPost("ForgetPassword")]
		public async Task<ActionResult<string>> ForgetPassword(string userName)
		{
			var applicationUser = await _userManager.FindByNameAsync(userName);

			if (applicationUser == null)
			{
				return NotFound("User not found.");
			}

			if (applicationUser.UserName == "Admin")
			{
				throw new Exception("Access Danied");
			}

			var token = await _userManager.GeneratePasswordResetTokenAsync(applicationUser);
			
				
			return Ok(token);
			
			
		}



		[HttpPost("ResetPassword")]
		public async Task<IActionResult> ResetPassword(string userName, string token, string newPassword)
		{
			var applicationUser = await _userManager.FindByNameAsync(userName);
			if (applicationUser == null)
			{
				return NotFound("User not found");
			}

			if (applicationUser.UserName == "Admin")
			{
				throw new Exception("Access Danied");
			}

			var result = await _userManager.ResetPasswordAsync(applicationUser, token, newPassword);
			if (result.Succeeded)
			{
				return Ok();
			}

			return BadRequest(result.Errors);
		}


	}
}

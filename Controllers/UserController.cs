using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Blog.Models;
using Blog.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Blog.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IWebHostEnvironment _hostingEnvironment;

		public UserController(UserManager<ApplicationUser> userManager, IWebHostEnvironment hostingEnvironment)
		{
			_hostingEnvironment = hostingEnvironment;
			_userManager = userManager;
		}


        // GET: api/User
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult<IEnumerable<ApplicationUserReadDTO>> GetAllUsers()
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
		public async Task<ActionResult<ApplicationUserReadDTO>> PostUser([FromForm] ApplicationUserCreateDTO applicationUserDto)
		{
			if (applicationUserDto.Password != applicationUserDto.ConfirmPassword)
			{
				return BadRequest("Passwords do not match.");
			}

			var user = new ApplicationUser
			{
				FirstName = applicationUserDto.FirstName,
				LastName = applicationUserDto.LastName,
				BirthDate = applicationUserDto.BirthDate,
				Email = applicationUserDto.Email,
				UserName = applicationUserDto.UserName,
				RegisterDate = DateTime.Now,
				Status = true
			};

			// Handle optional photo upload
			if (applicationUserDto.Photo != null && applicationUserDto.Photo.Length > 0)
			{
				var uploadsFolderPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads");
				var uniqueFileName = Guid.NewGuid().ToString() + "_" + applicationUserDto.Photo.FileName;
				var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);

				using (var fileStream = new FileStream(filePath, FileMode.Create))
				{
					await applicationUserDto.Photo.CopyToAsync(fileStream);
				}

				user.PhotoUrl = "/Uploads/" + uniqueFileName;
			}
			else
			{
				user.PhotoUrl = null; // Or some default URL
			}

			var result = await _userManager.CreateAsync(user, applicationUserDto.Password);

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


		// PUT: api/User/{id}
		[HttpPut("{id}")]
		[Authorize]
		public async Task<IActionResult> UpdateUser(string id, [FromForm] ApplicationUserUpdateDTO applicationUserDto)
		{

			var curretUserID  = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var currentUser = await _userManager.FindByIdAsync(curretUserID!);
			
			bool isAdmin = await _userManager.IsInRoleAsync(currentUser!, "Admin");

			if (!isAdmin && currentUser!.Id != id)
			{
				return StatusCode(403, "Access denied.");
			}



			var user = await _userManager.FindByIdAsync(id);
		
			if (user == null)
			{
				return NotFound("User not found.");
			}

			// Update user properties
			user.FirstName = applicationUserDto.FirstName;
			user.LastName = applicationUserDto.LastName;
			user.Email = applicationUserDto.Email;

			// Handle photo update
			if (applicationUserDto.Photo != null && applicationUserDto.Photo.Length > 0)
			{

				// Check if user already has a photo and delete it
				if (!string.IsNullOrEmpty(user.PhotoUrl))
				{
					var existingPhotoPath = Path.Combine(_hostingEnvironment.WebRootPath, user.PhotoUrl.TrimStart('/'));
					if (System.IO.File.Exists(existingPhotoPath))
					{
						System.IO.File.Delete(existingPhotoPath);
					}
				}

				var uploadsFolderPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads");
				var uniqueFileName = Guid.NewGuid().ToString() + "_" + applicationUserDto.Photo.FileName;
				var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);

				using (var fileStream = new FileStream(filePath, FileMode.Create))
				{
					await applicationUserDto.Photo.CopyToAsync(fileStream);
				}

				user.PhotoUrl = "/Uploads/" + uniqueFileName;  // Update the Photo URL
			}

			// Save changes
			var result = await _userManager.UpdateAsync(user);
			if (!result.Succeeded)
			{
				return BadRequest(result.Errors);
			}

			return NoContent(); // 204 No Content is typically returned for successful PUT requests.
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

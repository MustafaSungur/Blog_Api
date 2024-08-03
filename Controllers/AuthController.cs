using Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Controllers
{
	public class AuthController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;

		public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
		}

		[HttpPost("Login")]
		public ActionResult Login(string userName, string password)
		{
			ApplicationUser applicationUser = _userManager!.FindByNameAsync(userName).Result!;
			Microsoft.AspNetCore.Identity.SignInResult signInResult;

			if (applicationUser != null)
			{
				signInResult = _signInManager.PasswordSignInAsync(applicationUser, password, false, false).Result;
				if (signInResult.Succeeded == true)
				{
					return Ok();
				}
			}
			return Unauthorized();
		}

		[HttpGet("Logout")]
		[Authorize]
		public ActionResult Logout()
		{
			_signInManager.SignOutAsync();
			return Ok();
		}
	}
}

using Blog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Blog
{
	public class DbInitializer
	{
		public static async Task InitializeAsync(IServiceProvider serviceProvider)
		{
			using var scope = serviceProvider.CreateScope();
			var scopedProvider = scope.ServiceProvider;

			var context = scopedProvider.GetRequiredService<BlogContext>();
			var roleManager = scopedProvider.GetRequiredService<RoleManager<IdentityRole>>();
			var userManager = scopedProvider.GetRequiredService<UserManager<ApplicationUser>>();

			await context.Database.MigrateAsync();
			if (await roleManager.FindByNameAsync("Admin") == null)
			{
				var identityRole = new IdentityRole("Admin");
				await roleManager.CreateAsync(identityRole);

				var adminUser = new ApplicationUser
				{
					UserName = "admin",
					Email = "admin@admin.com",
					FirstName = "Admin",
					LastName = "admin",
					RegisterDate = DateTime.UtcNow,
					Status = true
				};

				var result = await userManager.CreateAsync(adminUser, "Admin123!");
				if (result.Succeeded)
				{
					await userManager.AddToRoleAsync(adminUser, "Admin");
				}
			}
		}
	}
}

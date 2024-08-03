using Blog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blog
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			builder.Services.AddDbContext<BlogContext>(options =>
			    options.UseSqlServer(builder.Configuration.GetConnectionString("BlogContext") ?? throw new InvalidOperationException("Connection string 'BlogContext' not found.")));


			builder.Services.AddIdentity<ApplicationUser, IdentityRole>().
			AddEntityFrameworkStores<BlogContext>().AddDefaultTokenProviders();

			builder.Services.AddControllers().AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
				options.JsonSerializerOptions.WriteIndented = true; // Optional, for better JSON readability
			});

			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();

			app.UseAuthentication();
			app.UseAuthorization();


			app.MapControllers();

			// Initial variables
			await DbInitializer.InitializeAsync(app.Services);

			app.Run();
		}
	}
}

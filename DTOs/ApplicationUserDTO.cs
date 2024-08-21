namespace Blog.DTOs
{
	
		public class ApplicationUserCreateDTO
		{
			public string FirstName { get; set; } = "";
			public string LastName { get; set; } = "";
			public DateTime BirthDate { get; set; }
			public string Email { get; set; } = "";
			public string UserName { get; set; } = "";
			public IFormFile? Photo { get; set; }
			public string Password { get; set; } = "";
			public string ConfirmPassword { get; set; } = "";
		}

	
			public class ApplicationUserReadDTO
			{
				public string Id { get; set; } = "";
				public string FirstName { get; set; } = "";
				public string LastName { get; set; } = "";
				public DateTime BirthDate { get; set; }
				public string Email { get; set; } = "";
				public string UserName { get; set; } = "";
				public string? PhotoUrl { get; set; }
				public DateTime RegisterDate { get; set; }
				public bool Status { get; set; }
			}
		

		public class ApplicationUserUpdateDTO
	{
		public string FirstName { get; set; } = "";
		public string LastName { get; set; } = "";
		public DateTime BirthDate { get; set; }
		public string Email { get; set; } = "";
		public string UserName { get; set; } = "";
		public IFormFile? Photo { get; set; }  // Changed to IFormFile
	
	}

}

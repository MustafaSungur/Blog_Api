
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Blog.Models;
using Blog.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Blog.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PostsController : ControllerBase
	{
		private readonly BlogContext _context;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IWebHostEnvironment _hostingEnvironment;

		public PostsController(BlogContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment hostingEnvironment)
		{
			_context = context;
			_userManager = userManager;
			_hostingEnvironment = hostingEnvironment;
		}


		[HttpGet]
		public async Task<ActionResult<IEnumerable<PostReadDTO>>> GetPosts()
		{
			var posts = await _context.Posts
						.Include(p => p.ApplicationUser)
						.Include(p => p.Comments)!
							.ThenInclude(c => c.ApplicationUser)
						.Include(p => p.Comments)!
							.ThenInclude(c => c.Replies)! // Yorumların yanıtlarını da içerecek şekilde güncelle
								.ThenInclude(r => r.ApplicationUser)
						.Include(p => p.Likes)!
							.ThenInclude(l => l.ApplicationUser)
						.ToListAsync();

			var postDTOs = posts.Select(item => new PostReadDTO
			{
				Id = item.Id,
				Title = item.Title,
				Content = item.Content,
				PhotoUrl = item.PhotoUrl,
				RegisterDate = item.RegisterDate,
				UserId = item.UserId,
				UserName = item.ApplicationUser!.UserName,
				Comments = item.Comments!.Select(c => new CommentReadDTO
				{
					Id = c.Id,
					Content = c.Content,
					UserId = c.UserId,
					UserName = c.ApplicationUser!.UserName,
					PostId = c.PostId,
					ParentCommentId = c.ParentCommentId,
					CreatedDate = c.CreatedDate,
					Replies = MapReplies(c.Replies!) // Yinelemeli yanıtları DTO'ya dönüştürmek için bir yardımcı method
				}).ToList(),
				Likes = item.Likes!.Select(l => new PostLikeReadDTO
				{
					UserId = l.UserId,
					UserName = l.ApplicationUser!.UserName!,
				}).ToList()
			}).ToList();

			return postDTOs;
		}

		private List<CommentReadDTO> MapReplies(ICollection<Comment> replies)
		{
			return replies.Select(r => new CommentReadDTO
			{
				Id = r.Id,
				Content = r.Content,
				UserId = r.UserId,
				UserName = r.ApplicationUser!.UserName,
				PostId = r.PostId,
				ParentCommentId = r.ParentCommentId,
				CreatedDate = r.CreatedDate,
				Replies = MapReplies(r.Replies!) // Recursive call to map nested replies
			}).ToList();
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<PostReadDTO>> GetPost(long id)
		{
			var post = await _context.Posts
				.Include(p => p.ApplicationUser)
				.Include(p => p.Comments)!
					.ThenInclude(c => c.ApplicationUser)
				.Include(p => p.Comments)!
					.ThenInclude(c=>c.Likes)
				.Include(p => p.Comments)!
					.ThenInclude(c => c.Replies)!
						.ThenInclude(r => r.ApplicationUser)
				.Include(p => p.Likes)!
					.ThenInclude(l => l.ApplicationUser)
				.FirstOrDefaultAsync(p => p.Id == id);

			if (post == null)
			{
				return NotFound();
			}

			var postDto = new PostReadDTO
			{
				Id = post.Id,
				Title = post.Title,
				Content = post.Content,
				PhotoUrl = post.PhotoUrl,
				RegisterDate = post.RegisterDate,
				UserId = post.UserId,
				UserName = post?.ApplicationUser?.UserName,
				Comments = post!.Comments!.Select(c => new CommentReadDTO
				{
					Id = c.Id,
					Content = c.Content,
					UserId = c.UserId,
					UserName = c.ApplicationUser!.UserName,
					PostId = c.PostId,
					ParentCommentId = c.ParentCommentId,
					CreatedDate = c.CreatedDate,
					Replies = MapReplies(c.Replies!)
				}).ToList(),
				Likes = post!.Likes!.Select(l => new PostLikeReadDTO
				{
					UserId = l.UserId,
					UserName = l.ApplicationUser!.UserName!
				}).ToList()
			};

			return postDto;
		}


// PUT: api/Posts/5
		[HttpPut("{id}")]
		[Authorize]
		public async Task<IActionResult> PutPost(long id, PostCreateDTO postDto)
		{



			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			var post = await _context.Posts.FindAsync(id);
			if (post == null)
			{
				return NotFound();
			}

			if(userId != post.UserId)
			{
				return StatusCode(403);
			}

			post.Title = postDto.Title;
			post.Content = postDto.Content;
			post.RegisterDate = DateTime.Now;
			post.UserId = userId!;


			// Handle optional photo upload
			if (postDto.Photo != null && postDto.Photo.Length > 0)
			{
				// Check if user already has a photo and delete it
				if (!string.IsNullOrEmpty(post.PhotoUrl))
				{
					var existingPhotoPath = Path.Combine(_hostingEnvironment.WebRootPath, post.PhotoUrl.TrimStart('/'));
					if (System.IO.File.Exists(existingPhotoPath))
					{
						System.IO.File.Delete(existingPhotoPath);
					}
				}
				var uploadsFolderPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads");
				var uniqueFileName = Guid.NewGuid().ToString() + "_" + postDto.Photo.FileName;
				var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);

				using (var fileStream = new FileStream(filePath, FileMode.Create))
				{
					await postDto.Photo.CopyToAsync(fileStream);
				}

				post.PhotoUrl = "/Uploads/" + uniqueFileName;
			}
			else
			{
				post.PhotoUrl = null; // Or some default URL
			}

			_context.Entry(post).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!PostExists(id))
				{
					return NotFound();
				}
				else
				{
					throw;
				}
			}

			return NoContent();
		}



		// POST: api/Posts
		[HttpPost]
		[Authorize]
		public async Task<ActionResult<PostReadDTO>> PostPost(PostCreateDTO postDto)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			var post = new Post
			{
				Title = postDto.Title,
				Content = postDto.Content,
				RegisterDate = DateTime.Now,
				UserId = userId!,
			};

			// Handle optional photo upload
			if (postDto.Photo != null && postDto.Photo.Length > 0)
			{
				var uploadsFolderPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads");
				var uniqueFileName = Guid.NewGuid().ToString() + "_" + postDto.Photo.FileName;
				var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);

				using (var fileStream = new FileStream(filePath, FileMode.Create))
				{
					await postDto.Photo.CopyToAsync(fileStream);
				}

				post.PhotoUrl = "/Uploads/" + uniqueFileName;
			}
			else
			{
				post.PhotoUrl = null; // Or some default URL
			}

			_context.Posts.Add(post);
			await _context.SaveChangesAsync();

			// Yeni eklenen postu veritabanından tam olarak yüklüyoruz
			post = await _context.Posts
				.Include(p => p.ApplicationUser)!
				.Include(p => p.Comments)!
				.ThenInclude(c => c.ApplicationUser)!
				.Include(p => p.Likes)!
				.ThenInclude(l => l.ApplicationUser)
				.FirstOrDefaultAsync(p => p.Id == post.Id);

			var postReadDto = new PostReadDTO
			{
				Id = post!.Id,
				Title = post.Title,
				Content = post.Content,
				PhotoUrl = post.PhotoUrl,
				RegisterDate = post.RegisterDate,
				UserId = post.UserId,
				UserName = post.ApplicationUser!.UserName,
				Comments = post.Comments!.Select(c => new CommentReadDTO
				{
					Id = c.Id,
					Content = c.Content,
					UserId = c.UserId,
					UserName = c.ApplicationUser!.UserName,
					PostId = c.PostId,
					ParentCommentId = c.ParentCommentId,
					CreatedDate = c.CreatedDate
				}).ToList(),
				Likes = post.Likes!.Select(l => new PostLikeReadDTO
				{
					UserId = l.UserId,
					UserName = l.ApplicationUser!.UserName!
					
				}).ToList()
			};

			return CreatedAtAction("GetPost", new { id = postReadDto.Id }, postReadDto);
		}

		// DELETE: api/Posts/5
		[HttpDelete("{id}")]
		[Authorize]
		public async Task<IActionResult> DeletePost(long id)
		{

			var currentUserID = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("You must login");
			var currentUser = _userManager.FindByIdAsync(currentUserID!).Result ?? throw new Exception("User not found");
			var role = _userManager.GetRolesAsync(currentUser!).Result;

			
			var post = await _context.Posts.Include(p => p.Comments)!
											.ThenInclude(c => c.Likes) // Ensure you load comments and their likes
											.Include(c => c.Likes) // Ensure you load comments and their likes
											.FirstOrDefaultAsync(e => e.Id == id);

			if (post == null)
			{
				return NotFound();
			}

			if (currentUserID != post.UserId && !role.Contains("Admin"))
			{
				return StatusCode(403);
			}


			// Remove all likes related to the comments of the post
			foreach (var comment in post.Comments!)
			{
				_context.CommentLikes.RemoveRange(comment.Likes!);
			}

			// Remove all comments of the post
			_context.Comments.RemoveRange(post.Comments);

			// Remove all likes directly associated with the post
			_context.PostLikes.RemoveRange(post.Likes!);

			// Finally, remove the post itself
			_context.Posts.Remove(post);

			await _context.SaveChangesAsync();

			return NoContent();
		}

		[HttpGet("User/{id}")]
		public async Task<ActionResult<IEnumerable<PostReadDTO>>> GetPostsByUserId(string id)
		{
			var user = await _userManager.Users
				.Include(u => u.Posts)!
					.ThenInclude(p => p.Likes)!
						.ThenInclude(l => l.ApplicationUser)!
				.Include(u => u.Posts)!
					.ThenInclude(p => p.Comments)!
						.ThenInclude(c => c.ApplicationUser)!
				.Include(u => u.Posts)!
					.ThenInclude(p => p.Comments)!
						.ThenInclude(c => c.Replies)!
							.ThenInclude(r => r.ApplicationUser)
				.Include(u => u.Posts)!
					.ThenInclude(p => p.Comments)!
						.ThenInclude(c => c.Likes)!
							.ThenInclude(l => l.ApplicationUser)
				.FirstOrDefaultAsync(u => u.Id == id);

			if (user == null)
			{
				return NotFound("User not found.");
			}

			var postsDto = user.Posts?.Select(item => new PostReadDTO
			{
				Id = item.Id,
				Title = item.Title,
				Content = item.Content,
				PhotoUrl = item.PhotoUrl,
				RegisterDate = item.RegisterDate,
				UserId = item.UserId,
				UserName = item.ApplicationUser!.UserName,
				Comments = item.Comments?.Select(c => new CommentReadDTO
				{
					Id = c.Id,
					Content = c.Content,
					UserId = c.UserId,
					UserName = c.ApplicationUser!.UserName,
					PostId = c.PostId,
					ParentCommentId = c.ParentCommentId,
					CreatedDate = c.CreatedDate,
					Likes = c.Likes?.Select(l => new CommentLikeReadDTO
					{
						UserName = l.ApplicationUser?.UserName!,
						UserId = l.ApplicationUser!.Id
					}).ToList() ?? new List<CommentLikeReadDTO>(),
					Replies = MapReplies(c.Replies ?? new List<Comment>())
				}).ToList() ?? new List<CommentReadDTO>(),
				Likes = item.Likes?.Select(l => new PostLikeReadDTO
				{
					UserId = l.UserId,
					UserName = l.ApplicationUser?.UserName!
				}).ToList() ?? new List<PostLikeReadDTO>()
			}).ToList() ?? new List<PostReadDTO>();

			return Ok(postsDto);
		}


		private bool PostExists(long id)
		{
			return _context.Posts.Any(e => e.Id == id);
		}
	}
}

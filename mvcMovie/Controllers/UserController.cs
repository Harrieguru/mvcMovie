using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mvcMovie.Data;
using mvcMovie.Models;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace mvcMovie.Controllers
{
    public class UserController : Controller
    {
        private readonly MovieDbContext _context;
        private readonly S3Service _s3Service;

        public UserController(MovieDbContext context, S3Service s3Service)
        {
            _context = context;
            _s3Service = s3Service;
        }

        public async Task<IActionResult> Index()
        {
            var files = await _s3Service.ListFilesAsync("m0viefiles");
            return View(files);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User  // Using mvcMovie.Models.User directly here
                {
                    Username = model.Username,
                    Email = model.Email,
                    Password = ComputeHash(model.Password),
                };

                try
                {
                    _context.Users.Add(user);  // Assuming your MovieDbContext Users DbSet points to mvcMovie.Models.User
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Account registered successfully! Please login.";
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    ViewData["ErrorMessage"] = "There was an error registering your account. Please try again.";
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        /*
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);

                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                {
                    return RedirectToAction("Index", "User");
                }

                ViewData["ErrorMessage"] = "Invalid login attempt.";
            }

            return View(model);
        }*/
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);

                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                {
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("UserId", user.UserID.ToString()) // Store UserID as a claim
            };

                    var claimsIdentity = new ClaimsIdentity(claims, "Login");
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity); // Create ClaimsPrincipal

                    await HttpContext.SignInAsync(claimsPrincipal); // Pass ClaimsPrincipal here

                    return RedirectToAction("Index", "User");
                }

                ViewData["ErrorMessage"] = "Invalid login attempt.";
            }

            return View(model);
        }


        public bool IsCurrentUserMovieOwner(Movie movie)
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim != null && movie.Comments != null)
            {
                // Check if any of the comments in the movie has the same UserId as the current user
                return movie.Comments.Any(comment => comment.UserId == userIdClaim.Value);
            }
            return false;
        }

        private static string ComputeHash(string input)
        {
            return BCrypt.Net.BCrypt.HashPassword(input);
        }
    }
}

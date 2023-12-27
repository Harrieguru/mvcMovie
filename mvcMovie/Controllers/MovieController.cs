using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mvcMovie.Models;
using Amazon.DynamoDBv2;
using Amazon.S3.Model;
using Amazon.S3;
using System.Threading.Tasks;
using System.Security.Claims;
using Amazon.DynamoDBv2.DocumentModel;

namespace mvcMovie.Controllers
{

    public class MovieController : Controller
    {
        private readonly DynamoDbContext _dynamoDbContext;

        private readonly S3Service _s3Service;
        public MovieController(DynamoDbContext dynamoDbContext, S3Service s3Service)
        {
            _dynamoDbContext = dynamoDbContext;
            _s3Service = s3Service;
        }

        /*
        public async Task<IActionResult> MovieList()
        {
            var allMovies = await _dynamoDbContext.Context.ScanAsync<Movie>(new List<ScanCondition>()).GetRemainingAsync();
            return View(allMovies);  
        }*/

        public async Task<IActionResult> MovieList(double? minRating = null, string genre = null)
        {
            List<ScanCondition> conditions = new List<ScanCondition>();

            if (minRating.HasValue)
            {
                conditions.Add(new ScanCondition("Rating", ScanOperator.GreaterThan, minRating.Value));
            }

            if (!string.IsNullOrEmpty(genre))
            {
                conditions.Add(new ScanCondition("Genre", ScanOperator.Equal, genre));
            }

            var allMovies = await _dynamoDbContext.Context.ScanAsync<Movie>(conditions).GetRemainingAsync();
            return View(allMovies);
        }


        [HttpGet]
        public IActionResult AddMovie()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> SaveMovie(MovieViewModel movieViewModel)
        {
            // If there are files, upload them to S3
            await UploadFilesToS3(movieViewModel);

            // Convert the ViewModel to a Movie model
            var movie = MapToMovieModel(movieViewModel);

            // Save movie to DynamoDB
            await _dynamoDbContext.Context.SaveAsync(movie);

            return RedirectToAction("MovieList");
        }


        private async Task UploadFilesToS3(MovieViewModel movieViewModel)
        {
            if (movieViewModel.ImageFile != null && movieViewModel.ImageFile.Length > 0)
            {
                movieViewModel.ImageLink = await _s3Service.UploadFileAsync("m0viefiles", movieViewModel.ImageFile);
            }

            if (movieViewModel.VideoFile != null && movieViewModel.VideoFile.Length > 0)
            {
                movieViewModel.S3Link = await _s3Service.UploadFileAsync("m0viefiles", movieViewModel.VideoFile);
            }
        }

        private async Task<bool> IsUserAuthorizedToModify(string movieId)
        {
            var existingMovie = await _dynamoDbContext.Context.LoadAsync<Movie>(movieId);
            return existingMovie != null && existingMovie.UserId == User.FindFirst("UserId")?.Value;
        }

        private Movie MapToMovieModel(MovieViewModel movieViewModel)
        {
            return new Movie
            {
                MovieId = movieViewModel.MovieId,
                Comments = movieViewModel.Comments,
                Description = movieViewModel.Description,
                Directors = movieViewModel.Directors,
                Genre = movieViewModel.Genre,
                ImageLink = movieViewModel.ImageLink,
                Rating = movieViewModel.Rating,
                ReleaseTime = movieViewModel.ReleaseTime,
                S3Link = movieViewModel.S3Link,
                Title = movieViewModel.Title,
                UserId = User.FindFirst("UserId")?.Value
            };
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMovie(string movieId)
        {
            var movieToDelete = await _dynamoDbContext.Context.LoadAsync<Movie>(movieId);
            if (movieToDelete != null && movieToDelete.UserId == User.FindFirst("UserId")?.Value)
            {
                await _dynamoDbContext.Context.DeleteAsync(movieToDelete);
                return RedirectToAction("MovieList");
            }
            return BadRequest("You do not have permission to delete this movie.");
        }

        [HttpGet]
        public async Task<IActionResult> ModifyMovie(string movieId)
        {
            var movie = await _dynamoDbContext.Context.LoadAsync<Movie>(movieId);

            if (await IsUserAuthorizedToModifyById(movieId)) // Changed this to use the modified version.
            {
                var movieViewModel = MapToMovieViewModel(movie);
                return View("ModifyMovie", movieViewModel);  // Use the ModifyMovie view and prepopulate with data
            }

            return BadRequest("You do not have permission to modify this movie.");
        }

        private async Task<bool> IsUserAuthorizedToModifyById(string movieId) // Renamed this method
        {
            var existingMovie = await _dynamoDbContext.Context.LoadAsync<Movie>(movieId);
            return existingMovie != null && existingMovie.UserId == User.FindFirst("UserId")?.Value;
        }


        private MovieViewModel MapToMovieViewModel(Movie movie)
        {
            return new MovieViewModel
            {
                MovieId = movie.MovieId,
                Comments = movie.Comments,
                Description = movie.Description,
                Directors = movie.Directors,
                Genre = movie.Genre,
                ImageLink = movie.ImageLink,
                Rating = movie.Rating,
                ReleaseTime = movie.ReleaseTime,
                S3Link = movie.S3Link,
                Title = movie.Title
            };
        }

        //comments method
        [HttpPost]
        public async Task<IActionResult> AddComment(string movieId, string commentText)
        {
            var movie = await _dynamoDbContext.Context.LoadAsync<Movie>(movieId);
            if (movie != null)
            {

                var comment = new DynamoComment
                {
                    CommentText = commentText,
                    UserId = User.FindFirst("UserId")?.Value,
                    MovieId = movie.MovieId, // Set the movieId for the comment
                    Title = movie.Title  // Set the movie title for the comment
                };
                if (movie.Comments == null) // Ensure there's a list to add the comment to
                {
                    movie.Comments = new List<DynamoComment>();
                }
                movie.Comments.Add(comment);
                await _dynamoDbContext.Context.SaveAsync(movie);
            }
            return RedirectToAction("MovieList");
        }


        /*
        [HttpGet]
        public async Task<IActionResult> ShowComments(string movieId)
        {
            var movie = await _dynamoDbContext.Context.LoadAsync<Movie>(movieId);
            if (movie == null)
            {
                return NotFound("Movie not found.");
            }
            return View(movie);
        }*/
        public async Task<IActionResult> ShowComments(string movieId)
        {
            var movie = await _dynamoDbContext.Context.LoadAsync<Movie>(movieId);
            if (movie == null)
            {
                return NotFound("Movie not found.");
            }

            // Add a CanEdit property to your DynamoComment class or create a CommentViewModel that includes this property.
            foreach (var comment in movie.Comments)
            {
                // Assuming DynamoComment now has a CanEdit property
                comment.CanEdit = IsUserAuthorizedToModifyComment(comment) &&
                                  DateTime.UtcNow.Subtract(comment.CreatedTime).TotalHours <= 24;
            }

            return View(movie);
        }

        //show all comments
        [HttpGet]
        public async Task<IActionResult> Comments()
        {
            var allMovies = await _dynamoDbContext.Context.ScanAsync<Movie>(new List<ScanCondition>()).GetRemainingAsync();

            // Getting all comments from all movies.
            var allComments = new List<DynamoComment>();
            foreach (var movie in allMovies)
            {
                allComments.AddRange(movie.Comments);
            }

            return View(allComments);
        }




        //rating
        [HttpPost]
        public async Task<IActionResult> RateMovie(string movieId, double rating)
        {
            var movie = await _dynamoDbContext.Context.LoadAsync<Movie>(movieId);
            if (movie != null)
            {
                movie.Rating = rating;  // You can modify this to average with existing ratings
                await _dynamoDbContext.Context.SaveAsync(movie);
            }
            return RedirectToAction("MovieList");
        }


        //modify comment
        [HttpPost]
        public async Task<IActionResult> ModifyComment(string movieId, string commentId, string modifiedText)
        {
            var movie = await _dynamoDbContext.Context.LoadAsync<Movie>(movieId);
            if (movie == null)
            {
                return BadRequest("Movie not found.");
            }

            // Find the comment by ID
            var comment = movie.Comments.FirstOrDefault(c => c.CommentId == commentId);
            if (comment == null)
            {
                return BadRequest("Comment not found.");
            }

            // Check if the comment is within the 24-hour window for editing
            if (DateTime.UtcNow.Subtract(comment.CreatedTime).TotalHours > 24)
            {
                return BadRequest("You can only modify comments within 24 hours of posting.");
            }

            // Replace the old comment with the modified text
            comment.CommentText = modifiedText;
            await _dynamoDbContext.Context.SaveAsync(movie);
            return RedirectToAction("ShowComments", new { movieId = movieId });
        }




        private bool IsUserAuthorizedToModifyComment(DynamoComment comment)
        {
            // Assumes that User is the ClaimsPrincipal for the current user, which is typically the case in a controller.
            // Adjust the claim type if your application uses a different claim for the user identifier.
            return comment.UserId == User.FindFirst("UserId")?.Value;
        }


    }
}

namespace mvcMovie.Models
{
    public class MovieViewModel
    {
        public string MovieId { get; set; }
        public List<DynamoComment> Comments { get; set; } = new List<DynamoComment>(); // Initialize for safety
        public string Description { get; set; }
        public List<string> Directors { get; set; } = new List<string>(); // Initialize for safety
        public string Genre { get; set; }
        public string ImageLink { get; set; }
        public double Rating { get; set; }
        public string ReleaseTime { get; set; }
        public string S3Link { get; set; }
        public string Title { get; set; }
        public IFormFile ImageFile { get; set; }
        public IFormFile VideoFile { get; set; }
    }
}

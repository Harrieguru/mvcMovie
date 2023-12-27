using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;

namespace mvcMovie.Models
{
    [DynamoDBTable("Movies")]
    public class Movie
    {
        [DynamoDBHashKey("movieId")]
        public string MovieId { get; set; }

        [DynamoDBProperty("comments")]
        public List<DynamoComment> Comments { get; set; }

        [DynamoDBProperty("description")]
        public string Description { get; set; }

        [DynamoDBProperty("directors")]
        public List<string> Directors { get; set; }

        [DynamoDBProperty("genre")]
        public string Genre { get; set; }

        [DynamoDBProperty("imageLink")]
        public string ImageLink { get; set; }

        [DynamoDBProperty("rating")]
        public double Rating { get; set; }

        [DynamoDBProperty("releaseTime")]
        public string ReleaseTime { get; set; }

        [DynamoDBProperty("s3Link")]
        public string S3Link { get; set; }

        [DynamoDBProperty("title")]
        public string Title { get; set; }


        [NotMapped] // Ensure it doesn't try to save to DynamoDB
        public IFormFile ImageFile { get; set; }

        [NotMapped]
        public IFormFile VideoFile { get; set; }

        [DynamoDBProperty("userId")]
        public string UserId { get; set; }

        


    }

    public class DynamoComment
    {
        // Unique identifier for each comment
        [DynamoDBHashKey]
        public string CommentId { get; set; }

        // Content of the comment
        [DynamoDBProperty("comment")]
        public string CommentText { get; set; }

        // ID of the movie the comment is for
        [DynamoDBRangeKey("movieId")]
        public string MovieId { get; set; }

        // Title of the movie (for reference, not necessarily needed as a key in DynamoDB)
        [DynamoDBProperty("title")]
        public string Title { get; set; }

        // User ID of the user who made the comment
        [DynamoDBProperty("userId")]
        public string UserId { get; set; }

        // Username of the user for display purposes
        [DynamoDBProperty("username")]
        public string Username { get; set; }

        // Timestamp when the comment was created
        [DynamoDBProperty("createdTime")]
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

        // Existing Timestamp property for when the comment was last updated
        [DynamoDBProperty("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [DynamoDBIgnore]
        public bool CanEdit { get; set; }
    }


}

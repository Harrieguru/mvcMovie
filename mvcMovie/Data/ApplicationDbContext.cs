using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.EntityFrameworkCore;

namespace mvcMovie.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<UserInfo> Users { get; set; }
        /*
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = "Server=mssqlserver4api.cqbtrrbzwebs.us-east-2.rds.amazonaws.com;Database=movie;User Id=harriet_atis;Password=$pongeBob1;";
            optionsBuilder.UseSqlServer(connectionString);
        }*/

        // Constructor to accept the DbContextOptions from DI
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
    }

    public class UserInfo
    {
        public string UserId { get; set; }
        // Other user properties
    }

    public class DynamoDbContext
    {
        public DynamoDBContext Context { get; }

        public DynamoDbContext(IAmazonDynamoDB dynamoDbClient)
        {
            Context = new DynamoDBContext(dynamoDbClient);
        }
    }
}

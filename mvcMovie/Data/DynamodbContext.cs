using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

public class DynamoDbContext
{

    public DynamoDBContext Context { get; }

    public DynamoDbContext(IAmazonDynamoDB dynamoDbClient)
    {
        Context = new DynamoDBContext(dynamoDbClient);
    }
}

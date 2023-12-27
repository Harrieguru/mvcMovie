using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using mvcMovie;
using System;
using System.Threading.Tasks;

public class S3Service
{
    private readonly IConfiguration _configuration;
    private readonly IAmazonS3 _s3Client;
    private readonly AWSConfig _awsConfig;
    public S3Service(IOptions<AWSConfig> awsConfigOptions, IAmazonS3 s3Client)
    {
        _awsConfig = awsConfigOptions.Value;
        _s3Client = s3Client;
    }

    public async Task<ListObjectsV2Response> ListFilesAsync(string bucketName)
    {
        try
        {
            var request = new ListObjectsV2Request
            {
                BucketName = bucketName
            };

            return await _s3Client.ListObjectsV2Async(request);
        }
        catch (Exception ex)
        {
            // Handle exception
            throw;
        }
    }



    // Add more methods for other S3 operations as needed
    public async Task<string> UploadFileAsync(string bucketName, IFormFile file)
    {
        try
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var request = new TransferUtilityUploadRequest
            {
                InputStream = memoryStream,
                BucketName = bucketName,
                Key = file.FileName,
                ContentType = file.ContentType
            };

            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(request);

            return $"https://{bucketName}.s3.amazonaws.com/{file.FileName}";  // Return the corrected public link to the uploaded file
        }
        catch (Exception ex)
        {
            // You might want to log the exception and handle it appropriately
            throw;
        }
    }

}

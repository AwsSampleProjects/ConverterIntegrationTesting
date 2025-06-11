using Amazon.S3;
using Amazon.S3.Model;
using System.Text.Json;

namespace ConverterApplication.Tests.Tools;

public class S3Helper
{
    private readonly IAmazonS3 _s3Client;

    /// <summary>
    /// Initializes a new instance of the S3Helper class.
    /// </summary>
    /// <param name="s3Client">An initialized IAmazonS3 client instance (e.g., configured for LocalStack).</param>
    public S3Helper(IAmazonS3 s3Client)
    {
        _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client), "IAmazonS3 client cannot be null.");
    }

    /// <summary>
    /// Uploads a file from a local drive path to an S3 bucket.
    /// Handles file streaming internally.
    /// </summary>
    /// <param name="bucketName">The name of the S3 bucket.</param>
    /// <param name="objectKey">The desired key (path/filename) for the object in S3.</param>
    /// <param name="localFilePath">The full path to the local file to upload.</param>
    /// <param name="contentType">Optional: The MIME type of the file (e.g., "image/jpeg", "text/plain").
    ///                            Defaults to "application/octet-stream" if not provided.</param>
    /// <returns>True if the upload was successful, false otherwise.</returns>
    public async Task<bool> UploadFile(
        string bucketName,
        string objectKey,
        string localFilePath,
        string contentType = "application/json")
    {
        if (string.IsNullOrWhiteSpace(bucketName))
            throw new ArgumentException("Bucket name cannot be null or empty.", nameof(bucketName));
        if (string.IsNullOrWhiteSpace(objectKey))
            throw new ArgumentException("Object key cannot be null or empty.", nameof(objectKey));
        if (string.IsNullOrWhiteSpace(localFilePath))
            throw new ArgumentException("Local file path cannot be null or empty.", nameof(localFilePath));

        if (!File.Exists(localFilePath))
        {
            Console.WriteLine($"Error: Local file not found at '{localFilePath}'.");
            return false;
        }

        try
        {
            // Open the local file as a stream
            // The 'using' statement ensures the FileStream is properly closed and disposed of
            await using var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read);
            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = objectKey,
                InputStream = fileStream, // Provide the stream here
                ContentType = contentType,
                // Optional: You can add more properties here if needed, e.g.:
                // CannedACL = S3CannedACL.PublicRead,
                // Metadata = { { "x-amz-meta-original-filename", Path.GetFileName(localFilePath) } }
            };

            PutObjectResponse response = await _s3Client.PutObjectAsync(putRequest);

            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine($"Successfully uploaded '{localFilePath}' to s3://{bucketName}/{objectKey}. ETag: {response.ETag}");
                return true;
            }

            Console.WriteLine($"Failed to upload '{localFilePath}'. Status: {response.HttpStatusCode}");
            return false;
        }
        catch (AmazonS3Exception s3Ex)
        {
            await Console.Error.WriteLineAsync($"S3 Error uploading file '{localFilePath}': {s3Ex.Message}");
            await Console.Error.WriteLineAsync($"AWS Request ID: {s3Ex.RequestId}");
            return false;
        }
        catch (IOException ioEx)
        {
            await Console.Error.WriteLineAsync($"IO Error accessing file '{localFilePath}': {ioEx.Message}");
            return false;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"An unexpected error occurred during upload of '{localFilePath}': {ex.Message}");
            return false;
        }
    }

    public async Task<T> DownloadFile<T>(string bucketName, string fileName)
    {
        var response = await _s3Client.GetObjectAsync(bucketName, fileName);
        
        using var reader = new StreamReader(response.ResponseStream);
        var json = await reader.ReadToEndAsync();
        
        var result = JsonSerializer.Deserialize<T>(json);
        if (result == null)
        {
            throw new InvalidOperationException($"Failed to deserialize file {fileName} from bucket {bucketName}");
        }

        return result;
    }
}
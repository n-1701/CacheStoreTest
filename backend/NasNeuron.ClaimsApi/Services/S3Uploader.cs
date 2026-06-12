using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace NasNeuron.ClaimsApi.Services;

/// <summary>
/// Uploads the release bundle to the iDrive e2 (S3-compatible) bucket.
/// Credentials come from the environment (AWS_ACCESS_KEY_ID / AWS_SECRET_ACCESS_KEY)
/// and never leave the backend.
/// </summary>
public class S3Uploader
{
    private readonly string _bucket;
    private readonly string _objectKey;
    private readonly string _serviceUrl;
    private readonly ILogger<S3Uploader> _logger;

    public S3Uploader(IConfiguration config, ILogger<S3Uploader> logger)
    {
        _serviceUrl = config["S3:ServiceUrl"] ?? "https://s3.eu-west-3.idrivee2.com";
        _bucket = config["S3:Bucket"] ?? "gorules-poc";
        _objectKey = config["S3:ObjectKey"] ?? "claim_validation.zip";
        _logger = logger;
    }

    public async Task UploadAsync(byte[] zipBytes, CancellationToken ct = default)
    {
        using var client = CreateClient();
        using var stream = new MemoryStream(zipBytes);

        var request = new PutObjectRequest
        {
            BucketName = _bucket,
            Key = _objectKey,
            InputStream = stream,
            ContentType = "application/zip",
            AutoCloseStream = false,
            DisablePayloadSigning = true // some S3-compatible providers reject streaming chunked signatures
        };

        await client.PutObjectAsync(request, ct);
        _logger.LogInformation("Uploaded {Bytes} bytes to s3://{Bucket}/{Key}", zipBytes.Length, _bucket, _objectKey);
    }

    private IAmazonS3 CreateClient()
    {
        var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
        var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");

        if (string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey))
            throw new InvalidOperationException(
                "Missing S3 credentials. Set AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY in the backend environment.");

        var s3Config = new AmazonS3Config
        {
            ServiceURL = _serviceUrl,
            ForcePathStyle = true,
            // The custom endpoint already encodes the region; signing still needs one set.
            AuthenticationRegion = "eu-west-3"
        };

        return new AmazonS3Client(new BasicAWSCredentials(accessKey, secretKey), s3Config);
    }
}

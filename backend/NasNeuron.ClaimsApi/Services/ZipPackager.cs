using System.IO.Compression;
using System.Text;

namespace NasNeuron.ClaimsApi.Services;

/// <summary>
/// Packages the ZEN release bundle in memory. The agent expects this exact layout:
///
///   claim_validation.zip
///   ├── .config/project.json
///   └── claim_validation.json
/// </summary>
public class ZipPackager
{
    private readonly string _accessToken;

    public ZipPackager(IConfiguration config)
    {
        _accessToken = config["Zen:AccessToken"] ?? "nnhs-poc-token";
    }

    /// <summary>Builds the zip bytes from a serialized JDM document.</summary>
    public byte[] Build(string claimValidationJson)
    {
        using var ms = new MemoryStream();
        // leaveOpen so we can read the buffer after the archive is disposed/flushed.
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteEntry(zip, ".config/project.json", ProjectJson());
            WriteEntry(zip, "claim_validation.json", claimValidationJson);
        }
        return ms.ToArray();
    }

    private string ProjectJson() => $$"""
        {
          "version": "1.0.0",
          "project": { "id": "claim_validation", "key": "claim_validation" },
          "accessTokens": ["{{_accessToken}}"],
          "release": { "id": "release-1", "version": "1.0.0" }
        }
        """;

    private static void WriteEntry(ZipArchive zip, string path, string content)
    {
        var entry = zip.CreateEntry(path, CompressionLevel.Optimal);
        using var stream = entry.Open();
        var bytes = Encoding.UTF8.GetBytes(content);
        stream.Write(bytes, 0, bytes.Length);
    }
}

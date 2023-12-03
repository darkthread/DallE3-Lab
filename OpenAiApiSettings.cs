using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;

public class OpenAiApiSettings
{
    public string EndPoint { get; set; }
    public string ApiKey { get; set; }
    public string ModelName { get; set; } = "dall-e";
    public string ApiVersion { get; set; } = "2021-03-01-preview";
    public bool IsValid() => !string.IsNullOrEmpty(this.EndPoint) && !string.IsNullOrEmpty(this.ApiKey);

    private string filePath = null;
    public OpenAiApiSettings() { }
    public OpenAiApiSettings(string path)
    {
        filePath = path;
        if (File.Exists(path))
        {
            var encrypted = File.ReadAllBytes(path);
            var json = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
            var settings = JsonSerializer.Deserialize<OpenAiApiSettings>(json);
            this.EndPoint = settings.EndPoint;
            this.ApiKey = settings.ApiKey;
        }
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(this);
        var encrypted = ProtectedData.Protect(Encoding.UTF8.GetBytes(json), null, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(filePath, encrypted);
    }
}
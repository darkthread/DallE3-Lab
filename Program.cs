using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var apiSettingsPath = Path.Combine(builder.Environment.ContentRootPath, "apisettings.bin");
var apiSettings = new OpenAiApiSettings(apiSettingsPath);

var app = builder.Build();

app.UseDefaultFiles();
app.UseFileServer();

app.MapPost("/api/setup", (HttpContext ctx) => {
    var request = ctx.Request;
    apiSettings.EndPoint = request.Form["endPoint"].ToString();
    apiSettings.ApiKey = request.Form["apiKey"].ToString();
    apiSettings.ModelName = request.Form["modelName"].ToString();
    apiSettings.ApiVersion = request.Form["apiVersion"].ToString();
    apiSettings.Save();
    return Results.Content("OK");
});
var httpClient = new HttpClient();

// https://learn.microsoft.com/zh-tw/azure/ai-services/openai/dall-e-quickstart?tabs=dalle3%2Ccommand-line&pivots=rest-api
app.MapPost("/api/generate", async (DallERequest request) => {
    if (!apiSettings.IsValid())
    {
        return Results.BadRequest("API settings not configured");
    }
    var url = $"{apiSettings.EndPoint}/openai/deployments/{apiSettings.ModelName}/images/generations?api-version={apiSettings.ApiVersion}";
    using (var reqMsg = new HttpRequestMessage(HttpMethod.Post, url))
    {
        reqMsg.Headers.Add("Api-Key", apiSettings.ApiKey);
        reqMsg.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var result = await httpClient.SendAsync(reqMsg);
        var body = await result.Content.ReadAsStringAsync();
        return Results.Json(body);
    }
});

app.Run();

class DallERequest
{
    public string Prompt { get; set;} // text to generate from
    public string Size { get; set; } // 1792x1024, 1024x1024, 1024x1792
    public int N { get; set; } // number of completions to generate
    public string Quality { get; set; } // hd, standard
    public string Style { get; set; } // natural, vivid
}
using ChurchPosterGenAI.Api.Services;
using Microsoft.AspNetCore.Mvc;
using OllamaSharp;
using Microsoft.Extensions.AI;
using OllamaSharp.Models;
namespace ChurchPosterGenAI.Api.Controllers;


public class  PosterClassifierService
{   
    private readonly IConfiguration _config;
    public PosterClassifierService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<string> PosterClassifier(string imagePath)
{
    DotNetEnv.Env.Load();
    string apiKey = _config["OpenRouter:ApiKey"] ?? throw new Exception("Key not found");

    byte[] imageBytes = await System.IO.File.ReadAllBytesAsync(imagePath);
    string base64Image = Convert.ToBase64String(imageBytes);

    string prompt = @"You are a church event classifier. When the user sends you a message, announcement, or event description, your job is to read it and return the single most appropriate theme from the list below.

        Rules:
        - Return only the theme name, nothing else.
        - If the message matches multiple themes, pick the one that is most specific and pick only one never pick multiple.
        - If no theme fits, return 'General Worship'.

        Themes:
        Sunday Morning Service, Evening Service, Midweek Service, Early Morning Prayer, Revival / Crusade, Conference / Summit, Seminar / Workshop, Concert / Gospel Night, Thanksgiving Service, Anniversary / Celebration, Dedication Service, Commissioning Service, Ordination Service, Easter / Good Friday / Resurrection Sunday, Christmas / Nativity, New Year / Crossover Night, Ash Wednesday / Lent, Pentecost Sunday, Harvest Thanksgiving, Prayer & Fasting, All Night Prayer / Vigil, Prayer Mountain, Intercessory Prayer Meeting, Children's Ministry / Sunday School, Youth Ministry, Women's Fellowship / Women's Conference, Men's Fellowship / Men's Conference, Singles Ministry, Couples / Marriage Ministry, Choir / Music Ministry, Ushering / Protocol Ministry, Media / Tech Ministry, Street Evangelism, Community Outreach, Mission Trip, Open Air Crusade, Baptism / Water Baptism, Holy Communion / Lord's Supper, Baby Dedication, Wedding / Marriage Ceremony, Funeral / Memorial Service, Building Fund / Project, Tithe & Offering, Charity / Donation Drive, Scripture / Bible Verse Poster, Inspirational Quote, General Worship, Welcome / Visitor Poster, Announcement / Notice";

    var requestBody = new
    {
        model = "openai/gpt-4o",  // ✅ Valid vision-capable model on OpenRouter
        max_tokens = 120,
        messages = new[]
        {
            new
            {
                role = "user",
                content = new object[]
                {
                    new { type = "image_url", image_url = new { url = $"data:image/jpeg;base64,{base64Image}" } },
                    new { type = "text", text = prompt }
                }
            }
        }
    };

    using var httpClient = new HttpClient();  // ✅ Dispose properly
    httpClient.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

    var json = System.Text.Json.JsonSerializer.Serialize(requestBody);
    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

    var response = await httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
    var result = await response.Content.ReadAsStringAsync();

    // ✅ Check for HTTP errors before parsing
    if (!response.IsSuccessStatusCode)
    {
        throw new Exception($"OpenRouter API error {(int)response.StatusCode}: {result}");
    }

    using var doc = System.Text.Json.JsonDocument.Parse(result);
    var root = doc.RootElement;

    // ✅ Validate the response structure before indexing into it
    if (!root.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
    {
        throw new Exception($"Unexpected API response structure: {result}");
    }

    var text = choices[0]
        .GetProperty("message")
        .GetProperty("content")
        .GetString();

    return text?.Trim() ?? throw new Exception("No content returned from model");
}
  
        
}
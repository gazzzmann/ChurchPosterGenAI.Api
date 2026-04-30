using ChurchPosterGenAI.Api.Data;
using ChurchPosterGenAI.Api.Services;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString(
        "ChurchPosterDbConnectionString");

builder.Services.AddDbContext<ChurchPosterDbContext>(options =>
    options.UseSqlServer(connectionString));

var hfToken = builder.Configuration["HuggingFace:Token"];

builder.Services.AddHttpClient("HuggingFace", client =>
{
    client.BaseAddress =
        new Uri("https://api-inference.huggingface.co/models/");

    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", hfToken);

    client.Timeout = TimeSpan.FromMinutes(3);
});

builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<IGenerationService, GenerationService>();
builder.Services.AddScoped<IAIImageService, AIImageService>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters
        .Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
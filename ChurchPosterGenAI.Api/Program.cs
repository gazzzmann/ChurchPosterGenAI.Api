using ChurchPosterGenAI.Api.Data;
using ChurchPosterGenAI.Api.Services;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("ChurchPosterDbConnectionString");
builder.Services.AddDbContext<ChurchPosterDbContext>(options => options.UseSqlServer(connectionString));

var apiKey = builder.Configuration["OpenAI:ApiKey"];

builder.Services.AddHttpClient("OpenAI", client =>
{
    client.BaseAddress = new Uri("https://api.openai.com/v1/");
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", apiKey);
});

builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<IGenerationService, GenerationService>();
builder.Services.AddScoped<IAIImageService, AIImageService>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // This tells the serializer to use the string name of the enum, not the integer
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
 
app.UseStaticFiles();

app.MapControllers();

app.Run();

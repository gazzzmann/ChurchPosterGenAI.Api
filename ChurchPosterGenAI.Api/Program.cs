using ChurchPosterGenAI.Api.Data;
using ChurchPosterGenAI.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("ChurchPosterDbConnectionString");
builder.Services.AddDbContext<ChurchPosterDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<ITemplateService, TemplateService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();
 
app.UseStaticFiles();

app.MapControllers();

app.Run();

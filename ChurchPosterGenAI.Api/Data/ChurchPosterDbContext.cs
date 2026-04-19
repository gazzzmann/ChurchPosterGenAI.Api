using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ChurchPosterGenAI.Api.Data;

public class ChurchPosterDbContext : DbContext
{
    public ChurchPosterDbContext(DbContextOptions<ChurchPosterDbContext> options) : base
        (options)
    {

    }

    public DbSet<GeneratedPoster> Posters { get; set; }
    public DbSet<GenerationRequest> Requests { get; set; }
    public DbSet<PosterTemplate> Templates { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    }

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchPosterGenAI.Api.Data.Configurations;

public class RequestConfiguration : IEntityTypeConfiguration<GenerationRequest>
{
    public void Configure(EntityTypeBuilder<GenerationRequest> builder)
    {
        builder.Property(q => q.Status)
              .HasConversion<string>()
              .HasMaxLength(20);
    }
}

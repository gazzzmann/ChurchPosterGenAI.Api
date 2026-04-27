using ChurchPosterGenAI.Api.Data;
using ChurchPosterGenAI.Api.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchPosterGenAI.Api.Configurations;

public class TemplateConfiguration : IEntityTypeConfiguration<PosterTemplate>
{
    public void Configure(EntityTypeBuilder<PosterTemplate> builder)
    {

        builder.HasData(
            new PosterTemplate
            {
                Id = 1,
                Title = "Youth Revival Flyer",
                Category = PosterCategory.Conference,
                ImageUrl = "/images/templates/conference.jpg"    
            },
            new PosterTemplate
            {
                Id = 2,
                Title = "Special Service Flyer",
                Category = PosterCategory.SpecialEvent,
                ImageUrl = "/images/templates/worship.jpg"
            }
        );
    }
}
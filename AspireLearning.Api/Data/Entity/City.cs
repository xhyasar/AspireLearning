namespace AspireLearning.Api.Data.Entity;

using Microsoft.EntityFrameworkCore;
using ServiceDefaults.GlobalAttribute;
using ServiceDefaults.GlobalConstant;
using ServiceDefaults.GlobalEnum;
using ServiceDefaults.GlobalInterface;

public class City 
{
    public Guid Id { get; set; }
    
    public Guid CountryId { get; set; }
    
    public Country? Country { get; set; }
    
    public ICollection<CityText>? Texts { get; set; }
}

public class CityText
{
    public Guid Id { get; set; }
    
    public Guid CityId { get; set; }
    
    public LanguageEnum Language { get; set; }
    
    public string Name { get; set; } = null!;
    
    public City? City { get; set; }
}

public class CityConfigurator : IEntityConfigurator
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<City>(entity =>
        {
            entity.ToTable("Cities");
            
            entity.HasKey(c => c.Id);
            
            entity.HasOne(c => c.Country)
                .WithMany(c => c.Cities)
                .HasForeignKey(c => c.CountryId)
                .OnDelete(DeleteBehavior.NoAction);
            
            entity.HasMany(c => c.Texts)
                .WithOne(ct => ct.City)
                .HasForeignKey(ct => ct.CityId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<CityText>(entity =>
        {
            entity.ToTable("CityTexts");
            
            entity.HasKey(ct => ct.Id);
            
            entity.Property(ct => ct.Name)
                .IsRequired()
                .HasMaxLength(255);
        });
    }
}

public class CitySeeder : IEntitySeeder
{
    public void Seed(ModelBuilder modelBuilder)
    {
        var countries = new List<City>
        {
            new City
            {
                Id = CityConstants.Istanbul.Id,
                CountryId = CityConstants.Istanbul.CountryId,
            },
            new City
            {
                Id = CityConstants.LosAngeles.Id,
                CountryId = CityConstants.LosAngeles.CountryId,
            }
        };

        modelBuilder.Entity<City>().HasData(countries);
    }
}

[SeedAfter(nameof(CitySeeder))]
public class CityTextSeeder : IEntitySeeder
{
    public void Seed(ModelBuilder modelBuilder)
    {
        var countries = new List<CityText>
        {
            new()
            {
                Id = CityConstants.Istanbul.Texts.TR.Id,
                CityId = CityConstants.Istanbul.Id,
                Language = LanguageEnum.TR,
                Name = CityConstants.Istanbul.Texts.TR.Name
            },
            new()
            {   
                Id = CityConstants.Istanbul.Texts.EN.Id,
                CityId = CityConstants.Istanbul.Id,
                Language = LanguageEnum.EN,
                Name = CityConstants.Istanbul.Texts.EN.Name
            },
            new()
            {
                Id = CityConstants.LosAngeles.Texts.TR.Id,
                CityId = CityConstants.LosAngeles.Id,
                Language = LanguageEnum.TR,
                Name = CityConstants.LosAngeles.Texts.TR.Name
            },
            new()
            {
                Id = CityConstants.LosAngeles.Texts.EN.Id,
                CityId = CityConstants.LosAngeles.Id,
                Language = LanguageEnum.EN,
                Name = CityConstants.LosAngeles.Texts.EN.Name
            }
        };

        modelBuilder.Entity<CityText>().HasData(countries);
    }
}

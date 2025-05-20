namespace AspireLearning.Api.Data.Entity;

using Microsoft.EntityFrameworkCore;
using ServiceDefaults.GlobalAttribute;
using ServiceDefaults.GlobalConstant;
using ServiceDefaults.GlobalEnum;
using ServiceDefaults.GlobalInterface;

public class Country 
{
    public Guid Id { get; set; }
    
    public ICollection<City>? Cities { get; set; }
    
    public ICollection<CountryText>? Texts { get; set; }
}

public class CountryText
{
    public Guid Id { get; set; }
    
    public Guid CountryId { get; set; }
    
    public Language Language { get; set; }
    
    public string Name { get; set; } = null!;
    
    public string Code { get; set; } = null!;
    
    public Country? Country { get; set; }
}

public class CountryConfigurator : IEntityConfigurator
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Country>(entity =>
        {
            entity.ToTable("Countries");
            
            entity.HasKey(c => c.Id);
            
            entity.HasMany(c => c.Cities)
                .WithOne(c => c.Country)
                .HasForeignKey(c => c.CountryId)
                .OnDelete(DeleteBehavior.NoAction);
            
            entity.HasMany(c => c.Texts)
                .WithOne(ct => ct.Country)
                .HasForeignKey(ct => ct.CountryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<CountryText>(entity =>
        {
            entity.ToTable("CountryTexts");
            
            entity.HasKey(ct => ct.Id);
            
            entity.Property(ct => ct.Name)
                .IsRequired()
                .HasMaxLength(255);
        });
    }
}

public class CountrySeeder : IEntitySeeder
{
    public void Seed(ModelBuilder modelBuilder)
    {
        var countries = new List<Country>
        {
            new Country
            {
                Id = CountryConstants.Turkey.Id,
            },
            new Country
            {
                Id = CountryConstants.USA.Id,
            }
        };

        modelBuilder.Entity<Country>().HasData(countries);
    }
}

[SeedAfter(nameof(CountrySeeder))]
public class CountryTextSeeder : IEntitySeeder
{
    public void Seed(ModelBuilder modelBuilder)
    {
        var countries = new List<CountryText>
        {
            new()
            {
                Id = CountryConstants.USA.Texts.TR.Id,
                CountryId = CountryConstants.USA.Id,
                Language = Language.TR,
                Code = CountryConstants.USA.Texts.TR.Code,
                Name = CountryConstants.USA.Texts.TR.Name
            },
            new()
            {   
                Id = CountryConstants.USA.Texts.EN.Id,
                CountryId = CountryConstants.USA.Id,
                Language = Language.EN,
                Code = CountryConstants.USA.Texts.EN.Code,
                Name = CountryConstants.USA.Texts.EN.Name
            },
            new()
            {
                Id = CountryConstants.Turkey.Texts.TR.Id,
                CountryId = CountryConstants.Turkey.Id,
                Language = Language.TR,
                Code = CountryConstants.Turkey.Texts.TR.Code,
                Name = CountryConstants.Turkey.Texts.TR.Name
            },
            new()
            {
                Id = CountryConstants.Turkey.Texts.EN.Id,
                CountryId = CountryConstants.Turkey.Id,
                Language = Language.EN,
                Code = CountryConstants.Turkey.Texts.EN.Code,
                Name = CountryConstants.Turkey.Texts.EN.Name
            }
        };

        modelBuilder.Entity<CountryText>().HasData(countries);
    }
}

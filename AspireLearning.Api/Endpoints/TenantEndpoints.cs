using AspireLearning.Api.Data.Context;
using AspireLearning.Api.Data.Entity;
using AspireLearning.ServiceDefaults.GlobalConstant;
using AspireLearning.ServiceDefaults.GlobalModel.Session;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspireLearning.Api.Endpoints;

public static class TenantEndpoints
{
    public static void MapTenantEndpoints(this WebApplication app)
    {
        // Tüm tenant'ları getir - Sadece SuperAdmin
        app.MapGet("tenants", 
                async (
                [FromServices] Context context,
                [FromServices] SessionModel session) =>
                {
                    var tenants = await context.Tenants
                        .Include(t => t.Domains)
                        .Where(t => !t.IsDeleted)
                        .Select(t => new TenantViewModel(
                            t.Id,
                            t.Name,
                            t.ContactName,
                            t.ContactEmail, 
                            t.ContactPhone,
                            t.CreatedAt,
                            t.IsActive,
                            t.Domains!.Where(d => !d.IsDeleted)
                                .Select(d => new TenantDomainViewModel(
                                    d.Id,
                                    d.Domain,
                                    d.IsActive
                                )).ToList()
                        ))
                        .ToListAsync();

                    return Results.Ok(tenants);
                })
            .WithTags(EndpointConstants.TenantOperations)
            .WithDescription("Get all tenants (SuperAdmin only)")
            .RequireAuthorization("SuperAdmin")
            .Produces<List<TenantViewModel>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
        
        // Belirli bir tenant'ı getir - SuperAdmin tümünü, TenantAdmin sadece kendisini
        app.MapGet("tenants/{id}", [Authorize(Roles = "SuperAdmin,TenantAdmin")]
                async (
                [FromRoute] Guid id,
                [FromServices] Context context,
                [FromServices] SessionModel session) =>
                {
                    // SuperAdmin tüm tenant'ları, TenantAdmin sadece kendi tenant'ını görebilir
                    var query = context.Tenants
                        .Include(t => t.Domains)
                        .Where(t => !t.IsDeleted);

                    // TenantAdmin rolündeyse sadece kendi tenant'ını görebilir
                    if (session.User.Roles.Contains("TenantAdmin") && !session.User.Roles.Contains("SuperAdmin"))
                    {
                        query = query.Where(t => t.Id == session.TenantId);
                        
                        // Kendi tenant'ı değilse erişim yok
                        if (id != session.TenantId)
                        {
                            return Results.NotFound();
                        }
                    }

                    var tenant = await query
                        .Where(t => t.Id == id)
                        .Select(t => new TenantViewModel(
                            t.Id,
                            t.Name,
                            t.ContactName,
                            t.ContactEmail,
                            t.ContactPhone,
                            t.CreatedAt,
                            t.IsActive,
                            t.Domains!.Where(d => !d.IsDeleted)
                                .Select(d => new TenantDomainViewModel(
                                    d.Id,
                                    d.Domain,
                                    d.IsActive
                                )).ToList()
                        ))
                        .FirstOrDefaultAsync();

                    return tenant != null ? Results.Ok(tenant) : Results.NotFound();
                })
            .WithTags(EndpointConstants.TenantOperations)
            .WithDescription("Get tenant by ID (SuperAdmin: any tenant, TenantAdmin: own tenant only)")
            .Produces<TenantViewModel>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
        
        // Yeni tenant oluştur - Sadece SuperAdmin
        app.MapPost("tenants", 
                async (
                [FromBody] TenantCreateModel model,
                [FromServices] Context context,
                [FromServices] SessionModel session) =>
                {
                    // Tenant oluşturma - sadece SuperAdmin yapabilir
                    var tenant = new Tenant
                    {
                        Id = Guid.NewGuid(),
                        Name = model.Name,
                        ContactName = model.ContactName,
                        ContactEmail = model.ContactEmail,
                        ContactPhone = model.ContactPhone,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = Guid.Parse(session.UserId),
                        IsActive = true
                    };

                    context.Tenants.Add(tenant);
                    
                    // Tenant'ın domain'ini ekle
                    var domain = new TenantDomain
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenant.Id,
                        Domain = model.Domain,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = Guid.Parse(session.UserId),
                        IsActive = true
                    };
                    
                    context.TenantDomains.Add(domain);
                    
                    await context.SaveChangesAsync();
                    
                    return Results.Created($"/tenants/{tenant.Id}", new { id = tenant.Id });
                })
            .WithTags(EndpointConstants.TenantOperations)
            .WithDescription("Create a new tenant (SuperAdmin only)")
            .RequireAuthorization("SuperAdmin")
            .Produces<object>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
        
        // Tenant güncelle - SuperAdmin tümünü, TenantAdmin sadece kendisini
        app.MapPut("tenants/{id}", [Authorize(Roles = "SuperAdmin,TenantAdmin")]
                async (
                [FromRoute] Guid id,
                [FromBody] TenantUpdateModel model,
                [FromServices] Context context,
                [FromServices] SessionModel session) =>
                {
                    // Tenant güncelleme - SuperAdmin tüm tenant'ları, TenantAdmin sadece kendi tenant'ını güncelleyebilir
                    var tenant = await context.Tenants.FindAsync(id);
                    
                    if (tenant == null || tenant.IsDeleted)
                    {
                        return Results.NotFound();
                    }
                    
                    // TenantAdmin rolündeyse sadece kendi tenant'ını güncelleyebilir
                    if (session.User.Roles.Contains("TenantAdmin") && !session.User.Roles.Contains("SuperAdmin"))
                    {
                        if (id != session.TenantId)
                        {
                            return Results.NotFound();
                        }
                    }
                    
                    // Sadece verilen alanları güncelle
                    if (!string.IsNullOrWhiteSpace(model.Name))
                        tenant.Name = model.Name;
                    
                    if (!string.IsNullOrWhiteSpace(model.ContactName))
                        tenant.ContactName = model.ContactName;
                    
                    if (!string.IsNullOrWhiteSpace(model.ContactEmail))
                        tenant.ContactEmail = model.ContactEmail;
                    
                    if (!string.IsNullOrWhiteSpace(model.ContactPhone))
                        tenant.ContactPhone = model.ContactPhone;
                    
                    tenant.ModifiedAt = DateTime.UtcNow;
                    tenant.ModifiedBy = Guid.Parse(session.UserId);
                    
                    await context.SaveChangesAsync();
                    
                    return Results.Ok();
                })
            .WithTags(EndpointConstants.TenantOperations)
            .WithDescription("Update a tenant (SuperAdmin: any tenant, TenantAdmin: own tenant only)")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
        
        // Tenant sil - Sadece SuperAdmin
        app.MapDelete("tenants/{id}", 
                async (
                [FromRoute] Guid id,
                [FromServices] Context context,
                [FromServices] SessionModel session) =>
                {
                    var tenant = await context.Tenants.FindAsync(id);
                    
                    if (tenant == null || tenant.IsDeleted)
                        return Results.NotFound();
                    
                    // Silme işlemi (soft delete)
                    tenant.IsDeleted = true;
                    tenant.IsActive = false;
                    tenant.RemovedAt = DateTime.UtcNow;
                    tenant.RemovedBy = Guid.Parse(session.UserId);
                    
                    await context.SaveChangesAsync();
                    
                    return Results.Ok();
                })
            .WithTags(EndpointConstants.TenantOperations)
            .WithDescription("Delete a tenant (SuperAdmin only)")
            .RequireAuthorization("SuperAdmin")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
        
        // Tenant'a domain ekle - Sadece SuperAdmin
        app.MapPost("tenant-domains", 
                async (
                [FromBody] TenantDomainCreateModel model,
                [FromServices] Context context,
                [FromServices] SessionModel session) =>
                {
                    var tenant = await context.Tenants.FindAsync(model.TenantId);
                    
                    if (tenant == null || tenant.IsDeleted)
                        return Results.NotFound("Tenant.NotFound");
                    
                    // Domain ekle
                    var domain = new TenantDomain
                    {
                        Id = Guid.NewGuid(),
                        TenantId = model.TenantId,
                        Domain = model.Domain,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = Guid.Parse(session.UserId),
                        IsActive = true
                    };
                    
                    context.TenantDomains.Add(domain);
                    await context.SaveChangesAsync();
                    
                    return Results.Created($"/tenant-domains/{domain.Id}", new { id = domain.Id });
                })
            .WithTags(EndpointConstants.TenantOperations)
            .WithDescription("Add a domain to a tenant (SuperAdmin only)")
            .RequireAuthorization("SuperAdmin")
            .Produces<object>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
        
        // Tenant domain sil - Sadece SuperAdmin
        app.MapDelete("tenant-domains/{id}", 
                async (
                [FromRoute] Guid id,
                [FromServices] Context context,
                [FromServices] SessionModel session) =>
                {
                    var domain = await context.TenantDomains.FindAsync(id);
                    
                    if (domain == null || domain.IsDeleted)
                        return Results.NotFound();
                    
                    // Silme işlemi (soft delete)
                    domain.IsDeleted = true;
                    domain.IsActive = false;
                    domain.RemovedAt = DateTime.UtcNow;
                    domain.RemovedBy = Guid.Parse(session.UserId);
                    
                    await context.SaveChangesAsync();
                    
                    return Results.Ok();
                })
            .WithTags(EndpointConstants.TenantOperations)
            .WithDescription("Delete a tenant domain (SuperAdmin only)")
            .RequireAuthorization("SuperAdmin")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }
}

// Record tanımları
public record TenantViewModel(
    Guid Id,
    string Name,
    string ContactName,
    string ContactEmail,
    string ContactPhone,
    DateTime CreatedAt,
    bool IsActive,
    List<TenantDomainViewModel> Domains
);

public record TenantDomainViewModel(
    Guid Id,
    string Domain,
    bool IsActive
);

public record TenantCreateModel(
    string Name,
    string ContactName,
    string ContactEmail,
    string ContactPhone,
    string Domain
);

public record TenantUpdateModel(
    string? Name,
    string? ContactName,
    string? ContactEmail,
    string? ContactPhone
);

public record TenantDomainCreateModel(
    Guid TenantId,
    string Domain
);

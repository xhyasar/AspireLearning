namespace AspireLearning.Api.Endpoints;

using Data.Context;
using Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceDefaults.GlobalConstant;
using ServiceDefaults.GlobalModel.Session;
using ServiceDefaults.GlobalUtility;

public static class WarehouseEndpoints {
    public static void MapWarehouseEndpoints(this WebApplication app)
    {
        app.MapPost("/warehouse", async ([FromBody] WarehouseCreateModel model, 
         [FromServices] Context context,
         [FromServices] SessionModel session) => {
                // City ve Country uyumluluğunu kontrol et
                var city = await context.Cities
                    .FirstOrDefaultAsync(c => c.Id == model.City);
                
                if (city == null)
                    return Results.BadRequest("City.NotFound");
                
                if (city.CountryId != model.Country)
                    return Results.BadRequest("City.Country.Mismatch");

                var warehouse = new Warehouse
                {
                    Name = model.Name,
                    PersonInChargeId = model.PersonInChargeId,
                    CountryId = model.Country,
                    CityId = model.City,
                    MapUrl = model.MapUrl,
                    Address = model.Address,
                    TenantId = session.TenantId,
                    CategoryId = model.CategoryId
                };

                context.Warehouses.Add(warehouse);
                await context.SaveChangesAsync();

                return Results.Created($"/warehouse", null);
            })
            .WithTags(EndpointConstants.WarehouseOperations)
            .WithDescription("Create a new warehouse")
            .RequireAuthorization(Permissions.Warehouse.Add)
            .Produces(StatusCodes.Status201Created);

        app.MapGet("/warehouse", async (
                [FromServices]Context context,
                [FromServices]SessionModel session,
                [FromQuery] string? searchName,
                [FromQuery] string? sortBy,
                [FromQuery] string? sortDirection,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10) => {
                var warehouses = context.Warehouses
                    .Where(w => w.TenantId == session.TenantId);

                if (!string.IsNullOrEmpty(searchName))
                    warehouses = warehouses.Where(w => w.Name.Contains(searchName));

                warehouses = sortBy?.ToLower() switch
                {
                    "name" when sortDirection == "desc" => warehouses.OrderByDescending(w => w.Name),
                    "name" => warehouses.OrderBy(w => w.Name),
                    "lastactivity" when sortDirection == "desc" => warehouses.OrderByDescending(w => w.LastActivity),
                    "lastactivity" => warehouses.OrderBy(w => w.LastActivity),
                    _ => warehouses.OrderBy(w => w.Name)
                };

                var totalCount = await warehouses.CountAsync();
                var paginatedQuery = warehouses.Skip(((pageNumber - 1) * pageSize)!).Take(pageSize);

                var queryResult = await paginatedQuery.Select(x => new WarehouseViewModel
                (
                    x.Id,
                    x.Name,
                    x.City!.Texts!.FirstOrDefault(ct => ct.Language == session.Language)!.Name,
                    x.Country!.Texts!.FirstOrDefault(ct => ct.Language == session.Language)!.Name,
                    x.LastActivity,
                    x.Category!.Name
                )).ToListAsync();

                var result = new PaginatedResult<WarehouseViewModel>
                {
                    TotalCount = totalCount, PageNumber = pageNumber!, PageSize = pageSize, Data = queryResult
                };
              
                return Results.Ok(result);
            })
            .WithTags(EndpointConstants.WarehouseOperations)
            .WithDescription("Get all warehouses with pagination")
            .RequireAuthorization(Permissions.Warehouse.Read)
            .Produces<PaginatedResult<WarehouseViewModel>>(200, "application/json")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        // Adres ve Harita URL'si güncelleme
        app.MapPatch("/warehouse/{id}/address", async (
            Guid id,
            [FromBody] WarehouseAddressUpdateModel model,
            [FromServices]Context context,
            [FromServices]SessionModel session) =>
        {
            var warehouse = await context.Warehouses
                .FirstOrDefaultAsync(w => w.Id == id && w.TenantId == session.TenantId);

            if (warehouse == null)
                return Results.NotFound("Warehouse.NotFound");

            warehouse.Address = model.Address;
            warehouse.MapUrl = model.MapUrl;
            warehouse.ModifiedAt = DateTime.UtcNow;
            warehouse.ModifiedBy = Guid.Parse(session.UserId);

            await context.SaveChangesAsync();
            return Results.Ok();
        })
        .WithTags(EndpointConstants.WarehouseOperations)
        .WithDescription("Update warehouse address and map URL")
        .RequireAuthorization(Permissions.Warehouse.Update)
        .Produces(200)
        .Produces(404);

        // Şehir ve Ülke güncelleme
        app.MapPatch("/warehouse/{id}/location", async (
            Guid id,
            [FromBody] WarehouseLocationUpdateModel model,
            [FromServices]Context context,
            [FromServices]SessionModel session) =>
        {
            var warehouse = await context.Warehouses
                .FirstOrDefaultAsync(w => w.Id == id && w.TenantId == session.TenantId);

            if (warehouse == null)
                return Results.NotFound("Warehouse.NotFound");

            // City ve Country uyumluluğunu kontrol et
            var city = await context.Cities
                .FirstOrDefaultAsync(c => c.Id == model.CityId);

            if (city == null)
                return Results.BadRequest("City.NotFound");

            if (city.CountryId != model.CountryId)
                return Results.BadRequest("City.Country.Mismatch");

            warehouse.CityId = model.CityId;
            warehouse.CountryId = model.CountryId;
            warehouse.ModifiedAt = DateTime.UtcNow;
            warehouse.ModifiedBy = Guid.Parse(session.UserId);

            await context.SaveChangesAsync();
            return Results.Ok();
        })
        .WithTags(EndpointConstants.WarehouseOperations)
        .WithDescription("Update warehouse city and country")
        .RequireAuthorization(Permissions.Warehouse.Update)
        .Produces(200)
        .Produces(404)
        .Produces(400);

        // İsim güncelleme
        app.MapPatch("/warehouse/{id}/name", async (
            Guid id,
            [FromBody] WarehouseNameUpdateModel model,
            [FromServices]Context context,
            [FromServices]SessionModel session) =>
        {
            var warehouse = await context.Warehouses
                .FirstOrDefaultAsync(w => w.Id == id && w.TenantId == session.TenantId);

            if (warehouse == null)
                return Results.NotFound("Warehouse.NotFound");

            warehouse.Name = model.Name;
            warehouse.ModifiedAt = DateTime.UtcNow;
            warehouse.ModifiedBy = Guid.Parse(session.UserId);

            await context.SaveChangesAsync();
            return Results.Ok();
        })
        .WithTags(EndpointConstants.WarehouseOperations)
        .WithDescription("Update warehouse name")
        .RequireAuthorization(Permissions.Warehouse.Update)
        .Produces(200)
        .Produces(404);

        // Status güncelleme
        app.MapPatch("/warehouse/{id}/status", async (
            Guid id,
            [FromBody] WarehouseStatusUpdateModel model,
            [FromServices]Context context,
            [FromServices]SessionModel session) =>
        {
            var warehouse = await context.Warehouses
                .FirstOrDefaultAsync(w => w.Id == id && w.TenantId == session.TenantId);

            if (warehouse == null)
                return Results.NotFound("Warehouse.NotFound");

            warehouse.Status = model.Status;
            warehouse.ModifiedAt = DateTime.UtcNow;
            warehouse.ModifiedBy = Guid.Parse(session.UserId);

            await context.SaveChangesAsync();
            return Results.Ok();
        })
        .WithTags(EndpointConstants.WarehouseOperations)
        .WithDescription("Update warehouse status")
        .RequireAuthorization(Permissions.Warehouse.Update)
        .Produces(200)
        .Produces(404);

        // Sorumlu kişi güncelleme
        app.MapPatch("/warehouse/{id}/person-in-charge", async (
            Guid id,
            [FromBody] WarehousePersonInChargeUpdateModel model,
            [FromServices]Context context,
            [FromServices]SessionModel session) =>
        {
            var warehouse = await context.Warehouses
                .FirstOrDefaultAsync(w => w.Id == id && w.TenantId == session.TenantId);

            if (warehouse == null)
                return Results.NotFound("Warehouse.NotFound");

            // Kullanıcı kontrolü
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == model.PersonInChargeId && u.TenantId == session.TenantId);

            if (user == null)
                return Results.BadRequest("PersonInCharge.NotFound");

            warehouse.PersonInChargeId = model.PersonInChargeId;
            warehouse.ModifiedAt = DateTime.UtcNow;
            warehouse.ModifiedBy = Guid.Parse(session.UserId);

            await context.SaveChangesAsync();
            return Results.Ok();
        })
        .WithTags(EndpointConstants.WarehouseOperations)
        .WithDescription("Update warehouse person in charge")
        .RequireAuthorization(Permissions.Warehouse.Update)
        .Produces(200)
        .Produces(404)
        .Produces(400);
    }
}

public record WarehouseCreateModel(string Name, Guid PersonInChargeId, Guid City, Guid Country, string? MapUrl, string Address, Guid CategoryId);
public record WarehouseViewModel(Guid Id, string Name, string City, string Country, DateTime LastActivity, string CategoryName);

// Update modelleri
public record WarehouseAddressUpdateModel(string Address, string? MapUrl);
public record WarehouseLocationUpdateModel(Guid CityId, Guid CountryId);
public record WarehouseNameUpdateModel(string Name);
public record WarehouseStatusUpdateModel(WarehouseStatus Status);
public record WarehousePersonInChargeUpdateModel(Guid PersonInChargeId);

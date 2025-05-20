namespace AspireLearning.Api.Endpoints;

using Data.Context;
using Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceDefaults.GlobalConstant;
using ServiceDefaults.GlobalModel.Session;
using ServiceDefaults.GlobalUtility;

public static class WarehouseCategoryEndpoints
{
    public static void MapWarehouseCategoryEndpoints(this WebApplication app)
    {
        app.MapPost("/warehouse-category", async (
            [FromBody] WarehouseCategoryCreateModel model,
            [FromServices] Context context,
            [FromServices] SessionModel session) =>
        {
            var category = new WarehouseCategory
            {
                Name = model.Name,
                TenantId = session.TenantId
            };

            context.WarehouseCategories.Add(category);
            await context.SaveChangesAsync();

            return Results.Created($"/warehouse-category", null);
        })
        .WithTags("WarehouseCategoryOperations")
        .WithDescription("Create a new warehouse category")
        .RequireAuthorization(Permissions.Warehouse.Add)
        .Produces(StatusCodes.Status201Created);

        app.MapGet("/warehouse-category", async (
            [FromServices] Context context,
            [FromServices] SessionModel session,
            [FromQuery] string? searchName,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDirection,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10) =>
        {
            var categories = context.WarehouseCategories
                .Where(wc => wc.TenantId == session.TenantId);

            if (!string.IsNullOrEmpty(searchName))
                categories = categories.Where(wc => wc.Name.Contains(searchName));

            categories = sortBy?.ToLower() switch
            {
                "name" when sortDirection == "desc" => categories.OrderByDescending(wc => wc.Name),
                "name" => categories.OrderBy(wc => wc.Name),
                _ => categories.OrderBy(wc => wc.Name)
            };

            var totalCount = await categories.CountAsync();
            var paginatedQuery = categories.Skip(((pageNumber - 1) * pageSize)!).Take(pageSize);

            var queryResult = await paginatedQuery.Select(x => new WarehouseCategoryViewModel
            (
                x.Id,
                x.Name
            )).ToListAsync();

            var result = new PaginatedResult<WarehouseCategoryViewModel>
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = queryResult
            };

            return Results.Ok(result);
        })
        .WithTags("WarehouseCategoryOperations")
        .WithDescription("Get all warehouse categories with pagination")
        .RequireAuthorization(Permissions.Warehouse.Read)
        .Produces<PaginatedResult<WarehouseCategoryViewModel>>(200, "application/json");

        app.MapPatch("/warehouse-category/{id}/name", async (
            Guid id,
            [FromBody] WarehouseCategoryNameUpdateModel model,
            [FromServices] Context context,
            [FromServices] SessionModel session) =>
        {
            var category = await context.WarehouseCategories
                .FirstOrDefaultAsync(wc => wc.Id == id && wc.TenantId == session.TenantId);

            if (category == null)
                return Results.NotFound("WarehouseCategory.NotFound");

            category.Name = model.Name;
            category.ModifiedAt = DateTime.UtcNow;
            category.ModifiedBy = Guid.Parse(session.UserId);

            await context.SaveChangesAsync();
            return Results.Ok();
        })
        .WithTags("WarehouseCategoryOperations")
        .WithDescription("Update warehouse category name")
        .RequireAuthorization(Permissions.Warehouse.Update)
        .Produces(200)
        .Produces(404);
    }
}

public record WarehouseCategoryCreateModel(string Name);
public record WarehouseCategoryViewModel(Guid Id, string Name);
public record WarehouseCategoryNameUpdateModel(string Name); 
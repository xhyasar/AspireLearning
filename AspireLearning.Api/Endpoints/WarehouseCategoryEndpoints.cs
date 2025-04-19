namespace AspireLearning.Api.Endpoints;

using Data.Context;
using Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        .Produces(StatusCodes.Status201Created);

        app.MapGet("/warehouse-category", async (
            [FromQuery] WarehouseCategoryQueryFilterModel query,
            [FromServices] Context context,
            [FromServices] SessionModel session) =>
        {
            var categories = context.WarehouseCategories
                .Where(wc => wc.TenantId == session.TenantId);

            if (!string.IsNullOrEmpty(query.SearchName))
                categories = categories.Where(wc => wc.Name.Contains(query.SearchName));

            categories = query.SortBy?.ToLower() switch
            {
                "name" when query.SortDirection == "desc" => categories.OrderByDescending(wc => wc.Name),
                "name" => categories.OrderBy(wc => wc.Name),
                _ => categories.OrderBy(wc => wc.Name)
            };

            var totalCount = await categories.CountAsync();
            var paginatedQuery = categories.Skip((int)((query.PageNumber - 1) * query.PageSize)!).Take((int)query.PageSize!);

            var queryResult = await paginatedQuery.Select(x => new WarehouseCategoryViewModel
            (
                x.Id,
                x.Name
            )).ToListAsync();

            var result = new PaginatedResult<WarehouseCategoryViewModel>
            {
                TotalCount = totalCount,
                PageNumber = (int)query.PageNumber!,
                PageSize = (int)query.PageSize!,
                Data = queryResult
            };

            return Results.Ok(result);
        })
        .WithTags("WarehouseCategoryOperations")
        .WithDescription("Get all warehouse categories with pagination")
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
        .Produces(200)
        .Produces(404);
    }
}

public record WarehouseCategoryCreateModel(string Name);
public record WarehouseCategoryViewModel(Guid Id, string Name);
public record WarehouseCategoryQueryFilterModel(string? SearchName, string? SortBy, string? SortDirection, int? PageNumber, int? PageSize) : IParsable<WarehouseCategoryQueryFilterModel>
{
    public static WarehouseCategoryQueryFilterModel Parse(string s, IFormatProvider? provider) => new(null, null, null, null, null);
    public static bool TryParse(string? s, IFormatProvider? provider, out WarehouseCategoryQueryFilterModel result)
    {
        result = new(null, null, null, null, null);
        return true;
    }
}
public record WarehouseCategoryNameUpdateModel(string Name); 
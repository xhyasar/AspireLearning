namespace AspireLearning.Api.Endpoints;

using Data.Context;
using Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceDefaults.GlobalModel.Session;
using ServiceDefaults.GlobalUtility;

public static class ProductStockEndpoints
{
    public static void MapProductStockEndpoints(this WebApplication app)
    {
        app.MapPost("/product-stock", async (
            [FromBody] ProductStockCreateModel model,
            [FromServices] Context context,
            [FromServices] SessionModel session) =>
        {
            var product = await context.Products
                .FirstOrDefaultAsync(p => p.Id == model.ProductId && p.TenantId == session.TenantId);

            if (product == null)
                return Results.BadRequest("Product.NotFound");

            var warehouse = await context.Warehouses
                .FirstOrDefaultAsync(w => w.Id == model.WarehouseId && w.TenantId == session.TenantId);

            if (warehouse == null)
                return Results.BadRequest("Warehouse.NotFound");

            var stock = new ProductStock
            {
                ProductId = model.ProductId,
                WarehouseId = model.WarehouseId,
                Quantity = model.Quantity,
                MinimumQuantity = model.MinimumQuantity,
                MaximumQuantity = model.MaximumQuantity
            };

            context.ProductStocks.Add(stock);
            await context.SaveChangesAsync();

            return Results.Created($"/product-stock", null);
        })
        .WithTags("ProductStockOperations")
        .WithDescription("Create a new product stock")
        .Produces(StatusCodes.Status201Created);

        app.MapGet("/product-stock", async (
            [FromQuery] ProductStockQueryFilterModel query,
            [FromServices] Context context,
            [FromServices] SessionModel session) =>
        {
            var stocks = context.ProductStocks
                .Include(ps => ps.Product)
                .Include(ps => ps.Warehouse)
                .Where(ps => ps.Product.TenantId == session.TenantId);

            if (query.ProductId.HasValue)
                stocks = stocks.Where(ps => ps.ProductId == query.ProductId);

            if (query.WarehouseId.HasValue)
                stocks = stocks.Where(ps => ps.WarehouseId == query.WarehouseId);

            if (query.MinQuantity.HasValue)
                stocks = stocks.Where(ps => ps.Quantity <= query.MinQuantity);

            if (query.MaxQuantity.HasValue)
                stocks = stocks.Where(ps => ps.Quantity >= query.MaxQuantity);

            stocks = query.SortBy?.ToLower() switch
            {
                "quantity" when query.SortDirection == "desc" => stocks.OrderByDescending(ps => ps.Quantity),
                "quantity" => stocks.OrderBy(ps => ps.Quantity),
                "product" when query.SortDirection == "desc" => stocks.OrderByDescending(ps => ps.Product.Name),
                "product" => stocks.OrderBy(ps => ps.Product.Name),
                "warehouse" when query.SortDirection == "desc" => stocks.OrderByDescending(ps => ps.Warehouse.Name),
                "warehouse" => stocks.OrderBy(ps => ps.Warehouse.Name),
                _ => stocks.OrderBy(ps => ps.Product.Name)
            };

            var totalCount = await stocks.CountAsync();
            var paginatedQuery = stocks.Skip((int)((query.PageNumber - 1) * query.PageSize)!).Take((int)query.PageSize!);

            var queryResult = await paginatedQuery.Select(x => new ProductStockViewModel
            (
                x.Id,
                x.Product.Name,
                x.Warehouse.Name,
                x.Quantity,
                x.MinimumQuantity,
                x.MaximumQuantity
            )).ToListAsync();

            var result = new PaginatedResult<ProductStockViewModel>
            {
                TotalCount = totalCount,
                PageNumber = (int)query.PageNumber!,
                PageSize = (int)query.PageSize!,
                Data = queryResult
            };

            return Results.Ok(result);
        })
        .WithTags("ProductStockOperations")
        .WithDescription("Get all product stocks with pagination")
        .Produces<PaginatedResult<ProductStockViewModel>>(200, "application/json");

        app.MapPatch("/product-stock/{id}", async (
            Guid id,
            [FromBody] ProductStockUpdateModel model,
            [FromServices] Context context,
            [FromServices] SessionModel session) =>
        {
            var stock = await context.ProductStocks
                .Include(ps => ps.Product)
                .FirstOrDefaultAsync(ps => ps.Id == id && ps.Product.TenantId == session.TenantId);

            if (stock == null)
                return Results.NotFound("ProductStock.NotFound");

            stock.Quantity = model.Quantity ?? stock.Quantity;
            stock.MinimumQuantity = model.MinimumQuantity ?? stock.MinimumQuantity;
            stock.MaximumQuantity = model.MaximumQuantity ?? stock.MaximumQuantity;
            stock.ModifiedAt = DateTime.UtcNow;
            stock.ModifiedBy = Guid.Parse(session.UserId);

            await context.SaveChangesAsync();
            return Results.Ok();
        })
        .WithTags("ProductStockOperations")
        .WithDescription("Update product stock")
        .Produces(200)
        .Produces(404);

        app.MapDelete("/product-stock/{id}", async (
            Guid id,
            [FromServices] Context context,
            [FromServices] SessionModel session) =>
        {
            var stock = await context.ProductStocks
                .Include(ps => ps.Product)
                .FirstOrDefaultAsync(ps => ps.Id == id && ps.Product.TenantId == session.TenantId);

            if (stock == null)
                return Results.NotFound("ProductStock.NotFound");

            stock.IsDeleted = true;
            stock.RemovedAt = DateTime.UtcNow;
            stock.RemovedBy = Guid.Parse(session.UserId);

            await context.SaveChangesAsync();
            return Results.Ok();
        })
        .WithTags("ProductStockOperations")
        .WithDescription("Delete product stock")
        .Produces(200)
        .Produces(404);
    }
}

public record ProductStockCreateModel(Guid ProductId, Guid WarehouseId, decimal Quantity, decimal MinimumQuantity, decimal MaximumQuantity);
public record ProductStockViewModel(Guid Id, string ProductName, string WarehouseName, decimal Quantity, decimal MinimumQuantity, decimal MaximumQuantity);
public record ProductStockQueryFilterModel(Guid? ProductId, Guid? WarehouseId, decimal? MinQuantity, decimal? MaxQuantity, string? SortBy, string? SortDirection, int? PageNumber, int? PageSize) : IParsable<ProductStockQueryFilterModel>
{
    public static ProductStockQueryFilterModel Parse(string s, IFormatProvider? provider) => new(null, null, null, null, null, null, null, null);
    public static bool TryParse(string? s, IFormatProvider? provider, out ProductStockQueryFilterModel result)
    {
        result = new(null, null, null, null, null, null, null, null);
        return true;
    }
}
public record ProductStockUpdateModel(decimal? Quantity, decimal? MinimumQuantity, decimal? MaximumQuantity); 
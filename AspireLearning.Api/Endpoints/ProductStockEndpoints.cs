namespace AspireLearning.Api.Endpoints;

using Data.Context;
using Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceDefaults.GlobalConstant;
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
        .WithTags(EndpointConstants.ProductStockOperations)
        .WithDescription("Create a new product stock")
        .RequireAuthorization(Permissions.Stock.Add)
        .Produces(StatusCodes.Status201Created);

        app.MapGet("/product-stock", async (
            [FromServices] Context context,
            [FromServices] SessionModel session,
            [FromQuery] Guid? productId,
            [FromQuery] Guid? warehouseId,
            [FromQuery] decimal? minQuantity,
            [FromQuery] decimal? maxQuantity,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDirection,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10) =>
        {
            var stocks = context.ProductStocks
                .Include(ps => ps.Product)
                .Include(ps => ps.Warehouse)
                .Where(ps => ps.Product!.TenantId == session.TenantId);

            if (productId.HasValue)
                stocks = stocks.Where(ps => ps.ProductId == productId);

            if (warehouseId.HasValue)
                stocks = stocks.Where(ps => ps.WarehouseId == warehouseId);

            if (minQuantity.HasValue)
                stocks = stocks.Where(ps => ps.Quantity <= minQuantity);

            if (maxQuantity.HasValue)
                stocks = stocks.Where(ps => ps.Quantity >= maxQuantity);

            stocks = sortBy?.ToLower() switch
            {
                "quantity" when sortDirection == "desc" => stocks.OrderByDescending(ps => ps.Quantity),
                "quantity" => stocks.OrderBy(ps => ps.Quantity),
                "product" when sortDirection == "desc" => stocks.OrderByDescending(ps => ps.Product != null ? ps.Product.Name : string.Empty),
                "product" => stocks.OrderBy(ps => ps.Product != null ? ps.Product.Name : string.Empty),
                "warehouse" when sortDirection == "desc" => stocks.OrderByDescending(ps => ps.Warehouse != null ? ps.Warehouse.Name : string.Empty),
                "warehouse" => stocks.OrderBy(ps => ps.Warehouse != null ? ps.Warehouse.Name : string.Empty),
                _ => stocks.OrderBy(ps => ps.Product != null ? ps.Product.Name : string.Empty)
            };

            var totalCount = await stocks.CountAsync();
            var paginatedQuery = stocks.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            var queryResult = await paginatedQuery.Select(x => new ProductStockViewModel
            (
                x.Id,
                x.Product != null ? x.Product.Name : string.Empty,
                x.Warehouse != null ? x.Warehouse.Name : string.Empty,
                x.Quantity,
                x.MinimumQuantity,
                x.MaximumQuantity
            )).ToListAsync();

            var result = new PaginatedResult<ProductStockViewModel>
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = queryResult
            };

            return Results.Ok(result);
        })
        .WithTags(EndpointConstants.ProductStockOperations)
        .WithDescription("Get all product stocks with pagination")
        .RequireAuthorization(Permissions.Stock.Read)
        .Produces<PaginatedResult<ProductStockViewModel>>(200, "application/json");

        app.MapPatch("/product-stock/{id}", async (
            Guid id,
            [FromBody] ProductStockUpdateModel model,
            [FromServices] Context context,
            [FromServices] SessionModel session) =>
        {
            var stock = await context.ProductStocks
                .Include(ps => ps.Product)
                .FirstOrDefaultAsync(ps => ps.Id == id && ps.Product != null && ps.Product.TenantId == session.TenantId);

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
        .WithTags(EndpointConstants.ProductStockOperations)
        .WithDescription("Update product stock")
        .RequireAuthorization(Permissions.Stock.Update)
        .Produces(200)
        .Produces(404);

        app.MapDelete("/product-stock/{id}", async (
            Guid id,
            [FromServices] Context context,
            [FromServices] SessionModel session) =>
        {
            var stock = await context.ProductStocks
                .Include(ps => ps.Product)
                .FirstOrDefaultAsync(ps => ps.Id == id && ps.Product != null && ps.Product.TenantId == session.TenantId);

            if (stock == null)
                return Results.NotFound("ProductStock.NotFound");

            stock.IsDeleted = true;
            stock.RemovedAt = DateTime.UtcNow;
            stock.RemovedBy = Guid.Parse(session.UserId);

            await context.SaveChangesAsync();
            return Results.Ok();
        })
        .WithTags(EndpointConstants.ProductStockOperations)
        .WithDescription("Delete product stock")
        .RequireAuthorization(Permissions.Stock.Delete)
        .Produces(200)
        .Produces(404);
    }
}

public record ProductStockCreateModel(Guid ProductId, Guid WarehouseId, decimal Quantity, decimal MinimumQuantity, decimal MaximumQuantity);
public record ProductStockViewModel(Guid Id, string ProductName, string WarehouseName, decimal Quantity, decimal MinimumQuantity, decimal MaximumQuantity);
public record ProductStockUpdateModel(decimal? Quantity, decimal? MinimumQuantity, decimal? MaximumQuantity); 
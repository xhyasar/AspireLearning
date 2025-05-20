using AspireLearning.ServiceDefaults.GlobalConstant;

namespace AspireLearning.Api.Endpoints;

using Data.Context;
using Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceDefaults.GlobalModel.Session;
using ServiceDefaults.GlobalUtility;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        app.MapPost("/product",
            async (
            [FromBody] ProductCreateModel model,
            [FromServices] Context context,
            [FromServices] SessionModel session) =>
        {
            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Barcode = model.Barcode,
                SKU = model.SKU,
                UnitPrice = model.UnitPrice,
                Unit = model.Unit,
                TenantId = session.TenantId
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            return Results.Created($"/product", null);
        })
        .WithTags(EndpointConstants.ProductOperations)
        .WithDescription("Create a new product")
        .RequireAuthorization(Permissions.Product.Add)
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        app.MapGet("/product",
            async (
            [FromServices] Context context,
            [FromServices] SessionModel session,
            [FromQuery] string? searchName,
            [FromQuery] string? searchBarcode,
            [FromQuery] string? searchSku,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDirection,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10) =>
        {
            var products = context.Products
                .Where(p => p.TenantId == session.TenantId);

            if (!string.IsNullOrEmpty(searchName))
                products = products.Where(p => p.Name.Contains(searchName));

            if (!string.IsNullOrEmpty(searchBarcode))
                products = products.Where(p => p.Barcode == searchBarcode);

            if (!string.IsNullOrEmpty(searchSku))
                products = products.Where(p => p.SKU == searchSku);

            products = sortBy?.ToLower() switch
            {
                "name" when sortDirection == "desc" => products.OrderByDescending(p => p.Name),
                "name" => products.OrderBy(p => p.Name),
                "price" when sortDirection == "desc" => products.OrderByDescending(p => p.UnitPrice),
                "price" => products.OrderBy(p => p.UnitPrice),
                _ => products.OrderBy(p => p.Name)
            };

            var totalCount = await products.CountAsync();
            var paginatedQuery = products.Skip(((pageNumber - 1) * pageSize)!).Take(pageSize);

            var queryResult = await paginatedQuery.Select(x => new ProductViewModel
            (
                x.Id,
                x.Name,
                x.Description,
                x.Barcode,
                x.SKU,
                x.UnitPrice,
                x.Unit
            )).ToListAsync();

            var result = new PaginatedResult<ProductViewModel>
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = queryResult
            };

            return Results.Ok(result);
        })
        .WithTags(EndpointConstants.ProductOperations)
        .WithDescription("Get all products with pagination")
        .RequireAuthorization(Permissions.Product.Read)
        .Produces<PaginatedResult<ProductViewModel>>(200, "application/json")
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        app.MapPatch("/product/{id:guid}",
            async (
            Guid id,
            [FromBody] ProductUpdateModel model,
            [FromServices] Context context,
            [FromServices] SessionModel session) =>
        {
            var product = await context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == session.TenantId);

            if (product == null)
                return Results.NotFound("Product.NotFound");

            product.Name = model.Name ?? product.Name;
            product.Description = model.Description ?? product.Description;
            product.Barcode = model.Barcode ?? product.Barcode;
            product.SKU = model.SKU ?? product.SKU;
            product.UnitPrice = model.UnitPrice ?? product.UnitPrice;
            product.Unit = model.Unit ?? product.Unit;
            product.ModifiedAt = DateTime.UtcNow;
            product.ModifiedBy = Guid.Parse(session.UserId);

            await context.SaveChangesAsync();
            return Results.Ok();
        })
        .WithTags(EndpointConstants.ProductOperations)
        .WithDescription("Update product")
        .RequireAuthorization(Permissions.Product.Update)
        .Produces(200)
        .Produces(404)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        app.MapDelete("/product/{id:guid}", 
            async (
            Guid id,
            [FromServices] Context context,
            [FromServices] SessionModel session) =>
        {
            var product = await context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == session.TenantId);

            if (product == null)
                return Results.NotFound("Product.NotFound");

            product.IsDeleted = true;
            product.RemovedAt = DateTime.UtcNow;
            product.RemovedBy = Guid.Parse(session.UserId);

            await context.SaveChangesAsync();
            return Results.Ok();
        })
        .WithTags(EndpointConstants.ProductOperations)
        .WithDescription("Delete product")
        .RequireAuthorization(Permissions.Product.Delete)
        .Produces(200)
        .Produces(404)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}

public record ProductCreateModel(string Name, string? Description, string? Barcode, string? SKU, decimal UnitPrice, string? Unit);
public record ProductViewModel(Guid Id, string Name, string? Description, string? Barcode, string? SKU, decimal UnitPrice, string? Unit);
public record ProductUpdateModel(string? Name, string? Description, string? Barcode, string? SKU, decimal? UnitPrice, string? Unit); 
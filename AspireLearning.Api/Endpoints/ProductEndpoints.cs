using AspireLearning.ServiceDefaults.GlobalConstant;
using Microsoft.AspNetCore.Authorization;

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
        app.MapPost("/product", [Authorize(Policy = Permissions.Product.Add)]
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
        .WithTags("ProductOperations")
        .WithDescription("Create a new product")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        app.MapGet("/product", [Authorize(Policy = Permissions.Product.Read)]
            async (
            [FromQuery] ProductQueryFilterModel query,
            [FromServices] Context context,
            [FromServices] SessionModel session) =>
        {
            var products = context.Products
                .Where(p => p.TenantId == session.TenantId);

            if (!string.IsNullOrEmpty(query.SearchName))
                products = products.Where(p => p.Name.Contains(query.SearchName));

            if (!string.IsNullOrEmpty(query.SearchBarcode))
                products = products.Where(p => p.Barcode == query.SearchBarcode);

            if (!string.IsNullOrEmpty(query.SearchSKU))
                products = products.Where(p => p.SKU == query.SearchSKU);

            products = query.SortBy?.ToLower() switch
            {
                "name" when query.SortDirection == "desc" => products.OrderByDescending(p => p.Name),
                "name" => products.OrderBy(p => p.Name),
                "price" when query.SortDirection == "desc" => products.OrderByDescending(p => p.UnitPrice),
                "price" => products.OrderBy(p => p.UnitPrice),
                _ => products.OrderBy(p => p.Name)
            };

            var totalCount = await products.CountAsync();
            var paginatedQuery = products.Skip((int)((query.PageNumber - 1) * query.PageSize)!).Take((int)query.PageSize!);

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
                PageNumber = (int)query.PageNumber!,
                PageSize = (int)query.PageSize!,
                Data = queryResult
            };

            return Results.Ok(result);
        })
        .WithTags("ProductOperations")
        .WithDescription("Get all products with pagination")
        .Produces<PaginatedResult<ProductViewModel>>(200, "application/json")
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        app.MapPatch("/product/{id}", [Authorize(Policy = Permissions.Product.Update)]
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
        .WithTags("ProductOperations")
        .WithDescription("Update product")
        .Produces(200)
        .Produces(404)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        app.MapDelete("/product/{id}", [Authorize(Policy = Permissions.Product.Delete)]
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
        .WithTags("ProductOperations")
        .WithDescription("Delete product")
        .Produces(200)
        .Produces(404)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}

public record ProductCreateModel(string Name, string? Description, string? Barcode, string? SKU, decimal UnitPrice, string? Unit);
public record ProductViewModel(Guid Id, string Name, string? Description, string? Barcode, string? SKU, decimal UnitPrice, string? Unit);
public record ProductQueryFilterModel(string? SearchName, string? SearchBarcode, string? SearchSKU, string? SortBy, string? SortDirection, int? PageNumber, int? PageSize) : IParsable<ProductQueryFilterModel>
{
    public static ProductQueryFilterModel Parse(string s, IFormatProvider? provider) => new(null, null, null, null, null, null, null);
    public static bool TryParse(string? s, IFormatProvider? provider, out ProductQueryFilterModel result)
    {
        result = new(null, null, null, null, null, null, null);
        return true;
    }
}
public record ProductUpdateModel(string? Name, string? Description, string? Barcode, string? SKU, decimal? UnitPrice, string? Unit); 
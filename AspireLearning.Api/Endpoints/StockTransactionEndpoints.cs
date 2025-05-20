namespace AspireLearning.Api.Endpoints;

using Data.Context;
using Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceDefaults.GlobalConstant;
using ServiceDefaults.GlobalModel.Session;
using ServiceDefaults.GlobalUtility;

public static class StockTransactionEndpoints
{
    public static void MapStockTransactionEndpoints(this WebApplication app)
    {
        app.MapPost("/stock-transaction", async (
            [FromBody] StockTransactionCreateModel model,
            [FromServices] Context context,
            [FromServices] SessionModel session) =>
        {
            var productStock = await context.ProductStocks
                .Include(ps => ps.Product)
                .FirstOrDefaultAsync(ps => ps.Id == model.ProductStockId && ps.Product != null && ps.Product.TenantId == session.TenantId);

            if (productStock == null)
                return Results.BadRequest("ProductStock.NotFound");

            var transaction = new StockTransaction
            {
                ProductStockId = model.ProductStockId,
                Type = model.Type,
                Quantity = model.Quantity,
                PreviousQuantity = productStock.Quantity,
                NewQuantity = model.Type switch
                {
                    TransactionType.In => productStock.Quantity + model.Quantity,
                    TransactionType.Out => productStock.Quantity - model.Quantity,
                    TransactionType.Adjust => model.Quantity,
                    _ => productStock.Quantity
                },
                Description = model.Description,
                ReferenceNumber = model.ReferenceNumber,
                CreatedBy = Guid.Parse(session.UserId)
            };

            productStock.Quantity = transaction.NewQuantity;

            context.StockTransactions.Add(transaction);
            await context.SaveChangesAsync();

            return Results.Created($"/stock-transaction/{transaction.Id}", null);
        })
        .WithTags(EndpointConstants.StockTransactionOperations)
        .WithDescription("Create a new stock transaction")
        .RequireAuthorization(Permissions.Stock.Add)
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        app.MapGet("/stock-transaction", async (
            [FromServices] Context context,
            [FromServices] SessionModel session,
            [FromQuery] Guid? productStockId,
            [FromQuery] TransactionType? type,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDirection,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10) =>
        {
            var transactions = context.StockTransactions
                .Include(st => st.ProductStock)
                .ThenInclude(ps => ps!.Product)
                .Where(st => st.ProductStock != null && st.ProductStock.Product != null && st.ProductStock.Product.TenantId == session.TenantId);

            if (productStockId.HasValue)
                transactions = transactions.Where(st => st.ProductStockId == productStockId);

            if (type.HasValue)
                transactions = transactions.Where(st => st.Type == type);

            if (startDate.HasValue)
                transactions = transactions.Where(st => st.CreatedAt >= startDate);

            if (endDate.HasValue)
                transactions = transactions.Where(st => st.CreatedAt <= endDate);

            transactions = sortBy?.ToLower() switch
            {
                "date" when sortDirection == "desc" => transactions.OrderByDescending(st => st.CreatedAt),
                "date" => transactions.OrderBy(st => st.CreatedAt),
                "quantity" when sortDirection == "desc" => transactions.OrderByDescending(st => st.Quantity),
                "quantity" => transactions.OrderBy(st => st.Quantity),
                _ => transactions.OrderByDescending(st => st.CreatedAt)
            };

            var totalCount = await transactions.CountAsync();
            var paginatedQuery = transactions.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            var queryResult = await paginatedQuery.Select(x => new StockTransactionViewModel
            (
                x.Id,
                x.ProductStock != null && x.ProductStock.Product != null ? x.ProductStock.Product.Name : "Unknown Product",
                x.Type,
                x.Quantity,
                x.PreviousQuantity,
                x.NewQuantity,
                x.Description,
                x.ReferenceNumber,
                x.CreatedAt
            )).ToListAsync();

            var result = new PaginatedResult<StockTransactionViewModel>
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = queryResult
            };

            return Results.Ok(result);
        })
        .WithTags(EndpointConstants.StockTransactionOperations)
        .WithDescription("Get all stock transactions with pagination")
        .RequireAuthorization(Permissions.Stock.Read)
        .Produces<PaginatedResult<StockTransactionViewModel>>(200, "application/json");

        app.MapGet("/stock-transaction/{id}", async (
            Guid id,
            [FromServices] Context context,
            [FromServices] SessionModel session) =>
        {
            var transaction = await context.StockTransactions
                .Include(st => st.ProductStock)
                .ThenInclude(ps => ps!.Product)
                .FirstOrDefaultAsync(st => st.Id == id && st.ProductStock != null && st.ProductStock.Product != null && st.ProductStock.Product.TenantId == session.TenantId);

            if (transaction == null)
                return Results.NotFound("StockTransaction.NotFound");

            var result = new StockTransactionViewModel
            (
                transaction.Id,
                transaction.ProductStock != null && transaction.ProductStock.Product != null ? transaction.ProductStock.Product.Name : "Unknown Product",
                transaction.Type,
                transaction.Quantity,
                transaction.PreviousQuantity,
                transaction.NewQuantity,
                transaction.Description,
                transaction.ReferenceNumber,
                transaction.CreatedAt
            );

            return Results.Ok(result);
        })
        .WithTags(EndpointConstants.StockTransactionOperations)
        .WithDescription("Get stock transaction by id")
        .RequireAuthorization(Permissions.Stock.Read)
        .Produces<StockTransactionViewModel>(200, "application/json")
        .Produces(404);
    }
}

public record StockTransactionCreateModel(
    Guid ProductStockId,
    TransactionType Type,
    decimal Quantity,
    string? Description,
    string? ReferenceNumber);

public record StockTransactionViewModel(
    Guid Id,
    string ProductName,
    TransactionType Type,
    decimal Quantity,
    decimal PreviousQuantity,
    decimal NewQuantity,
    string? Description,
    string? ReferenceNumber,
    DateTime CreatedAt); 
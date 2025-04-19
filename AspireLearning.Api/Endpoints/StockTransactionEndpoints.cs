namespace AspireLearning.Api.Endpoints;

using Data.Context;
using Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                .FirstOrDefaultAsync(ps => ps.Id == model.ProductStockId && ps.Product.TenantId == session.TenantId);

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
        .WithDescription("Create a new stock transaction")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        app.MapGet("/stock-transaction", async (
            [FromQuery] StockTransactionQueryFilterModel query,
            [FromServices] Context context,
            [FromServices] SessionModel session) =>
        {
            var transactions = context.StockTransactions
                .Include(st => st.ProductStock)
                .ThenInclude(ps => ps.Product)
                .Where(st => st.ProductStock.Product.TenantId == session.TenantId);

            if (query.ProductStockId.HasValue)
                transactions = transactions.Where(st => st.ProductStockId == query.ProductStockId);

            if (query.Type.HasValue)
                transactions = transactions.Where(st => st.Type == query.Type);

            if (query.StartDate.HasValue)
                transactions = transactions.Where(st => st.CreatedAt >= query.StartDate);

            if (query.EndDate.HasValue)
                transactions = transactions.Where(st => st.CreatedAt <= query.EndDate);

            transactions = query.SortBy?.ToLower() switch
            {
                "date" when query.SortDirection == "desc" => transactions.OrderByDescending(st => st.CreatedAt),
                "date" => transactions.OrderBy(st => st.CreatedAt),
                "quantity" when query.SortDirection == "desc" => transactions.OrderByDescending(st => st.Quantity),
                "quantity" => transactions.OrderBy(st => st.Quantity),
                _ => transactions.OrderByDescending(st => st.CreatedAt)
            };

            var totalCount = await transactions.CountAsync();
            var paginatedQuery = transactions.Skip((int)((query.PageNumber - 1) * query.PageSize)!).Take((int)query.PageSize!);

            var queryResult = await paginatedQuery.Select(x => new StockTransactionViewModel
            (
                x.Id,
                x.ProductStock.Product.Name,
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
                PageNumber = (int)query.PageNumber!,
                PageSize = (int)query.PageSize!,
                Data = queryResult
            };

            return Results.Ok(result);
        })
        .WithDescription("Get all stock transactions with pagination")
        .Produces<PaginatedResult<StockTransactionViewModel>>(200, "application/json");

        app.MapGet("/stock-transaction/{id}", async (
            Guid id,
            [FromServices] Context context,
            [FromServices] SessionModel session) =>
        {
            var transaction = await context.StockTransactions
                .Include(st => st.ProductStock)
                .ThenInclude(ps => ps.Product)
                .FirstOrDefaultAsync(st => st.Id == id && st.ProductStock.Product.TenantId == session.TenantId);

            if (transaction == null)
                return Results.NotFound("StockTransaction.NotFound");

            var result = new StockTransactionViewModel
            (
                transaction.Id,
                transaction.ProductStock.Product.Name,
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
        .WithDescription("Get stock transaction by id")
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

public record StockTransactionQueryFilterModel(
    Guid? ProductStockId,
    TransactionType? Type,
    DateTime? StartDate,
    DateTime? EndDate,
    string? SortBy,
    string? SortDirection,
    int? PageNumber,
    int? PageSize) : IParsable<StockTransactionQueryFilterModel>
{
    public static StockTransactionQueryFilterModel Parse(string s, IFormatProvider? provider) => new(null, null, null, null, null, null, null, null);
    public static bool TryParse(string? s, IFormatProvider? provider, out StockTransactionQueryFilterModel result)
    {
        result = new(null, null, null, null, null, null, null, null);
        return true;
    }
} 
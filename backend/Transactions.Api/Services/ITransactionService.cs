using Transactions.Api.Dtos;

namespace Transactions.Api.Services;

public interface ITransactionService
{
    Task<PagedResult<TransactionResponse>> GetPagedAsync(TransactionQueryParameters query);
    Task<ProductHistoryResponse> GetProductHistoryAsync(int productId, TransactionQueryParameters query);
    Task<TransactionResponse> GetByIdAsync(int id);
    Task<TransactionResponse> CreateAsync(CreateTransactionRequest request);
    Task<TransactionResponse> UpdateAsync(int id, UpdateTransactionRequest request);
    Task DeleteAsync(int id);
}

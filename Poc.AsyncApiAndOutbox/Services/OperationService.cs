using System.Text.Json;
using Poc.AsyncApiAndOutbox.Outbox;

namespace Poc.AsyncApiAndOutbox.Services;

public sealed class OperationService
{
    private readonly OutboxService outboxService;

    public OperationService(OutboxService outboxService) => this.outboxService = outboxService;

    public async Task<OperationRequest<T>> SaveOperationAsync<T>(T request) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(request);

        OperationRequest<T> operationRequest = new()
        {
            ClientRequest = request
        };

        OutboxMessage outboxMessage = new(operationRequest.RequestId, JsonSerializer.Serialize(operationRequest));
        await this.outboxService.SaveAsync(outboxMessage).ConfigureAwait(false);

        return operationRequest;
    }

    public async Task<OperationRequest<T>?> GetOperationAsync<T>(Guid requestId) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(requestId);

        OutboxMessage? outboxMessage = await this.outboxService.GetByTransactionIdAsync(requestId).ConfigureAwait(false);
        return outboxMessage switch
        {
            null => null,
            _ => CreateOperationRequest<T>(outboxMessage),
        };
    }

    private static OperationRequest<T> CreateOperationRequest<T>(OutboxMessage outboxMessage) where T : notnull
    {
        OperationRequest<T> operationRequest = JsonSerializer.Deserialize<OperationRequest<T>>(outboxMessage.Data)!;
        operationRequest.RequestStatus = (OperationRequestStatus)outboxMessage.State;

        return operationRequest;
    }
}

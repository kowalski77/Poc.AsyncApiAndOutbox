using System.Text.Json;
using Microsoft.Extensions.Options;
using Poc.AsyncApiAndOutbox.Outbox;

namespace Poc.AsyncApiAndOutbox.Services;

public sealed class OperationRequestService
{
    private readonly OutboxService outboxService;
    private readonly IOptionsMonitor<OperationRequestOptions> options;
    private readonly ITimeProvider timeProvider;

    public OperationRequestService(OutboxService outboxService, IOptionsMonitor<OperationRequestOptions> options, ITimeProvider timeProvider)
    {
        this.outboxService = outboxService;
        this.options = options;
        this.timeProvider = timeProvider;
    }

    public async Task<OperationRequest<T>> SaveOperationAsync<T>(T request) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(request);

        OperationRequest<T> operationRequest = new()
        {
            ClientRequest = request,
            EstimatedCompletionTime = this.timeProvider.GetUtcNowOffset().AddMinutes(this.options.CurrentValue.CompletionTimeInMinutes),
        };

        OutboxMessage outboxMessage = new(
            operationRequest.RequestId, 
            this.timeProvider.GetUtcNow(),
            JsonSerializer.Serialize(operationRequest));

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

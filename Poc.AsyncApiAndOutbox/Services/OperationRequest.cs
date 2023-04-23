namespace Poc.AsyncApiAndOutbox.Services;

public sealed class OperationRequest<T> where T : notnull
{
    public Guid RequestId { get; set; } = Guid.NewGuid();

    public T ClientRequest { get; set; } = default!;

    public DateTimeOffset EstimatedCompletionTime { get; set; } 

    public OperationRequestStatus RequestStatus { get; set; }
}

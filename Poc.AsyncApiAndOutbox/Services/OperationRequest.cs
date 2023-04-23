namespace Poc.AsyncApiAndOutbox.Services;

public sealed class OperationRequest<T> where T : notnull
{
    public Guid RequestId { get; set; } = Guid.NewGuid();

    public T ClientRequest { get; set; } = default!;

    public DateTimeOffset EstimatedCompletionTime { get; set; } = DateTime.UtcNow.AddMinutes(20);

    public OperationRequestStatus RequestStatus { get; set; }
}

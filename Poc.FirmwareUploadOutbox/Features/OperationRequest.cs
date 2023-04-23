namespace Poc.FirmwareUploadOutbox.Features;

public sealed class OperationRequest
{
    public Guid RequestId { get; set; } = Guid.NewGuid();

    public ClientRequest ClientRequest { get; set; } = default!;

    public DateTimeOffset EstimatedCompletionTime { get; set; } = DateTime.UtcNow.AddMinutes(20);

    public RequestStatus RequestStatus { get; set; }
}

public enum RequestStatus
{
    Accepted = 0,
    Completed = 1,
    Failed = 2
}

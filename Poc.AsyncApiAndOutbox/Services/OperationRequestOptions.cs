using System.ComponentModel.DataAnnotations;

namespace Poc.AsyncApiAndOutbox.Services;

public class OperationRequestOptions
{
    [Range(1, 60)]
    public int CompletionTimeInMinutes { get; init; }

    [Range(1, 10)]
    public int RetryIntervalInMinutes { get; init; }
}

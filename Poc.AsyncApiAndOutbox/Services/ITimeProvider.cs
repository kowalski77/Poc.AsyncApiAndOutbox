namespace Poc.AsyncApiAndOutbox.Services;

public interface ITimeProvider
{
    DateTimeOffset GetUtcNowOffset();

    DateTime GetUtcNow();
}

public class TimeProvider : ITimeProvider
{
    public DateTimeOffset GetUtcNowOffset() => DateTimeOffset.UtcNow;
    public DateTime GetUtcNow() => DateTime.UtcNow;
}

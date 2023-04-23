namespace Poc.AsyncApiAndOutbox.Outbox;

public enum EventState
{
    Pending = 0,
    Finalized = 1,
    Failed = 2
}

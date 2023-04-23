namespace Poc.AsyncApiAndOutbox.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; private set; }

    public Guid TransactionId { get; set; }

    public DateTime OccurredOn { get; private set; }

    public string Data { get; private set; }

    public EventState State { get; set; }

    public OutboxMessage(
        Guid transactionId,
        string data)
    {
        this.Id = Guid.NewGuid();
        this.TransactionId = transactionId;
        this.OccurredOn = DateTime.UtcNow;
        this.Data = data;
        this.State = EventState.Pending;
    }
}

using Microsoft.EntityFrameworkCore;

namespace Poc.AsyncApiAndOutbox.Outbox;

public sealed class OutboxService
{
    private readonly OutboxContext outboxContext;

    public OutboxService(OutboxContext outboxContext) => this.outboxContext = outboxContext;

    public async Task SaveAsync(OutboxMessage outboxMessage)
    {
        outboxContext.OutboxMessages.Add(outboxMessage);
        await outboxContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task UpdateAsync(OutboxMessage outboxMessage)
    {
        outboxContext.OutboxMessages.Update(outboxMessage);
        await outboxContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<OutboxMessage?> GetByTransactionIdAsync(Guid transactionId) => 
        await outboxContext.OutboxMessages.SingleOrDefaultAsync(x => x.TransactionId == transactionId).ConfigureAwait(false);

    public async Task<IReadOnlyList<OutboxMessage>> GetPendingOutboxMessagesAsync(CancellationToken cancellationToken) =>
        await outboxContext.OutboxMessages
            .Where(x => x.State == EventState.Pending)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
}

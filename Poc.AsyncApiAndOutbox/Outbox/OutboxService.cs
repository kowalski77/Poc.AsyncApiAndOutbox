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

    public async Task<OutboxMessage?> GetByTransactionIdAsync(Guid transactionId) => 
        await outboxContext.OutboxMessages.SingleOrDefaultAsync(x => x.TransactionId == transactionId).ConfigureAwait(false);
}

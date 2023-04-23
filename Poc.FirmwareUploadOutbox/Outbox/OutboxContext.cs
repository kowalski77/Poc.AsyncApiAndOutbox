using Microsoft.EntityFrameworkCore;

namespace Poc.FirmwareUploadOutbox.Outbox;

public class OutboxContext : DbContext
{
    public OutboxContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<OutboxMessage> OutboxMessages { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfiguration(new OutboxMessageEntityTypeConfiguration());
    }
}

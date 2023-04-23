using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Poc.AsyncApiAndOutbox.Features;

namespace Poc.AsyncApiAndOutbox.Outbox;

public class OutboxHostedService : BackgroundService
{
    private readonly ILogger<OutboxHostedService> logger;
    private readonly IServiceProvider serviceProvider;

    public OutboxHostedService(ILogger<OutboxHostedService> logger, IServiceProvider serviceProvider)
    {
        this.logger = logger;
        this.serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.logger.LogInformation("Outbox Hosted Service running.");

        await this.DoWork(stoppingToken).ConfigureAwait(false);
    }

    // TODO: clean up this method
    private async Task DoWork(CancellationToken stoppingToken)
    {
        this.logger.LogInformation("Outbox Hosted Service is working.");

        while (!stoppingToken.IsCancellationRequested)
        {
            this.logger.LogInformation("Outbox Hosted Service is doing background work.");

            using IServiceScope scope = this.serviceProvider.CreateScope();
            OutboxContext outboxContext = scope.ServiceProvider.GetRequiredService<OutboxContext>();
            ServiceTwo serviceTwo = scope.ServiceProvider.GetRequiredService<ServiceTwo>();

            List<OutboxMessage> outboxMessages = await outboxContext.OutboxMessages
                .Where(x => x.State == EventState.Pending)
                .ToListAsync(stoppingToken)
                .ConfigureAwait(false);

            foreach (OutboxMessage outboxMessage in outboxMessages)
            {
                try
                {
                    //OperationRequest operationRequest = JsonSerializer.Deserialize<OperationRequest>(outboxMessage.Data)!;
                    //serviceTwo.DoSomething(operationRequest.ClientRequest);

                    outboxMessage.State = EventState.Finalized;
                }
                // TODO: NEVER catch generic exceptions, ALWAYS catch specific exceptions
                // 2 scenarios:
                // 1) failed because API exception --> outboxMessage.State = EventState.Failed
                // 2) failed because API operation two is not ready --> don't update outboxMessage.State
                catch (Exception exception)
                {
                    this.logger.LogError(exception, "Error processing message {outboxMessage}", outboxMessage);
                    outboxMessage.State = EventState.Failed;
                }

                await outboxContext.SaveChangesAsync(stoppingToken).ConfigureAwait(false);
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken).ConfigureAwait(false);
        }
    }
}

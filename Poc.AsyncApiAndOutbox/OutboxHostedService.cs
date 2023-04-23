using System.Text.Json;
using Poc.AsyncApiAndOutbox.Features;
using Poc.AsyncApiAndOutbox.Outbox;
using Poc.AsyncApiAndOutbox.Services;

namespace Poc.AsyncApiAndOutbox;

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
        logger.LogInformation("Outbox Hosted Service running.");

        await DoWork(stoppingToken).ConfigureAwait(false);
    }

    // TODO: clean up this method
    private async Task DoWork(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox Hosted Service is working.");

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Outbox Hosted Service is doing background work.");

            using IServiceScope scope = serviceProvider.CreateScope();

            OutboxService outboxService = scope.ServiceProvider.GetRequiredService<OutboxService>();
            ServiceTwo serviceTwo = scope.ServiceProvider.GetRequiredService<ServiceTwo>();

            IReadOnlyList<OutboxMessage> outboxMessages = await outboxService.GetPendingOutboxMessagesAsync(stoppingToken).ConfigureAwait(false);

            foreach (OutboxMessage outboxMessage in outboxMessages)
            {
                try
                {
                    OperationRequest<ClientRequest> operationRequest = JsonSerializer.Deserialize<OperationRequest<ClientRequest>>(outboxMessage.Data)!;
                    serviceTwo.DoSomething(operationRequest.ClientRequest);

                    outboxMessage.State = EventState.Finalized;
                }
                // TODO: NEVER catch generic exceptions, ALWAYS catch specific exceptions
                // 2 scenarios:
                // 1) failed because API exception --> outboxMessage.State = EventState.Failed
                // 2) failed because API operation two is not ready --> don't update outboxMessage.State
                catch (Exception exception)
                {
                    logger.LogError(exception, "Error processing message {outboxMessage}", outboxMessage);
                    outboxMessage.State = EventState.Failed;
                }

                await outboxService.UpdateAsync(outboxMessage).ConfigureAwait(false);
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken).ConfigureAwait(false);
        }
    }
}

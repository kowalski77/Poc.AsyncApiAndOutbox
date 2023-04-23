using System.Text.Json;
using Poc.AsyncApiAndOutbox.Outbox;
using Poc.AsyncApiAndOutbox.Services;

namespace Poc.AsyncApiAndOutbox.Features;

public class ServiceTwoHostedService : BackgroundService
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<ServiceTwoHostedService> logger;

    public ServiceTwoHostedService(IServiceProvider serviceProvider, ILogger<ServiceTwoHostedService> logger)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await DoWorkAsync(stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        using IServiceScope scope = serviceProvider.CreateScope();

        OutboxService outboxService = scope.ServiceProvider.GetRequiredService<OutboxService>();
        IReadOnlyList<OutboxMessage> outboxMessages = await outboxService.GetPendingOutboxMessagesAsync(stoppingToken).ConfigureAwait(false);

        ServiceTwo serviceTwo = scope.ServiceProvider.GetRequiredService<ServiceTwo>();

        foreach (OutboxMessage outboxMessage in outboxMessages)
        {
            try
            {
                OperationRequest<ClientRequest> operationRequest = JsonSerializer.Deserialize<OperationRequest<ClientRequest>>(outboxMessage.Data)!;
                serviceTwo.DoSomething(operationRequest.ClientRequest);

                outboxMessage.MarkAsFinalized();
            }
            catch (InvalidOperationException e)
            {
                this.logger.LogError(e, "Error while processing outbox message");
                outboxMessage.MarkAsFailed();
            }

            await outboxService.UpdateAsync(outboxMessage).ConfigureAwait(false);
        }

        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).ConfigureAwait(false);
    }
}

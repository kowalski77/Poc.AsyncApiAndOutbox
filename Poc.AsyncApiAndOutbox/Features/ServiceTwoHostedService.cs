using System.Text.Json;
using Microsoft.Extensions.Options;
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
            await this.DoWorkAsync(stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        using IServiceScope scope = this.serviceProvider.CreateScope();

        OutboxService outboxService = scope.ServiceProvider.GetRequiredService<OutboxService>();
        ServiceTwo serviceTwo = scope.ServiceProvider.GetRequiredService<ServiceTwo>();
        IOptionsMonitor<OperationRequestOptions> options = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<OperationRequestOptions>>();

        IReadOnlyList<OutboxMessage> outboxMessages = await outboxService.GetPendingOutboxMessagesAsync(stoppingToken).ConfigureAwait(false);

        foreach (OutboxMessage outboxMessage in outboxMessages)
        {
            this.TryExecuteServiceTwo(serviceTwo, outboxMessage);

            await outboxService.UpdateAsync(outboxMessage).ConfigureAwait(false);
        }

        await Task.Delay(TimeSpan.FromMinutes(options.CurrentValue.RetryIntervalInMinutes), stoppingToken).ConfigureAwait(false);
    }

    private void TryExecuteServiceTwo(ServiceTwo serviceTwo, OutboxMessage outboxMessage)
    {
        try
        {
            OperationRequest<ClientRequest> operationRequest = JsonSerializer.Deserialize<OperationRequest<ClientRequest>>(outboxMessage.Data)!;
            var succeed = serviceTwo.DoSomething(operationRequest.ClientRequest);
            if (succeed)
            {
                outboxMessage.MarkAsFinalized();
            }
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Error while processing outbox message");
            outboxMessage.MarkAsFailed();
            throw;
        }
    }
}

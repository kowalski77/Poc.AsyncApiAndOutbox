namespace Poc.AsyncApiAndOutbox.Features;

public class ServiceOne
{
    private readonly ILogger<ServiceOne> logger;

    public ServiceOne(ILogger<ServiceOne> logger) => this.logger = logger;

    public void DoSomething(ClientRequest request)
    {
        logger.LogInformation("Service one does something with {request} ...", request);
    }
}

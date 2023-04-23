namespace Poc.AsyncApiAndOutbox.Features;

public class ServiceTwo
{
    private readonly ILogger<ServiceTwo> logger;

    public ServiceTwo(ILogger<ServiceTwo> logger) => this.logger = logger;

    public void DoSomething(ClientRequest request)
    {
        logger.LogInformation("Service two does something with {request} ...", request);
    }
}

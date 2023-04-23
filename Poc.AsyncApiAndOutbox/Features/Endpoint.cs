using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Poc.AsyncApiAndOutbox.Outbox;

namespace Poc.AsyncApiAndOutbox.Features;

[ApiController]
[Route("api/[controller]")]
public class Endpoint : ControllerBase
{
    private readonly ServiceOne serviceOne;
    private readonly OutboxContext outboxContext;

    public Endpoint(ServiceOne serviceOne, OutboxContext outboxContext)
    {
        this.serviceOne = serviceOne;
        this.outboxContext = outboxContext;
    }

    [HttpPost]
    public async Task<IActionResult> Post(ClientRequest request)
    {
        serviceOne.DoSomething(request);

        OperationRequest operationRequest = new()
        {
            ClientRequest = request,
        };

        OutboxMessage outboxMessage = new(operationRequest.RequestId, JsonSerializer.Serialize(operationRequest));
        outboxContext.OutboxMessages.Add(outboxMessage);
        await outboxContext.SaveChangesAsync().ConfigureAwait(false);

        return this.AcceptedAtRoute(nameof(Status), new { requestId = operationRequest.RequestId }, operationRequest);
    }

    [HttpGet("{requestId:guid}", Name = nameof(Status))]
    public IActionResult Status(Guid requestId)
    {
        OutboxMessage? outboxMessage = outboxContext.OutboxMessages.SingleOrDefault(x => x.TransactionId == requestId);

        return outboxMessage switch
        {
            null => this.NotFound(),
            { State: EventState.Finalized } => this.Ok(),
            { State: EventState.Failed } => this.BadRequest(),
            _ => NotFinalizedOperation(outboxMessage),
        };
    }

    private NotFoundObjectResult NotFinalizedOperation(OutboxMessage outboxMessage)
    {
        // TODO: update estimated completion time
        OperationRequest operationRequest = JsonSerializer.Deserialize<OperationRequest>(outboxMessage.Data)!;

        return this.NotFound(operationRequest);
    }
}

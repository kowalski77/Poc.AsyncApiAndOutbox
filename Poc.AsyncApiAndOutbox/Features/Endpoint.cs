using Microsoft.AspNetCore.Mvc;
using Poc.AsyncApiAndOutbox.Services;

namespace Poc.AsyncApiAndOutbox.Features;

[ApiController]
[Route("api/[controller]")]
public class Endpoint : ControllerBase
{
    private readonly ServiceOne serviceOne;
    private readonly OperationRequestService operationService;

    public Endpoint(ServiceOne serviceOne, OperationRequestService operationService)
    {
        this.serviceOne = serviceOne;
        this.operationService = operationService;
    }

    [HttpPost]
    public async Task<IActionResult> Post(ClientRequest request)
    {
        serviceOne.DoSomething(request);

        OperationRequest<ClientRequest> operationRequest = await this.operationService.SaveOperationAsync(request).ConfigureAwait(false);

        return this.AcceptedAtRoute(nameof(Status), new { requestId = operationRequest.RequestId }, operationRequest);
    }

    [HttpGet("{requestId:guid}", Name = nameof(Status))]
    public async Task<IActionResult> Status(Guid requestId)
    {
        OperationRequest<ClientRequest>? operationRequest = await this.operationService.GetOperationAsync<ClientRequest>(requestId).ConfigureAwait(false);

        return operationRequest switch
        {
            null => this.NotFound(),
            { RequestStatus: OperationRequestStatus.Completed } => this.Ok(),
            { RequestStatus: OperationRequestStatus.Failed } => this.BadRequest(),
            _ => this.NotFound(operationRequest),
        };
    }
}

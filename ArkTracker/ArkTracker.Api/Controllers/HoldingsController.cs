using ArkTracker.Application;
using ArkTracker.Application.CompareHoldings;
using ArkTracker.Application.GetAvailableHoldingDates;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ArkTracker.Api.Controllers;

[ApiController]
[Route("api/holdings")]
public class HoldingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public HoldingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("compare")]
    public async Task<IActionResult> Compare(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var fromUtc = from?.ToUniversalTime();
        var toUtc = to?.ToUniversalTime();
        var result = await _mediator.Send(
            new CompareHoldingsQuery(fromUtc, toUtc),
            cancellationToken);

        return Ok(result);
    }
    
    [HttpGet("dates")]
    public async Task<IActionResult> GetDates(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetAvailableHoldingDatesQuery(),
            cancellationToken);

        return Ok(result);
    }
}
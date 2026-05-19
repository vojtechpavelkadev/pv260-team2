using ArkTracker.Application.CompareHoldings;
using ArkTracker.Application.GetAvailableHoldingDates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArkTracker.Api.Controllers;

[Authorize]
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
        DateTime? fromUtc = from?.ToUniversalTime();
        DateTime? toUtc = to?.ToUniversalTime();
        CompareHoldingsResult result = await _mediator.Send(
            new CompareHoldingsQuery(fromUtc, toUtc),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("dates")]
    public async Task<IActionResult> GetDates(CancellationToken cancellationToken)
    {
        GetAvailableHoldingDatesResult result = await _mediator.Send(
            new GetAvailableHoldingDatesQuery(),
            cancellationToken);

        return Ok(result);
    }
}
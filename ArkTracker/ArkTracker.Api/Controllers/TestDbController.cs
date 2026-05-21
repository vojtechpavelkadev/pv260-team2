using ArkTracker.Application.Interfaces;
using ArkTracker.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArkTracker.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/test-db")]
    public class TestDbController(IDatabaseHealthService databaseHealthService) : ControllerBase
    {
        [HttpGet("check")]
        public async Task<IActionResult> CheckConnection()
        {
            try
            {
                int userCount = await databaseHealthService.GetUserCountAsync();
                return Ok(new { Message = "Spojení s NeonDB je OK!", Count = userCount });
            }
            catch (DatabaseConnectionException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { Message = "Chyba připojení!", Error = ex.Message });
            }
        }
    }
}

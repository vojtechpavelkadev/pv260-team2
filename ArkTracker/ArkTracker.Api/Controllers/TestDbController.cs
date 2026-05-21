using ArkTracker.Application.Interfaces;
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
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Chyba připojení!", Error = ex.Message });
            }
        }
    }
}

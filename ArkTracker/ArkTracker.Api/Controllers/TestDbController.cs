using ArkTracker.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArkTracker.Api.Controllers
{
    [ApiController]
    [Route("api/test-db")]
    public class TestDbController(AppDbContext context) : ControllerBase
    {
        [HttpGet("check")]
        public async Task<IActionResult> CheckConnection()
        {
            try
            {
                int userCount = await context.Users.CountAsync();
                return Ok(new { Message = "Spojení s NeonDB je OK!", Count = userCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Chyba připojení!", Error = ex.Message });
            }
        }
    }
}

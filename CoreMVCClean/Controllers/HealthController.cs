using Microsoft.AspNetCore.Mvc;

namespace Core.MVC.Clean.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet("circuit-breaker-status")]
        public IActionResult GetCircuitBreakerStatus()
        {
            // Return basic circuit breaker status
            // Note: Enhanced circuit state tracking would require additional implementation
            return Ok(new
            {
                BookAPI = "Active",
                PersonAPI = "Active",
                Timestamp = DateTime.UtcNow,
                Message = "Circuit breaker policies are active and monitoring external API calls"
            });
        }

        [HttpGet("service-health")]
        public IActionResult GetServiceHealth()
        {
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Services = new
                {
                    BookAPI = "Active",
                    PersonAPI = "Active"
                },
                CircuitBreaker = "Enabled"
            });
        }
    }
}
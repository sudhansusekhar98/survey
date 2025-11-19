using Microsoft.AspNetCore.Mvc;
using SurveyApp.Services;

namespace SurveyApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly ILocationApiService _locationService;
        private readonly ILogger<LocationController> _logger;

        public LocationController(ILocationApiService locationService, ILogger<LocationController> logger)
        {
            _locationService = locationService;
            _logger = logger;
        }

        [HttpGet("states")]
        public async Task<IActionResult> GetStates()
        {
            try
            {
                var states = await _locationService.GetStatesAsync();
                return Ok(states);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching states");
                return StatusCode(500, new { message = "Error fetching states", error = ex.Message });
            }
        }

        [HttpGet("cities/{stateId}")]
        public async Task<IActionResult> GetCitiesByState(int stateId)
        {
            try
            {
                if (stateId <= 0)
                {
                    return BadRequest(new { message = "Invalid state ID" });
                }

                var cities = await _locationService.GetCitiesByStateAsync(stateId);
                return Ok(cities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching cities for state {StateId}", stateId);
                return StatusCode(500, new { message = "Error fetching cities", error = ex.Message });
            }
        }

        [HttpGet("state/{stateId}/name")]
        public async Task<IActionResult> GetStateName(int stateId)
        {
            try
            {
                var states = await _locationService.GetStatesAsync();
                var state = states.FirstOrDefault(s => s.id == stateId);
                
                if (state != null)
                {
                    return Ok(new { name = state.name });
                }
                
                return NotFound(new { message = "State not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching state name for {StateId}", stateId);
                return StatusCode(500, new { message = "Error fetching state name", error = ex.Message });
            }
        }

        [HttpGet("city/{cityId}/name")]
        public async Task<IActionResult> GetCityName(int cityId, [FromQuery] int stateId)
        {
            try
            {
                if (stateId <= 0)
                {
                    return BadRequest(new { message = "State ID is required" });
                }

                var cities = await _locationService.GetCitiesByStateAsync(stateId);
                var city = cities.FirstOrDefault(c => c.id == cityId);
                
                if (city != null)
                {
                    return Ok(new { name = city.name });
                }
                
                return NotFound(new { message = "City not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching city name for {CityId}", cityId);
                return StatusCode(500, new { message = "Error fetching city name", error = ex.Message });
            }
        }
    }
}

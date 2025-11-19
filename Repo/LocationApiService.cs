using SurveyApp.Models;
using System.Text.Json;

namespace SurveyApp.Services
{
    public interface ILocationApiService
    {
        Task<List<StateModel>> GetStatesAsync();
        Task<List<CityModel>> GetCitiesByStateAsync(int stateId);
    }

    public class LocationApiService : ILocationApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LocationApiService> _logger;

        public LocationApiService(HttpClient httpClient, ILogger<LocationApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            
            // Configure HttpClient for external API
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<List<StateModel>> GetStatesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching states from API...");
                var response = await _httpClient.GetStringAsync("https://csc.sidsworld.co.in/api/states/101");
                _logger.LogInformation("States API response received: {Response}", response.Substring(0, Math.Min(200, response.Length)));
                
                // Deserialize the wrapper response
                var apiResponse = JsonSerializer.Deserialize<StatesApiResponse>(response, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                var states = apiResponse?.states ?? new List<StateModel>();
                _logger.LogInformation("Successfully loaded {Count} states", states.Count);
                return states;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error fetching states from API");
                return new List<StateModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching states from API");
                return new List<StateModel>();
            }
        }

        public async Task<List<CityModel>> GetCitiesByStateAsync(int stateId)
        {
            try
            {
                _logger.LogInformation("Fetching cities for state {StateId}...", stateId);
                var response = await _httpClient.GetStringAsync($"https://csc.sidsworld.co.in/api/cities/{stateId}");
                _logger.LogInformation("Cities API response received for state {StateId}", stateId);
                
                // Deserialize the wrapper response
                var apiResponse = JsonSerializer.Deserialize<CitiesApiResponse>(response, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                var cities = apiResponse?.cities ?? new List<CityModel>();
                _logger.LogInformation("Successfully loaded {Count} cities for state {StateId}", cities.Count, stateId);
                return cities;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error fetching cities for state {StateId}", stateId);
                return new List<CityModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching cities for state {StateId}", stateId);
                return new List<CityModel>();
            }
        }
    }
}

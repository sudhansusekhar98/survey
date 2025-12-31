using SurveyApp.Models;
using System.Text;
using System.Text.Json;

namespace SurveyApp.Services
{
    public interface ILocationApiService
    {
        Task<List<StateModel>> GetStatesAsync();
        Task<List<CityModel>> GetCitiesByStateAsync(int stateId);
        Task<List<CityModel>> GetCitiesByStateNameAsync(string stateName);
    }

    public class LocationApiService : ILocationApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LocationApiService> _logger;
        private const string COUNTRY_NAME = "India";
        
        // Cache for Indian states data
        private static List<StateModel>? _cachedStates;
        private static DateTime _cacheExpiry = DateTime.MinValue;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

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
                // Return cached data if still valid
                if (_cachedStates != null && DateTime.UtcNow < _cacheExpiry)
                {
                    _logger.LogInformation("Returning cached states data ({Count} states)", _cachedStates.Count);
                    return _cachedStates;
                }

                _logger.LogInformation("Fetching states from countriesnow.space API...");
                
                // Use the countriesnow.space API to get states for India
                var requestBody = new { country = COUNTRY_NAME };
                var content = new StringContent(
                    JsonSerializer.Serialize(requestBody), 
                    Encoding.UTF8, 
                    "application/json"
                );
                
                var response = await _httpClient.PostAsync(
                    "https://countriesnow.space/api/v0.1/countries/states", 
                    content
                );
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch states: HTTP {StatusCode}", response.StatusCode);
                    return GetFallbackIndianStates();
                }
                
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("States API response received");
                
                var apiResponse = JsonSerializer.Deserialize<CountriesNowStatesResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (apiResponse?.Error == true || apiResponse?.Data?.States == null)
                {
                    _logger.LogWarning("API returned error or no data, using fallback");
                    return GetFallbackIndianStates();
                }
                
                var states = apiResponse.Data.States
                    .Select((s, index) => new StateModel
                    {
                        id = index + 1,
                        name = s.Name,
                        country_id = 101,
                        iso2 = s.StateCode
                    })
                    .OrderBy(s => s.name)
                    .ToList();
                
                // Cache the results
                _cachedStates = states;
                _cacheExpiry = DateTime.UtcNow.Add(CacheDuration);
                
                _logger.LogInformation("Successfully loaded {Count} states", states.Count);
                return states;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error fetching states from API, using fallback");
                return GetFallbackIndianStates();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching states from API, using fallback");
                return GetFallbackIndianStates();
            }
        }

        public async Task<List<CityModel>> GetCitiesByStateAsync(int stateId)
        {
            try
            {
                // Get the state name from the state ID
                var states = await GetStatesAsync();
                var state = states.FirstOrDefault(s => s.id == stateId);
                
                if (state == null)
                {
                    _logger.LogWarning("State not found for ID {StateId}", stateId);
                    return new List<CityModel>();
                }
                
                return await GetCitiesByStateNameAsync(state.name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching cities for state {StateId}", stateId);
                return new List<CityModel>();
            }
        }

        public async Task<List<CityModel>> GetCitiesByStateNameAsync(string stateName)
        {
            try
            {
                _logger.LogInformation("Fetching cities for state '{StateName}'...", stateName);
                
                // Use the countriesnow.space API to get cities for a state
                var requestBody = new { country = COUNTRY_NAME, state = stateName };
                var content = new StringContent(
                    JsonSerializer.Serialize(requestBody), 
                    Encoding.UTF8, 
                    "application/json"
                );
                
                var response = await _httpClient.PostAsync(
                    "https://countriesnow.space/api/v0.1/countries/state/cities", 
                    content
                );
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch cities: HTTP {StatusCode}", response.StatusCode);
                    return new List<CityModel>();
                }
                
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Cities API response received for state '{StateName}'", stateName);
                
                var apiResponse = JsonSerializer.Deserialize<CountriesNowCitiesResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (apiResponse?.Error == true || apiResponse?.Data == null)
                {
                    _logger.LogWarning("API returned error or no data for cities");
                    return new List<CityModel>();
                }
                
                // Get state ID for reference
                var states = await GetStatesAsync();
                var state = states.FirstOrDefault(s => s.name.Equals(stateName, StringComparison.OrdinalIgnoreCase));
                int stateId = state?.id ?? 0;
                
                var cities = apiResponse.Data
                    .Select((cityName, index) => new CityModel
                    {
                        id = index + 1,
                        name = cityName,
                        state_id = stateId
                    })
                    .OrderBy(c => c.name)
                    .ToList();
                
                _logger.LogInformation("Successfully loaded {Count} cities for state '{StateName}'", cities.Count, stateName);
                return cities;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error fetching cities for state '{StateName}'", stateName);
                return new List<CityModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching cities for state '{StateName}'", stateName);
                return new List<CityModel>();
            }
        }

        /// <summary>
        /// Fallback Indian states list in case the API is unavailable
        /// </summary>
        private List<StateModel> GetFallbackIndianStates()
        {
            return new List<StateModel>
            {
                new StateModel { id = 1, name = "Andaman and Nicobar Islands", country_id = 101, iso2 = "AN" },
                new StateModel { id = 2, name = "Andhra Pradesh", country_id = 101, iso2 = "AP" },
                new StateModel { id = 3, name = "Arunachal Pradesh", country_id = 101, iso2 = "AR" },
                new StateModel { id = 4, name = "Assam", country_id = 101, iso2 = "AS" },
                new StateModel { id = 5, name = "Bihar", country_id = 101, iso2 = "BR" },
                new StateModel { id = 6, name = "Chandigarh", country_id = 101, iso2 = "CH" },
                new StateModel { id = 7, name = "Chhattisgarh", country_id = 101, iso2 = "CT" },
                new StateModel { id = 8, name = "Dadra and Nagar Haveli", country_id = 101, iso2 = "DN" },
                new StateModel { id = 9, name = "Daman and Diu", country_id = 101, iso2 = "DD" },
                new StateModel { id = 10, name = "Delhi", country_id = 101, iso2 = "DL" },
                new StateModel { id = 11, name = "Goa", country_id = 101, iso2 = "GA" },
                new StateModel { id = 12, name = "Gujarat", country_id = 101, iso2 = "GJ" },
                new StateModel { id = 13, name = "Haryana", country_id = 101, iso2 = "HR" },
                new StateModel { id = 14, name = "Himachal Pradesh", country_id = 101, iso2 = "HP" },
                new StateModel { id = 15, name = "Jammu and Kashmir", country_id = 101, iso2 = "JK" },
                new StateModel { id = 16, name = "Jharkhand", country_id = 101, iso2 = "JH" },
                new StateModel { id = 17, name = "Karnataka", country_id = 101, iso2 = "KA" },
                new StateModel { id = 18, name = "Kerala", country_id = 101, iso2 = "KL" },
                new StateModel { id = 19, name = "Ladakh", country_id = 101, iso2 = "LA" },
                new StateModel { id = 20, name = "Lakshadweep", country_id = 101, iso2 = "LD" },
                new StateModel { id = 21, name = "Madhya Pradesh", country_id = 101, iso2 = "MP" },
                new StateModel { id = 22, name = "Maharashtra", country_id = 101, iso2 = "MH" },
                new StateModel { id = 23, name = "Manipur", country_id = 101, iso2 = "MN" },
                new StateModel { id = 24, name = "Meghalaya", country_id = 101, iso2 = "ML" },
                new StateModel { id = 25, name = "Mizoram", country_id = 101, iso2 = "MZ" },
                new StateModel { id = 26, name = "Nagaland", country_id = 101, iso2 = "NL" },
                new StateModel { id = 27, name = "Odisha", country_id = 101, iso2 = "OR" },
                new StateModel { id = 28, name = "Puducherry", country_id = 101, iso2 = "PY" },
                new StateModel { id = 29, name = "Punjab", country_id = 101, iso2 = "PB" },
                new StateModel { id = 30, name = "Rajasthan", country_id = 101, iso2 = "RJ" },
                new StateModel { id = 31, name = "Sikkim", country_id = 101, iso2 = "SK" },
                new StateModel { id = 32, name = "Tamil Nadu", country_id = 101, iso2 = "TN" },
                new StateModel { id = 33, name = "Telangana", country_id = 101, iso2 = "TG" },
                new StateModel { id = 34, name = "Tripura", country_id = 101, iso2 = "TR" },
                new StateModel { id = 35, name = "Uttar Pradesh", country_id = 101, iso2 = "UP" },
                new StateModel { id = 36, name = "Uttarakhand", country_id = 101, iso2 = "UT" },
                new StateModel { id = 37, name = "West Bengal", country_id = 101, iso2 = "WB" }
            };
        }
    }

    // Response models for countriesnow.space API
    public class CountriesNowStatesResponse
    {
        public bool Error { get; set; }
        public string? Msg { get; set; }
        public CountriesNowStatesData? Data { get; set; }
    }

    public class CountriesNowStatesData
    {
        public string? Name { get; set; }
        public string? Iso3 { get; set; }
        public string? Iso2 { get; set; }
        public List<CountriesNowStateItem>? States { get; set; }
    }

    public class CountriesNowStateItem
    {
        public string Name { get; set; } = string.Empty;
        public string? StateCode { get; set; }
    }

    public class CountriesNowCitiesResponse
    {
        public bool Error { get; set; }
        public string? Msg { get; set; }
        public List<string>? Data { get; set; }
    }
}

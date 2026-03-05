using SurveyApp.Models;
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
        private readonly ILogger<LocationApiService> _logger;
        private readonly IWebHostEnvironment _env;
        
        // Cached data loaded from local JSON
        private static Dictionary<string, StateWithCities>? _cachedData;
        private static List<StateModel>? _cachedStates;
        private static readonly object _lockObj = new object();

        public LocationApiService(HttpClient httpClient, ILogger<LocationApiService> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        /// <summary>
        /// Loads the local JSON file containing all Indian states and their cities.
        /// Data is cached in memory after first load.
        /// </summary>
        private void EnsureDataLoaded()
        {
            if (_cachedData != null) return;

            lock (_lockObj)
            {
                if (_cachedData != null) return;

                try
                {
                    var jsonPath = Path.Combine(_env.WebRootPath, "data", "india_states_cities.json");
                    
                    if (!File.Exists(jsonPath))
                    {
                        _logger.LogWarning("Local states/cities data file not found at {Path}. Using fallback.", jsonPath);
                        LoadFallbackData();
                        return;
                    }

                    var jsonContent = File.ReadAllText(jsonPath);
                    var rawData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonContent);

                    if (rawData == null)
                    {
                        _logger.LogWarning("Failed to parse local states/cities JSON. Using fallback.");
                        LoadFallbackData();
                        return;
                    }

                    _cachedData = new Dictionary<string, StateWithCities>(StringComparer.OrdinalIgnoreCase);
                    var states = new List<StateModel>();
                    int stateIndex = 1;

                    foreach (var kvp in rawData.OrderBy(k => k.Key))
                    {
                        var stateName = kvp.Key;
                        var stateCode = "";
                        var cities = new List<string>();

                        if (kvp.Value.ValueKind == JsonValueKind.Object)
                        {
                            if (kvp.Value.TryGetProperty("state_code", out var codeElement))
                                stateCode = codeElement.GetString() ?? "";
                            
                            if (kvp.Value.TryGetProperty("cities", out var citiesElement) && 
                                citiesElement.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var city in citiesElement.EnumerateArray())
                                {
                                    var cityName = city.GetString();
                                    if (!string.IsNullOrWhiteSpace(cityName))
                                        cities.Add(cityName);
                                }
                            }
                        }

                        var stateModel = new StateModel
                        {
                            id = stateIndex,
                            name = stateName,
                            country_id = 101,
                            iso2 = stateCode
                        };

                        states.Add(stateModel);
                        _cachedData[stateName] = new StateWithCities
                        {
                            State = stateModel,
                            Cities = cities.OrderBy(c => c).ToList()
                        };

                        stateIndex++;
                    }

                    _cachedStates = states;
                    _logger.LogInformation("Loaded {StateCount} states with cities from local JSON file", states.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading local states/cities data. Using fallback.");
                    LoadFallbackData();
                }
            }
        }

        private void LoadFallbackData()
        {
            var fallbackStates = GetFallbackIndianStates();
            _cachedStates = fallbackStates;
            _cachedData = new Dictionary<string, StateWithCities>(StringComparer.OrdinalIgnoreCase);
            foreach (var state in fallbackStates)
            {
                _cachedData[state.name] = new StateWithCities
                {
                    State = state,
                    Cities = new List<string>()
                };
            }
        }

        public Task<List<StateModel>> GetStatesAsync()
        {
            EnsureDataLoaded();
            return Task.FromResult(_cachedStates ?? new List<StateModel>());
        }

        public async Task<List<CityModel>> GetCitiesByStateAsync(int stateId)
        {
            EnsureDataLoaded();
            
            var state = _cachedStates?.FirstOrDefault(s => s.id == stateId);
            if (state == null)
            {
                _logger.LogWarning("State not found for ID {StateId}", stateId);
                return new List<CityModel>();
            }

            return await GetCitiesByStateNameAsync(state.name);
        }

        public Task<List<CityModel>> GetCitiesByStateNameAsync(string stateName)
        {
            EnsureDataLoaded();

            if (_cachedData == null || !_cachedData.TryGetValue(stateName, out var stateData))
            {
                _logger.LogWarning("No data found for state '{StateName}'", stateName);
                return Task.FromResult(new List<CityModel>());
            }

            var stateId = stateData.State.id;
            var cities = stateData.Cities
                .Select((cityName, index) => new CityModel
                {
                    id = index + 1,
                    name = cityName,
                    state_id = stateId
                })
                .ToList();

            _logger.LogInformation("Returning {Count} cities for state '{StateName}' (offline)", cities.Count, stateName);
            return Task.FromResult(cities);
        }

        /// <summary>
        /// Fallback Indian states list in case the JSON file is missing
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

        // Internal model for cached state + cities
        private class StateWithCities
        {
            public StateModel State { get; set; } = new();
            public List<string> Cities { get; set; } = new();
        }
    }
}

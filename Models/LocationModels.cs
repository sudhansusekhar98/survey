namespace SurveyApp.Models
{
    // API Response wrapper models
    public class StatesApiResponse
    {
        public int status { get; set; }
        public List<StateModel> states { get; set; } = new();
    }

    public class CitiesApiResponse
    {
        public int status { get; set; }
        public List<CityModel> cities { get; set; } = new();
    }

    // State model matching API structure
    public class StateModel
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public int country_id { get; set; }
        public string? country_code { get; set; }
        public string? fips_code { get; set; }
        public string? iso2 { get; set; }
        public string? type { get; set; }
        public string? latitude { get; set; }
        public string? longitude { get; set; }
    }

    // City model matching API structure
    public class CityModel
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public int state_id { get; set; }
        public string? state_code { get; set; }
        public int? country_id { get; set; }
        public string? country_code { get; set; }
        public string? latitude { get; set; }
        public string? longitude { get; set; }
    }
}

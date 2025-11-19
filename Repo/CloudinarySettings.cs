namespace SurveyApp.Repo
{
    /// <summary>
    /// Configuration settings for Cloudinary integration
    /// </summary>
    public class CloudinarySettings
    {
        public string CloudName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
    }
}

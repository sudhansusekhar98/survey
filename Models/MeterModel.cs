namespace AnalyticaDocs.Models
{
    public class MeterModel
    {
        public int MeterId { get; set; }

        public string? MeterName { get; set; }

        public string? MeterDescription { get; set; }

        public string? Capacity { get; set; }

        public decimal? Mf { get; set; }

        public string? Uom { get; set; }

        public string? SubMeter { get; set; }

        public string? IsActive { get; set; }
    }
}

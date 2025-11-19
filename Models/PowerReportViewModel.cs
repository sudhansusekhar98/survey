namespace AnalyticaDocs.Models
{
    public class PowerReportViewModel
    {
        public int PeriodId { get; set; }
        public string? PeriodName { get; set; }
        public string? HeaderChartBase64 { get; set; }   // data:image/png;base64,...
        public string? AuxChartBase64 { get; set; }

    }
}

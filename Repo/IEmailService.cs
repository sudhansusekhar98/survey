namespace AnalyticaDocs.Repository
{
    public interface IEmailService
    {
        Task<bool> SendCanteenEmailAsync(string employeeName, string epin, string recipients);
        

    }
}

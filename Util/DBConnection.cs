namespace AnalyticaDocs.Util
{
    public abstract class DBConnection
    {
        // 🔐 Hardcoded string — suitable for isolated environments
         private static readonly string cs = "Server=10.0.32.135;Database=VLDev;UID=adminrole;Password=@dminr0le;Connect Timeout=360000;TrustServerCertificate=True";
        //private static readonly string cs = "Server=(Local);Database=VLDev;Integrated Security=True;Connect Timeout=360000;TrustServerCertificate=True";
        public static string ConnectionString => cs;
    }
}




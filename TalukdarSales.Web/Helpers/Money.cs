namespace TalukdarSales.Web.Helpers
{
    /// <summary>Money is stored as double; round at every write so floating-point noise (0.1 + 0.2) never reaches a balance.</summary>
    public static class Money
    {
        public static double Round(double value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}

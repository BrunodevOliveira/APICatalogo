using System.Threading.RateLimiting; // Added for potential use with RateLimiterOptions

namespace APICatalogo.Infrastructure.RateLimiting; // Updated namespace

public class MyRateLimitOptions
{
    public const string MyRateLimit = "MyRateLimit"; // This seems like a policy name
    public int PermitLimit { get; set; } = 5;
    public int Window { get; set; } = 10; // Seconds
    public int ReplenishmentPeriod { get; set; } = 2; // Seconds
    public int QueueLimit { get; set; } = 2;
    public int SegmentsPerWindow { get; set; } = 8;
    public int TokenLimit { get; set; } = 10; 
    public int TokenLimit2 { get; set; } = 20; // Unclear what this is for without context
    public int TokensPerPeriod { get; set; } = 4; // Renamed from TokenPerPeriod for clarity
    public bool AutoReplenishment { get; set; } = false;
}
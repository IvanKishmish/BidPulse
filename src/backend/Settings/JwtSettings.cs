namespace BidPulse.Settings;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";
 
    public string Key { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int ExpirationDays { get; init; } = 7;
}
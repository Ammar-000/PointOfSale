namespace POS_Server_PL.Models;

public class JwtSettingsModel
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string Key { get; set; }
    public double DurationInMinutes { get; set; }
}

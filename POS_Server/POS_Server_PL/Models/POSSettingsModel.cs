namespace POS_Server_PL.Models;

public class POSSettingsModel
{
    public string Port { get; set; }
    public ConnectionStringsModel ConnectionStrings { get; set; }
    public JwtSettingsModel JwtSettings { get; set; }
}

public class ConnectionStringsModel
{
    public string DefaultConnection { get; set; }
}

public class JwtSettingsModel
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string Key { get; set; }
    public double DurationInMinutes { get; set; }
}
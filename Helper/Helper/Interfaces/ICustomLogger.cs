namespace Helper.Interfaces;

public interface ICustomLogger
{
    public void LogError(string layerName, Exception? ex, string customMsg);
}

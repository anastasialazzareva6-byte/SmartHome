namespace SmartHomeSystem
{
    public interface IDeviceDiagnostic
    {
        string RunDiagnostics();
        string GetDeviceInfo();
    }
}
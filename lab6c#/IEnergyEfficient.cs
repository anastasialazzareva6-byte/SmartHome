namespace SmartHomeSystem
{
    public interface IEnergyEfficient
    {
        double CalculateEnergySaving();
        void EnableEcoMode();
        void DisableEcoMode();
        bool IsEcoModeEnabled { get; }
    }
}
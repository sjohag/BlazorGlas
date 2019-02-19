namespace GlasSimulator.App.Services
{
    public interface IGlasSimulatorFactory
    {
        IGlasSimulator SkapaSimulator(int sökRad, int sökNummer);
    }
}
namespace GlasSimulator.Client.Services
{
    public interface IGlasSimulatorFactory
    {
        IGlasSimulator SkapaSimulator(int sökRad, int sökNummer);
    }
}
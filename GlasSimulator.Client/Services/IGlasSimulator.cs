using Rationals;

namespace GlasSimulator.Client.Services
{
    public interface IGlasSimulator
    {
        Rational TidpuntFullt { get; }
        int AntalGlasModell { get; }
    }
}
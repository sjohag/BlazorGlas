using Rationals;

namespace GlasSimulator.App.Services
{
    public interface IGlasSimulator
    {
        Rational TidpuntFullt { get; }
        int AntalGlasModell { get; }
    }
}
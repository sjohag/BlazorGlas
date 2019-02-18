using GlasSimulator.App.Services;
using Microsoft.AspNetCore.Components;
using Rationals;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


namespace GlasSimulator.App.Pages
{
    public class GlasModel : ComponentBase
    {
        protected bool BeräkningPågår { get; set; } = false;
        protected string SöktRad { get; set; }
        protected string SöktNummer { get; set; }
        protected string MeddelandeRubrik { get; set; }
        protected string MeddelandeText { get; set; }
        protected string ResultatRubrik { get; set; }
        protected List<string> ResultatText { get; set; }
        [Inject]
        private GlasSimulatorFactory glasSimulatorFactory { get; set; }
        protected async Task Simulera()
        {
            MeddelandeRubrik = null;
            ResultatRubrik = null;
            BeräkningPågår = true;
            StateHasChanged();
            try
            {
                if (string.IsNullOrWhiteSpace(SöktRad) || string.IsNullOrWhiteSpace(SöktNummer))
                    throw new Exception("Ange värden för rad och nummer");
                var rad = int.Parse(SöktRad);
                var nummer = int.Parse(SöktNummer);
                if (!(1 <= rad && rad <= 100))
                    throw new Exception("Ange rad i intervallet 1-100");
                if (!(1 <= nummer && nummer <= rad))
                    throw new Exception("Ange nummer i intervallet 1-rad");
                ResultatText = await Task.Run(() => KörSimulator(rad, nummer));
                ResultatRubrik = "Vi har ett resultat!";
                BeräkningPågår = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                MeddelandeRubrik = "Något blev fel";
                MeddelandeText = ex.Message;
                BeräkningPågår = false;
                StateHasChanged();
            }
        }
        private List<string> KörSimulator(int sökRad, int sökNummer)
        {
            var resultat = new List<string>();
            var tidtagning = Stopwatch.StartNew();
            var simulator = glasSimulatorFactory.SkapaSimulator(sökRad, sökNummer);
            var tid = simulator.SöktGlas.TidpunktGlasetFullt;
            tidtagning.Stop();
            resultat.Add($"Tid: {tid:W} (c:a {tid.Presentera()}), glaset {simulator.SöktGlas.Rad}-{simulator.SöktGlas.Nummer} är fullt!");
            resultat.Add($"Exekveringstid (ms): {tidtagning.ElapsedMilliseconds}");
            resultat.Add($"Antal glas i modellen: {simulator.AllaGlas.Count}");
            return resultat;
        }
    }
}
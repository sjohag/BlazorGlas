using GlasSimulator.App.Extensions;
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
        protected bool Ber�kningP�g�r { get; set; } = false;
        protected string S�ktRad { get; set; }
        protected string S�ktNummer { get; set; }
        protected string MeddelandeRubrik { get; set; }
        protected string MeddelandeText { get; set; }
        protected string ResultatRubrik { get; set; }
        protected List<string> ResultatText { get; set; }
        [Inject]
        private IGlasSimulatorFactory glasSimulatorFactory { get; set; }
        protected async Task Simulera()
        {
            MeddelandeRubrik = null;
            ResultatRubrik = null;
            Ber�kningP�g�r = true;
            StateHasChanged();
            try
            {
                if (string.IsNullOrWhiteSpace(S�ktRad) || string.IsNullOrWhiteSpace(S�ktNummer))
                    throw new Exception("Ange v�rden f�r rad och nummer");
                var rad = int.Parse(S�ktRad);
                var nummer = int.Parse(S�ktNummer);
                if (!(1 <= rad && rad <= 100))
                    throw new Exception("Ange rad i intervallet 1-100");
                if (!(1 <= nummer && nummer <= rad))
                    throw new Exception("Ange nummer i intervallet 1-rad");
                ResultatText = await Task.Run(() => K�rSimulator(rad, nummer));
                ResultatRubrik = "Vi har ett resultat!";
                Ber�kningP�g�r = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                MeddelandeRubrik = "N�got blev fel";
                MeddelandeText = ex.Message;
                Ber�kningP�g�r = false;
                StateHasChanged();
            }
        }
        private List<string> K�rSimulator(int s�kRad, int s�kNummer)
        {
            var resultat = new List<string>();
            var tidtagning = Stopwatch.StartNew();
            var simulator = glasSimulatorFactory.SkapaSimulator(s�kRad, s�kNummer);
            var tid = simulator.TidpuntFullt;
            tidtagning.Stop();
            resultat.Add($"Tid: {tid:W} sekunder (c:a {tid.Presentera()} s), glaset {s�kRad}-{s�kNummer} �r fullt!");
            resultat.Add($"Exekveringstid (ms): {tidtagning.ElapsedMilliseconds}");
            resultat.Add($"Antal glas i modellen: {simulator.AntalGlasModell}");
            return resultat;
        }
    }
}
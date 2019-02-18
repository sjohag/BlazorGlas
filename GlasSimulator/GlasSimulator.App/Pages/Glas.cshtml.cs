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
            var simulator = new GlasSimulator(s�kRad, s�kNummer);
            var tid = simulator.S�ktGlas.TidpunktGlasetFullt;
            tidtagning.Stop();
            resultat.Add($"Tid: {tid:W} (c:a {tid.Presentera()}), glaset {simulator.S�ktGlas.Rad}-{simulator.S�ktGlas.Nummer} �r fullt!");
            resultat.Add($"Exekveringstid (ms): {tidtagning.ElapsedMilliseconds}");
            resultat.Add($"Antal glas i modellen: {simulator.AllaGlas.Count}");
            return resultat;
        }
        public class GlasSimulator
        {
            public Glas ToppGlas { get; private set; }
            public Glas S�ktGlas { get; private set; }
            public Dictionary<(int rad, int nummer), Glas> AllaGlas;
            public GlasSimulator(int s�kRad, int s�kNummer)
            {
                AllaGlas = new Dictionary<(int rad, int nummer), Glas>();
                for (int rad = 1; rad <= s�kRad; rad++)
                {
                    var tillrinningsomr�de = Tillrinningsomr�de(rad, s�kRad, s�kNummer);
                    var nummerFr�n = Math.Max(tillrinningsomr�de.fr�nNummer, 1);
                    var nummerTill = Math.Min(tillrinningsomr�de.tillNummer, rad);
                    for (int nummer = nummerFr�n; nummer <= nummerTill; nummer++)
                    {
                        var nyttGlas = new Glas { Rad = rad, Nummer = nummer };
                        AllaGlas.Add((rad: rad, nummer: nummer), nyttGlas);
                        //Ovanf�r till v�nster
                        var v�nster = (rad: rad - 1, glas: nummer - 1);
                        if (AllaGlas.ContainsKey(v�nster))
                        {
                            nyttGlas.�verV�nster = AllaGlas[v�nster];
                        }
                        //Ovanf�r till h�ger
                        var h�ger = (rad: rad - 1, glas: nummer);
                        if (AllaGlas.ContainsKey(h�ger))
                        {
                            nyttGlas.�verH�ger = AllaGlas[h�ger];
                        }
                        //Spegelbild (till v�nster)
                        var spegelbild = (rad: rad, glas: rad - nummer + 1);
                        if (spegelbild.glas < nummer && AllaGlas.ContainsKey(spegelbild))
                        {
                            nyttGlas.Spegelbild = AllaGlas[spegelbild];
                        }
                    }
                }
                ToppGlas = AllaGlas[(rad: 1, nummer: 1)];
                S�ktGlas = AllaGlas[(rad: s�kRad, nummer: s�kNummer)];
            }
            private (int fr�nNummer, int tillNummer) Tillrinningsomr�de(int rad, int s�kRad, int s�kNummer)
            {
                var radskillnad = s�kRad - rad;
                return (fr�nNummer: s�kNummer - radskillnad, tillNummer: s�kNummer);
            }
            public class Glas
            {
                public Glas �verV�nster { get; set; }
                public Glas �verH�ger { get; set; }
                public Glas Spegelbild { get; set; }
                public int Rad { get; set; }
                public int Nummer { get; set; }
                public Rational Volym { get; set; } = 10;
                private List<Fl�de> infl�den;
                public List<Fl�de> Infl�den
                {
                    get
                    {
                        if (infl�den != null)
                            return infl�den;
                        if (Rad == 1)
                            infl�den = new List<Fl�de> { new Fl�de { Start = 0, V�rde = 1 } };
                        else
                        {
                            infl�den = new List<Fl�de>();
                            if (�verV�nster != null)
                                infl�den.AddRange(�verV�nster.Utfl�den);
                            if (�verH�ger != null)
                                infl�den.AddRange(�verH�ger.Utfl�den);
                        }
                        return infl�den;
                    }
                }
                private Rational tidpunktGlasetFullt;
                public Rational TidpunktGlasetFullt
                {
                    get
                    {
                        if (tidpunktGlasetFullt > 0)
                            return tidpunktGlasetFullt;
                        tidpunktGlasetFullt = Utfl�den.Min(u => u.Start);
                        return tidpunktGlasetFullt;
                    }
                }
                private List<Fl�de> utfl�den;
                public List<Fl�de> Utfl�den
                {
                    get
                    {
                        if (utfl�den != null)
                            return utfl�den;
                        if (Spegelbild != null)
                            return Spegelbild.Utfl�den;
                        var infl�denInnanFull = Infl�den;
                        while (true)
                        {
                            var tidFullt = ((Volym + infl�denInnanFull.Sum(i => i.Start * i.V�rde)) / infl�denInnanFull.Sum(i => i.V�rde)).CanonicalForm;
                            if (!infl�denInnanFull.Any(i => i.Start > tidFullt))//Giltig l�sning, inget fl�de medr�knat som startar efter tidpunkten
                            {
                                tidpunktGlasetFullt = tidFullt;
                                break;
                            }
                            infl�denInnanFull = infl�denInnanFull.Where(i => i.Start < tidFullt).ToList();//Ta bort fl�den som startar efter ber�knade tidpunkten
                        }
                        utfl�den = new List<Fl�de>();
                        utfl�den.Add(new Fl�de { Start = tidpunktGlasetFullt, V�rde = (Infl�den.Where(i => i.Start <= tidpunktGlasetFullt).Sum(i => i.V�rde) / 2).CanonicalForm });
                        utfl�den.AddRange(from i in Infl�den
                                          where i.Start > tidpunktGlasetFullt
                                          group i.V�rde by i.Start into ig
                                          select new Fl�de
                                          {
                                              Start = ig.Key.CanonicalForm,
                                              V�rde = (ig.Sum() / 2).CanonicalForm
                                          });
                        return utfl�den;
                    }
                }
            }
            public class Fl�de
            {
                public Rational Start { get; set; }
                public Rational V�rde { get; set; }
            }
        }
    }
    public static class RationalsExtension
    {
        public static Rational Sum(this IEnumerable<Rational> rationals)
        {
            var resultat = Rational.Zero;
            foreach (var r in rationals)
            {
                resultat = (resultat + r).CanonicalForm;
            }
            return resultat;
        }
        public static Rational Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, Rational> selector)
        {
            return source.Select(selector).Sum();
        }
        public static string Presentera(this Rational rational)
        {
            try
            {
                return $"{((decimal)rational):0.000}";
            }
            catch
            {
                try
                {
                    return $"{((double)rational):0.000}";
                }
                catch
                {
                    return $"{rational.WholePart}";
                }
            }
        }
    }
}
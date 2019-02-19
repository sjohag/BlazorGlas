using Rationals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlasSimulator.Client.Extensions;

namespace GlasSimulator.Client.Services
{
    public class GlasSimulatorFactory : IGlasSimulatorFactory
    {
        public IGlasSimulator SkapaSimulator(int sökRad, int sökNummer)
        {
            return new GlasSimulator(sökRad, sökNummer);
        }
    }
    public class GlasSimulator : IGlasSimulator
    {
        public Rational TidpuntFullt => SöktGlas.TidpunktGlasetFullt;
        public int AntalGlasModell => AllaGlas.Count;
        private Glas ToppGlas { get;  set; }
        private Glas SöktGlas { get;  set; }
        private Dictionary<(int rad, int nummer), Glas> AllaGlas;
        public GlasSimulator(int sökRad, int sökNummer)
        {
            AllaGlas = new Dictionary<(int rad, int nummer), Glas>();
            for (int rad = 1; rad <= sökRad; rad++)
            {
                var tillrinningsområde = Tillrinningsområde(rad, sökRad, sökNummer);
                var nummerFrån = Math.Max(tillrinningsområde.frånNummer, 1);
                var nummerTill = Math.Min(tillrinningsområde.tillNummer, rad);
                for (int nummer = nummerFrån; nummer <= nummerTill; nummer++)
                {
                    var nyttGlas = new Glas { Rad = rad, Nummer = nummer };
                    AllaGlas.Add((rad: rad, nummer: nummer), nyttGlas);
                    //Ovanför till vänster
                    var vänster = (rad: rad - 1, glas: nummer - 1);
                    if (AllaGlas.ContainsKey(vänster))
                    {
                        nyttGlas.ÖverVänster = AllaGlas[vänster];
                    }
                    //Ovanför till höger
                    var höger = (rad: rad - 1, glas: nummer);
                    if (AllaGlas.ContainsKey(höger))
                    {
                        nyttGlas.ÖverHöger = AllaGlas[höger];
                    }
                    //Spegelbild (till vänster)
                    var spegelbild = (rad: rad, glas: rad - nummer + 1);
                    if (spegelbild.glas < nummer && AllaGlas.ContainsKey(spegelbild))
                    {
                        nyttGlas.Spegelbild = AllaGlas[spegelbild];
                    }
                }
            }
            ToppGlas = AllaGlas[(rad: 1, nummer: 1)];
            SöktGlas = AllaGlas[(rad: sökRad, nummer: sökNummer)];
        }
        private (int frånNummer, int tillNummer) Tillrinningsområde(int rad, int sökRad, int sökNummer)
        {
            var radskillnad = sökRad - rad;
            return (frånNummer: sökNummer - radskillnad, tillNummer: sökNummer);
        }
        private class Glas 
        {
            public Glas ÖverVänster { get; set; }
            public Glas ÖverHöger { get; set; }
            public Glas Spegelbild { get; set; }
            public int Rad { get; set; }
            public int Nummer { get; set; }
            public Rational Volym { get; set; } = 10;
            private List<Flöde> inflöden;
            public List<Flöde> Inflöden
            {
                get
                {
                    if (inflöden != null)
                        return inflöden;
                    if (Rad == 1)
                        inflöden = new List<Flöde> { new Flöde { Start = 0, Värde = 1 } };
                    else
                    {
                        inflöden = new List<Flöde>();
                        if (ÖverVänster != null)
                            inflöden.AddRange(ÖverVänster.Utflöden);
                        if (ÖverHöger != null)
                            inflöden.AddRange(ÖverHöger.Utflöden);
                    }
                    return inflöden;
                }
            }
            private Rational tidpunktGlasetFullt;
            public Rational TidpunktGlasetFullt
            {
                get
                {
                    if (tidpunktGlasetFullt > 0)
                        return tidpunktGlasetFullt;
                    tidpunktGlasetFullt = Utflöden.Min(u => u.Start);
                    return tidpunktGlasetFullt;
                }
            }
            private List<Flöde> utflöden;
            public List<Flöde> Utflöden
            {
                get
                {
                    if (utflöden != null)
                        return utflöden;
                    if (Spegelbild != null)
                        return Spegelbild.Utflöden;
                    var inflödenInnanFull = Inflöden;
                    while (true)
                    {
                        var tidFullt = ((Volym + inflödenInnanFull.Sum(i => i.Start * i.Värde)) / inflödenInnanFull.Sum(i => i.Värde)).CanonicalForm;
                        if (!inflödenInnanFull.Any(i => i.Start > tidFullt))//Giltig lösning, inget flöde medräknat som startar efter tidpunkten
                        {
                            tidpunktGlasetFullt = tidFullt;
                            break;
                        }
                        inflödenInnanFull = inflödenInnanFull.Where(i => i.Start < tidFullt).ToList();//Ta bort flöden som startar efter beräknade tidpunkten
                    }
                    utflöden = new List<Flöde>();
                    utflöden.Add(new Flöde { Start = tidpunktGlasetFullt, Värde = (Inflöden.Where(i => i.Start <= tidpunktGlasetFullt).Sum(i => i.Värde) / 2).CanonicalForm });
                    utflöden.AddRange(from i in Inflöden
                                      where i.Start > tidpunktGlasetFullt
                                      group i.Värde by i.Start into ig
                                      select new Flöde
                                      {
                                          Start = ig.Key.CanonicalForm,
                                          Värde = (ig.Sum() / 2).CanonicalForm
                                      });
                    return utflöden;
                }
            }
        }
        private class Flöde 
        {
            public Rational Start { get; set; }
            public Rational Värde { get; set; }
        }
    }
}

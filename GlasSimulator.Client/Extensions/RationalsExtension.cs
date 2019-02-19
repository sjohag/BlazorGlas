using Rationals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlasSimulator.Client.Extensions
{
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

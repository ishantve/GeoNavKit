//
//  TrailSampler.cs
//  GeoNavKit
//
//  Pure geometry for placing history-trail dots along a recent path. Returns
//  coordinates only — no rendering dependency.
//

using System;
using System.Collections.Generic;

namespace GeoNavKit
{
    public static class TrailSampler
    {
        private const double MetersPerNM = Distance.MetersPerNauticalMile;

        /// <summary>
        /// <paramref name="count"/> positions evenly spread along the recent path
        /// (last 8 samples). Ordered oldest → newest.
        /// </summary>
        public static List<GeoCoordinate> EqualSpaced(IReadOnlyList<GeoCoordinate> history, int count)
        {
            if (history == null) throw new ArgumentNullException(nameof(history));

            var window = Suffix(history, 8);
            if (window.Count < 2) return Suffix(window, count);

            var cum = new double[window.Count];
            for (int i = 1; i < window.Count; i++)
            {
                cum[i] = cum[i - 1] + Geo.DistanceMeters(window[i - 1], window[i]);
            }
            double total = cum[cum.Length - 1];
            if (total <= 0) return new List<GeoCoordinate> { window[window.Count - 1] };

            var result = new List<GeoCoordinate>(count);
            for (int k = 1; k <= count; k++)
            {
                double target = total * k / count;
                int seg = window.Count - 2;
                for (int i = 0; i < window.Count - 1; i++)
                {
                    if (cum[i + 1] >= target) { seg = i; break; }
                }
                int seg1 = Math.Min(seg + 1, window.Count - 1);
                double segLen = cum[seg1] - cum[seg];
                double t = segLen > 0 ? (target - cum[seg]) / segLen : 0.0;
                result.Add(new GeoCoordinate(
                    window[seg].Latitude + t * (window[seg1].Latitude - window[seg].Latitude),
                    window[seg].Longitude + t * (window[seg1].Longitude - window[seg].Longitude)));
            }
            return result;
        }

        /// <summary>
        /// Exactly <paramref name="count"/> positions spaced <paramref name="spacingNM"/> NM
        /// apart, walking backward from the newest point; short history is projected
        /// backward. Oldest → newest.
        /// </summary>
        public static List<GeoCoordinate> FixedSpaced(IReadOnlyList<GeoCoordinate> history,
                                                      int count, double spacingNM)
        {
            if (history == null) throw new ArgumentNullException(nameof(history));
            if (history.Count < 2) return new List<GeoCoordinate>();

            double spacingMeters = spacingNM * MetersPerNM;

            var result = new List<GeoCoordinate>(count);
            double walked = 0.0;
            int dotNum = 1;
            int i = history.Count - 1;

            while (i > 0 && result.Count < count)
            {
                var segTo = history[i];
                var segFrom = history[i - 1];
                double segLen = Geo.DistanceMeters(segFrom, segTo);

                while (dotNum * spacingMeters <= walked + segLen && result.Count < count)
                {
                    double t = segLen > 0 ? (dotNum * spacingMeters - walked) / segLen : 0.0;
                    result.Add(new GeoCoordinate(
                        segTo.Latitude + t * (segFrom.Latitude - segTo.Latitude),
                        segTo.Longitude + t * (segFrom.Longitude - segTo.Longitude)));
                    dotNum += 1;
                }

                walked += segLen;
                i -= 1;
            }

            if (result.Count < count)
            {
                double backBearing = Geo.Bearing(history[1], history[0]);
                while (result.Count < count)
                {
                    double extra = dotNum * spacingMeters - walked;
                    result.Add(Geo.Offset(history[0], extra, backBearing));
                    dotNum += 1;
                }
            }

            result.Reverse();
            return result;
        }

        /// <summary>The last <paramref name="n"/> elements, or all of them if there are fewer.</summary>
        private static List<GeoCoordinate> Suffix(IReadOnlyList<GeoCoordinate> source, int n)
        {
            int start = Math.Max(0, source.Count - n);
            var slice = new List<GeoCoordinate>(source.Count - start);
            for (int i = start; i < source.Count; i++) slice.Add(source[i]);
            return slice;
        }
    }
}

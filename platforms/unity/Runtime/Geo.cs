//
//  Geo.cs
//  GeoNavKit
//
//  Great-circle geographic helpers: bearing, distance, and offset. Pure maths
//  over GeoCoordinate — no UnityEngine dependency, so it runs in edit mode,
//  play mode, tests and plain .NET alike.
//

using System;

namespace GeoNavKit
{
    public static class Geo
    {
        private const double Deg = Math.PI / 180.0;
        private const double Rad = 180.0 / Math.PI;

        /// <summary>Mean Earth radius in meters — the sphere used by <see cref="Offset"/>.</summary>
        private const double EarthRadiusMeters = 6_371_000.0;

        // WGS-84 ellipsoid, used by DistanceMeters to match Apple's
        // CLLocation.distance(from:) and Android's Location.distanceBetween.
        private const double Wgs84A = 6_378_137.0;              // semi-major axis
        private const double Wgs84F = 1.0 / 298.257223563;      // flattening
        private static readonly double Wgs84B = Wgs84A * (1.0 - Wgs84F); // semi-minor axis

        /// <summary>Initial bearing in degrees (0–360) from one coordinate to another.</summary>
        public static double Bearing(GeoCoordinate from, GeoCoordinate to)
        {
            double lat1 = from.Latitude * Deg;
            double lat2 = to.Latitude * Deg;
            double dLon = (to.Longitude - from.Longitude) * Deg;

            double y = Math.Sin(dLon) * Math.Cos(lat2);
            double x = Math.Cos(lat1) * Math.Sin(lat2)
                       - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);
            double deg = Math.Atan2(y, x) * Rad;

            return (deg + 360.0) % 360.0;
        }

        /// <summary>
        /// Geodesic (WGS-84) distance in meters between two coordinates, via
        /// Vincenty's inverse formula. Near-antipodal pairs, where Vincenty does
        /// not converge, fall back to a spherical haversine distance.
        /// </summary>
        public static double DistanceMeters(GeoCoordinate from, GeoCoordinate to)
        {
            double L = (to.Longitude - from.Longitude) * Deg;
            double u1 = Math.Atan((1.0 - Wgs84F) * Math.Tan(from.Latitude * Deg));
            double u2 = Math.Atan((1.0 - Wgs84F) * Math.Tan(to.Latitude * Deg));
            double sinU1 = Math.Sin(u1), cosU1 = Math.Cos(u1);
            double sinU2 = Math.Sin(u2), cosU2 = Math.Cos(u2);

            double lambda = L;
            double sinSigma = 0, cosSigma = 0, sigma = 0, cosSqAlpha = 0, cos2SigmaM = 0;
            bool converged = false;

            for (int i = 0; i < 200; i++)
            {
                double sinLambda = Math.Sin(lambda);
                double cosLambda = Math.Cos(lambda);

                double t1 = cosU2 * sinLambda;
                double t2 = cosU1 * sinU2 - sinU1 * cosU2 * cosLambda;
                sinSigma = Math.Sqrt(t1 * t1 + t2 * t2);
                if (sinSigma == 0) return 0; // coincident points

                cosSigma = sinU1 * sinU2 + cosU1 * cosU2 * cosLambda;
                sigma = Math.Atan2(sinSigma, cosSigma);

                double sinAlpha = cosU1 * cosU2 * sinLambda / sinSigma;
                cosSqAlpha = 1.0 - sinAlpha * sinAlpha;
                cos2SigmaM = cosSqAlpha == 0 ? 0 : cosSigma - 2.0 * sinU1 * sinU2 / cosSqAlpha;

                double c = Wgs84F / 16.0 * cosSqAlpha * (4.0 + Wgs84F * (4.0 - 3.0 * cosSqAlpha));
                double previous = lambda;
                lambda = L + (1.0 - c) * Wgs84F * sinAlpha
                         * (sigma + c * sinSigma
                            * (cos2SigmaM + c * cosSigma * (-1.0 + 2.0 * cos2SigmaM * cos2SigmaM)));

                if (Math.Abs(lambda - previous) < 1e-12)
                {
                    converged = true;
                    break;
                }
            }

            if (!converged) return HaversineMeters(from, to);

            double uSq = cosSqAlpha * (Wgs84A * Wgs84A - Wgs84B * Wgs84B) / (Wgs84B * Wgs84B);
            double a = 1.0 + uSq / 16384.0 * (4096.0 + uSq * (-768.0 + uSq * (320.0 - 175.0 * uSq)));
            double b = uSq / 1024.0 * (256.0 + uSq * (-128.0 + uSq * (74.0 - 47.0 * uSq)));
            double deltaSigma = b * sinSigma
                * (cos2SigmaM + b / 4.0
                   * (cosSigma * (-1.0 + 2.0 * cos2SigmaM * cos2SigmaM)
                      - b / 6.0 * cos2SigmaM * (-3.0 + 4.0 * sinSigma * sinSigma)
                        * (-3.0 + 4.0 * cos2SigmaM * cos2SigmaM)));

            return Wgs84B * a * (sigma - deltaSigma);
        }

        /// <summary>Spherical great-circle distance in meters (haversine).</summary>
        private static double HaversineMeters(GeoCoordinate from, GeoCoordinate to)
        {
            double lat1 = from.Latitude * Deg;
            double lat2 = to.Latitude * Deg;
            double dLat = lat2 - lat1;
            double dLon = (to.Longitude - from.Longitude) * Deg;

            double sinLat = Math.Sin(dLat / 2.0);
            double sinLon = Math.Sin(dLon / 2.0);
            double h = sinLat * sinLat + Math.Cos(lat1) * Math.Cos(lat2) * sinLon * sinLon;
            return 2.0 * EarthRadiusMeters * Math.Asin(Math.Min(1.0, Math.Sqrt(h)));
        }

        /// <summary>
        /// Destination coordinate reached by travelling <paramref name="distanceMeters"/>
        /// from <paramref name="from"/> along <paramref name="bearingDegrees"/>
        /// (clockwise from north). Spherical model.
        /// </summary>
        public static GeoCoordinate Offset(GeoCoordinate from, double distanceMeters, double bearingDegrees)
        {
            double angular = distanceMeters / EarthRadiusMeters;
            double bearing = bearingDegrees * Deg;

            double lat1 = from.Latitude * Deg;
            double lon1 = from.Longitude * Deg;

            double lat2 = Math.Asin(Math.Sin(lat1) * Math.Cos(angular)
                                    + Math.Cos(lat1) * Math.Sin(angular) * Math.Cos(bearing));
            double lon2 = lon1 + Math.Atan2(Math.Sin(bearing) * Math.Sin(angular) * Math.Cos(lat1),
                                            Math.Cos(angular) - Math.Sin(lat1) * Math.Sin(lat2));

            return new GeoCoordinate(lat2 * Rad, lon2 * Rad);
        }
    }
}

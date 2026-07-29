//
//  Distance.cs
//  GeoNavKit
//
//  Nautical-mile ↔ meter conversion.
//

namespace GeoNavKit
{
    public static class Distance
    {
        /// <summary>Meters in one nautical mile.</summary>
        public const double MetersPerNauticalMile = 1852.0;

        /// <summary>Interprets <paramref name="nauticalMiles"/> as NM and returns meters.</summary>
        public static double NauticalMilesToMeters(this double nauticalMiles) =>
            nauticalMiles * MetersPerNauticalMile;

        /// <summary>Interprets <paramref name="meters"/> as meters and returns nautical miles.</summary>
        public static double MetersToNauticalMiles(this double meters) =>
            meters / MetersPerNauticalMile;
    }
}

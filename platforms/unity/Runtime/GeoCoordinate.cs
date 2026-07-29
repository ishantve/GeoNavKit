//
//  GeoCoordinate.cs
//  GeoNavKit
//
//  A WGS-84 geographic coordinate in degrees — the C# stand-in for
//  CLLocationCoordinate2D.
//

using System;
using System.Globalization;

namespace GeoNavKit
{
    /// <summary>A WGS-84 geographic coordinate, in degrees.</summary>
    [Serializable]
    public struct GeoCoordinate : IEquatable<GeoCoordinate>
    {
        /// <summary>Latitude in degrees, positive north.</summary>
        public double Latitude;

        /// <summary>Longitude in degrees, positive east.</summary>
        public double Longitude;

        public GeoCoordinate(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        /// <summary>The null island — 0°, 0°.</summary>
        public static GeoCoordinate Zero => new GeoCoordinate(0, 0);

        public bool Equals(GeoCoordinate other) =>
            Latitude.Equals(other.Latitude) && Longitude.Equals(other.Longitude);

        public override bool Equals(object obj) => obj is GeoCoordinate other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Latitude.GetHashCode() * 397) ^ Longitude.GetHashCode();
            }
        }

        public static bool operator ==(GeoCoordinate a, GeoCoordinate b) => a.Equals(b);

        public static bool operator !=(GeoCoordinate a, GeoCoordinate b) => !a.Equals(b);

        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "({0:F6}, {1:F6})", Latitude, Longitude);
    }
}

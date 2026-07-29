//
//  ColliderGeometry.cs
//  GeoNavKit
//
//  Pure geographic shape geometry for colliders and range circles. Each
//  method returns a closed ring of coordinates. NM → metres uses 1 NM = 1852 m.
//

namespace GeoNavKit
{
    public static class ColliderGeometry
    {
        private const double MetersPerNM = Distance.MetersPerNauticalMile;

        /// <summary>
        /// The 4 corners of a heading-aligned diamond (+ closing point):
        /// front → headingDeg, right → +90°, back → +180°, left → +270°.
        /// </summary>
        public static GeoCoordinate[] Diamond(GeoCoordinate center,
                                              double forwardNM, double sideNM,
                                              double headingDeg)
        {
            var pts = new GeoCoordinate[5];
            pts[0] = Geo.Offset(center, forwardNM * MetersPerNM, headingDeg);         // front
            pts[1] = Geo.Offset(center, sideNM * MetersPerNM, headingDeg + 90);       // right
            pts[2] = Geo.Offset(center, forwardNM * MetersPerNM, headingDeg + 180);   // back
            pts[3] = Geo.Offset(center, sideNM * MetersPerNM, headingDeg + 270);      // left
            pts[4] = pts[0];                                                          // close the shape
            return pts;
        }

        /// <summary>The 4 corners of a heading-aligned rectangle as [fL, fR, bR, bL, fL].</summary>
        public static GeoCoordinate[] NoseRect(GeoCoordinate center,
                                               double forwardNM, double sideNM,
                                               double headingDeg)
        {
            var front = Geo.Offset(center, forwardNM * MetersPerNM, headingDeg);
            var back = Geo.Offset(center, forwardNM * MetersPerNM, headingDeg + 180);
            var fR = Geo.Offset(front, sideNM * MetersPerNM, headingDeg + 90);
            var fL = Geo.Offset(front, sideNM * MetersPerNM, headingDeg - 90);
            var bR = Geo.Offset(back, sideNM * MetersPerNM, headingDeg + 90);
            var bL = Geo.Offset(back, sideNM * MetersPerNM, headingDeg - 90);
            return new[] { fL, fR, bR, bL, fL };
        }

        /// <summary>A geographic circle approximated as a polygon with <paramref name="steps"/> segments.</summary>
        public static GeoCoordinate[] Circle(GeoCoordinate center, double radiusNM, int steps = 36)
        {
            var pts = new GeoCoordinate[steps];
            for (int i = 0; i < steps; i++)
            {
                pts[i] = Geo.Offset(center, radiusNM * MetersPerNM, i * 360.0 / steps);
            }
            return pts;
        }
    }
}

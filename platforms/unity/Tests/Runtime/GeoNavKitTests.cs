//
//  GeoNavKitTests.cs
//  GeoNavKit
//
//  Mirrors Tests/GeoNavKitTests/GeoNavKitTests.swift.
//

using System.Collections.Generic;
using NUnit.Framework;

namespace GeoNavKit.Tests
{
    public class GeoNavKitTests
    {
        private static readonly GeoCoordinate Delhi = new GeoCoordinate(28.5665, 77.1031);

        [Test]
        public void BearingDueEast()
        {
            var east = Geo.Offset(Delhi, 5000, 90);
            Assert.AreEqual(90.0, Geo.Bearing(Delhi, east), 0.5);
        }

        [Test]
        public void BearingWrapsIntoZeroTo360()
        {
            var west = Geo.Offset(Delhi, 5000, 270);
            Assert.AreEqual(270.0, Geo.Bearing(Delhi, west), 0.5);

            var north = Geo.Offset(Delhi, 5000, 0);
            double b = Geo.Bearing(Delhi, north);
            Assert.GreaterOrEqual(b, 0.0);
            Assert.Less(b, 360.0);
        }

        [Test]
        public void DistanceRoundTrip()
        {
            // Offset() is spherical, DistanceMeters() is ellipsoidal (WGS-84), so a
            // round-trip carries a small (~0.1%) model mismatch — allow for it.
            var p = Geo.Offset(Delhi, 1852, 45);
            Assert.AreEqual(1852.0, Geo.DistanceMeters(Delhi, p), 5.0);
        }

        [Test]
        public void DistanceOfCoincidentPointsIsZero()
        {
            Assert.AreEqual(0.0, Geo.DistanceMeters(Delhi, new GeoCoordinate(28.5665, 77.1031)));
        }

        [Test]
        public void DistanceMatchesKnownGeodesic()
        {
            var jfk = new GeoCoordinate(40.6413, -73.7781);
            var lax = new GeoCoordinate(33.9416, -118.4085);
            Assert.AreEqual(3_983_080.0, Geo.DistanceMeters(jfk, lax), 5.0);
        }

        [Test]
        public void DistanceIsSymmetric()
        {
            var p = Geo.Offset(Delhi, 250_000, 137);
            Assert.AreEqual(Geo.DistanceMeters(Delhi, p), Geo.DistanceMeters(p, Delhi), 1e-6);
        }

        [Test]
        public void NearAntipodalDistanceFallsBack()
        {
            var antipode = new GeoCoordinate(-28.5665, -102.8969);
            double d = Geo.DistanceMeters(Delhi, antipode);
            Assert.Greater(d, 19_000_000.0);
        }

        [Test]
        public void NauticalMileConversion()
        {
            Assert.AreEqual(1852.0, Distance.MetersPerNauticalMile);
            Assert.AreEqual(3704.0, 2.0.NauticalMilesToMeters(), 0.001);
            Assert.AreEqual(2.0, 3704.0.MetersToNauticalMiles(), 1e-9);
        }

        [Test]
        public void CircleHasStepsPointsAtRadius()
        {
            var ring = ColliderGeometry.Circle(Delhi, 2.5);
            Assert.AreEqual(36, ring.Length);
            foreach (var p in ring)
            {
                double dNM = Geo.DistanceMeters(Delhi, p) / Distance.MetersPerNauticalMile;
                Assert.AreEqual(2.5, dNM, 0.05);
            }
        }

        [Test]
        public void CircleHonoursCustomStepCount()
        {
            Assert.AreEqual(8, ColliderGeometry.Circle(Delhi, 1, 8).Length);
        }

        [Test]
        public void DiamondIsClosed()
        {
            var d = ColliderGeometry.Diamond(Delhi, 0.6, 0.6, 45);
            Assert.AreEqual(5, d.Length);
            Assert.AreEqual(d[0].Latitude, d[4].Latitude, 1e-9);
            Assert.AreEqual(d[0].Longitude, d[4].Longitude, 1e-9);
        }

        [Test]
        public void NoseRectIsClosed()
        {
            var r = ColliderGeometry.NoseRect(Delhi, 1, 0.5, 90);
            Assert.AreEqual(5, r.Length);
            Assert.AreEqual(r[0], r[4]);
        }

        [Test]
        public void FixedSpacedReturnsCount()
        {
            var pts = new List<GeoCoordinate>();
            var p = Delhi;
            for (int i = 0; i < 25; i++)
            {
                pts.Add(p);
                p = Geo.Offset(p, 0.3 * 1852, 0);
            }
            Assert.AreEqual(6, TrailSampler.FixedSpaced(pts, 6, 0.6).Count);
        }

        [Test]
        public void FixedSpacedProjectsBackwardWhenHistoryIsShort()
        {
            var a = Delhi;
            var b = Geo.Offset(a, 200, 0);
            Assert.AreEqual(4, TrailSampler.FixedSpaced(new List<GeoCoordinate> { a, b }, 4, 0.6).Count);
        }

        [Test]
        public void FixedSpacedNeedsTwoSamples()
        {
            Assert.IsEmpty(TrailSampler.FixedSpaced(new List<GeoCoordinate> { Delhi }, 4, 0.6));
        }

        [Test]
        public void EqualSpacedReturnsCountOldestToNewest()
        {
            var pts = new List<GeoCoordinate>();
            var p = Delhi;
            for (int i = 0; i < 8; i++)
            {
                pts.Add(p);
                p = Geo.Offset(p, 500, 0);
            }
            var dots = TrailSampler.EqualSpaced(pts, 6);
            Assert.AreEqual(6, dots.Count);
            for (int i = 1; i < dots.Count; i++)
            {
                Assert.Greater(dots[i].Latitude, dots[i - 1].Latitude);
            }
        }

        [Test]
        public void EqualSpacedToleratesSingleSample()
        {
            var dots = TrailSampler.EqualSpaced(new List<GeoCoordinate> { Delhi }, 6);
            Assert.AreEqual(1, dots.Count);
            Assert.AreEqual(Delhi, dots[0]);
        }

        [Test]
        public void EqualSpacedCollapsesStationaryTrack()
        {
            var dots = TrailSampler.EqualSpaced(new List<GeoCoordinate> { Delhi, Delhi, Delhi }, 6);
            Assert.AreEqual(1, dots.Count);
        }
    }
}

// Mirrors Tests/GeoNavKitTests/GeoNavKitTests.swift, run against the built
// ESM output with Node's built-in test runner (no test framework dependency).
import test from 'node:test';
import assert from 'node:assert/strict';

import {
  Geo,
  Distance,
  ColliderGeometry,
  TrailSampler,
  coordinate,
  nauticalMilesToMeters,
  metersToNauticalMiles,
} from '../dist/esm/index.js';

const delhi = coordinate(28.5665, 77.1031);

/** assert.equal with a tolerance, matching XCTAssertEqual(_:_:accuracy:). */
function close(actual, expected, accuracy, message) {
  assert.ok(
    Math.abs(actual - expected) <= accuracy,
    message ?? `${actual} is not within ${accuracy} of ${expected}`
  );
}

test('bearing due east', () => {
  const east = Geo.offset(delhi, 5000, 90);
  close(Geo.bearing(delhi, east), 90, 0.5);
});

test('bearing wraps into 0–360', () => {
  const west = Geo.offset(delhi, 5000, 270);
  close(Geo.bearing(delhi, west), 270, 0.5);
  const north = Geo.offset(delhi, 5000, 0);
  const b = Geo.bearing(delhi, north);
  assert.ok(b >= 0 && b < 360);
  close(b, 0, 0.5);
});

test('distance round trip', () => {
  // offset() is spherical, distanceMeters() is ellipsoidal (WGS-84), so a
  // round-trip carries a small (~0.1%) model mismatch — allow for it.
  const p = Geo.offset(delhi, 1852, 45);
  close(Geo.distanceMeters(delhi, p), 1852, 5.0);
});

test('distance of coincident points is zero', () => {
  assert.equal(Geo.distanceMeters(delhi, coordinate(28.5665, 77.1031)), 0);
});

test('distance matches a known geodesic (JFK → LAX)', () => {
  const jfk = coordinate(40.6413, -73.7781);
  const lax = coordinate(33.9416, -118.4085);
  // Reference value from CLLocation.distance(from:) / Vincenty on WGS-84.
  close(Geo.distanceMeters(jfk, lax), 3_983_080, 5);
});

test('distance is symmetric', () => {
  const p = Geo.offset(delhi, 250_000, 137);
  close(
    Geo.distanceMeters(delhi, p),
    Geo.distanceMeters(p, delhi),
    1e-6
  );
});

test('near-antipodal distance falls back without throwing', () => {
  const antipode = coordinate(-28.5665, -102.8969);
  const d = Geo.distanceMeters(delhi, antipode);
  assert.ok(Number.isFinite(d) && d > 19_000_000);
});

test('nautical mile conversion', () => {
  assert.equal(Distance.metersPerNauticalMile, 1852);
  close(nauticalMilesToMeters(2), 3704, 0.001);
  close(metersToNauticalMiles(3704), 2, 1e-9);
});

test('circle has `steps` points at radius', () => {
  const ring = ColliderGeometry.circle(delhi, 2.5);
  assert.equal(ring.length, 36);
  for (const p of ring) {
    const dNM = Geo.distanceMeters(delhi, p) / Distance.metersPerNauticalMile;
    close(dNM, 2.5, 0.05);
  }
});

test('circle honours a custom step count', () => {
  assert.equal(ColliderGeometry.circle(delhi, 1, 8).length, 8);
});

test('diamond is closed', () => {
  const d = ColliderGeometry.diamond(delhi, 0.6, 0.6, 45);
  assert.equal(d.length, 5);
  close(d[0].latitude, d[4].latitude, 1e-9);
  close(d[0].longitude, d[4].longitude, 1e-9);
});

test('noseRect is closed and 5 points', () => {
  const r = ColliderGeometry.noseRect(delhi, 1, 0.5, 90);
  assert.equal(r.length, 5);
  close(r[0].latitude, r[4].latitude, 1e-12);
  close(r[0].longitude, r[4].longitude, 1e-12);
});

test('fixedSpaced returns count', () => {
  const pts = [];
  let p = delhi;
  for (let i = 0; i < 25; i++) {
    pts.push(p);
    p = Geo.offset(p, 0.3 * 1852, 0);
  }
  assert.equal(TrailSampler.fixedSpaced(pts, 6, 0.6).length, 6);
});

test('fixedSpaced projects backward when history is short', () => {
  const a = delhi;
  const b = Geo.offset(a, 200, 0);
  const dots = TrailSampler.fixedSpaced([a, b], 4, 0.6);
  assert.equal(dots.length, 4);
});

test('fixedSpaced needs at least two samples', () => {
  assert.deepEqual(TrailSampler.fixedSpaced([delhi], 4, 0.6), []);
});

test('equalSpaced returns count, oldest to newest', () => {
  const pts = [];
  let p = delhi;
  for (let i = 0; i < 8; i++) {
    pts.push(p);
    p = Geo.offset(p, 500, 0);
  }
  const dots = TrailSampler.equalSpaced(pts, 6);
  assert.equal(dots.length, 6);
  for (let i = 1; i < dots.length; i++) {
    assert.ok(dots[i].latitude > dots[i - 1].latitude);
  }
});

test('equalSpaced tolerates a single sample', () => {
  assert.deepEqual(TrailSampler.equalSpaced([delhi], 6), [delhi]);
});

test('equalSpaced collapses a stationary track to one point', () => {
  const dots = TrailSampler.equalSpaced([delhi, delhi, delhi], 6);
  assert.equal(dots.length, 1);
});

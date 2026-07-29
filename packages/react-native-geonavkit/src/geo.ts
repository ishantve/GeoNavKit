/**
 * Great-circle geographic helpers: bearing, distance, and offset. Pure maths
 * over {@link Coordinate} — no platform dependencies, so the same code runs on
 * iOS, Android, web and Node.
 */

import type { Coordinate } from './types.js';

const DEG = Math.PI / 180;
const RAD = 180 / Math.PI;

/** Mean Earth radius in meters — the sphere used by {@link offset}. */
const EARTH_RADIUS_METERS = 6_371_000;

// WGS-84 ellipsoid, used by `distanceMeters` to match Apple's
// `CLLocation.distance(from:)` and Android's `Location.distanceBetween`.
const WGS84_A = 6_378_137.0; // semi-major axis
const WGS84_F = 1 / 298.257223563; // flattening
const WGS84_B = WGS84_A * (1 - WGS84_F); // semi-minor axis

/** Initial bearing in degrees (0–360) from one coordinate to another. */
export function bearing(from: Coordinate, to: Coordinate): number {
  const lat1 = from.latitude * DEG;
  const lat2 = to.latitude * DEG;
  const dLon = (to.longitude - from.longitude) * DEG;

  const y = Math.sin(dLon) * Math.cos(lat2);
  const x =
    Math.cos(lat1) * Math.sin(lat2) -
    Math.sin(lat1) * Math.cos(lat2) * Math.cos(dLon);
  const deg = Math.atan2(y, x) * RAD;

  return (deg + 360) % 360;
}

/**
 * Geodesic (WGS-84) distance in meters between two coordinates.
 *
 * Uses Vincenty's inverse formula, which agrees with
 * `CLLocation.distance(from:)` to well under a meter. For the rare
 * near-antipodal pairs where Vincenty does not converge, falls back to the
 * haversine great-circle distance.
 */
export function distanceMeters(from: Coordinate, to: Coordinate): number {
  const L = (to.longitude - from.longitude) * DEG;
  const U1 = Math.atan((1 - WGS84_F) * Math.tan(from.latitude * DEG));
  const U2 = Math.atan((1 - WGS84_F) * Math.tan(to.latitude * DEG));
  const sinU1 = Math.sin(U1);
  const cosU1 = Math.cos(U1);
  const sinU2 = Math.sin(U2);
  const cosU2 = Math.cos(U2);

  let lambda = L;
  let sinSigma = 0;
  let cosSigma = 0;
  let sigma = 0;
  let cosSqAlpha = 0;
  let cos2SigmaM = 0;
  let converged = false;

  for (let i = 0; i < 200; i++) {
    const sinLambda = Math.sin(lambda);
    const cosLambda = Math.cos(lambda);

    sinSigma = Math.hypot(
      cosU2 * sinLambda,
      cosU1 * sinU2 - sinU1 * cosU2 * cosLambda
    );
    if (sinSigma === 0) return 0; // coincident points

    cosSigma = sinU1 * sinU2 + cosU1 * cosU2 * cosLambda;
    sigma = Math.atan2(sinSigma, cosSigma);

    const sinAlpha = (cosU1 * cosU2 * sinLambda) / sinSigma;
    cosSqAlpha = 1 - sinAlpha * sinAlpha;
    cos2SigmaM =
      cosSqAlpha === 0 ? 0 : cosSigma - (2 * sinU1 * sinU2) / cosSqAlpha;

    const C =
      (WGS84_F / 16) * cosSqAlpha * (4 + WGS84_F * (4 - 3 * cosSqAlpha));
    const previous = lambda;
    lambda =
      L +
      (1 - C) *
        WGS84_F *
        sinAlpha *
        (sigma +
          C *
            sinSigma *
            (cos2SigmaM + C * cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM)));

    if (Math.abs(lambda - previous) < 1e-12) {
      converged = true;
      break;
    }
  }

  if (!converged) return haversineMeters(from, to);

  const uSq =
    (cosSqAlpha * (WGS84_A * WGS84_A - WGS84_B * WGS84_B)) /
    (WGS84_B * WGS84_B);
  const A =
    1 + (uSq / 16384) * (4096 + uSq * (-768 + uSq * (320 - 175 * uSq)));
  const B = (uSq / 1024) * (256 + uSq * (-128 + uSq * (74 - 47 * uSq)));
  const deltaSigma =
    B *
    sinSigma *
    (cos2SigmaM +
      (B / 4) *
        (cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM) -
          (B / 6) *
            cos2SigmaM *
            (-3 + 4 * sinSigma * sinSigma) *
            (-3 + 4 * cos2SigmaM * cos2SigmaM)));

  return WGS84_B * A * (sigma - deltaSigma);
}

/** Spherical great-circle distance in meters (haversine). */
function haversineMeters(from: Coordinate, to: Coordinate): number {
  const lat1 = from.latitude * DEG;
  const lat2 = to.latitude * DEG;
  const dLat = lat2 - lat1;
  const dLon = (to.longitude - from.longitude) * DEG;

  const h =
    Math.sin(dLat / 2) ** 2 +
    Math.cos(lat1) * Math.cos(lat2) * Math.sin(dLon / 2) ** 2;
  return 2 * EARTH_RADIUS_METERS * Math.asin(Math.min(1, Math.sqrt(h)));
}

/**
 * Destination coordinate reached by travelling `distanceMeters` from `from`
 * along `bearingDegrees` (clockwise from north). Spherical model, matching the
 * Swift implementation exactly.
 */
export function offset(
  from: Coordinate,
  distanceMeters: number,
  bearingDegrees: number
): Coordinate {
  const angular = distanceMeters / EARTH_RADIUS_METERS;
  const brng = bearingDegrees * DEG;

  const lat1 = from.latitude * DEG;
  const lon1 = from.longitude * DEG;

  const lat2 = Math.asin(
    Math.sin(lat1) * Math.cos(angular) +
      Math.cos(lat1) * Math.sin(angular) * Math.cos(brng)
  );
  const lon2 =
    lon1 +
    Math.atan2(
      Math.sin(brng) * Math.sin(angular) * Math.cos(lat1),
      Math.cos(angular) - Math.sin(lat1) * Math.sin(lat2)
    );

  return { latitude: lat2 * RAD, longitude: lon2 * RAD };
}

/** Namespace object mirroring the Swift `Geo` enum. */
export const Geo = { bearing, distanceMeters, offset } as const;

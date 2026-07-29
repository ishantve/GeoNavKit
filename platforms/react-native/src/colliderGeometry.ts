/**
 * Pure geographic shape geometry for colliders and range circles. Each
 * function returns a closed ring of coordinates. NM → metres uses 1 NM = 1852 m.
 */

import { offset } from './geo.js';
import type { Coordinate } from './types.js';

const M_PER_NM = 1852;

/**
 * The 4 corners of a heading-aligned diamond (+ closing point):
 * front → headingDeg, right → +90°, back → +180°, left → +270°.
 */
export function diamond(
  center: Coordinate,
  forwardNM: number,
  sideNM: number,
  headingDeg: number
): Coordinate[] {
  const offsets: Array<[number, number]> = [
    [forwardNM * M_PER_NM, headingDeg], // front
    [sideNM * M_PER_NM, headingDeg + 90], // right
    [forwardNM * M_PER_NM, headingDeg + 180], // back
    [sideNM * M_PER_NM, headingDeg + 270], // left
  ];
  const pts = offsets.map(([d, b]) => offset(center, d, b));
  pts.push(pts[0]!); // close the shape
  return pts;
}

/** The 4 corners of a heading-aligned rectangle as [fL, fR, bR, bL, fL]. */
export function noseRect(
  center: Coordinate,
  forwardNM: number,
  sideNM: number,
  headingDeg: number
): Coordinate[] {
  const front = offset(center, forwardNM * M_PER_NM, headingDeg);
  const back = offset(center, forwardNM * M_PER_NM, headingDeg + 180);
  const fR = offset(front, sideNM * M_PER_NM, headingDeg + 90);
  const fL = offset(front, sideNM * M_PER_NM, headingDeg - 90);
  const bR = offset(back, sideNM * M_PER_NM, headingDeg + 90);
  const bL = offset(back, sideNM * M_PER_NM, headingDeg - 90);
  return [fL, fR, bR, bL, fL];
}

/** A geographic circle approximated as a polygon with `steps` segments. */
export function circle(
  center: Coordinate,
  radiusNM: number,
  steps = 36
): Coordinate[] {
  const pts: Coordinate[] = [];
  for (let i = 0; i < steps; i++) {
    pts.push(offset(center, radiusNM * M_PER_NM, (i * 360) / steps));
  }
  return pts;
}

/** Namespace object mirroring the Swift `ColliderGeometry` enum. */
export const ColliderGeometry = { diamond, noseRect, circle } as const;

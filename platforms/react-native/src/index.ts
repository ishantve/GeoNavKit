/**
 * GeoNavKit — lightweight, dependency-free geospatial math.
 *
 * A faithful port of the Swift package: same function names, same maths, no
 * native modules. Runs unchanged on React Native (iOS + Android), Expo, web
 * and Node.
 */

export type { Coordinate } from './types.js';
export { coordinate } from './types.js';

export { Geo, bearing, distanceMeters, offset } from './geo.js';
export { Distance, nauticalMilesToMeters, metersToNauticalMiles } from './distance.js';
export {
  ColliderGeometry,
  circle,
  diamond,
  noseRect,
} from './colliderGeometry.js';
export { TrailSampler, equalSpaced, fixedSpaced } from './trailSampler.js';

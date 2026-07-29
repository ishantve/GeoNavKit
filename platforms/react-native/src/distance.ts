/**
 * Nautical-mile ↔ meter conversion.
 */

export const Distance = {
  /** Meters in one nautical mile. */
  metersPerNauticalMile: 1852,
} as const;

/** Interprets `nm` as nautical miles and returns the equivalent in meters. */
export function nauticalMilesToMeters(nm: number): number {
  return nm * Distance.metersPerNauticalMile;
}

/** Interprets `meters` as meters and returns the equivalent in nautical miles. */
export function metersToNauticalMiles(meters: number): number {
  return meters / Distance.metersPerNauticalMile;
}

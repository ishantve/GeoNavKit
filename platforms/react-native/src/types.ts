/**
 * A WGS-84 geographic coordinate in degrees.
 *
 * Field names match `react-native-maps`' `LatLng`, so any ring returned by
 * GeoNavKit can be handed straight to `<Polygon coordinates={...} />`.
 */
export interface Coordinate {
  latitude: number;
  longitude: number;
}

/** Convenience constructor — mirrors `CLLocationCoordinate2D(latitude:longitude:)`. */
export function coordinate(latitude: number, longitude: number): Coordinate {
  return { latitude, longitude };
}

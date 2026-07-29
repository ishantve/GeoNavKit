# @ishant89/react-native-geonavkit

[![npm](https://img.shields.io/npm/v/@ishant89/react-native-geonavkit.svg)](https://www.npmjs.com/package/@ishant89/react-native-geonavkit)
[![types](https://img.shields.io/npm/types/@ishant89/react-native-geonavkit.svg)](https://www.npmjs.com/package/@ishant89/react-native-geonavkit)
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg)](LICENSE)

Lightweight, dependency-free **geospatial math** for React Native, Expo, web and
Node. Great-circle bearing/distance/destination, nautical-mile conversion,
geographic shape polygons, and path resampling — all pure functions.

This is the JavaScript/TypeScript port of the Swift
[GeoNavKit](https://github.com/ishantve/GeoNavKit) package: same function names,
same maths.

**No native module.** It is plain TypeScript, so there is nothing to link, no
`pod install`, and no config plugin — it works in Expo Go, bare React Native on
both iOS and Android, the browser, and Node alike.

## Installation

```sh
npm install @ishant89/react-native-geonavkit
# or
yarn add @ishant89/react-native-geonavkit
# or
npx expo install @ishant89/react-native-geonavkit
```

Zero runtime dependencies. Ships ESM + CJS builds and its own TypeScript types.

## Usage

```ts
import {
  Geo,
  Distance,
  ColliderGeometry,
  TrailSampler,
  nauticalMilesToMeters,
} from '@ishant89/react-native-geonavkit';

const jfk = { latitude: 40.6413, longitude: -73.7781 };
const lax = { latitude: 33.9416, longitude: -118.4085 };

// Bearing & distance
const heading = Geo.bearing(jfk, lax);        // ≈ 274°
const meters  = Geo.distanceMeters(jfk, lax); // ≈ 3,983,080 m

// Destination point: 10 NM north-east of JFK
const waypoint = Geo.offset(jfk, 10 * Distance.metersPerNauticalMile, 45);

// Unit conversion
const meters20NM = nauticalMilesToMeters(20); // 37,040

// A 5 NM geo-fence ring around JFK
const ring = ColliderGeometry.circle(jfk, 5);

// Trail dots: 6 evenly-spaced points along a recent track
const dots = TrailSampler.equalSpaced(recentPositions, 6);
```

### With `react-native-maps`

`Coordinate` is `{ latitude, longitude }` — structurally identical to
`react-native-maps`' `LatLng` — so any ring drops straight into a `<Polygon>`:

```tsx
import MapView, { Polygon, Marker } from 'react-native-maps';
import { ColliderGeometry, TrailSampler } from '@ishant89/react-native-geonavkit';

<MapView initialRegion={region}>
  <Polygon
    coordinates={ColliderGeometry.circle(aircraft, 5)}
    strokeColor="#ff9500"
    fillColor="rgba(255,149,0,0.15)"
  />
  <Polygon
    coordinates={ColliderGeometry.diamond(aircraft, 0.6, 0.6, headingDeg)}
    strokeColor="#34c759"
  />
  {TrailSampler.fixedSpaced(history, 6, 0.6).map((dot, i) => (
    <Marker key={i} coordinate={dot} anchor={{ x: 0.5, y: 0.5 }} />
  ))}
</MapView>
```

## API

Every export is a pure function — there is no state to manage. Functions are
also grouped into namespace objects (`Geo`, `Distance`, `ColliderGeometry`,
`TrailSampler`) mirroring the Swift `enum` namespaces; use whichever style you
prefer.

### `Coordinate`

```ts
interface Coordinate {
  latitude: number;   // degrees
  longitude: number;  // degrees
}

coordinate(latitude, longitude): Coordinate  // convenience constructor
```

### `Geo` — great-circle navigation math

| Function | Returns |
|---|---|
| `bearing(from, to)` | initial bearing in degrees, `0–360` |
| `distanceMeters(from, to)` | geodesic (WGS-84) distance in meters |
| `offset(from, distanceMeters, bearingDegrees)` | destination `Coordinate` |

### `Distance` — nautical-mile ↔ meter conversion

| Export | Value |
|---|---|
| `Distance.metersPerNauticalMile` | `1852` |
| `nauticalMilesToMeters(nm)` | meters |
| `metersToNauticalMiles(meters)` | nautical miles |

### `ColliderGeometry` — closed coordinate rings

| Function | Shape |
|---|---|
| `circle(center, radiusNM, steps = 36)` | circle polygon (geo-fence / range ring), `steps` points |
| `diamond(center, forwardNM, sideNM, headingDeg)` | heading-aligned diamond, 5 points (closed) |
| `noseRect(center, forwardNM, sideNM, headingDeg)` | heading-aligned rectangle, 5 points (closed) |

### `TrailSampler` — dots along a recent path

| Function | Behaviour |
|---|---|
| `equalSpaced(history, count)` | `count` points spread evenly over the last 8 samples, oldest → newest |
| `fixedSpaced(history, count, spacingNM)` | exactly `count` points `spacingNM` apart walking back from the newest; short history is projected backward |

## Notes on accuracy

- `distanceMeters` uses **Vincenty's inverse formula** on the WGS-84 ellipsoid,
  which agrees with iOS `CLLocation.distance(from:)` and Android
  `Location.distanceBetween` to well under a meter. Near-antipodal pairs, where
  Vincenty does not converge, fall back to a spherical haversine distance.
- `offset` uses a spherical model (mean Earth radius 6,371 km), matching the
  Swift implementation exactly. Over short ranges the two models agree to
  within ~0.1%; for sub-meter precision at large distances, prefer a dedicated
  geodesic library.

## Requirements

| | |
|---|---|
| TypeScript | 4.7+ (for the `exports` map); JS works anywhere |
| Node | 18+ |
| React Native | any version; no native code, no linking |
| Dependencies | none |

## Related packages

| Platform | Package |
|---|---|
| Swift (iOS/macOS) | [`GeoNavKit`](https://github.com/ishantve/GeoNavKit) — SPM + CocoaPods |
| Unity (C#) | [`com.ishant89.geonavkit`](https://github.com/ishantve/GeoNavKit/tree/main/packages/unity) — UPM |

## License

MIT. See [LICENSE](LICENSE).

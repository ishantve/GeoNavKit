# GeoNavKit for Unity

Lightweight, dependency-free **geospatial math** for Unity. Great-circle
bearing/distance/destination, nautical-mile conversion, geographic shape
polygons, and path resampling — all pure static methods.

This is the C# port of the Swift [GeoNavKit](https://github.com/ishantve/GeoNavKit)
package: same method names, same maths.

The runtime assembly declares `noEngineReferences`, so the maths has **no
`UnityEngine` dependency** — it runs in the editor, at runtime, in tests, and in
plain .NET code alike.

## Installation

### Via Package Manager (Git URL)

**Window → Package Manager → + → Add package from git URL…** and paste:

```
https://github.com/ishantve/GeoNavKit.git?path=platforms/unity
```

Pin a version by appending a tag:

```
https://github.com/ishantve/GeoNavKit.git?path=platforms/unity#1.0.2
```

### Via `manifest.json`

Add to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.ishant89.geonavkit": "https://github.com/ishantve/GeoNavKit.git?path=platforms/unity#1.0.2"
  }
}
```

Requires Unity **2021.3** or newer.

## Usage

```csharp
using UnityEngine;
using GeoNavKit;

public class RangeRing : MonoBehaviour
{
    void Start()
    {
        var jfk = new GeoCoordinate(40.6413, -73.7781);
        var lax = new GeoCoordinate(33.9416, -118.4085);

        // Bearing & distance
        double heading = Geo.Bearing(jfk, lax);        // ≈ 274°
        double meters  = Geo.DistanceMeters(jfk, lax); // ≈ 3,983,080 m

        // Destination point: 10 NM north-east of JFK
        var waypoint = Geo.Offset(jfk, 10 * Distance.MetersPerNauticalMile, 45);

        // Unit conversion (extension methods on double)
        double meters20NM = 20.0.NauticalMilesToMeters(); // 37,040

        // A 5 NM geo-fence ring around JFK
        GeoCoordinate[] ring = ColliderGeometry.Circle(jfk, 5);

        // Trail dots: 6 evenly-spaced points along a recent track
        var dots = TrailSampler.EqualSpaced(history, 6);

        Debug.Log($"{heading:F1}° / {meters:F0} m / {ring.Length} ring points");
    }
}
```

### Drawing a ring with `LineRenderer`

GeoNavKit returns geographic coordinates, not scene positions — project them
with whatever mapping your scene uses. A simple local-tangent-plane projection
around an origin:

```csharp
Vector3 ToLocal(GeoCoordinate origin, GeoCoordinate p)
{
    double metersPerDegLat = 111_132.0;
    double metersPerDegLon = 111_320.0 * Mathf.Cos((float)(origin.Latitude * Mathf.Deg2Rad));
    return new Vector3(
        (float)((p.Longitude - origin.Longitude) * metersPerDegLon),
        0f,
        (float)((p.Latitude  - origin.Latitude)  * metersPerDegLat));
}

var ring = ColliderGeometry.Circle(center, radiusNM: 5);
var line = GetComponent<LineRenderer>();
line.loop = true;
line.positionCount = ring.Length;
for (int i = 0; i < ring.Length; i++) line.SetPosition(i, ToLocal(center, ring[i]));
```

## API

### `GeoCoordinate`

A serializable `struct` with `double Latitude` / `double Longitude` in degrees,
value equality, and `GeoCoordinate.Zero`.

### `Geo` — great-circle navigation math

| Method | Returns |
|---|---|
| `Geo.Bearing(from, to)` | initial bearing in degrees, `0–360` |
| `Geo.DistanceMeters(from, to)` | geodesic (WGS-84) distance in meters |
| `Geo.Offset(from, distanceMeters, bearingDegrees)` | destination `GeoCoordinate` |

### `Distance` — nautical-mile ↔ meter conversion

| Member | Value |
|---|---|
| `Distance.MetersPerNauticalMile` | `1852` |
| `someDouble.NauticalMilesToMeters()` | meters |
| `someDouble.MetersToNauticalMiles()` | nautical miles |

### `ColliderGeometry` — closed coordinate rings

| Method | Shape |
|---|---|
| `Circle(center, radiusNM, steps = 36)` | circle polygon (geo-fence / range ring), `steps` points |
| `Diamond(center, forwardNM, sideNM, headingDeg)` | heading-aligned diamond, 5 points (closed) |
| `NoseRect(center, forwardNM, sideNM, headingDeg)` | heading-aligned rectangle, 5 points (closed) |

All three return `GeoCoordinate[]`.

### `TrailSampler` — dots along a recent path

| Method | Behaviour |
|---|---|
| `EqualSpaced(history, count)` | `count` points spread evenly over the last 8 samples, oldest → newest |
| `FixedSpaced(history, count, spacingNM)` | exactly `count` points `spacingNM` apart walking back from the newest; short history is projected backward |

Both take `IReadOnlyList<GeoCoordinate>` and return `List<GeoCoordinate>`.

## Notes on accuracy

- `DistanceMeters` uses **Vincenty's inverse formula** on the WGS-84 ellipsoid,
  matching iOS `CLLocation.distance(from:)` to well under a meter.
  Near-antipodal pairs, where Vincenty does not converge, fall back to a
  spherical haversine distance.
- `Offset` uses a spherical model (mean Earth radius 6,371 km), matching the
  Swift implementation exactly. Over short ranges the two models agree to within
  ~0.1%.
- All maths is `double`, not `float` — convert to `float` only at the point you
  hand positions to Unity.

## Tests

The package ships an NUnit suite under `Tests/Runtime`. To run it, enable
testables in your project's `Packages/manifest.json`:

```json
{
  "testables": ["com.ishant89.geonavkit"]
}
```

then open **Window → General → Test Runner**.

## Related packages

| Platform | Package |
|---|---|
| Swift (iOS/macOS) | [`GeoNavKit`](https://github.com/ishantve/GeoNavKit) — SPM + CocoaPods |
| JS / React Native | [`@ishant89/react-native-geonavkit`](https://www.npmjs.com/package/@ishant89/react-native-geonavkit) — npm |

## License

MIT. See [LICENSE.md](LICENSE.md).

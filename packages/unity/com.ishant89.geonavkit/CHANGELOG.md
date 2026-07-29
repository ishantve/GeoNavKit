# Changelog

All notable changes to the Unity package are documented here. The format is
based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) and this
package adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.1] - 2026-07-29

### Added
- Initial Unity (UPM) release — C# port of the Swift GeoNavKit 1.0.1 API.
- `GeoCoordinate` — serializable lat/lon struct with value equality.
- `Geo` — `Bearing`, `DistanceMeters` (Vincenty on WGS-84, haversine fallback),
  and `Offset`.
- `Distance` — `MetersPerNauticalMile` plus `NauticalMilesToMeters` /
  `MetersToNauticalMiles` extension methods on `double`.
- `ColliderGeometry` — `Circle`, `Diamond`, and `NoseRect` ring builders.
- `TrailSampler` — `EqualSpaced` and `FixedSpaced` path resampling.
- NUnit test suite mirroring the Swift tests.

### Notes
- The version number tracks the Swift package it ports, so the first Unity
  release is 1.0.1 rather than 1.0.0.
- The runtime assembly sets `noEngineReferences`, so the maths has no
  `UnityEngine` dependency.

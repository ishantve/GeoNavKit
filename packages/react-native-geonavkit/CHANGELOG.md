# Changelog

All notable changes to the `@ishant89/react-native-geonavkit` npm package are
documented here. The
format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) and
this package adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.1] - 2026-07-29

### Added
- Initial npm release — TypeScript port of the Swift GeoNavKit 1.0.1 API.
- `Coordinate` / `coordinate()` — `{ latitude, longitude }`, structurally
  compatible with `react-native-maps`' `LatLng`.
- `Geo` — `bearing`, `distanceMeters` (Vincenty on WGS-84, haversine fallback),
  and `offset`.
- `Distance` — `metersPerNauticalMile` plus `nauticalMilesToMeters` and
  `metersToNauticalMiles`.
- `ColliderGeometry` — `circle`, `diamond`, and `noseRect` ring builders.
- `TrailSampler` — `equalSpaced` and `fixedSpaced` path resampling.
- Dual ESM + CJS builds, bundled TypeScript declarations, zero dependencies.
- Test suite (`node:test`) mirroring the Swift tests.

### Notes
- The version number tracks the Swift package it ports, so the first npm release
  is 1.0.1 rather than 1.0.0.

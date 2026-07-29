# Changelog

All notable changes to this project are documented here. The format is based on
[Keep a Changelog](https://keepachangelog.com/en/1.0.0/) and this project
adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.2] - 2026-07-29

### Added
- **npm package** `@ishant89/react-native-geonavkit` (`platforms/react-native`)
  — TypeScript port for React Native, Expo, web and Node. No native module, so
  no `pod install`. See its [CHANGELOG](platforms/react-native/CHANGELOG.md).
- **Unity package** `com.ishant89.geonavkit` (`platforms/unity`) — C# port
  distributed over UPM. See its [CHANGELOG](platforms/unity/CHANGELOG.md).
- Both ports keep the Swift API surface; `distanceMeters` uses Vincenty's
  inverse formula (WGS-84) in place of `CLLocation.distance(from:)`, which is
  unavailable off Apple platforms.

### Changed
- README: React Native and Unity installation sections.
- All three platforms now share one version number and one git tag, so the
  Swift package moves to 1.0.2 with no API changes.

## [1.0.1] - 2026-07-28

### Changed
- README: real SPM install URL and a CocoaPods installation section.
- Docs-only release; no API changes.

## [1.0.0] - 2026-07-28

### Added
- `Geo` — great-circle `bearing(from:to:)`, `distanceMeters(from:to:)`, and
  `offset(from:distanceMeters:bearingDegrees:)`.
- `Distance` — `metersPerNauticalMile` constant and the
  `Double.nauticalMilesToMeters` convenience.
- `ColliderGeometry` — `circle`, `diamond`, and `noseRect` coordinate-ring
  builders.
- `TrailSampler` — `equalSpaced` and `fixedSpaced` path-resampling helpers.
- Unit test suite covering bearing, distance, offset round-trips, and shape
  geometry.

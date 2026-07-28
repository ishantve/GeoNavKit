Pod::Spec.new do |s|
  s.name             = 'GeoNavKit'
  s.version          = '1.0.1'
  s.summary          = 'Lightweight, dependency-free geospatial math for Swift.'
  s.description      = <<-DESC
    Great-circle bearing/distance/destination, nautical-mile conversion,
    geographic shape polygons, and path resampling — pure functions over
    CLLocationCoordinate2D. No third-party dependencies (Foundation + CoreLocation).
  DESC

  s.homepage         = 'https://github.com/ishantve/GeoNavKit'
  s.license          = { :type => 'MIT', :file => 'LICENSE' }
  s.author           = { 'Ishant' => 'ishant@zibaltech.com' }
  s.source           = { :git => 'https://github.com/ishantve/GeoNavKit.git', :tag => s.version.to_s }

  s.ios.deployment_target   = '15.0'
  s.osx.deployment_target   = '12.0'
  s.swift_version           = '5.9'

  s.source_files     = 'Sources/GeoNavKit/**/*.swift'
  s.frameworks       = 'Foundation', 'CoreLocation'
end

# Change Log

All notable changes to this project will be documented in this file.
This project adheres to [Semantic Versioning](http://semver.org/).
This file follows the convention described at
[Keep a Changelog](http://keepachangelog.com/en/1.0.0/).

## [0.11.0] - 2019-04-17
### Changed
- Updated `bindgen` dependency to 0.31 release and fixed compiler issues.
  Enum definitions changed again, default in bindgen is different now, and
  using `default_enum_style()` caused endless compiler errors.
- Made `get_exception_type()`, `get_exception()`, and `clear_exception()`
  on the various wand implementations.

## [0.10.0] - 2018-08-11
### Added
- Mewp: Add ping_image and ping_image_blob functions.
- Mewp: Add reset_image_page function.
- Mewp: Add set_image_alpha_channel function.
- NQNStudios: Adding binding for MagickAddImage function.
- NQNStudios: Adding doc comment and rotate_image function.
- NQNStudios: Adding binding for adaptive_resize_image function.

## [0.9.0] - 2018-05-05
### Added
- Mewp: Numerous additional MagickWand functions
### Changed
- Mewp: crop_image() now returns a Result
### Fixed
- Mewp: Fixed memory management in `string_get!`
- sindreij: Fix exporting pdf->jpeg for multi-page pdf

## [0.8.0] - 2018-02-16
### Added
- little-bobby-tables: add color-related getters and mutations
- sindreij: Add crop_image() to MagickWand

## [0.7.1] - 2017-12-30
### Changed
- gentoo90: Hide more types from bindgen to fix the build for some systems
- gentoo90: Build now supports Windows

## [0.7.0] - 2017-08-26
### Changed
- Upgrade bindgen to 0.29
- little-bobby-tables: Change to MagickWand 7.0;
  this introduces backward incompatible changes...
- `get_quantum` and `set_quantum` now take `Quantum` instead of `u16`
- `resize_image` no longer takes a `blur_factor` argument
- `InterpolatePixelMethod` was renamed `PixelInterpolateMethod`

## [0.6.6] - 2017-07-08
### Changed
- Downgrade to version 0.25.5 of `bindgen` library to avoid errors on Linux.

## [0.6.5] - 2017-07-07
### Added
- Add `compare_images()` method to `MagickWand` type.
### Changed
- Update to latest release of `bindgen` library.

## [0.6.4] - 2017-04-08
### Changed
- Actually set the version this time.

## [0.6.3] - 2017-04-08
### Changed
- Changed to use `pkg-config` crate to get MagickWand compiler settings.
- Fixed bindings generation on FreeBSD (i.e. no longer hard-coded).
- Changed the bindings generation to use `libc` prefix for C types.
- Changed the bindings generation and interface code to use Rust enums.

## [0.6.2] - 2016-10-20
### Changed
- Presence of `pkg-config` checked in `build.rs` script at build time.

## [0.6.1] - 2016-10-16
### Changed
- MagickWand version enforced in `build.rs` script at build time.

## [0.6.0] - 2016-09-20
### Changed
- Update to 0.19.0 version of rust-bindgen; rebuilds are much faster.
- Hacked bindings for FreeBSD systems due to rust-bindgen bug #385.
- gadomski: add `set_option()` method to wand API.
- gadomski: add `write_images_blob()` to create animated GIFs.

## [0.5.2] - 2016-07-17
### Changed
- Streamline error handling in `build.rs` script.
- Fix the crate version number (previously stuck at 0.4.0).

## [0.5.1] - 2016-06-25
### Changed
- hjr3: Changed `read_image_blob()` to borrow data rather than take ownership.

## [0.5.0] - 2016-05-18
### Added
- marjakm: Added numerous functions and enabled cross-compile support.

## [0.4.0] - 2016-03-29
### Added
- Add functions for detecting and correcting image orientation.

## [0.3.3] - 2016-03-17
### Changed
- Allow libc version 0.2 or higher

## [0.3.2] - 2016-02-10
### Changed
- Automatically generate `bindings.rs` using `rust-bindgen` via `build.rs` script.

## [0.3.1] - 2016-01-02
### Changed
- Fix bug `get_image_property()` to ensure C string is copied.

## [0.3.0] - 2016-01-02
### Added
- Add `get_image_property()` function to retrieve, for example, EXIF data.

## [0.2.3] - 2015-12-26
### Changed
- Upgrade to libc 0.2.4 in hopes of fixing downstream build incompatibilities.

## [0.2.2] - 2015-12-23
### Changed
- Change the build to specify the likely path to ImageMagick, for easier setup.

## [0.2.1] - 2015-09-07
### Changed
- Fix the cargo package name (replace dash with underscore).

## [0.2.0] - 2015-06-10
### Added
- Add a `fit()` function for fitting an image to a given bounds.

## [0.1.0] - 2015-06-09
### Changed
- Initial release

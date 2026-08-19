# Changelog

## [3.0.0] - 2026-08-17

### Changed

- [Breaking Change] Task input parameters (output file properties, document settings, and content) are now grouped under `Input` and `Options` parameter to align with Frends platform standards.
- Task method now requires a `CancellationToken` parameter.
- The `Options` class now includes an `ErrorMessageOnFailure` property, allowing you to customize the error message returned or thrown when the task fails.
- When `ThrowErrorOnFailure` is false, the result now includes an `Error` object with the failure details.

## [2.2.0] - 2026-05-28

### Changed

- Removed outdated Linux package requirements from the documentation

## [2.1.0] - 2026-05-06

### Fixed

- Fallback font is now used as an error font as well.
- Add bundled font to use in case any other font can't be resolved.

## [2.0.0] - 2026-02-03

### Changed

- [Breaking Change] Upgrade to .net8.0
- [Breaking Change] Remove the deprecated option to choose Unicode vs. ANSI text (Code is always Unicode now)

### Added

- Add an option to set up a custom fonts directory
- Add an option to set up the default font
- Add support for Linux systems

### Fixed

- Resolve issues with not responsive Windows GDI methods (Failing to get fonts)

## [1.2.0] - 2026-02-03

### Changed

- Rename task prefix from PDF to Pdf

## [1.1.0] - 2024-08-23

### Changed

- Updated the Newtonsoft.Json package to version 13.0.3.

## [1.0.1] - 2023-03-01

### Fixed

- Fixed document link to task portal.
- Added requirements to the summary.

## [1.0.0] - 2022-05-10

### Added

- Initial implementation of Frends.PDF.Create.

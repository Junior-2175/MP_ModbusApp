# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


## [1.2.0] - 2026-05-12

### Added
- **Support Link:** Integrated a "Buy Me a Coffee" link within the AboutBox dialog to support further development.
- **Robust Scanning:** Implemented a retry mechanism in the Device Scan feature, allowing up to 100 polling iterations to ensure communication stability with industrial hardware.

### Changed
- **UI Optimization:** Relocated the Microsoft Store rating link from the main window's StatusStrip to the AboutBox dialog for a cleaner and more professional user interface.
- **Visual Overhaul:** Updated the styling and layout of the AboutBox window to improve readability and aesthetics.

## [1.1.0] - 2026-04-07
### Added
- Added a link to rate the application in the Microsoft Store (in the toolstrip).
- Added a link to the application Changelog (in the toolstrip).

### Fixed
- Fixed an issue causing data gaps in charts when the chart tab was not focused/opened by the user.

- ### Changed
- Improved the Communication Log to display 'DeviceScan (x)' or 'AddressScan (x) - [y]' in the device column during active scanning operations for better readability.

## [1.0.1] - 2026-03-19
### Added
- Initial release of MP Modbus App.
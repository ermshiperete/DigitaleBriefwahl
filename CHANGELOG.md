# Change Log

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

<!-- Available types of changes:
### Added
### Changed
### Fixed
### Deprecated
### Removed
### Security
-->

## [Unreleased]

### Added

- Add launcher app. This decreases the likelyhood that the ballot will be falsely detected
  as a virus on Windows systems.

### Changed

- `wahl.ini` now comes with \r\n line endings to support editing with notepad on Windows
- Ballot written with Windows line endings to support editing with notepad on Windows
- Improve formatting of (empty) ballot

### Fixed

- Sending of emails on Windows if MAPI is not configured. If MAPI fails we now try mailto:.

## [0.1.1] - 2017-11-16

### Changed

- show exception details in ErrorReport
- write votes on the first _n_ nominees in an empty ballot

### Fixed

- YesNo view now allows to vote _N_ if only one candidate

## [0.1.0] - 2017-11-15

### Added

- Initial beta version
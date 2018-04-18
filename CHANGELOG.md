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

- Append log data when sending exception to Bugsnag

### Changed

- Update Bugsnag library and exception handling to v2 API
- [launcher] Improve robustness

## [1.0.2] - 2018-04-09

### Fixed

- [launcher] Catch and ignore CannotUnloadAppDomainException when shutting down app

## [1.0.1] - 2018-04-04

### Fixed

- [packer] Handling of Google Drive URLs

## [1.0.0] - 2018-04-04

### Changed

- Removed analytics of beta version

## [0.3.4] - 2018-04-03

### Changed

- Add Outlook email handler for cases where MAPI isn't set up

### Fixed

- [launcher] Download on Linux

## [0.3.3] - 2018-03-28

### Changed

- Improve console messages
- [packer] Improve handling of Google Drive URLs
- [packer] Check download URL when packing
- Support `wahlurl://` URLs
- Improve handling of emails. Improve detecting Outlook and Thunderbird on Windows.

### Fixed

- [launcher] Fix crash trying to download if offline
- Catch exception when sending email fails
- Fix email handling if no default email client is set in registry but Thunderbird is installed

## [0.3.2] - 2018-03-23

### Fixed

- bugfix on Linux. Now requires Mono >= 5

## [0.3.1] - 2018-03-23

### Fixed

- small bugfix for Windows

## [0.3.0] - 2018-03-23

### Added

- Add support for `.wahlurl` file that contains the URL that the launcher uses to download the
  `.wahl` file
- [packer] Allow to enter download URL, generate `.wahlurl` file and convert Google Drive
  URL to directly downloadable link

### Fixed

- [packer] Detect encoding of `wahl.ini` and rewrite with UTF-8 encoding to prevent crash and
  display special chars correctly

### Changed

- [packer] Wait for keypress if we get an error, not just in success case

## [0.2.2] - 2018-03-07

### Fixed

- Fix Mac OS X installer

### Changed

- Use real random number instead of GUID on ballot and for filenames. GUIDs include the
  network MAC address and so theoretically can be traced back

## [0.2.1] - 2018-03-05

### Fixed

- Fix crash on Windows when checking for updates
- Fix build for Mac OS X

## [0.2.0] - 2018-03-05

### Added

- Add launcher app. This decreases the likelyhood that the ballot will be falsely detected
  as a virus on Windows systems
- Add launcher for Mac OS X (while it was supported previously it required starting from the
  command line which seemed to be too complicated for Mac users)

### Changed

- `wahl.ini` now comes with \r\n line endings to support editing with notepad on Windows
- Ballot written with Windows line endings to support editing with notepad on Windows
- Improve formatting of (empty) ballot
- Add random number to ballot so that two otherwise identical ballots encrypt differently
- Allow to select directory when writing files
- Allow abstention

### Fixed

- Sending of emails on Windows if MAPI is not configured. If MAPI fails we now try `mailto:`.

## [0.1.1] - 2017-11-16

### Changed

- show exception details in ErrorReport
- write votes on the first _n_ nominees in an empty ballot

### Fixed

- YesNo view now allows to vote _N_ if only one candidate

## [0.1.0] - 2017-11-15

### Added

- Initial beta version

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

### Fixed

- gracefully deal with permission exception (#32)
- show correct button text if we have only one election tab
- improve sending emails and logging

## [1.3.0] - 2023-04-03

### Fixed

- when the user aborts sending the MAPI email, we no longer try to send with a different approach
- fixed arguments passed to OutlookEmailProvider
- fixed potential crash figuring out preferred email app
- handle exception during zip extraction (#24)

## [1.2.2] - 2022-04-02

### Fixed

- sending of emails on Linux (again)

## [1.2.1] - 2022-04-01

### Fixed

- Fixed crash if portable thunderbird (or other email program)
  is not available

## [1.2.0] - 2022-03-31

### Added

- support Thunderbird Portable (#12)
- [Packer] Add uploading to Gist (#10)

### Changed

- random number at end of ballot and filename now match
- use .NET Framework 4.7.2 to reduce size

### Fixed

- fixed crash (#13)
- fixed length of ballot filename
- sending emails on Linux

## [1.1.3] - 2021-04-16

### Fixed

- display of progress in Launcher
- sending of emails on Linux and Mac

## [1.1.2] - 2021-04-16

### Fixed

- display of progress in Launcher
- clarified output of launcher

## [1.1.1] - 2021-04-15

### Fixed

- another problem sending emails with Outlook
- copyright date

## [1.1.0] - 2021-04-15

### Added

- sending or viewing logfiles (#5)

### Changed

- improved logging and display of progress

### Fixed

- Sending emails with Outlook (#6)

## [1.0.6] - 2021-04-13

Re-release

## [1.0.5] - 2021-04-13

### Fixed

- sending emails through Thunderbird (#6)

### Changed

- logging output

### Added

- save encrypted ballot to file (#4)

## [1.0.4] - 2020-04-09

### Added

- [Packer] Support Microsoft OneDrive

### Changed

- Update dependencies
- [Packer] Verify file from download link

## [1.0.3] - 2019-04-05

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

[Unreleased]: https://github.com/ermshiperete/DigitaleBriefwahl/compare/v1.3.0...master

[1.3.0]: https://github.com/ermshiperete/DigitaleBriefwahl/compare/v1.2.2...v1.3.0
[1.2.2]: https://github.com/ermshiperete/DigitaleBriefwahl/compare/v1.2.1...v1.2.2
[1.2.1]: https://github.com/ermshiperete/DigitaleBriefwahl/compare/v1.2.0...v1.2.1
[1.2.0]: https://github.com/ermshiperete/DigitaleBriefwahl/compare/v1.1.3...v1.2.0
[1.1.3]: https://github.com/ermshiperete/DigitaleBriefwahl/compare/v1.1.2...v1.1.3
[1.1.2]: https://github.com/ermshiperete/DigitaleBriefwahl/compare/v1.1.1...v1.1.2
[1.1.1]: https://github.com/ermshiperete/DigitaleBriefwahl/compare/v1.1.0...v1.1.1
[1.1.0]: https://github.com/ermshiperete/DigitaleBriefwahl/compare/v1.0.4...v1.1.0
[1.0.6]: https://github.com/ermshiperete/DigitaleBriefwahl/compare/v1.0.4...v1.0.6
[1.0.5]: https://github.com/ermshiperete/DigitaleBriefwahl/compare/v1.0.4...v1.0.5
[1.0.4]: https://github.com/ermshiperete/DigitaleBriefwahl/compare/v1.0.3...v1.0.4
[1.0.3]: https://github.com/ermshiperete/DigitaleBriefwahl/compare/v1.0.2...v1.0.3
[1.0.2]: https://github.com/ermshiperete/DigitaleBriefwahl/compare/v1.0.1...v1.0.2
[1.0.1]: https://github.com/ermshiperete/DigitaleBriefwahl/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/ermshiperete/DigitaleBriefwahl/compare/v0.3.4...v1.0.0
[0.3.4]: https://github.com/ermshiperete/DigitaleBriefwahl/compare/v0.3.3...v0.3.4
[0.3.3]: https://github.com/ermshiperete/DigitaleBriefwahl/compare/v0.3.2...v0.3.3
[0.3.2]: https://github.com/ermshiperete/DigitaleBriefwahl/compare/v0.3.1...v0.3.2
[0.3.1]: https://github.com/ermshiperete/DigitaleBriefwahl/compare/v0.3.0...v0.3.1
[0.3.0]: https://github.com/ermshiperete/DigitaleBriefwahl/compare/v0.2.2...v0.3.0
[0.2.2]: https://github.com/ermshiperete/DigitaleBriefwahl/compare/v0.2.1...v0.2.2
[0.2.1]: https://github.com/ermshiperete/DigitaleBriefwahl/compare/v0.2.0...v0.2.1
[0.2.0]: https://github.com/ermshiperete/DigitaleBriefwahl/compare/v0.1.1...v0.2.0
[0.1.1]: https://github.com/ermshiperete/DigitaleBriefwahl/compare/v0.1...v0.1.1
[0.1.0]: https://github.com/ermshiperete/DigitaleBriefwahl/compare/18cba03...v0.1

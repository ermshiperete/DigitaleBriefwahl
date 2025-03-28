# Release workflow

This document describes how to create a new release.

## Prepare source

- update `CHANGELOG.md` with new version number
- create new changelog entry for debian package: `dch -i`
- commit changes
- create new tag: `git tag -s v1.0.3`

## Building binaries

### Windows

- The binaries for Windows are built on TC. The artifacts from the last release
  will be automatically downloaded from GitHub into the `Releases` folder so
  that squirrel can properly calculate the delta files, but it might necessary
  to delete old versions from `Releases` that are no longer needed.
- Run the build from https://build.palaso.org/viewType.html?buildTypeId=DigitaleBriefwahl
- Download the generated artifacts and attach to GitHub release

### Packer

- The packer binaries can be built on Linux: `msbuild /t:Test build/DigitaleBriefwahl.proj`
- Compress all files from `output/Release/net472` into a `DigitaleBriefwahl.Packer-1.0.3.zip` file.
  You can omit the language subdirectories (`de`, `es`,...) but you'll have to include the
  `lib` subdirectory!

### DigiTally

- Compress all files from `output/Release/DigiTally/net472` into a
  `DigitaleBriefwahl.DigiTally-1.4.0.zip` file.

### MacOS

- The binaries for OSX can be built on Linux: `cd macos && make`
- This will create a package for Mac in `Releases`

### Linux

- Run `dch -r`
- Run `./mksource`
- this creates a source package in the parent directory
- sign the source package: `debsign -k${DEBSIGN_KEYID} ../digitale-briefwahl_1.5.0_source.changes`
- dput the created source package:
  `dput ppa:ermshiperete/digitale-briefwahl ../digitale-briefwahl_1.5.0_source.changes`

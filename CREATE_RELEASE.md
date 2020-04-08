# Release workflow

This document describes how to create a new release.

## Prepare source

- update `CHANGELOG.md` with new version number
- create new changelog entry for debian package: `dch -i`
- commit changes
- create new tag: `git tag -s v1.0.3`

## Building binaries

### Windows

- The binaries for Windows are built on TC. It might be necessary to upload the artifacts from the last
  build to the `Releases` folder so that squirrel can properly calculate the delta files. Also, delete
  old versions from `Releases` that are no longer needed.
- Run the build from https://build.palaso.org/viewType.html?buildTypeId=DigitaleBriefwahl
- Download the generated artifacts

### Packer

- The packer binaries can be built on Linux: `msbuild /t:Test build/DigitaleBriefwahl.proj`
- Compress all files from `output/Release/net461` into a `DigitaleBriefwahl.Packer-1.0.3.zip` file

### MacOS

- The binaries for OSX can be built on Linux: `cd macos && make`
- This will create a package for Mac in `Releases`

### Linux

- Run `./mksource`
- this creates a source package in the parent directory
- dput the created source package:
  `dput ppa:ermshiperete/digitale-briefwahl ../digitale-briefwahl_1.0.3_source.changes`

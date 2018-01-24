# Mac OS X installer

This directory contains the necessary files and scripts to create an installer for Mac OS X
on a Linux machine.

## Prerequisites

- [xar](https://github.com/mackyle/xar)
- [BomUtils](https://github.com/hogliux/bomutils)

## Build installer

- Compile binaries:

		xbuild /t:Build build/DigitaleBriefwahl.proj

- `make`

The makefile adjusts the number of files, size and version. However, if the minimum required OS X
version or names etc changes, this must be adjusted in the `Makefile`.

The installer will end up in `../Releases`.
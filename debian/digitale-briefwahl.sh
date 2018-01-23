#!/bin/sh

LIB=/usr/lib/digitale-briefwahl
SHARE=/usr/share/digitale-briefwahl

cd "$SHARE"
. ./environ
cd "$OLDPWD"

exec mono --debug "$LIB"/DigitaleBriefwahl.Launcher.exe "$@"

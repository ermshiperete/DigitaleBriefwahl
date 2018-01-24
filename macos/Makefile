INSTALLERDIR=$(PWD)/../Releases
TARGETDIR=$(PWD)/../output/macos
APPDIR=root/Applications/DigitaleBriefwahl.Launcher.app/Contents
VERSION=$(shell cat ../DigitaleBriefwahl.Launcher/bin/Version.txt)
NUMFILES=$(shell find $(TARGETDIR)/root | wc -l)
SIZE=$(shell du -BK -s $(TARGETDIR)/root | cut -f1 -d'K')

all: installer

prepare:
	mkdir -p $(INSTALLERDIR)
	mkdir -p $(TARGETDIR)/flat/base.pkg
	mkdir -p $(TARGETDIR)/Resources/en.lproj
	mkdir -p $(TARGETDIR)/root/Applications

prepare-app:
	mkdir -p $(TARGETDIR)/$(APPDIR)/MacOS/app
	mkdir -p $(TARGETDIR)/$(APPDIR)/Resources

app: prepare-app
	cp Info.plist $(TARGETDIR)/$(APPDIR)/
	cp digitale-briefwahl $(TARGETDIR)/$(APPDIR)/MacOS/
	cp ../Resources/icons.icns $(TARGETDIR)/$(APPDIR)/Resources/
	cp -r ../DigitaleBriefwahl.Launcher/bin/Release/*.* $(TARGETDIR)/$(APPDIR)/MacOS/app/

.ONESHELL:

installer: prepare app create-base-pkg create-flat
	( cd $(TARGETDIR)/flat && xar --compression none -cf "$(INSTALLERDIR)/DigitaleBriefwahl.Launcher.pkg" * )

create-base-pkg: prepare app
	cd $(TARGETDIR)
	( cd root && find . | cpio -o --format odc --owner 0:80 | gzip -c ) > flat/base.pkg/Payload
	cat > flat/base.pkg/PackageInfo <<-EOF
	<pkg-info format-version="2" identifier="DigitaleBriefwahl.Launcher.base.pkg" version="$(VERSION)" install-location="/" auth="root">
		<payload installKBytes="$(SIZE)" numberOfFiles="$(NUMFILES)"/>
		<bundle-version>
			<bundle id="DigitaleBriefwahl.Launcher" CFBundleIdentifier="DigitaleBriefwahl.Launcher" path="./Applications/DigitaleBriefwahl.Launcher.app" CFBundleVersion="$(VERSION)"/>
		</bundle-version>
	</pkg-info>
	EOF
	mkbom -u 0 -g 80 root flat/base.pkg/Bom

create-flat: create-base-pkg
	cd $(TARGETDIR)
	cat > flat/Distribution <<-EOF
	<?xml version="1.0" encoding="utf-8"?>
	<installer-script minSpecVersion="1.000000" authoringTool="com.apple.PackageMaker" authoringToolVersion="3.0.3" authoringToolBuild="174">
		<title>Digitale Briefwahl</title>
		<options customize="never" allow-external-scripts="no"/>
		<domains enable_anywhere="true"/>
		<installation-check script="pm_install_check();"/>
		<script>function pm_install_check() {
			if(!(system.compareVersions(system.version.ProductVersion,'10.7') >= 0)) {
				my.result.title = 'Failure';
				my.result.message = 'You need at least Mac OS X 10.7 to install DigitaleBriefwahl.';
				my.result.type = 'Fatal';
				return false;
			}
			return true;
		}
		</script>
		<choices-outline>
			<line choice="choice1"/>
		</choices-outline>
		<choice id="choice1" title="base">
			<pkg-ref id="DigitaleBriefwahl.Launcher.base.pkg"/>
		</choice>
		<pkg-ref id="DigitaleBriefwahl.Launcher.base.pkg" installKBytes="$(SIZE)" version="$(VERSION)" auth="Root">#base.pkg</pkg-ref>
	</installer-script>
	EOF
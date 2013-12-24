#!/usr/bin/perl
use warnings;
use strict;
use File::Basename qw(dirname basename fileparse);
use File::Spec;
use File::Copy;
use File::Path;
use Getopt::Long;

my $root = "";
my $nant = "mono --runtime=v4.0.30319 /usr/lib/NAnt/NAnt.exe";
my $mdSource = "";
my $mdRoot = "";

my $forceMacBuild;
my $skipUpdate;

main();

sub main {
	
	#parse CLI options
	my $options = GetOptions("force-mac-build" => \$forceMacBuild, "skip-update" => \$skipUpdate);
	if(!$forceMacBuild) {
		die "MonoDevelop for Mac must be built on a Linux machine" if $^O ne "linux";
	}
	else {
		#building on Mac, re-set some variables
		$ENV{PKG_CONFIG_PATH} = "/Library/Frameworks/Mono.framework/Versions/Current/lib/pkgconfig";
		$nant = "mono --runtime=v4.0 /usr/local/share/NAnt/bin/NAnt.exe -t:mono-4.0";
	}
	get_root();
	if(!$skipUpdate) {
		prepare_sources();
	}
	build_monodevelop();
	build_debugger();
	# build_monodevelop_hg();
	finalize_monodevelop();
	build_boo();
	build_boo_extensions();
	build_unityscript();
	build_boo_md_addins(); 
	package_monodevelop();
}

sub get_root {
	$root = File::Spec->rel2abs( dirname($0) );
	chdir $root;
	$root = File::Spec->rel2abs( File::Spec->updir() );

	$mdSource = "$root/monodevelop/main/build";
	$mdRoot = "$root/tmp/monodevelop";
}

sub prepare_sources {
	if (!$ENV{UNITY_THISISABUILDMACHINE})
	{	
		printf ("Detected that this is not a build machine.\n");
		printf ("Pulling latest MonoDevelop . . .\n");
		chdir "$root/monodevelop";
		system ("git pull") && die ("failed to update monodevelop");
		printf ("Pulling latest MonoDevelop.Debugger.Soft.Unity . . .\n");
		chdir "$root/MonoDevelop.Debugger.Soft.Unity";
		system ("git pull") && die ("failed to update MonoDevelop.Debugger.Soft.Unity");
		printf ("Pulling latest boo . . .\n");
		chdir "$root/boo";
		system ("git pull") && die ("failed to update boo");
		printf ("Pulling latest unityscript . . .\n");
		chdir "$root/unityscript";
		system ("git pull") && die ("failed to update unityscript");
		printf ("Pulling laest boo-md-addins . . .\n");
		chdir "$root/boo-md-addins";
		system ("git pull") && die ("failed to update boo-md-addins");
		# chdir "$root/monodevelop-hg";
		# system ("hg pull --update") && die ("failed to update monodevelop-hg");
	}
	chdir $root;

	# Check sources
	die ("Must grab Unity MonoDevelop Soft Debugger source from github first") if !-d "MonoDevelop.Debugger.Soft.Unity";
	# die ("Must grab monodevelop-hg source from bitbucket first") if !-d "monodevelop-hg";
	die ("Must grab Boo implementation") if !-d "boo";
	die ("Must grab Boo extensions") if !-d "boo-extensions";
	die ("Must grab Boo MD addins implementation") if !-d "boo-md-addins";
	die ("Must grab Unityscript implementation") if !-d "unityscript";
}

sub build_monodevelop {
	chdir "$root/monodevelop";
	system("./configure --profile=mac");
	system("make") && die("Failed building MonoDevelop");
	mkpath("main/build/bin/branding");
	copy("branding/Branding.xml", "main/build/bin/branding/Branding.xml");
}

sub build_debugger {
	chdir "$root/MonoDevelop.Debugger.Soft.Unity";
	system("xbuild /property:Configuration=Release /t:Rebuild") && die("Failed building Unity debugger addin");
}

sub build_monodevelop_hg {
	chdir "$root/monodevelop-hg/monodevelop-hg";
	system("xbuild /property:Configuration=Release /t:Rebuild") && die("Failed building monodevelop-hg");
}

sub finalize_monodevelop {
	chdir $root;
	rmtree "tmp";
	mkpath "tmp";

	# TODO: First, we assembled the final monodevelop product in tmp/monodevelop, but now, currently (maybe temporarily) we need to build monodevelop, send the build folder to a mac and package it there, so the monodevelop dir needs all the bits in place before sending (can't be assembled in another folder)
	system("ln -s ../monodevelop/main/build tmp/monodevelop");

	# Unity soft debugger
	mkpath "$mdRoot/AddIns/MonoDevelop.Debugger.Soft.Unity";
	copy "$root/MonoDevelop.Debugger.Soft.Unity/obj/Release/MonoDevelop.Debugger.Soft.Unity.dll", "$mdRoot/AddIns/MonoDevelop.Debugger.Soft.Unity" or die ("Failed to copy MonoDevelop.Debugger.Soft.Unity");	
	# Unity utilities
	copy "$root/MonoDevelop.Debugger.Soft.Unity/obj/Release/UnityUtilities.dll", "$mdRoot/AddIns" or die ("Failed to copy UnityUtilities");	
	# monodevelop-hg
	# copy "$root/monodevelop-hg/monodevelop-hg/bin/Release/MonoDevelop.VersionControl.Mercurial.dll", "$mdRoot/AddIns/VersionControl" or die ("Failed to copy monodevelop-hg");	
	# copy "$root/monodevelop-hg/monodevelop-hg/bin/Release/Mercurial.dll", "$mdRoot/AddIns/VersionControl" or die ("Failed to copy monodevelop-hg");	
}

sub build_boo {
	chdir "$root/boo";
	system("$nant rebuild") && die ("Failed to build Boo");
	mkpath "$mdRoot/AddIns/BackendBindings/Boo/boo";
	system("rsync -av --exclude=*.mdb  \"$root/boo/build/\" \"$mdRoot/AddIns/BackendBindings/Boo/boo\"");
}

sub build_boo_extensions {
	chdir "$root/boo-extensions";
	copy "$root/monodevelop/dependencies/build.properties", "$root/boo-extensions/build.properties";
	system("$nant rebuild") && die ("Failed to build Boo");
}

sub build_unityscript {
	chdir "$root/unityscript";
	my $javascriptFiles =  "$mdRoot/AddIns/BackendBindings/UnityScript/bin";
	mkpath $javascriptFiles;
	rmtree "$root/unityscript/bin";
	system("$nant rebuild") && die ("Failed to build UnityScript");
	system("rsync -av --exclude=*.mdb --exclude=*Tests* --exclude=nunit* \"$root/unityscript/bin/\" \"$javascriptFiles\"");
}

sub build_boo_md_addins {
	chdir "$root/boo-md-addins";
	copy "$root/monodevelop/dependencies/build.properties", "$root/boo-md-addins/build.properties";
	system("$nant rebuild") && die ("Failed to build Boo-based addins");
	copy "$root/boo-md-addins/build/Boo.MonoDevelop.dll", "$mdRoot/AddIns/BackendBindings/Boo";
	copy "$root/boo-md-addins/build/Boo.Ide.dll", "$mdRoot/AddIns/BackendBindings/Boo";
	copy "$root/boo-md-addins/build/UnityScript.Ide.dll", "$mdRoot/AddIns/BackendBindings/Boo";
	copy "$root/boo-md-addins/build/Boo.MonoDevelop.Util.dll", "$mdRoot/AddIns/BackendBindings/Boo";
	copy "$root/boo-md-addins/build/UnityScript.MonoDevelop.dll", "$mdRoot/AddIns/BackendBindings/UnityScript";
	copy "$root/boo-md-addins/build/UnityScript.Ide.dll", "$mdRoot/AddIns/BackendBindings/UnityScript";
	copy "$root/boo-md-addins/build/Boo.Ide.dll", "$mdRoot/AddIns/BackendBindings/UnityScript";
	copy "$root/boo-md-addins/build/Boo.MonoDevelop.Util.dll", "$mdRoot/AddIns/BackendBindings/UnityScript";
}

sub package_monodevelop {
	system("cp -R $mdRoot/* $root/monodevelop/main/build");
	chdir "$root/monodevelop";
	if(!$forceMacBuild) {
		print "Collecting built files so they can be packaged on a mac\n";
	}
	unlink "MonoDevelop.tar.gz";
	system("tar cfz MonoDevelop.tar.gz main extras");
	if(!$forceMacBuild) {
		move "MonoDevelop.tar.gz", "$root";
	}
	else {
		print "Build completed, now run package_mac.pl\n";
	}
	chdir "$root";
}

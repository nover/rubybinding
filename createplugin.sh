#!/bin/bash
if [ $# -ne 1 ]
then
	echo "I need to know the build configuration (Debug, Release)"
	exit 1
fi;

echo "Creating monodevelop addin for configuration '$1'"
mkdir addin
cp RubyBinding.addin.xml bin/$1
cd bin/$1
mdtool setup pack RubyBinding.addin.xml -d:../../addin
if [ $? -eq 0 ]
then
	cd ../../
	echo "The addin is now available in `pwd`/addin"
else
	echo "Something went terribly wrong"
fi;

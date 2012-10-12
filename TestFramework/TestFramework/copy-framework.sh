#!/bin/sh

NATIVE_DIR=$1
TARGET_DIR=$2
FRAMEWORK_NAME=$3

rm -rf $TARGET_DIR/Contents/Frameworks/$FRAMEWORK_NAME
mkdir -p $TARGET_DIR/Contents/Frameworks
cp -a $NATIVE_DIR/$FRAMEWORK_NAME $TARGET_DIR/Contents/Frameworks

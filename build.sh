#!/bin/bash

# Set environment variables
UNITY_EDITOR_PATH="${UNITY_EDITOR_PATH:-/Applications/Unity/Hub/Editor/2021.3.22f1/Editor}"
UNITY_BUILD_PATH="${UNITY_BUILD_PATH:-Build}"
LOGFILE_PATH="${LOGFILE_PATH:-$UNITY_BUILD_PATH/build.log}"

# Run Unity in batch mode
"$UNITY_EDITOR_PATH/Unity" -batchmode -quit -projectPath "$(pwd)" -buildTarget Android -executeMethod AndroidBuilder.Build -logfile "$LOGFILE_PATH" -outputPath="$UNITY_BUILD_PATH" "$@"

# List and echo the generated APK path
APK_PATH=$(find "$UNITY_BUILD_PATH" -name '*.apk' -print)
echo "Generated APK: $APK_PATH"

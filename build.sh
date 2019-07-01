#!/usr/bin/env bash

##########################################################################
# This is a customized Cake bootstrapper script for Shell.
##########################################################################

echo "Preparing to run build script..."

cd build
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
TOOLS_DIR=$SCRIPT_DIR/tools
CAKE_BINARY_PATH=$TOOLS_DIR/"cake.coreclr"

SCRIPT="build.cake"
CAKE_CSPROJ=$SCRIPT_DIR/"cakebuild.csproj"

# Parse arguments.
CAKE_ARGUMENTS=()
for i in "$@"; do
    case $1 in
        -s|--script) SCRIPT="$2"; shift ;;
        --) shift; CAKE_ARGUMENTS+=("$@"); break ;;
        *) CAKE_ARGUMENTS+=("$1") ;;
    esac
    shift
done

# Install the required tools locally.
echo "Restoring cake tools..."
dotnet restore $CAKE_CSPROJ --packages $TOOLS_DIR > /dev/null 2>&1

# Search for the CakeBuild binary.
CAKE_BINARY=$(find $CAKE_BINARY_PATH -name "Cake.dll")

# Start Cake
echo "Running build script..."

dotnet "$CAKE_BINARY" $SCRIPT "${CAKE_ARGUMENTS[@]}"

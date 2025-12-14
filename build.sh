#!/bin/bash

# Build script for macOS using Docker

echo "Building Windows Timer Lock application..."

# Build the Docker image
echo "Step 1: Building Docker image..."
docker build -t windows-timer-lock-builder .

if [ $? -ne 0 ]; then
    echo "Error: Docker build failed!"
    exit 1
fi

# Create output directory
mkdir -p output/win-x64

# Run the build in Docker container
echo "Step 2: Building .NET application..."
docker run --rm \
    -v "$(pwd):/app" \
    -w /app \
    windows-timer-lock-builder \
    dotnet publish WindowsTimerLock.csproj \
    -c Release \
    -r win-x64 \
    --self-contained true \
    /p:PublishSingleFile=true \
    /p:IncludeNativeLibrariesForSelfExtract=true \
    /p:EnableCompressionInSingleFile=true \
    -o output/win-x64

if [ $? -ne 0 ]; then
    echo "Error: Build failed!"
    exit 1
fi

echo ""
echo "========================================="
echo "Build completed successfully!"
echo "========================================="
echo ""
echo "Output location: output/win-x64/WindowsTimerLock.exe"
echo ""
ls -lh output/win-x64/WindowsTimerLock.exe
echo ""
echo "To deploy:"
echo "1. Copy WindowsTimerLock.exe to your Windows machine"
echo "2. Run as Administrator"
echo ""
echo "To activate kill switch:"
echo "Create a file named 'kill_switch.txt' in the same directory as the EXE"
echo ""

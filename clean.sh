#!/bin/bash

# Clean build artifacts

echo "Cleaning build artifacts..."

# Remove output directories
rm -rf output/
rm -rf bin/
rm -rf obj/

# Remove data files
rm -f timer_data.bin
rm -f kill_switch.txt

echo "Clean completed!"

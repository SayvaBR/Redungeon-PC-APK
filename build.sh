#!/usr/bin/env bash
set -e

# Colors and styles
BOLD="\033[1m"
ITALIC="\033[3m"
GREEN="\033[32m"
YELLOW="\033[33m"
CYAN="\033[36m"
RED="\033[31m"
RESET="\033[0m"

SOURCE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
RELEASES_DIR="${SOURCE_DIR}/releases"

echo -e "${BOLD}${CYAN}=== Build proccess start for Redungeon ===${RESET}\n"

# Build folders
mkdir -p "${RELEASES_DIR}/PC-Windows"
mkdir -p "${RELEASES_DIR}/PC-Linux"
mkdir -p "${RELEASES_DIR}/Android"

# === TODO: split RedungeonPC.csproj into 2 .csproj files for Tests
# 1. Test stage
# echo -e "${BOLD}${YELLOW}[1/4] Starting Input Contract Tests...${RESET}"
# dotnet run --project "${SOURCE_DIR}/RedungeonPC_ResolutionManager/Tests/RedungeonPC.InputContractTests" \ 
#     -c Debug \
#     -p:SelfContained=false
# echo -e "${GREEN}✓ Tests successfully passed!${RESET}\n"

# 2. Build for Windows stage (win-x64)
echo -e "${BOLD}${YELLOW}[2/4] Compiling build for Windows (win-x64)...${RESET}"
dotnet publish "${SOURCE_DIR}/RedungeonPC_ResolutionManager/RedungeonPC.csproj" \
    -c Release \
    -r win-x64 \
    -o "${RELEASES_DIR}/PC-Windows"
# Goofy fix of copying frost-border.png for Windows build
cp "${SOURCE_DIR}/RedungeonPC_ResolutionManager/Content/Images/frost-border.png" "${RELEASES_DIR}/PC-Windows/Content/Images/frost-border.png"
echo -e "${GREEN}✓ Build for Windows successfully compiled!${RESET}\n"

# 3. Build for Linux stage (linux-x64)
echo -e "${BOLD}${YELLOW}[3/4] Compiling build for Linux (linux-x64)...${RESET}"
dotnet publish "${SOURCE_DIR}/RedungeonPC_ResolutionManager/RedungeonPC.csproj" \
    -c Release \
    -r linux-x64 \
    -o "${RELEASES_DIR}/PC-Linux"
echo -e "${GREEN}✓ Build for Linux successfully compiled!${RESET}\n"

# 4. Build for Android stage (.apk)
echo -e "${BOLD}${YELLOW}[4/4] Compiling build for Android...${RESET}"
if [ -n "$ANDROID_HOME" ]; then
    dotnet build "${SOURCE_DIR}/RedungeonPC_ResolutionManager/Android/Redungeon.Android.csproj" \
        -c Release \
        -p:AndroidSdkDirectory="${ANDROID_HOME}"
else
    dotnet build "${SOURCE_DIR}/RedungeonPC_ResolutionManager/Android/Redungeon.Android.csproj" -c Release
fi

# Seacrhing and copying Android build
APK_BUILD_PATH=$(find "${SOURCE_DIR}/RedungeonPC_ResolutionManager/Android/bin/Release" -type f -name "*-Signed.apk" | head -n 1)

if [ -f "$APK_BUILD_PATH" ]; then
    cp "$APK_BUILD_PATH" "${RELEASES_DIR}/Android/Redungeon.apk"
    echo -e "${GREEN}✓ Build for Android successfully compiled!${RESET}\n"
else
    echo -e "${RED}${BOLD}ERROR: Signed APK not found after building.${RESET}"
    exit 1
fi

echo -e "${BOLD}${GREEN}=== All builds successfully compiled and copied to folder: ${RESET} ${ITALIC}${RELEASES_DIR}${RESET}"
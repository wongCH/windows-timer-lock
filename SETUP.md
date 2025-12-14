# macOS Development Environment Setup Guide

## Windows Timer Lock - Complete Setup Instructions

This guide will walk you through setting up a complete .NET development environment on macOS to build Windows applications using Docker.

---

## 📋 Prerequisites

- macOS (10.15 Catalina or later)
- At least 4GB of free disk space
- Internet connection

---

## 🐳 Step 1: Install Docker Desktop

Docker allows you to run a Linux container with .NET SDK to build Windows applications from macOS.

### Option A: Install via Homebrew (Recommended)

```bash
# Install Homebrew if not already installed
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# Install Docker Desktop
brew install --cask docker
```

### Option B: Manual Installation

1. Download Docker Desktop from: https://www.docker.com/products/docker-desktop/
2. Open the downloaded DMG file
3. Drag Docker to Applications folder
4. Launch Docker from Applications
5. Grant necessary permissions when prompted

### Verify Installation

```bash
# Check Docker version
docker --version

# Test Docker
docker run hello-world
```

**Expected output:** You should see a "Hello from Docker!" message.

---

## 🛠️ Step 2: Prepare the Project

Navigate to your project directory:

```bash
cd /Users/wongchuinhun/Documents/03\ Programming/_NET/timer
```

Make scripts executable:

```bash
chmod +x build.sh clean.sh
```

---

## 🏗️ Step 3: Build the Application

Run the build script:

```bash
./build.sh
```

### What happens during the build:

1. **Docker Image Creation** (~2-3 minutes first time)
   - Downloads .NET SDK 8.0 Linux image (~500MB)
   - Installs build tools
   - Caches for future builds

2. **Application Compilation** (~30-60 seconds)
   - Compiles C# code
   - Creates single-file executable
   - Bundles all dependencies
   - Optimizes and trims unused code

3. **Output**
   - Creates `output/win-x64/WindowsTimerLock.exe`
   - Typical size: 8-15 MB (single file)

### Build Output Example

```
Building Windows Timer Lock application...
Step 1: Building Docker image...
[+] Building 45.2s (8/8) FINISHED
Step 2: Building .NET application...
Microsoft (R) Build Engine version 17.0.0
  WindowsTimerLock -> /app/output/win-x64/WindowsTimerLock.exe

Build completed successfully!
Output location: output/win-x64/WindowsTimerLock.exe
-rwxr-xr-x  1 user  staff   12M Dec 14 10:30 output/win-x64/WindowsTimerLock.exe
```

---

## 🧪 Step 4: Verify the Build

Check the output file:

```bash
ls -lh output/win-x64/

# Check file details
file output/win-x64/WindowsTimerLock.exe
```

Expected: PE32+ executable (GUI) x86-64, for MS Windows

---

## 📦 Step 5: Deploy to Windows

### Transfer Methods

**Option A: USB Drive**
```bash
# Copy to mounted USB drive
cp output/win-x64/WindowsTimerLock.exe /Volumes/USB_DRIVE/
```

**Option B: Network Share**
```bash
# Copy to network location
cp output/win-x64/WindowsTimerLock.exe /Volumes/NetworkShare/
```

**Option C: Cloud Storage**
- Upload to Google Drive, Dropbox, OneDrive
- Download on Windows machine

**Option D: Email/AirDrop**
- The file is small enough to email
- Use AirDrop if you have an iPhone to transfer

---

## 🖥️ Step 6: Run on Windows

### First Time Setup

1. **Copy the EXE** to Windows machine (e.g., `C:\TimerLock\`)

2. **Run as Administrator**
   - Right-click `WindowsTimerLock.exe`
   - Select "Run as administrator"
   - Accept UAC prompt

3. **Verify Running**
   - Check Task Manager
   - Look for `WindowsTimerLock.exe` process
   - Should see console window (or run hidden)

### Optional: Install as Windows Service

For automatic startup on boot:

```powershell
# Download NSSM (Non-Sucking Service Manager)
# From: https://nssm.cc/download

# Open PowerShell as Administrator
cd C:\path\to\nssm\win64

# Install service
.\nssm install WindowsTimerLock "C:\TimerLock\WindowsTimerLock.exe"

# Configure service
.\nssm set WindowsTimerLock AppDirectory "C:\TimerLock"
.\nssm set WindowsTimerLock Start SERVICE_AUTO_START
.\nssm set WindowsTimerLock Description "Windows usage timer and lock service"

# Start service
.\nssm start WindowsTimerLock

# Check status
.\nssm status WindowsTimerLock
```

---

## 🔧 Development Workflow

### Making Changes

1. **Edit source code**
   ```bash
   # Use your preferred editor
   nano Program.cs
   # or
   code Program.cs
   ```

2. **Rebuild**
   ```bash
   ./build.sh
   ```

3. **Deploy updated EXE to Windows**

### Clean Build

If you encounter issues:

```bash
./clean.sh
./build.sh
```

---

## 🎛️ Configuration Options

Edit [Program.cs](Program.cs) to customize:

### Change Time Limit

```csharp
private const int MAX_HOURS = 4;  // Change to desired hours
```

### Change Save Interval

```csharp
// Line ~49: Change from 30 seconds to 60 seconds
saveTimer = new Timer(SaveUsageData, null, 
    TimeSpan.FromSeconds(60), 
    TimeSpan.FromSeconds(60));
```

### Change Kill Switch Check Interval

```csharp
// Line ~52: Change from 10 seconds to 5 seconds
checkTimer = new Timer(CheckKillSwitch, null, 
    TimeSpan.FromSeconds(5), 
    TimeSpan.FromSeconds(5));
```

After changes, rebuild and redeploy.

---

## 🆘 Kill Switch Usage

### Activate Kill Switch on Windows

**Method 1: Command Prompt**
```cmd
cd C:\TimerLock
echo. > kill_switch.txt
```

**Method 2: PowerShell**
```powershell
cd C:\TimerLock
New-Item -ItemType File -Name "kill_switch.txt"
```

**Method 3: File Explorer**
- Navigate to application directory
- Right-click → New → Text Document
- Name it `kill_switch.txt`

**What happens:**
- Application detects the file within 10 seconds
- Stops counting
- Deletes `timer_data.bin`
- Exits the application
- System is unlocked

---

## 🐛 Troubleshooting

### Docker Issues

**Problem:** `docker: command not found`
```bash
# Ensure Docker Desktop is running
open -a Docker

# Wait for Docker to fully start (check menu bar icon)
# Then retry your command
```

**Problem:** Permission denied
```bash
# Add user to docker group (may require restart)
sudo dscl . append /Groups/docker GroupMembership $USER
```

**Problem:** Build is very slow
```bash
# Clean Docker cache
docker system prune -a

# Rebuild
./build.sh
```

### Build Issues

**Problem:** Build fails with "restore failed"
```bash
# Clean and rebuild
./clean.sh
docker system prune -f
./build.sh
```

**Problem:** "Unable to load project"
- Check that all files exist
- Verify [WindowsTimerLock.csproj](WindowsTimerLock.csproj) is valid XML
- Ensure line endings are correct (LF, not CRLF)

### Windows Runtime Issues

**Problem:** "Application failed to start"
- Ensure running as Administrator
- Check Windows Event Viewer for errors
- Verify .NET Framework is not required (it's self-contained)

**Problem:** EXE is blocked by Windows
- Right-click EXE → Properties
- Check "Unblock" if present
- Click OK and retry

**Problem:** Timer resets unexpectedly
- Check if `timer_data.bin` exists
- Verify application has write permissions
- Ensure only one instance is running

---

## 📊 File Descriptions

| File | Purpose |
|------|---------|
| `Dockerfile` | Docker image definition for .NET SDK 8.0 |
| `.dockerignore` | Files to exclude from Docker context |
| `WindowsTimerLock.csproj` | .NET project configuration |
| `app.manifest` | Windows manifest (requests admin privileges) |
| `Program.cs` | Main application source code |
| `build.sh` | Automated build script for macOS |
| `clean.sh` | Clean build artifacts |
| `README.md` | User documentation |
| `SETUP.md` | This file - development setup guide |

---

## 🔍 How It Works

```
┌─────────────────────────────────────────────────────┐
│                   User Activity                      │
└─────────────────┬───────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────┐
│  WindowsTimerLock.exe (Running as Admin)            │
│  ┌──────────────────────────────────────────────┐   │
│  │ ⏱️  Count Usage Time (when active)            │   │
│  │ 💾 Save every 30s → timer_data.bin           │   │
│  │ 🔍 Check kill_switch.txt every 10s           │   │
│  │ 🌙 Reset at midnight                         │   │
│  └──────────────────────────────────────────────┘   │
│                                                      │
│  Events Monitored:                                   │
│  • System Suspend/Resume                             │
│  • User Lock/Unlock                                  │
│  • Logon/Logoff                                      │
│  • Lid Close/Open                                    │
└─────────────────┬───────────────────────────────────┘
                  │
                  ▼
          ┌───────────────┐
          │ Usage >= 4hrs? │
          └───────┬───────┘
                  │
         Yes ─────┤
                  │
                  ▼
    ┌──────────────────────────┐
    │ 🔒 Lock Windows           │
    │ Require Admin Unlock      │
    └──────────────────────────┘
```

---

## 🚀 Next Steps

1. **Test thoroughly** on a non-production Windows machine first
2. **Document** your specific deployment process
3. **Consider** implementing additional features:
   - Email notifications
   - Usage reports
   - Remote management
   - Multiple time profiles
   - Break reminders

---

## 📝 Notes

- The application uses **native Windows APIs** (requires Windows)
- **Single EXE** contains all dependencies (~12MB)
- **No installation** required (xcopy deployment)
- **Persists** across reboots via `timer_data.bin`
- **Secure**: Requires Administrator privileges

---

## ✅ Quick Reference Commands

```bash
# Setup
cd /Users/wongchuinhun/Documents/03\ Programming/_NET/timer
chmod +x build.sh clean.sh

# Build
./build.sh

# Clean
./clean.sh

# Check output
ls -lh output/win-x64/

# Copy to USB
cp output/win-x64/WindowsTimerLock.exe /Volumes/USB_DRIVE/
```

---

**Happy Building! 🎉**

If you need help, check the troubleshooting section or review the console output for specific error messages.

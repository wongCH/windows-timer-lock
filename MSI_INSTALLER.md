# Creating MSI Installer for Windows Timer Lock

This guide explains how to create an MSI installer that automatically configures the application to run at Windows startup.

## Prerequisites

### On Windows Machine:

1. **WiX Toolset v3.11 or later**
   - Download from: https://github.com/wixtoolset/wix3/releases
   - Install the `.exe` installer
   - Default install path: `C:\Program Files (x86)\WiX Toolset v3.11\`

2. **.NET SDK 8.0 or later** (if building from source)
   - Download from: https://dotnet.microsoft.com/download

3. **Windows 10/11** with PowerShell 5.1 or later

## Method 1: Build MSI on Windows

### Step 1: Transfer Files to Windows

Copy these files to your Windows machine:
```
timer/
├── output/win-x64/WindowsTimerLock.exe
├── Installer.wxs
├── License.rtf
├── build-msi.ps1
└── app.ico (optional - for custom icon)
```

### Step 2: Install WiX Toolset

1. Download WiX Toolset from: https://github.com/wixtoolset/wix3/releases
2. Run the installer (e.g., `wix311.exe`)
3. Follow installation wizard
4. Default install location: `C:\Program Files (x86)\WiX Toolset v3.11\`

### Step 3: Build the MSI

Open PowerShell as Administrator in the project directory:

```powershell
# If WiX is in default location:
.\build-msi.ps1

# If WiX is in custom location:
.\build-msi.ps1 -WixPath "C:\Path\To\WiX\bin"
```

The MSI will be created at: `installer\WindowsTimerLock.msi`

## Method 2: Build MSI using Docker (Advanced)

Create a Dockerfile for building MSI:

```dockerfile
FROM mcr.microsoft.com/windows/servercore:ltsc2022

# Install WiX
ADD https://github.com/wixtoolset/wix3/releases/download/wix3112rtm/wix311.exe C:\wix311.exe
RUN C:\wix311.exe /install /quiet /norestart

# Set environment
ENV PATH="C:\Program Files (x86)\WiX Toolset v3.11\bin;%PATH%"

WORKDIR C:\build
```

Then build:
```powershell
docker build -t wix-builder .
docker run -v ${PWD}:C:\build wix-builder powershell -File build-msi.ps1
```

## Installing the MSI

### As Administrator:
```powershell
msiexec /i installer\WindowsTimerLock.msi /qb
```

### Interactive Installation:
1. Double-click `WindowsTimerLock.msi`
2. Follow installation wizard
3. Choose installation directory (default: `C:\Program Files\WindowsTimerLock`)
4. Complete installation

### Silent Installation:
```powershell
# Silent install
msiexec /i WindowsTimerLock.msi /quiet /norestart

# Silent install with log
msiexec /i WindowsTimerLock.msi /quiet /norestart /l*v install.log
```

## What the MSI Does

The installer automatically:

1. ✅ Installs executable to `C:\Program Files\WindowsTimerLock\`
2. ✅ Creates Windows Scheduled Task named "WindowsTimerLock"
   - Trigger: At system startup
   - Run as: SYSTEM
   - Privilege level: Highest
3. ✅ Sets NTFS permissions (users can't delete)
4. ✅ Creates Start Menu shortcut
5. ✅ Registers with Programs and Features (Add/Remove Programs)

## Uninstalling

### Via Control Panel:
1. Open "Programs and Features"
2. Find "Windows Timer Lock"
3. Click "Uninstall"

### Via Command Line:
```powershell
# Interactive uninstall
msiexec /x "{Product-GUID}" /qb

# Silent uninstall
msiexec /x "{Product-GUID}" /quiet /norestart

# Or by MSI file
msiexec /x installer\WindowsTimerLock.msi /quiet
```

The uninstaller will:
- Remove all files
- Delete scheduled task
- Remove Start Menu shortcuts
- Clean up registry entries

## Customizing the Installer

### Edit Company Information

In `Installer.wxs`, modify:

```xml
<Product Name="Windows Timer Lock" 
         Manufacturer="Your Company Name"
         ...>
  
<Property Id="ARPCOMMENTS" Value="..." />
<Property Id="ARPCONTACT" Value="support@yourcompany.com" />
<Property Id="ARPHELPLINK" Value="https://yourwebsite.com" />
```

### Change Installation Directory

Default: `C:\Program Files\WindowsTimerLock`

To change, modify in `Installer.wxs`:
```xml
<Directory Id="INSTALLFOLDER" Name="WindowsTimerLock" />
```

### Add Custom Icon

1. Create or obtain an `.ico` file
2. Save as `app.ico` in the project root
3. The installer will use it automatically

### Modify License Agreement

Edit `License.rtf` with any RTF-compatible editor (WordPad, Word, etc.)

## Troubleshooting

### Error: "candle.exe not found"
- Install WiX Toolset
- Or specify path: `.\build-msi.ps1 -WixPath "C:\Path\To\WiX\bin"`

### Error: "Source file not found"
- Ensure `output\win-x64\WindowsTimerLock.exe` exists
- Build the application first: `.\build.sh` (on macOS) or `dotnet publish` (on Windows)

### Error: "ICE validation errors"
- These are warnings, MSI usually works fine
- To suppress: Add `-sval` to light.exe command in `build-msi.ps1`

### Scheduled Task Not Created
- Check installation log: `msiexec /i WindowsTimerLock.msi /l*v install.log`
- Manually create: `schtasks /create /tn "WindowsTimerLock" /tr "C:\Program Files\WindowsTimerLock\WindowsTimerLock.exe" /sc ONSTART /ru SYSTEM /rl HIGHEST`

### MSI Too Large
- The MSI includes the entire .NET runtime (~49 MB)
- This is normal for self-contained .NET applications
- Consider framework-dependent build (requires .NET installed on target)

## Alternative: Advanced Installer (GUI Tool)

If you prefer a GUI tool instead of WiX:

1. Download **Advanced Installer** (free edition available)
   - https://www.advancedinstaller.com/
2. Create new project → Simple → Installer
3. Add files → Browse to `WindowsTimerLock.exe`
4. Set install location → `C:\Program Files\WindowsTimerLock`
5. **Launch Conditions** → Require Administrator
6. **Services and Scheduled Tasks** → Add Scheduled Task:
   - Name: WindowsTimerLock
   - Trigger: At startup
   - Program: `[APPDIR]WindowsTimerLock.exe`
   - Account: Local System
   - Run with highest privileges
7. Build → Generate MSI

## Deployment to Multiple Machines

### Active Directory Group Policy:
1. Place MSI on network share
2. Open Group Policy Management
3. Computer Configuration → Policies → Software Settings → Software Installation
4. Right-click → New → Package
5. Select the MSI file
6. Choose "Assigned"

### SCCM/Intune:
- Use Microsoft Endpoint Manager
- Create application package with MSI
- Deploy to device collection

### Manual Deployment:
Copy MSI to each machine and run:
```powershell
psexec \\computer-name -s msiexec /i "\\network\share\WindowsTimerLock.msi" /quiet
```

## Building on macOS (Cross-Platform)

Unfortunately, WiX Toolset only runs on Windows. To build MSI from macOS:

**Option 1:** Use Windows VM (Parallels, VMware, VirtualBox)
**Option 2:** Use remote Windows machine
**Option 3:** Use GitHub Actions Windows runner (automated CI/CD)

Example GitHub Action:
```yaml
- name: Build MSI
  runs-on: windows-latest
  steps:
    - uses: actions/checkout@v2
    - name: Install WiX
      run: |
        choco install wixtoolset
    - name: Build Installer
      run: .\build-msi.ps1
```

## Security Considerations

The MSI installer:
- ✅ Requires administrator privileges
- ✅ Digitally signed (if you have code signing certificate)
- ✅ Sets restrictive NTFS permissions
- ✅ Runs application as SYSTEM (highest privilege)
- ⚠️ Users cannot easily disable or remove without admin password

**Recommendation:** Sign the MSI with a code signing certificate to avoid Windows SmartScreen warnings.

## Next Steps

After creating the MSI:
1. Test on clean Windows VM
2. Verify scheduled task is created: `Get-ScheduledTask -TaskName "WindowsTimerLock"`
3. Test auto-start by rebooting
4. Test uninstall process
5. Sign MSI with code signing certificate (optional but recommended)

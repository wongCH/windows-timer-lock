# Windows Timer Lock

A Windows GUI application with system tray integration that limits daily computer usage with a countdown timer, automatically locking the system when the time limit is reached. Features a password-protected admin interface for configuration.

## Features

### Core Functionality
- ✅ **Countdown Timer** - Real-time display in system tray tooltip
- ✅ **30-Second Sound Alert** - Audible warning (3 beeps) when 30 seconds remaining
- ✅ **Custom Lock Screen** - Full-screen password-protected lock when time limit reached
- ✅ **Admin Unlock** - Unlock with admin password (temporary access)
- ✅ **Reset Timer on Lock** - Reset counter to zero directly from lock screen
- ✅ **Daily Reset** - Automatically resets at midnight
- ✅ **Smart Pause** - Stops counting when:
  - Screen is locked (Win+L)
  - Laptop lid is closed
  - System goes to sleep/suspend
  - User logs out
- ✅ **Task Switching Disabled** - Blocks Alt+Tab, Win key, Ctrl+Esc to prevent app switching
- ✅ **Persistence** - Usage time survives reboots

### GUI & Admin Controls
- ✅ **System Tray Icon** - Always visible with live countdown
- ✅ **Live Countdown Dialog** - Real-time updates every second
- ✅ **Color-Coded Warnings** - Visual alerts when time running low
- ✅ **Password Protection** - Admin interface secured (default: `admin123`)
- ✅ **Configurable Limit** - Change max hours (1-24 hours)
- ✅ **Reset Counter** - Admin can reset today's usage to zero
- ✅ **Enable/Disable** - Turn timer enforcement on/off
- ✅ **Change Password** - Update admin password anytime
- ✅ **Kill Switch** - Emergency shutdown and unlock

### Technical
- ✅ **Single EXE** - All dependencies included (~49 MB)
- ✅ **Admin Privileges** - Required for system-level operations
- ✅ **Secure** - SHA-256 password hashing
- ✅ **System-Level Keyboard Hook** - Prevents task switching and bypassing

## System Requirements

- Windows 10/11 (64-bit)
- .NET 10.0 Runtime (included in the single-file EXE)
- Administrator privileges

## Development Environment Setup (macOS)

### Prerequisites

1. **Install Docker Desktop for macOS**
   ```bash
   # Download from: https://www.docker.com/products/docker-desktop/
   # Or install via Homebrew:
   brew install --cask docker
   ```

2. **Verify Docker Installation**
   ```bash
   docker --version
   docker run hello-world
   ```

### Building the Application

1. **Clone or navigate to the project directory**
   ```bash
   cd /path/to/timer
   ```

2. **Make the build script executable**
   ```bash
   chmod +x build.sh clean.sh
   ```

3. **Build the application**
   ```bash
   ./build.sh
   ```

   This will:
   - Build a Docker image with .NET SDK 10.0
   - Compile the Windows Forms application
   - Create a single EXE file with all dependencies (~49 MB)
   - Output to `output/win-x64/WindowsTimerLock.exe`

4. **Clean build artifacts (optional)**
   ```bash
   ./clean.sh
   ```

## Deployment to Windows

### Method 1: MSI Installer (Recommended)

**Automatic installation with startup configuration**

1. **Build the MSI installer** (on Windows machine):
   - Install WiX Toolset v3.11+: https://github.com/wixtoolset/wix3/releases
   - Transfer files: `WindowsTimerLock.exe`, `Installer.wxs`, `build-msi.ps1`, `License.rtf`
   - Run PowerShell as Administrator: `.\build-msi.ps1`
   - MSI created at: `installer\WindowsTimerLock.msi`

2. **Install the MSI**:
   - Double-click `WindowsTimerLock.msi` and follow wizard
   - Or silent install: `msiexec /i WindowsTimerLock.msi /quiet`

3. **What it does automatically**:
   - ✅ Installs to `C:\Program Files\WindowsTimerLock\`
   - ✅ Creates scheduled task to run at Windows startup
   - ✅ Runs as SYSTEM with highest privileges
   - ✅ Sets permissions (users can't delete)
   - ✅ Adds Start Menu shortcut
   - ✅ Registers in Add/Remove Programs for easy uninstall

📖 See [MSI_INSTALLER.md](MSI_INSTALLER.md) for complete documentation

### Method 2: PowerShell Deployment Script

**Semi-automatic installation**

1. Transfer `WindowsTimerLock.exe` to `C:\temp\` on Windows
2. Transfer `deploy.ps1` to Windows
3. Run PowerShell as Administrator: `.\deploy.ps1`

This will:
- Install to `C:\Program Files\WindowsTimerLock\`
- Create scheduled task for startup
- Set file permissions

### Method 3: Manual Installation

**Basic deployment without startup**

1. **Transfer the EXE**
   Copy `output/win-x64/WindowsTimerLock.exe` to your Windows machine

2. **Run as Administrator**
   Right-click `WindowsTimerLock.exe` and select "Run as administrator"

3. **Configure Startup (Optional)**
   - Press `Win + R`, type `shell:startup`, press Enter
   - Create shortcut to `WindowsTimerLock.exe` in this folder
   - Right-click shortcut → Properties → Advanced → "Run as administrator"

The application will:
- Start counting immediately
- Appear as a shield icon in the system tray (bottom-right corner)
- Display countdown timer in the tray tooltip
- Create `timer_data.bin` and `config.bin` files to persist settings
- Lock the computer when time limit is reached

## Basic Usage

**View Countdown:**
- Hover over the tray icon to see remaining time
- Double-click the tray icon for detailed view

**Access Admin Settings:**
1. Right-click the tray icon
2. Select "Admin Settings..."
3. Enter password: `admin123` (change this immediately!)
4. Configure settings:
   - Change maximum hours per day (1-24)
   - Enable/disable timer
   - Reset counter to zero
   - Change admin password

**Exit Application:**
1. Right-click tray icon → "Exit (Admin)"
2. Enter admin password
3. Confirm exit

## Automatic Startup Options Comparison

| Method | Difficulty | Runs As | Uninstall | Notes |
|--------|-----------|---------|-----------|-------|
| **MSI Installer** | Easy | SYSTEM | Add/Remove Programs | ⭐ Recommended - Most professional |
| **PowerShell Script** | Medium | SYSTEM | Manual task deletion | Good for IT deployment |
| **Startup Folder** | Easy | User | Delete shortcut | Simple but less secure |
| **Task Scheduler** | Medium | SYSTEM | Task Scheduler UI | Manual setup required |

## Kill Switch

To emergency stop and unlock the system:

1. Create a file named `kill_switch.txt` in the same directory as the EXE:
   ```cmd
   echo. > kill_switch.txt
   ```

2. The application will detect this file within 10 seconds and:
   - Stop the timer
   - Delete all usage data
   - Exit the application
   - Unlock the system

## Configuration

### Via Admin Interface (Recommended)

**Default Admin Password:** `admin123`

1. Right-click tray icon → "Admin Settings"
2. Enter admin password
3. Modify settings:
   - **Maximum Hours:** 1-24 hours per day
   - **Timer Enabled:** Toggle enforcement on/off
   - **Reset Counter:** Clear today's usage
   - **Change Password:** Update admin password (min 4 characters)

### Via Code Modification

To modify default settings, edit [Program.cs](Program.cs):

- `DEFAULT_PASSWORD`: Default admin password (line 34)
- `maxHours`: Default daily time limit (line 49)
- Update timer interval: 1000ms = 1 second (line 61)
- Save timer interval: 30000ms = 30 seconds (line 67)
- Kill switch check: 10000ms = 10 seconds (line 72)

After editing, rebuild with `./build.sh`

## How It Works

1. **Time Tracking**: The application runs continuously and tracks active usage time
2. **Persistence**: Usage data is saved every 30 seconds to `timer_data.bin`
3. **Daily Reset**: Automatically resets at midnight (00:00)
4. **Pause Events**: Stops counting when:
   - User locks the screen (Win+L)
   - System suspends/sleeps
   - User logs out
   - Lid closes (triggers suspend)
   - Remote desktop disconnects
5. **Resume Events**: Resumes counting when:
   - User unlocks the screen
   - System wakes from sleep
   - User logs back in
   - Remote desktop reconnects
6. **Keyboard Hook**: System-wide hook blocks:
   - Alt+Tab (task switching)
   - Windows key (Start menu)
   - Ctrl+Esc (Start menu)
7. **Lock Mechanism**: Uses Windows API `LockWorkStation()` to lock the computer
8. **Admin Required**: Manifest requires administrator privileges for system-level operations

## File Structure

```
timer/
├── Dockerfile              # Docker image for building on macOS
├── .dockerignore          # Docker ignore patterns
├── WindowsTimerLock.csproj # .NET project configuration
├── app.manifest           # Windows manifest (admin rights)
├── Program.cs             # Main application code (GUI + logic)
├── build.sh               # Build script for macOS
├── clean.sh               # Clean script
├── deploy.ps1             # PowerShell deployment script
├── Installer.wxs          # WiX MSI installer configuration
├── build-msi.ps1          # MSI builder script
├── License.rtf            # License agreement for installer
├── README.md              # User documentation
├── SETUP.md               # Development setup guide
├── TEST_CASES.md          # Comprehensive test scenarios
├── MSI_INSTALLER.md       # MSI creation guide
└── output/
    └── win-x64/
        └── WindowsTimerLock.exe  # Final executable (~49 MB)
```

### Runtime Files (Created on Windows)
```
├── timer_data.bin         # Daily usage data
├── config.bin             # Settings and password hash
└── kill_switch.txt        # Emergency shutdown trigger
```

### Installer Output
```
installer/
├── WindowsTimerLock.msi   # MSI installer package
└── Installer.wixobj       # Compiled WiX object (intermediate)
```

## GUI Overview

### System Tray Icon

The shield icon appears in the Windows system tray with a tooltip showing:
- **When Running:** `Remaining: HH:MM:SS` (countdown updates every second)
- **When Disabled:** `Timer: DISABLED`
- **When Time Up:** `Timer: TIME UP!`

### Context Menu

Right-click the tray icon to access:
- **Show Countdown** - View detailed timer information
- **Admin Settings...** - Password-protected configuration
- **Exit (Admin)** - Password-protected exit

### Countdown Dialog (Live Updates)

Double-click tray icon or select "Show Countdown" to view:
- **Live updates every second** - Values refresh in real-time
- **Current status** - ▶️ RUNNING / ⏸️ PAUSED / ⏹️ DISABLED (color-coded)
- **Time remaining** - HH:MM:SS format with color warnings:
  - 🟢 Green: > 15 minutes
  - 🟠 Orange: 5-15 minutes
  - 🔴 Red: < 5 minutes
- **Time used today** - HH:MM:SS format (updates every second)
- **Maximum allowed hours** - Configured limit
- **Daily reset time** - 00:00 (Midnight)
- **Monospaced font** - Easy-to-read time displays
- **Keep open** - Dialog continues updating while displayed

### Admin Settings Dialog

Access via password-protected menu:
- **Current Usage Display** - Shows today's usage
- **Maximum Hours** - Slider to set 1-24 hours
- **Timer Enabled** - Checkbox to enable/disable
- **Reset Counter** - Button to clear today's usage
- **Change Password** - Fields to update admin password

### Lock Screen (Time Limit Reached)

When time limit is reached, a full-screen lock appears:
- **Cannot be closed** - No X button, Alt+F4 disabled, TopMost
- **Live clock** - Shows current date and time (updates every second)
- **Usage display** - Shows current vs. maximum time (e.g., "Used: 04:00:00 / 04:00:00")
- **Password required** - Must enter admin password to unlock or reset
- **Two unlock options**:
  - **Unlock** - Temporary access (re-locks if time still exceeded)
  - **Reset Timer** - Reset counter to zero and unlock permanently
- **Visual feedback** - Screen shakes on wrong password
- **Auto re-lock** - If unlocked without reset, locks again when time exceeded

**Lock Screen Features:**
- 🛑 Full-screen dark overlay
- ⏰ "⏱️ TIME LIMIT REACHED" header
- 📅 Live date and time display
- 📊 Usage information (current / max)
- 🔐 Password entry field
- 🔓 **Unlock** button (temporary access)
- 🔄 **Reset Timer** button (reset counter + unlock)
- ❌ Cannot minimize, close, or bypass
- ✅ Both buttons require admin password

## Troubleshooting

### Application doesn't start
- Ensure you're running as Administrator
- Check Windows Event Viewer for error messages

### Time resets unexpectedly
- Check if `timer_data.bin` exists and is not being deleted
- Verify the application is running continuously

### Can't access Admin Settings
- Default password is `admin123`
- If you changed the password and forgot it:
  - Use kill switch to reset
  - Delete `config.bin` and restart application
  - Default password will be restored

### Can't unlock lock screen
- Enter the correct admin password (default: `admin123`)
- Password is case-sensitive
- Two options available on lock screen:
  - **Unlock** - Temporary access (re-locks if time still exceeded)
  - **Reset Timer** - Reset counter to zero and unlock permanently
- If password forgotten:
  - Boot to Windows Safe Mode
  - Navigate to application directory
  - Create `kill_switch.txt` file
  - Restart normally - application will exit
  - Delete `config.bin` to reset password

### Lock screen won't close / Can't switch applications
- This is intentional - only correct password unlocks
- Alt+Tab, Win key, and Ctrl+Esc are blocked to prevent bypassing
- Lock screen will re-appear if time still exceeded
- Admin can unlock temporarily to:
  - Save work
  - Reset counter via Admin Settings
  - Change max hours
  - Disable timer
- After changes, lock screen won't reappear if under new limit
- **Safety:** Ctrl+Alt+Del still works to access Task Manager

### Tray icon not visible
- Check hidden icons area (click arrow in system tray)
- Verify application is running in Task Manager
- Restart application as Administrator

### Build fails on macOS
- Ensure Docker is running: `docker ps`
- Verify Docker has enough disk space
- Try: `./clean.sh && ./build.sh`

### Password won't save
- Ensure minimum 4 characters
- Confirm password matches
- Check file permissions on `config.bin`

## Uninstalling

### MSI Installation
1. Open **Settings** → **Apps** → **Installed apps**
2. Find "Windows Timer Lock"
3. Click **Uninstall**
   - Or command line: `msiexec /x WindowsTimerLock.msi /quiet`

### PowerShell/Manual Installation
1. Delete scheduled task:
   ```powershell
   schtasks /delete /tn "WindowsTimerLock" /f
   ```
2. Delete application files:
   ```powershell
   Remove-Item "C:\Program Files\WindowsTimerLock" -Recurse -Force
   ```
3. Remove startup shortcut (if using Method 3):
   - Press `Win + R`, type `shell:startup`, delete shortcut

## Security Considerations

⚠️ **Important Security Notes:**

1. This application requires Administrator privileges
2. Implements system-wide keyboard hook to prevent task switching
3. Blocks Alt+Tab, Windows key, and Ctrl+Esc during operation
4. Users with Administrator access can still:
   - Use Ctrl+Alt+Del to access Task Manager
   - Terminate the process via Task Manager
   - Delete the `timer_data.bin` file
   - Boot into Safe Mode to disable the scheduled task
   - Uninstall via Add/Remove Programs (MSI) or delete files manually

**Restricted Actions:**
- ❌ Alt+Tab (task switching) - Blocked
- ❌ Windows key (Start menu) - Blocked
- ❌ Ctrl+Esc (Start menu) - Blocked
- ✅ Ctrl+Alt+Del (security screen) - Available for safety

For production use in a managed environment, consider:
- Using Windows Group Policy to restrict access to Task Manager
- Using Windows Group Policy to restrict access to Task Scheduler
- Setting NTFS permissions (done automatically by MSI installer)
- Monitoring via MDM (Mobile Device Management) solutions
- Code signing the MSI to avoid SmartScreen warnings
- Implementing additional process protection mechanisms
- Disabling Safe Mode boot options via Group Policy

## License

This is a utility application. Use at your own risk. The author is not responsible for any data loss or system issues.

## Development

To modify and rebuild:

```bash
# Edit source files
nano Program.cs

# Rebuild
./build.sh

# Test on Windows VM or physical machine
```

## Testing

Comprehensive test cases are available in [TEST_CASES.md](TEST_CASES.md):
- 31 functional test scenarios
- Performance benchmarks
- Automated PowerShell test script
- Bug reporting template

**Recommended:** Test in a Windows VM before deploying to production.

## Support

For issues or questions:
1. Review [TEST_CASES.md](TEST_CASES.md) for testing procedures
2. Check Windows Event Viewer logs
3. Verify admin password and permissions
4. Use kill switch for emergency recovery

## Screenshots

### System Tray Display
The shield icon shows real-time countdown in the tooltip.

### Countdown Dialog
- Status indicator (RUNNING/PAUSED/DISABLED)
- Real-time countdown display
- Today's usage statistics
- Reset time information

### Admin Settings
- Password-protected access
- Configure maximum hours (1-24)
- Enable/disable timer
- Reset counter button
- Change password fields

## Version History

### v2.3 (Current)
- ➕ Added 30-second countdown sound alert
- 🔊 Plays 3 beeps (800 Hz) when 30 seconds remaining
- 🔔 Alert plays once per day, resets at midnight
- 🔇 Gracefully handles systems where sound is not supported
- ⚠️ Provides audible warning before time limit reached

### v2.2
- ➕ Added live updating countdown dialog (replaces static MessageBox)
- ➕ Real-time updates every second for Used Today and Time Remaining
- ➕ Color-coded status indicators (Running/Paused/Disabled)
- ➕ Dynamic color warnings based on remaining time (green/orange/red)
- ➕ Professional layout with monospaced fonts for time displays
- 📊 Improved user experience with live visual feedback

### v2.1
- ➕ Added custom full-screen lock screen with admin password unlock
- ➕ Added live clock display on lock screen
- ➕ Added visual feedback (shake effect) for wrong password
- ➕ Added auto re-lock if time still exceeded
- ➕ Lock screen cannot be closed or bypassed (TopMost, no Alt+F4)
- 🔒 Improved security - admin password required to unlock

### v2.0
- ➕ Added Windows Forms GUI
- ➕ Added system tray icon with live countdown
- ➕ Added password-protected admin interface
- ➕ Added configurable max hours (1-24)
- ➕ Added enable/disable toggle
- ➕ Added reset counter function
- ➕ Added change password feature
- ⬆️ Upgraded to .NET 10.0
- 📦 Size: ~49 MB (includes GUI framework)

### v1.0
- ✅ Basic console application
- ✅ Fixed 4-hour limit
- ✅ Background service
- 📦 Size: ~11 MB

---

**Built with .NET 10.0 on macOS using Docker** 🐳 🪟

# Gen Ai Instruction
1/ i am using Claude Sonnet 4.5

My Prompts:
1/ I want to write a windows application running without any GUI. This application will lock down the windows if it uses more than 4 hours. It needed an admin password to unlock the windows. It reset every day 23:59. It needs to keep the total time per day, user cannot restart the windows to reset the total time per day. The counter stop is the laptop lid is close or windows log out.Add a kill switch to shutdown entire programme and unlock the windows.providers me step by step to setup development environment on MacOs using Docker. Combine all the dll into 1 single EXE for easy deployment.

2/ change the code to allow count down timer. the GUI also provides admin UI which is protected by password to allow reset of counter, to change the hours or disable the count down.

3/ create a test script to write up all the test usecases

4/ when the screen is lock how to unlock with admin password?

5/ in the GUI, the Used Today and Time Remainding must be in tick to update the time.
6/ the application is in the lock mode, when key in the the admin password, reset the timer counter.
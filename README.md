# Windows Timer Lock

A Windows GUI application with system tray integration that limits daily computer usage with a countdown timer, automatically locking the system when the time limit is reached. Features a password-protected admin interface for configuration.

## Features

### Core Functionality
- ✅ **Countdown Timer** - Real-time display in system tray tooltip
- ✅ **Custom Lock Screen** - Full-screen password-protected lock when time limit reached
- ✅ **Admin Unlock** - Unlock with admin password (temporary access)
- ✅ **Daily Reset** - Automatically resets at midnight
- ✅ **Smart Pause** - Stops counting when:
  - Laptop lid is closed
  - System goes to sleep/suspend
  - User logs out or locks the session
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

### Step 1: Transfer the EXE
Copy `output/win-x64/WindowsTimerLock.exe` to your Windows machine.

### Step 2: Run as Administrator
Right-click `WindowsTimerLock.exe` and select "Run as administrator"

The application will:
- Start counting immediately
- Appear as a shield icon in the system tray (bottom-right corner)
- Display countdown timer in the tray tooltip
- Create `timer_data.bin` and `config.bin` files to persist settings
- Lock the computer when time limit is reached

### Step 3: Basic Usage

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

### Step 3: Install as Windows Service (Optional)

To run the application automatically on startup, install it as a Windows service using NSSM:

1. Download NSSM: https://nssm.cc/download
2. Open Command Prompt as Administrator
3. Run:
   ```cmd
   nssm install WindowsTimerLock "C:\path\to\WindowsTimerLock.exe"
   nssm set WindowsTimerLock Start SERVICE_AUTO_START
   nssm start WindowsTimerLock
   ```

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
   - System suspends/sleeps
   - User locks the screen
   - User logs out
   - Lid closes (triggers suspend)
5. **Lock Mechanism**: Uses Windows API `LockWorkStation()` to lock the computer
6. **Admin Required**: Manifest requires administrator privileges for system-level operations

## File Structure

```
timer/
├── Dockerfile              # Docker image for building on macOS
├── .dockerignore          # Docker ignore patterns
├── WindowsTimerLock.csproj # .NET project configuration
├── app.manifest           # Windows manifest (admin rights)
├── Program.cs             # Main application code (GUI + logic)
├── build.sh              # Build script for macOS
├── clean.sh              # Clean script
├── README.md             # User documentation
├── SETUP.md              # Development setup guide
├── TEST_CASES.md         # Comprehensive test scenarios
└── output/
    └── win-x64/
        └── WindowsTimerLock.exe  # Final executable (~49 MB)
```

### Runtime Files (Created on Windows)
```
├── timer_data.bin         # Daily usage data
├── config.bin            # Settings and password hash
└── kill_switch.txt       # Emergency shutdown trigger
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
- **Password required** - Must enter admin password to unlock
- **Visual feedback** - Screen shakes on wrong password
- **Auto re-lock** - If time still exceeded after unlock, locks again
- **Temporary access** - Admin can unlock to save work/extend time

**Lock Screen Features:**
- 🛑 Full-screen dark overlay
- ⏰ "⏱️ TIME LIMIT REACHED" header
- 📅 Live date and time display
- 🔐 Password entry field
- 🔓 Unlock button
- ❌ Cannot minimize, close, or bypass
- 🔄 Re-locks if time still exceeded

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
- If password forgotten:
  - Boot to Windows Safe Mode
  - Navigate to application directory
  - Create `kill_switch.txt` file
  - Restart normally - application will exit
  - Delete `config.bin` to reset password

### Lock screen won't close
- This is intentional - only correct password unlocks
- Lock screen will re-appear if time still exceeded
- Admin can unlock temporarily to:
  - Save work
  - Reset counter via Admin Settings
  - Change max hours
  - Disable timer
- After changes, lock screen won't reappear if under new limit

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

## Security Considerations

⚠️ **Important Security Notes:**

1. This application requires Administrator privileges
2. Users with Administrator access can terminate the process
3. Advanced users can:
   - Delete the `timer_data.bin` file
   - Use Task Manager to kill the process
   - Boot into Safe Mode to disable the service

For production use in a managed environment, consider:
- Implementing process protection
- Using Windows Group Policy to restrict access
- Monitoring via MDM (Mobile Device Management) solutions

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

### v2.2 (Current)
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
# Windows Timer Lock - Test Cases

## Test Environment Setup

**Requirements:**
- Windows 10/11 (64-bit)
- Administrator privileges
- WindowsTimerLock.exe deployed to test directory (e.g., C:\TimerTest)
- Default admin password: `admin123`

---

## Test Case 1: Initial Installation & Startup

### Objective
Verify application starts correctly and creates necessary files

### Preconditions
- Clean installation (no existing config files)
- Run as Administrator

### Steps
1. Right-click `WindowsTimerLock.exe` → Run as administrator
2. Check system tray (bottom-right corner)
3. Hover over the shield icon
4. Check application directory for created files

### Expected Results
- ✅ Shield icon appears in system tray
- ✅ Tooltip shows: "Remaining: 04:00:00" (or similar)
- ✅ Files created: `timer_data.bin`, `config.bin`
- ✅ No error messages

### Pass/Fail: ⬜

---

## Test Case 2: View Live Countdown Display

### Objective
Verify countdown timer display shows accurate information with real-time updates

### Steps
1. Double-click the system tray icon
2. Observe the countdown dialog
3. Keep dialog open for 10 seconds
4. Watch the time values update

### Expected Results
- ✅ Custom dialog window opens (not MessageBox)
- ✅ Dialog shows:
  - Title: "⏱️ Timer Status"
  - Status: ▶️ RUNNING (with green color)
  - Time Remaining: HH:MM:SS format with color coding:
    - Green: > 15 minutes remaining
    - Orange: 5-15 minutes remaining
    - Red: < 5 minutes or time up
  - Used Today: HH:MM:SS format (red color)
  - Max Allowed: X hours
  - Resets at: 00:00 (Midnight)
- ✅ Time Remaining decreases every second
- ✅ Used Today increases every second
- ✅ Values update live without refreshing
- ✅ Close button to dismiss dialog

### Pass/Fail: ⬜

---

## Test Case 3: Admin Login - Correct Password

### Objective
Verify admin login with correct password

### Steps
1. Right-click system tray icon
2. Select "Admin Settings..."
3. Enter password: `admin123`
4. Click "Login"

### Expected Results
- ✅ Admin Settings dialog opens
- ✅ Shows current usage
- ✅ Shows all configuration options

### Pass/Fail: ⬜

---

## Test Case 4: Admin Login - Incorrect Password

### Objective
Verify security with wrong password

### Steps
1. Right-click system tray icon
2. Select "Admin Settings..."
3. Enter password: `wrongpassword`
4. Click "Login"

### Expected Results
- ✅ Error message: "Incorrect password!"
- ✅ Login dialog remains open
- ✅ Password field cleared
- ✅ Admin Settings does NOT open

### Pass/Fail: ⬜

---

## Test Case 5: Change Maximum Hours

### Objective
Verify admin can change daily time limit

### Steps
1. Open Admin Settings (password: `admin123`)
2. Change "Maximum Hours Per Day" from 4 to 2
3. Click "Save"
4. Double-click tray icon to view countdown

### Expected Results
- ✅ Success message: "Settings saved successfully!"
- ✅ Countdown shows new max: "Max Allowed: 2 hours"
- ✅ Time remaining adjusted accordingly
- ✅ `config.bin` updated

### Pass/Fail: ⬜

---

## Test Case 6: Reset Counter

### Objective
Verify admin can reset today's usage counter

### Steps
1. Let timer run for 5 minutes (Used Today: ~00:05:00)
2. Open Admin Settings (password: `admin123`)
3. Click "Reset Counter to Zero"
4. Confirm dialog
5. Click "Save"
6. View countdown

### Expected Results
- ✅ Confirmation dialog: "Reset today's usage counter to zero?"
- ✅ Scheduled message: "Counter will be reset when you click Save"
- ✅ After save: Used Today returns to 00:00:00
- ✅ Time remaining reset to maximum

### Pass/Fail: ⬜

---

## Test Case 7: Disable Timer

### Objective
Verify timer can be disabled completely

### Steps
1. Open Admin Settings (password: `admin123`)
2. Uncheck "Timer Enabled"
3. Click "Save"
4. Observe tray icon tooltip
5. Wait 1 minute
6. Check countdown

### Expected Results
- ✅ Tray tooltip changes to: "Timer: DISABLED"
- ✅ Used time does NOT increase
- ✅ Countdown shows: Status: DISABLED
- ✅ Computer will NOT lock when time expires

### Pass/Fail: ⬜

---

## Test Case 8: Re-enable Timer

### Objective
Verify timer can be re-enabled

### Steps
1. With timer disabled, open Admin Settings
2. Check "Timer Enabled"
3. Click "Save"
4. Observe behavior

### Expected Results
- ✅ Tray tooltip shows remaining time again
- ✅ Used time starts increasing
- ✅ Countdown shows: Status: RUNNING
- ✅ Timer enforcement resumes

### Pass/Fail: ⬜

---

## Test Case 9: Change Admin Password

### Objective
Verify admin can change their password

### Steps
1. Open Admin Settings (password: `admin123`)
2. In "New Password" field, enter: `newpass123`
3. In "Confirm" field, enter: `newpass123`
4. Click "Save"
5. Close and reopen Admin Settings
6. Try old password: `admin123`
7. Try new password: `newpass123`

### Expected Results
- ✅ Settings saved successfully
- ✅ Old password (`admin123`) no longer works
- ✅ New password (`newpass123`) grants access
- ✅ Password persists after application restart

### Pass/Fail: ⬜

---

## Test Case 10: Password Validation - Mismatch

### Objective
Verify password confirmation works

### Steps
1. Open Admin Settings
2. New Password: `test123`
3. Confirm: `test456`
4. Click "Save"

### Expected Results
- ✅ Error message: "Passwords do not match!"
- ✅ Settings NOT saved
- ✅ Dialog remains open

### Pass/Fail: ⬜

---

## Test Case 11: Password Validation - Too Short

### Objective
Verify minimum password length

### Steps
1. Open Admin Settings
2. New Password: `abc`
3. Confirm: `abc`
4. Click "Save"

### Expected Results
- ✅ Error message: "Password must be at least 4 characters!"
- ✅ Settings NOT saved

### Pass/Fail: ⬜

---

## Test Case 12: Time Limit Reached - Custom Lock Screen

### Objective
Verify custom lock screen appears when time limit reached

### Steps
1. Set max hours to 1 minute (for testing, modify code temporarily)
2. Wait for countdown to reach 00:00:00
3. Observe behavior

### Expected Results
- ✅ Tray tooltip shows: "Timer: TIME UP!"
- ✅ Full-screen custom lock screen appears
- ✅ Lock screen shows:
  - "⏱️ TIME LIMIT REACHED" message
  - Current date and time (updating every second)
  - "Enter admin password to unlock" instruction
  - Password field
  - Unlock button
- ✅ Cannot close lock screen (no X, Alt+F4 disabled)
- ✅ Screen is TopMost (covers everything)
- ✅ Status changes to: PAUSED
- ✅ Time counter stops

### Pass/Fail: ⬜

---

## Test Case 13: Manual Lock (Windows + L)

### Objective
Verify timer pauses on manual lock

### Steps
1. Note current usage time
2. Press Windows + L to lock computer
3. Wait 2 minutes (locked)
4. Unlock computer
5. Check usage time

### Expected Results
- ✅ Timer pauses when locked
- ✅ Used time does NOT increase during lock
- ✅ Timer resumes after unlock
- ✅ Tray icon updates correctly

### Pass/Fail: ⬜

---

## Test Case 14: User Logout

### Objective
Verify timer pauses on logout

### Steps
1. Note current usage time
2. Log out of Windows
3. Wait 2 minutes
4. Log back in
5. Check the tray icon and usage

### Expected Results
- ✅ Application auto-starts (if configured as service)
- ✅ Used time preserved from before logout
- ✅ No time added during logout period
- ✅ Timer resumes after login

### Pass/Fail: ⬜

---

## Test Case 15: Laptop Lid Close (Suspend)

### Objective
Verify timer pauses when laptop lid closes

### Steps
1. Note current usage time
2. Close laptop lid (triggers suspend)
3. Wait 5 minutes
4. Open lid and unlock
5. Check usage time

### Expected Results
- ✅ Timer pauses on suspend
- ✅ No time added during suspend
- ✅ Timer resumes after wake
- ✅ Usage data preserved

### Pass/Fail: ⬜

---

## Test Case 16: System Restart

### Objective
Verify usage time persists across reboots

### Steps
1. Let timer run to 30 minutes used
2. Note exact usage time
3. Restart computer
4. Launch application
5. Check usage time

### Expected Results
- ✅ Application reads from `timer_data.bin`
- ✅ Usage time matches pre-reboot value
- ✅ Countdown continues from previous state
- ✅ No reset occurred

### Pass/Fail: ⬜

---

## Test Case 17: Daily Reset at Midnight

### Objective
Verify counter resets at midnight

### Steps
1. Let timer run to 2 hours used
2. Keep computer on past midnight
3. After midnight (00:00:01), check countdown

### Expected Results
- ✅ Used Today resets to 00:00:00
- ✅ Time Remaining resets to maximum (4 hours)
- ✅ Status remains RUNNING
- ✅ New day starts fresh

### Pass/Fail: ⬜

---

## Test Case 18: Kill Switch Activation

### Objective
Verify kill switch terminates application

### Steps
1. Navigate to application directory
2. Create empty file: `kill_switch.txt`
   - Command Prompt: `echo. > kill_switch.txt`
3. Wait 10 seconds
4. Check system tray
5. Check directory for files

### Expected Results
- ✅ Application exits within 10 seconds
- ✅ Tray icon disappears
- ✅ `timer_data.bin` deleted
- ✅ `config.bin` deleted
- ✅ `kill_switch.txt` deleted
- ✅ System remains unlocked

### Pass/Fail: ⬜

---

## Test Case 19: Exit via Admin Menu

### Objective
Verify password-protected exit

### Steps
1. Right-click tray icon
2. Select "Exit (Admin)"
3. Enter password
4. Confirm exit

### Expected Results
- ✅ Password prompt appears
- ✅ Confirmation dialog: "Are you sure you want to exit...?"
- ✅ After confirmation, application exits
- ✅ Tray icon removed
- ✅ Usage data saved to `timer_data.bin`

### Pass/Fail: ⬜

---

## Test Case 20: Exit Without Password

### Objective
Verify cannot exit without admin password

### Steps
1. Right-click tray icon
2. Select "Exit (Admin)"
3. Click "Cancel" on password prompt

### Expected Results
- ✅ Application continues running
- ✅ Tray icon remains visible
- ✅ Timer continues counting

### Pass/Fail: ⬜

---

## Test Case 21: Admin Password Unlock from Lock Screen

### Objective
Verify admin can unlock with correct password

### Steps
1. Reach time limit (lock screen appears)
2. Enter correct admin password in lock screen
3. Click "Unlock" button
4. Observe behavior

### Expected Results
- ✅ Lock screen accepts correct password
- ✅ If still over limit: Lock screen reappears immediately
- ✅ If under limit (new day/reset): Returns to normal operation
- ✅ Timer remains paused while locked
- ✅ Admin can temporarily unlock to save work

### Pass/Fail: ⬜

---

## Test Case 21a: Wrong Password on Lock Screen

### Objective
Verify lock screen rejects wrong password

### Steps
1. Reach time limit (lock screen appears)
2. Enter incorrect password
3. Click "Unlock" or press Enter
4. Observe behavior

### Expected Results
- ✅ Screen shakes (visual feedback)
- ✅ Password field turns red briefly
- ✅ Password field clears
- ✅ Lock screen remains displayed
- ✅ Focus returns to password field
- ✅ Cannot bypass lock screen

### Pass/Fail: ⬜

---

## Test Case 21b: Lock Screen Cannot Be Closed

### Objective
Verify lock screen cannot be bypassed

### Steps
1. Reach time limit (lock screen appears)
2. Try Alt+F4
3. Try Task Manager (Ctrl+Shift+Esc)
4. Try clicking outside the dialog
5. Try switching windows (Alt+Tab)

### Expected Results
- ✅ Alt+F4 does nothing
- ✅ Lock screen remains TopMost
- ✅ Cannot click outside to close
- ✅ Cannot switch to other windows easily
- ✅ Only correct password unlocks

### Pass/Fail: ⬜

---

## Test Case 22: Lock Screen Clock Updates

### Objective
Verify lock screen displays live clock

### Steps
1. Reach time limit (lock screen appears)
2. Observe the date/time display
3. Watch for 1 minute

### Expected Results
- ✅ Date and time displayed prominently
- ✅ Time updates every second
- ✅ Format: "Day, Month DD, YYYY  HH:MM:SS"
- ✅ Clock remains accurate

### Pass/Fail: ⬜

---

## Test Case 23: Real-time Display Synchronization

### Objective
Verify tray tooltip and countdown dialog update synchronously

### Steps
1. Open countdown dialog (double-click tray icon)
2. Keep dialog open
3. Hover over tray icon
4. Watch both displays for 30 seconds

### Expected Results
- ✅ Tray tooltip updates every second
- ✅ Countdown dialog updates every second
- ✅ Both show identical values
- ✅ Format: "Remaining: HH:MM:SS" in tooltip
- ✅ Format: "HH:MM:SS" in dialog
- ✅ Seconds count down correctly
- ✅ Minutes/hours adjust properly
- ✅ No lag or desynchronization

### Pass/Fail: ⬜

---

## Test Case 23: Context Menu Accessibility

### Objective
Verify all context menu items work

### Steps
1. Right-click tray icon
2. Verify menu items present
3. Test each menu item

### Expected Results
- ✅ "Show Countdown" - Opens countdown dialog
- ✅ "Admin Settings..." - Opens login prompt
- ✅ Separator line visible
- ✅ "Exit (Admin)" - Opens login prompt

### Pass/Fail: ⬜

---

## Test Case 24: Countdown Dialog Color Coding

### Objective
Verify time remaining changes color based on threshold

### Steps
1. Set max hours to 20 minutes (for testing)
2. Let timer run to various thresholds
3. Open countdown dialog at each threshold:
   - 20 minutes remaining
   - 10 minutes remaining
   - 4 minutes remaining
   - 0 minutes remaining

### Expected Results
- ✅ > 15 minutes: Time Remaining is GREEN
- ✅ 5-15 minutes: Time Remaining is ORANGE
- ✅ < 5 minutes: Time Remaining is RED
- ✅ 0 or negative: Time Remaining shows "00:00:00" in RED
- ✅ Status icon and color changes appropriately:
  - ▶️ RUNNING (green)
  - ⏸️ PAUSED (orange)
  - ⏹️ DISABLED (gray)

### Pass/Fail: ⬜

---

## Test Case 25: Countdown Dialog Live Updates

### Objective
Verify countdown dialog updates continuously while open

### Steps
1. Reset counter to zero
2. Open countdown dialog
3. Keep dialog open for 2 minutes
4. Observe continuous updates

### Expected Results
- ✅ Time Remaining decrements every second
- ✅ Used Today increments every second
- ✅ No freezing or lag
- ✅ Smooth, continuous updates
- ✅ Values remain synchronized with tray icon
- ✅ Can leave dialog open indefinitely
- ✅ Status changes reflect immediately (if paused/disabled)

### Pass/Fail: ⬜

---

## Test Case 26: Concurrent Usage Tracking

### Objective
Verify accurate time tracking during active use

### Steps
1. Reset counter to zero
2. Use computer actively for exactly 15 minutes (set timer)
3. Check countdown at 15 minute mark

### Expected Results
- ✅ Used Today shows ~00:15:00 (±5 seconds acceptable)
- ✅ Time Remaining shows ~03:45:00
- ✅ Accurate second-by-second counting

### Pass/Fail: ⬜

---

## Test Case 25: Configuration Persistence

### Objective
Verify settings persist across restarts

### Steps
1. Change max hours to 6
2. Change password to `testpass`
3. Disable timer
4. Exit application
5. Restart application
6. Open Admin Settings

### Expected Results
- ✅ Max hours still set to 6
- ✅ New password works (`testpass`)
- ✅ Timer still disabled
- ✅ All settings loaded from `config.bin`

### Pass/Fail: ⬜

---

## Test Case 26: File Corruption Recovery

### Objective
Verify application handles corrupted data files

### Steps
1. Close application
2. Delete or corrupt `config.bin`
3. Delete or corrupt `timer_data.bin`
4. Restart application

### Expected Results
- ✅ Application starts without error
- ✅ Creates new `config.bin` with defaults
- ✅ Creates new `timer_data.bin`
- ✅ Default password works: `admin123`
- ✅ Max hours reset to 4

### Pass/Fail: ⬜

---

## Test Case 27: System Tray Icon Double-Click

### Objective
Verify double-click opens countdown

### Steps
1. Double-click system tray icon
2. Close dialog
3. Double-click again

### Expected Results
- ✅ First click opens countdown dialog
- ✅ Dialog can be closed
- ✅ Second click opens countdown again
- ✅ Shows updated time values

### Pass/Fail: ⬜

---

## Test Case 28: Long Running Session

### Objective
Verify stability over extended period

### Steps
1. Let application run for 24 hours
2. Check for memory leaks (Task Manager)
3. Verify functionality throughout

### Expected Results
- ✅ No crashes or hangs
- ✅ Memory usage stable (< 100 MB)
- ✅ Countdown still accurate
- ✅ Daily reset occurs correctly at midnight

### Pass/Fail: ⬜

---

## Test Case 29: Rapid Settings Changes

### Objective
Verify application handles rapid configuration changes

### Steps
1. Open Admin Settings
2. Change max hours 5 times rapidly
3. Toggle enabled/disabled 5 times
4. Save changes
5. Verify stability

### Expected Results
- ✅ All changes processed correctly
- ✅ No crashes or errors
- ✅ Final settings applied correctly
- ✅ UI remains responsive

### Pass/Fail: ⬜

---

## Test Case 30: Edge Case - 24 Hour Limit

### Objective
Verify maximum setting works

### Steps
1. Set max hours to 24
2. Let run for several hours
3. Check countdown behavior

### Expected Results
- ✅ Accepts maximum value of 24 hours
- ✅ Countdown shows full 24 hours available
- ✅ Timer functions normally
- ✅ Lock occurs at 24 hours if reached

### Pass/Fail: ⬜

---

## Test Case 31: Edge Case - 1 Hour Limit

### Objective
Verify minimum setting works

### Steps
1. Set max hours to 1
2. Monitor countdown
3. Verify lock occurs

### Expected Results
- ✅ Accepts minimum value of 1 hour
- ✅ Countdown shows 01:00:00
- ✅ Lock occurs after 1 hour
- ✅ Timer functions normally

### Pass/Fail: ⬜

---

## Performance Benchmarks

### CPU Usage
- **Idle:** < 1% CPU
- **Active:** < 2% CPU
- **Pass/Fail:** ⬜

### Memory Usage
- **Initial:** ~40-60 MB
- **After 1 hour:** < 80 MB
- **After 24 hours:** < 100 MB
- **Pass/Fail:** ⬜

### Disk I/O
- **Save operations:** Every 30 seconds
- **File size:** `timer_data.bin` < 1 KB
- **File size:** `config.bin` < 1 KB
- **Pass/Fail:** ⬜

---

## Test Summary

**Total Test Cases:** 36 + Performance Benchmarks

| Category | Count |
|----------|-------|
| Installation & Startup | 1 |
| UI & Display | 6 |
| Authentication | 6 |
| Configuration | 6 |
| Timer Behavior | 9 |
| Persistence | 4 |
| Edge Cases | 2 |
| Lock Screen | 3 |
| Performance | 2 |

**Passed:** ___ / 36  
**Failed:** ___ / 36  
**Blocked:** ___ / 36  
**Not Tested:** ___ / 36

---

## Bug Reporting Template

### Bug ID: [BUG-XXX]
**Title:**  
**Severity:** Critical / High / Medium / Low  
**Test Case:** [Test Case #]  
**Description:**  
**Steps to Reproduce:**  
1. 
2. 
3. 

**Expected Result:**  
**Actual Result:**  
**Screenshots:** (if applicable)  
**System Info:**
- Windows Version:
- Application Version:
- Date/Time:

---

## Automated Test Script (PowerShell)

```powershell
# Quick validation script for Windows
# Save as: test-timer-lock.ps1

Write-Host "Windows Timer Lock - Quick Test Script" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan

# Check if running as admin
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "[FAIL] Not running as Administrator" -ForegroundColor Red
    exit 1
}
Write-Host "[PASS] Running as Administrator" -ForegroundColor Green

# Check if EXE exists
$exePath = ".\WindowsTimerLock.exe"
if (Test-Path $exePath) {
    Write-Host "[PASS] WindowsTimerLock.exe found" -ForegroundColor Green
} else {
    Write-Host "[FAIL] WindowsTimerLock.exe not found" -ForegroundColor Red
    exit 1
}

# Check file size
$fileSize = (Get-Item $exePath).Length / 1MB
Write-Host "[INFO] File size: $([math]::Round($fileSize, 2)) MB" -ForegroundColor Yellow

# Start application
Write-Host "[INFO] Starting application..." -ForegroundColor Yellow
Start-Process $exePath -Verb RunAs

Start-Sleep -Seconds 5

# Check if process is running
$process = Get-Process -Name "WindowsTimerLock" -ErrorAction SilentlyContinue
if ($process) {
    Write-Host "[PASS] Application is running (PID: $($process.Id))" -ForegroundColor Green
} else {
    Write-Host "[FAIL] Application is not running" -ForegroundColor Red
    exit 1
}

# Check memory usage
$memoryMB = [math]::Round($process.WorkingSet64 / 1MB, 2)
Write-Host "[INFO] Memory usage: $memoryMB MB" -ForegroundColor Yellow

# Check if config files created
Start-Sleep -Seconds 2
if (Test-Path "timer_data.bin") {
    Write-Host "[PASS] timer_data.bin created" -ForegroundColor Green
} else {
    Write-Host "[WARN] timer_data.bin not found" -ForegroundColor Yellow
}

if (Test-Path "config.bin") {
    Write-Host "[PASS] config.bin created" -ForegroundColor Green
} else {
    Write-Host "[WARN] config.bin not found" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Quick test completed. Application is running." -ForegroundColor Cyan
Write-Host "Check system tray for the shield icon." -ForegroundColor Cyan
Write-Host ""
Write-Host "To stop: Right-click tray icon > Exit (Admin) > Password: admin123" -ForegroundColor Yellow
```

---

## Notes for Testers

1. **Test in a VM first** - Avoid locking your primary workstation
2. **Document everything** - Screenshots help debugging
3. **Test edge cases** - Users will find creative ways to break things
4. **Performance matters** - Monitor resource usage
5. **Security is critical** - Verify password protection works
6. **User experience** - Is it intuitive? Clear messaging?

## Test Environment Recommendations

- **VM Software:** VMware Workstation, VirtualBox, or Hyper-V
- **Windows Snapshots:** Take before each major test
- **Test Duration:** Allocate 4-6 hours for full test suite
- **Multiple Testers:** Different perspectives catch different bugs

---

**Test Date:** __________  
**Tester Name:** __________  
**Application Version:** __________  
**Test Environment:** __________

# Security Enhancements - Hotkey Bypass Prevention

## Overview
Enhanced the Windows Timer Lock application to prevent bypass attempts through common Windows hotkeys and system shortcuts.

## Blocked Hotkeys

### Task Manager Access
- **Ctrl+Shift+Esc** - Direct Task Manager shortcut (BLOCKED)
- **Ctrl+Shift+Del** - Alternative Task Manager access (BLOCKED)
- **Registry-based Task Manager disable** - Additional layer when lock screen is active

### System Security Menu
- **Ctrl+Alt+Del** - While true Ctrl+Alt+Del is kernel-level, we block the Delete key with Ctrl+Alt modifiers (BLOCKED)

### Task Switching & Window Management
- **Alt+Tab** - Switch between applications (BLOCKED)
- **Alt+Esc** - Alternative task switcher (BLOCKED)
- **Alt+F4** - Close active window (BLOCKED)

### Start Menu & System Access
- **Ctrl+Escape** - Open Start Menu (BLOCKED)
- **Windows Key (Left & Right)** - All Windows key shortcuts (BLOCKED)
  - Blocks Win+D (Desktop), Win+L (Lock), Win+R (Run), Win+E (Explorer), Win+X (Power Menu), etc.

### Context Menu & Function Keys
- **Apps/Context Menu Key** - Right-click menu key (BLOCKED)
- **F1-F12 with modifiers** - Function keys with Ctrl/Alt/Shift combinations (BLOCKED)
  - Normal F-key usage without modifiers is still allowed

## Implementation Details

### 1. Enhanced Keyboard Hook
The `HookCallback` method now uses `GetAsyncKeyState` Windows API for more reliable key state detection:

```csharp
private bool IsKeyPressed(int vKey)
{
    return (GetAsyncKeyState(vKey) & 0x8000) != 0;
}
```

This provides better detection of key combinations compared to the previous `Control.ModifierKeys` approach.

### 2. Registry-Based Task Manager Disable
When the lock screen is shown, Task Manager is disabled at the registry level:

**Registry Key:** `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\System`
**Value:** `DisableTaskMgr = 1` (Disabled)

The Task Manager is automatically re-enabled when:
- User successfully unlocks the screen
- Application exits (in Dispose method)

### 3. Comprehensive Key Code Coverage
All virtual key codes (VK_*) for bypass attempts are monitored:
- VK_ESCAPE (0x1B)
- VK_TAB (0x09)
- VK_DELETE (0x2E)
- VK_F1-F12 (0x70-0x7B)
- VK_LWIN/RWIN (0x5B/0x5C)
- VK_APPS (0x5D)
- VK_CONTROL (0x11)
- VK_SHIFT (0x10)
- VK_MENU/Alt (0x12)

## Security Layers

### Primary Layer: Keyboard Hook
- Low-level keyboard hook intercepts all key presses
- Blocks dangerous key combinations before they reach Windows
- Active during normal operation to prevent all bypass attempts

### Secondary Layer: Registry Modification
- Disables Task Manager at Windows policy level
- Provides protection even if keyboard hook is somehow bypassed
- Automatically cleaned up on application exit

## Testing Recommendations

When testing on Windows, verify the following bypass attempts are blocked:

1. ✅ Press Ctrl+Shift+Esc (Task Manager should not open)
2. ✅ Press Ctrl+Shift+Del (Alternative Task Manager should not open)
3. ✅ Press Alt+Tab (Task switching should not work)
4. ✅ Press Alt+Esc (Task switching should not work)
5. ✅ Press Windows Key (Start Menu should not open)
6. ✅ Press Alt+F4 (Window closing should be blocked)
7. ✅ Press Ctrl+Escape (Start Menu should not open)
8. ✅ Right-click on taskbar → Task Manager (Should show "Task Manager has been disabled")
9. ✅ Try F-keys with modifiers (Should be blocked)
10. ✅ Normal F-keys without modifiers (Should work normally)

## Notes & Limitations

### Ctrl+Alt+Del Exception
The true Ctrl+Alt+Del sequence (Secure Attention Sequence) is handled by the Windows kernel and cannot be blocked by user-mode applications. However, we block the Delete key when used with Ctrl+Alt modifiers to prevent similar combinations.

### Administrative Privileges
Some registry operations may require appropriate permissions. The application includes error handling to continue operating if registry modifications fail (keyboard hook still provides primary protection).

### Application Exit
The `Dispose` method ensures Task Manager is always re-enabled when the application exits, preventing the system from being left in a restricted state.

## Future Considerations

1. Monitor for new Windows shortcuts in future OS versions
2. Consider blocking Ctrl+Shift+N (InPrivate browsing in some contexts)
3. Monitor Task Manager process spawn attempts via process monitoring
4. Add logging for bypass attempt tracking

## Compatibility

- **Windows 10/11**: Fully compatible
- **.NET 6.0+**: Required for Windows API interop
- **User Permissions**: Runs under current user context (registry modifications affect current user only)

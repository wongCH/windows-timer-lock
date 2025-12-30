using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WindowsTimerLock
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TimerLockApp());
        }
    }

    public class TimerLockApp : ApplicationContext
    {
        // P/Invoke declarations for Windows API
        [DllImport("user32.dll")]
        static extern bool LockWorkStation();

        // Keyboard hook declarations
        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private IntPtr hookId = IntPtr.Zero;

        // Configuration
        private const string DATA_FILE = "timer_data.bin";
        private const string CONFIG_FILE = "config.bin";
        private const string KILL_SWITCH_FILE = "kill_switch.txt";
        private const string DEFAULT_PASSWORD = "admin123";
        
        private NotifyIcon trayIcon;
        private System.Windows.Forms.Timer updateTimer;
        private System.Windows.Forms.Timer saveTimer;
        private System.Windows.Forms.Timer killSwitchTimer;
        private System.Windows.Forms.Timer powerModeCheckTimer;
        
        private TimeSpan totalUsageToday = TimeSpan.Zero;
        private DateTime lastUpdateTime = DateTime.Now;
        private DateTime currentDate = DateTime.Today;
        private bool isPaused = false;
        private bool isEnabled = true;
        private bool soundAlertPlayed = false;
        private bool isInActiveSession = true; // Track if system is in active session
        
        // Settings
        private int maxHours = 4; //maximum hours per day
        private string passwordHash;
        private LowLevelKeyboardProc keyboardProc;

        public TimerLockApp()
        {
            InitializeTrayIcon();
            LoadConfiguration();
            LoadUsageData();

            // Register system event handlers
            SystemEvents.SessionSwitch += OnSessionSwitch;
            SystemEvents.PowerModeChanged += OnPowerModeChanged;

            // Set up update timer (every second)
            updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Interval = 1000;
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();

            // Set up periodic save timer (every 30 seconds)
            saveTimer = new System.Windows.Forms.Timer();
            saveTimer.Interval = 30000;
            saveTimer.Tick += (s, e) => SaveUsageData();
            saveTimer.Start();

            // Set up kill switch check timer (every 10 seconds)
            killSwitchTimer = new System.Windows.Forms.Timer();
            killSwitchTimer.Interval = 10000;
            killSwitchTimer.Tick += (s, e) => CheckKillSwitch();
            killSwitchTimer.Start();

            // Set up power mode check timer (every 1 minute)
            powerModeCheckTimer = new System.Windows.Forms.Timer();
            powerModeCheckTimer.Interval = 60000; // 60 seconds
            powerModeCheckTimer.Tick += PowerModeCheckTimer_Tick;
            powerModeCheckTimer.Start();

            // Install keyboard hook to disable Alt+Tab
            keyboardProc = HookCallback;
            hookId = SetHook(keyboardProc);

            UpdateTrayIcon();
        }

        private void InitializeTrayIcon()
        {
            trayIcon = new NotifyIcon();
            trayIcon.Icon = SystemIcons.Shield;
            trayIcon.Visible = true;
            trayIcon.Text = "Windows Timer Lock";
            
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Show Countdown", null, ShowCountdown_Click);
            contextMenu.Items.Add("Admin Settings...", null, AdminSettings_Click);
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Exit (Admin)", null, Exit_Click);
            
            trayIcon.ContextMenuStrip = contextMenu;
            trayIcon.DoubleClick += (s, e) => ShowCountdown_Click(s, e);
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            
            // Check if we need to reset for a new day
            if (now.Date > currentDate)
            {
                totalUsageToday = TimeSpan.Zero;
                currentDate = now.Date;
                isPaused = false;
                soundAlertPlayed = false;
            }

            // If not paused and enabled, increment usage time
            if (!isPaused && isEnabled)
            {
                TimeSpan elapsed = now - lastUpdateTime;
                totalUsageToday += elapsed;
                
                // Check for 30-second warning
                TimeSpan remaining = TimeSpan.FromHours(maxHours) - totalUsageToday;
                if (!soundAlertPlayed && remaining.TotalSeconds <= 30 && remaining.TotalSeconds > 0)
                {
                    soundAlertPlayed = true;
                    PlayWarningSound();
                }
                
                // Check if time limit exceeded
                if (totalUsageToday.TotalHours >= maxHours)
                {
                    LockComputer();
                }
            }

            lastUpdateTime = now;
            UpdateTrayIcon();
        }

        private void PowerModeCheckTimer_Tick(object? sender, EventArgs e)
        {
            // If in active session and timer is paused, unpause it
            if (isInActiveSession && isPaused)
            {
                isPaused = false;
                lastUpdateTime = DateTime.Now;
            }
        }

        private void PlayWarningSound()
        {
            Task.Run(() =>
            {
                try
                {
                    // Play 3 beeps to alert user
                    for (int i = 0; i < 3; i++)
                    {
                        Console.Beep(800, 500); // 800 Hz for 500ms
                        System.Threading.Thread.Sleep(200);
                    }
                }
                catch
                {
                    // Ignore if beep is not supported
                }
            });
        }

        private void UpdateTrayIcon()
        {
            TimeSpan remaining = TimeSpan.FromHours(maxHours) - totalUsageToday;
            
            if (!isEnabled)
            {
                trayIcon.Text = "Timer: DISABLED";
            }
            else if (remaining.TotalSeconds <= 0)
            {
                trayIcon.Text = "Timer: TIME UP!";
            }
            else
            {
                int hours = (int)remaining.TotalHours;
                int minutes = remaining.Minutes;
                int seconds = remaining.Seconds;
                trayIcon.Text = $"Remaining: {hours:D2}:{minutes:D2}:{seconds:D2}";
            }
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule?.ModuleName ?? ""), 0);
            }
        }

        private bool IsKeyPressed(int vKey)
        {
            return (GetAsyncKeyState(vKey) & 0x8000) != 0;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = System.Runtime.InteropServices.Marshal.ReadInt32(lParam);
                
                // Get current modifier states using GetAsyncKeyState for more reliable detection
                bool ctrlPressed = IsKeyPressed(0x11);  // VK_CONTROL
                bool shiftPressed = IsKeyPressed(0x10); // VK_SHIFT
                bool altPressed = IsKeyPressed(0x12);   // VK_MENU (Alt)
                
                // Block Ctrl+Shift+Esc (Task Manager)
                if (vkCode == 0x1B && ctrlPressed && shiftPressed) // VK_ESCAPE = 0x1B
                {
                    return (IntPtr)1; // Block the key
                }
                
                // Block Ctrl+Shift+Del (alternative Task Manager shortcut)
                if (vkCode == 0x2E && ctrlPressed && shiftPressed) // VK_DELETE = 0x2E
                {
                    return (IntPtr)1; // Block the key
                }
                
                // Block Ctrl+Alt+Del simulation (we can only block Ctrl+Alt+Del-like key combos)
                // Note: True Ctrl+Alt+Del is handled by Windows at kernel level, but we can block Delete key with Ctrl+Alt
                if (vkCode == 0x2E && ctrlPressed && altPressed) // VK_DELETE = 0x2E
                {
                    return (IntPtr)1; // Block the key
                }
                
                // Block Alt+Tab (VK_TAB = 0x09)
                if (vkCode == 0x09 && altPressed)
                {
                    return (IntPtr)1; // Block the key
                }
                
                // Block Alt+Esc (another task switcher)
                if (vkCode == 0x1B && altPressed)
                {
                    return (IntPtr)1; // Block the key
                }
                
                // Block Ctrl+Escape (Start Menu)
                if (vkCode == 0x1B && ctrlPressed)
                {
                    return (IntPtr)1; // Block the key
                }
                
                // Block Alt+F4 (Close window)
                if (vkCode == 0x73 && altPressed) // VK_F4 = 0x73
                {
                    return (IntPtr)1; // Block the key
                }
                
                // Block Windows keys (VK_LWIN = 0x5B, VK_RWIN = 0x5C)
                if (vkCode == 0x5B || vkCode == 0x5C)
                {
                    return (IntPtr)1; // Block the key
                }
                
                // Block Win+D, Win+L, Win+R, Win+E, Win+X, etc. (handled by blocking Windows key above)
                
                // Block Function keys that might access system functions
                // F1-F12 keys (VK_F1 = 0x70 to VK_F12 = 0x7B)
                if (vkCode >= 0x70 && vkCode <= 0x7B)
                {
                    // Allow F-keys only without modifiers (for normal use)
                    // Block if used with Ctrl, Alt, or Shift (system shortcuts)
                    if (ctrlPressed || altPressed || shiftPressed)
                    {
                        return (IntPtr)1; // Block the key
                    }
                }
                
                // Block Ctrl+Shift (when pressed without other keys, as it might be preparation for Esc)
                // This is handled by the Ctrl+Shift+Esc check above
                
                // Block Apps/Context Menu key (VK_APPS = 0x5D)
                if (vkCode == 0x5D)
                {
                    return (IntPtr)1; // Block the key
                }
            }
            
            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        private void ShowCountdown_Click(object? sender, EventArgs e)
        {
            using (var countdownForm = new CountdownForm(this))
            {
                countdownForm.ShowDialog();
            }
        }

        // Public properties for CountdownForm to access
        public TimeSpan TotalUsageToday => totalUsageToday;
        public int MaxHours => maxHours;
        public bool IsEnabled => isEnabled;
        public bool IsPaused => isPaused;

        private void AdminSettings_Click(object? sender, EventArgs e)
        {
            using (var loginForm = new AdminLoginForm(passwordHash))
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    using (var settingsForm = new AdminSettingsForm(maxHours, isEnabled, totalUsageToday))
                    {
                        if (settingsForm.ShowDialog() == DialogResult.OK)
                        {
                            maxHours = settingsForm.MaxHours;
                            isEnabled = settingsForm.IsEnabled;
                            
                            if (settingsForm.ResetCounter)
                            {
                                totalUsageToday = TimeSpan.Zero;
                                lastUpdateTime = DateTime.Now;
                            }
                            
                            if (settingsForm.NewPassword != null)
                            {
                                passwordHash = HashPassword(settingsForm.NewPassword);
                            }
                            
                            SaveConfiguration();
                            SaveUsageData();
                            UpdateTrayIcon();
                            
                            MessageBox.Show("Settings saved successfully!", "Admin Settings", 
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        private void Exit_Click(object? sender, EventArgs e)
        {
            using (var loginForm = new AdminLoginForm(passwordHash))
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    
                    SaveUsageData();
                    trayIcon.Visible = false;
                    Application.Exit();
                }
            }
        }

        private void LoadConfiguration()
        {
            try
            {
                if (File.Exists(CONFIG_FILE))
                {
                    byte[] data = File.ReadAllBytes(CONFIG_FILE);
                    using (MemoryStream ms = new MemoryStream(data))
                    using (BinaryReader reader = new BinaryReader(ms))
                    {
                        maxHours = reader.ReadInt32();
                        passwordHash = reader.ReadString();
                        isEnabled = reader.ReadBoolean();
                    }
                }
                else
                {
                    passwordHash = HashPassword(DEFAULT_PASSWORD);
                    SaveConfiguration();
                }
            }
            catch
            {
                maxHours = 4;
                passwordHash = HashPassword(DEFAULT_PASSWORD);
                isEnabled = true;
            }
        }

        private void SaveConfiguration()
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    writer.Write(maxHours);
                    writer.Write(passwordHash);
                    writer.Write(isEnabled);
                    File.WriteAllBytes(CONFIG_FILE, ms.ToArray());
                }
            }
            catch { }
        }

        private void LoadUsageData()
        {
            try
            {
                if (File.Exists(DATA_FILE))
                {
                    byte[] data = File.ReadAllBytes(DATA_FILE);
                    using (MemoryStream ms = new MemoryStream(data))
                    using (BinaryReader reader = new BinaryReader(ms))
                    {
                        long dateTicks = reader.ReadInt64();
                        long usageTicks = reader.ReadInt64();
                        
                        DateTime savedDate = new DateTime(dateTicks);
                        
                        if (savedDate.Date == DateTime.Today)
                        {
                            totalUsageToday = new TimeSpan(usageTicks);
                            currentDate = savedDate.Date;
                        }
                        else
                        {
                            currentDate = DateTime.Today;
                        }
                    }
                }
                else
                {
                    currentDate = DateTime.Today;
                }
            }
            catch
            {
                currentDate = DateTime.Today;
            }
        }

        private void SaveUsageData()
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    writer.Write(currentDate.Ticks);
                    writer.Write(totalUsageToday.Ticks);
                    File.WriteAllBytes(DATA_FILE, ms.ToArray());
                }
            }
            catch { }
        }

        private void CheckKillSwitch()
        {
            if (File.Exists(KILL_SWITCH_FILE))
            {
                updateTimer?.Stop();
                saveTimer?.Stop();
                killSwitchTimer?.Stop();
                powerModeCheckTimer?.Stop();
                
                SystemEvents.SessionSwitch -= OnSessionSwitch;
                SystemEvents.PowerModeChanged -= OnPowerModeChanged;
                
                if (File.Exists(DATA_FILE))
                    File.Delete(DATA_FILE);
                if (File.Exists(CONFIG_FILE))
                    File.Delete(CONFIG_FILE);
                File.Delete(KILL_SWITCH_FILE);
                
                trayIcon.Visible = false;
                Application.Exit();
            }
        }

        private void LockComputer()
        {
            if (!isPaused)
            {
                isPaused = true;
                SaveUsageData();
                ShowLockScreen();
            }
        }

        private void ShowLockScreen()
        {
            // Disable Task Manager at system level for additional security
            DisableTaskManager();
            
            try
            {
                // Show custom lock screen with password unlock
                using (var lockScreen = new LockScreenForm(passwordHash, totalUsageToday, maxHours))
                {
                    var result = lockScreen.ShowDialog();
                    
                    if (result == DialogResult.OK)
                    {
                        // Admin unlocked without reset
                        if (totalUsageToday.TotalHours < maxHours)
                        {
                            isPaused = false;
                            lastUpdateTime = DateTime.Now;
                        }
                        else
                        {
                            // Still over limit, show lock screen again
                            ShowLockScreen();
                            return; // Don't re-enable Task Manager yet
                        }
                    }
                    else if (result == DialogResult.Retry)
                    {
                        // Admin unlocked and chose to reset timer
                        totalUsageToday = TimeSpan.Zero;
                        isPaused = false;
                        lastUpdateTime = DateTime.Now;
                        soundAlertPlayed = false;
                        SaveUsageData();
                    }
                }
            }
            finally
            {
                // Re-enable Task Manager when unlocked
                EnableTaskManager();
            }
        }

        private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                case SessionSwitchReason.SessionLogoff:
                case SessionSwitchReason.ConsoleDisconnect:
                case SessionSwitchReason.RemoteDisconnect:
                    // Pause timer on lock, logoff, or disconnect
                    SaveUsageData();
                    isPaused = true;
                    isInActiveSession = false;
                    break;

                case SessionSwitchReason.SessionUnlock:
                case SessionSwitchReason.SessionLogon:
                case SessionSwitchReason.ConsoleConnect:
                case SessionSwitchReason.RemoteConnect:
                    // Resume timer on unlock, logon, or connect
                    isPaused = false;
                    lastUpdateTime = DateTime.Now;
                    isInActiveSession = true;
                    
                    if (totalUsageToday.TotalHours >= maxHours && isEnabled)
                    {
                        Task.Delay(2000).ContinueWith(_ => LockComputer());
                    }
                    break;
            }
        }

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Suspend:
                    // Pause timer on suspend/sleep (includes lid close)
                    SaveUsageData();
                    isPaused = true;
                    isInActiveSession = false;
                    break;

                case PowerModes.Resume:
                    // Resume timer on wake
                    isPaused = false;
                    lastUpdateTime = DateTime.Now;
                    isInActiveSession = true;
                    break;
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        private void DisableTaskManager()
        {
            try
            {
                // Disable Task Manager via registry
                // HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\System
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System"))
                {
                    if (key != null)
                    {
                        // 1 = Disabled, 0 = Enabled
                        key.SetValue("DisableTaskMgr", 1, RegistryValueKind.DWord);
                    }
                }
            }
            catch
            {
                // If registry modification fails, continue anyway
                // The keyboard hook is still active
            }
        }

        private void EnableTaskManager()
        {
            try
            {
                // Re-enable Task Manager via registry
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true))
                {
                    if (key != null)
                    {
                        // Remove the restriction
                        key.DeleteValue("DisableTaskMgr", false);
                    }
                }
            }
            catch
            {
                // If registry modification fails, continue anyway
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Ensure Task Manager is re-enabled when app exits
                EnableTaskManager();
                // Unhook keyboard
                if (hookId != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(hookId);
                }
                
                updateTimer?.Dispose();
                saveTimer?.Dispose();
                killSwitchTimer?.Dispose();
                powerModeCheckTimer?.Dispose();
                trayIcon?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // Admin Login Form
    public class AdminLoginForm : Form
    {
        private TextBox passwordTextBox;
        private Button loginButton;
        private Button cancelButton;
        private string storedPasswordHash;

        public AdminLoginForm(string passwordHash)
        {
            storedPasswordHash = passwordHash;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Admin Login";
            this.Size = new Size(350, 150);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label label = new Label();
            label.Text = "Enter admin password:";
            label.Location = new Point(20, 20);
            label.Size = new Size(300, 20);

            passwordTextBox = new TextBox();
            passwordTextBox.Location = new Point(20, 45);
            passwordTextBox.Size = new Size(300, 25);
            passwordTextBox.PasswordChar = '*';
            passwordTextBox.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    e.Handled = true;
                    LoginButton_Click(s, e);
                }
            };

            loginButton = new Button();
            loginButton.Text = "Login";
            loginButton.Location = new Point(140, 80);
            loginButton.Size = new Size(80, 30);
            loginButton.Click += LoginButton_Click;

            cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.Location = new Point(240, 80);
            cancelButton.Size = new Size(80, 30);
            cancelButton.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.Add(label);
            this.Controls.Add(passwordTextBox);
            this.Controls.Add(loginButton);
            this.Controls.Add(cancelButton);
            this.AcceptButton = loginButton;
            this.CancelButton = cancelButton;
        }

        private void LoginButton_Click(object? sender, EventArgs e)
        {
            string enteredHash = HashPassword(passwordTextBox.Text);
            
            if (enteredHash == storedPasswordHash)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Incorrect password!", "Login Failed", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                passwordTextBox.Clear();
                passwordTextBox.Focus();
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
    }

    // Admin Settings Form
    public class AdminSettingsForm : Form
    {
        private NumericUpDown hoursNumeric;
        private CheckBox enabledCheckBox;
        private Button resetButton;
        private TextBox newPasswordTextBox;
        private TextBox confirmPasswordTextBox;
        private Button saveButton;
        private Button cancelButton;
        private Label usageLabel;

        public int MaxHours { get; private set; }
        public bool IsEnabled { get; private set; }
        public bool ResetCounter { get; private set; }
        public string? NewPassword { get; private set; }

        public AdminSettingsForm(int currentMaxHours, bool currentEnabled, TimeSpan currentUsage)
        {
            MaxHours = currentMaxHours;
            IsEnabled = currentEnabled;
            ResetCounter = false;
            InitializeComponents(currentUsage);
        }

        private void InitializeComponents(TimeSpan currentUsage)
        {
            this.Text = "Admin Settings";
            this.Size = new Size(400, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int y = 20;

            // Current Usage
            usageLabel = new Label();
            usageLabel.Text = $"Current Usage Today: {currentUsage.Hours:D2}:{currentUsage.Minutes:D2}:{currentUsage.Seconds:D2}";
            usageLabel.Location = new Point(20, y);
            usageLabel.Size = new Size(350, 20);
            usageLabel.Font = new Font(usageLabel.Font, FontStyle.Bold);
            y += 30;

            // Max Hours
            Label hoursLabel = new Label();
            hoursLabel.Text = "Maximum Hours Per Day:";
            hoursLabel.Location = new Point(20, y);
            hoursLabel.Size = new Size(200, 20);
            
            hoursNumeric = new NumericUpDown();
            hoursNumeric.Location = new Point(220, y);
            hoursNumeric.Size = new Size(80, 25);
            hoursNumeric.Minimum = 1;
            hoursNumeric.Maximum = 24;
            hoursNumeric.Value = MaxHours;
            y += 35;

            // Enabled Checkbox
            enabledCheckBox = new CheckBox();
            enabledCheckBox.Text = "Timer Enabled";
            enabledCheckBox.Location = new Point(20, y);
            enabledCheckBox.Size = new Size(200, 25);
            enabledCheckBox.Checked = IsEnabled;
            y += 35;

            // Reset Button
            resetButton = new Button();
            resetButton.Text = "Reset Counter to Zero";
            resetButton.Location = new Point(20, y);
            resetButton.Size = new Size(160, 30);
            resetButton.Click += (s, e) =>
            {
                if (MessageBox.Show("Reset today's usage counter to zero?", "Confirm Reset",
                                  MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    ResetCounter = true;
                    MessageBox.Show("Counter will be reset when you click Save.", "Reset Scheduled",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
            y += 45;

            // Change Password Section
            Label passwordLabel = new Label();
            passwordLabel.Text = "Change Password (leave blank to keep current):";
            passwordLabel.Location = new Point(20, y);
            passwordLabel.Size = new Size(350, 20);
            y += 25;

            Label newPasswordLabel = new Label();
            newPasswordLabel.Text = "New Password:";
            newPasswordLabel.Location = new Point(20, y);
            newPasswordLabel.Size = new Size(120, 20);
            
            newPasswordTextBox = new TextBox();
            newPasswordTextBox.Location = new Point(140, y);
            newPasswordTextBox.Size = new Size(220, 25);
            newPasswordTextBox.PasswordChar = '*';
            y += 30;

            Label confirmLabel = new Label();
            confirmLabel.Text = "Confirm:";
            confirmLabel.Location = new Point(20, y);
            confirmLabel.Size = new Size(120, 20);
            
            confirmPasswordTextBox = new TextBox();
            confirmPasswordTextBox.Location = new Point(140, y);
            confirmPasswordTextBox.Size = new Size(220, 25);
            confirmPasswordTextBox.PasswordChar = '*';
            y += 45;

            // Buttons
            saveButton = new Button();
            saveButton.Text = "Save";
            saveButton.Location = new Point(200, y);
            saveButton.Size = new Size(80, 30);
            saveButton.Click += SaveButton_Click;

            cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.Location = new Point(300, y);
            cancelButton.Size = new Size(80, 30);
            cancelButton.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.Add(usageLabel);
            this.Controls.Add(hoursLabel);
            this.Controls.Add(hoursNumeric);
            this.Controls.Add(enabledCheckBox);
            this.Controls.Add(resetButton);
            this.Controls.Add(passwordLabel);
            this.Controls.Add(newPasswordLabel);
            this.Controls.Add(newPasswordTextBox);
            this.Controls.Add(confirmLabel);
            this.Controls.Add(confirmPasswordTextBox);
            this.Controls.Add(saveButton);
            this.Controls.Add(cancelButton);
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            // Validate password if entered
            if (!string.IsNullOrWhiteSpace(newPasswordTextBox.Text))
            {
                if (newPasswordTextBox.Text != confirmPasswordTextBox.Text)
                {
                    MessageBox.Show("Passwords do not match!", "Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                if (newPasswordTextBox.Text.Length < 4)
                {
                    MessageBox.Show("Password must be at least 4 characters!", "Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                NewPassword = newPasswordTextBox.Text;
            }

            MaxHours = (int)hoursNumeric.Value;
            IsEnabled = enabledCheckBox.Checked;
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }

    // Countdown Display Form
    public class CountdownForm : Form
    {
        private Label statusLabel;
        private Label remainingLabel;
        private Label usedLabel;
        private Label maxLabel;
        private Label resetLabel;
        private System.Windows.Forms.Timer updateTimer;
        private TimerLockApp app;

        public CountdownForm(TimerLockApp timerApp)
        {
            app = timerApp;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Windows Timer Lock - Countdown";
            this.Size = new Size(450, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            int y = 20;

            // Title
            Label titleLabel = new Label();
            titleLabel.Text = "⏱️ Timer Status";
            titleLabel.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(0, 120, 215);
            titleLabel.Location = new Point(20, y);
            titleLabel.AutoSize = true;
            y += 50;

            // Status
            Label statusTitle = new Label();
            statusTitle.Text = "Status:";
            statusTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            statusTitle.Location = new Point(20, y);
            statusTitle.AutoSize = true;

            statusLabel = new Label();
            statusLabel.Font = new Font("Segoe UI", 11);
            statusLabel.Location = new Point(150, y);
            statusLabel.AutoSize = true;
            y += 40;

            // Time Remaining
            Label remainingTitle = new Label();
            remainingTitle.Text = "Time Remaining:";
            remainingTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            remainingTitle.Location = new Point(20, y);
            remainingTitle.AutoSize = true;

            remainingLabel = new Label();
            remainingLabel.Font = new Font("Consolas", 16, FontStyle.Bold);
            remainingLabel.ForeColor = Color.FromArgb(0, 150, 0);
            remainingLabel.Location = new Point(150, y - 5);
            remainingLabel.AutoSize = true;
            y += 45;

            // Used Today
            Label usedTitle = new Label();
            usedTitle.Text = "Used Today:";
            usedTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            usedTitle.Location = new Point(20, y);
            usedTitle.AutoSize = true;

            usedLabel = new Label();
            usedLabel.Font = new Font("Consolas", 16, FontStyle.Bold);
            usedLabel.ForeColor = Color.FromArgb(200, 50, 50);
            usedLabel.Location = new Point(150, y - 5);
            usedLabel.AutoSize = true;
            y += 45;

            // Max Allowed
            Label maxTitle = new Label();
            maxTitle.Text = "Max Allowed:";
            maxTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            maxTitle.Location = new Point(20, y);
            maxTitle.AutoSize = true;

            maxLabel = new Label();
            maxLabel.Font = new Font("Segoe UI", 11);
            maxLabel.Location = new Point(150, y);
            maxLabel.AutoSize = true;
            y += 35;

            // Resets at
            Label resetTitle = new Label();
            resetTitle.Text = "Resets at:";
            resetTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            resetTitle.Location = new Point(20, y);
            resetTitle.AutoSize = true;

            resetLabel = new Label();
            resetLabel.Text = "00:00 (Midnight)";
            resetLabel.Font = new Font("Segoe UI", 11);
            resetLabel.Location = new Point(150, y);
            resetLabel.AutoSize = true;
            y += 50;

            // Close button
            Button closeButton = new Button();
            closeButton.Text = "Close";
            closeButton.Font = new Font("Segoe UI", 10);
            closeButton.Size = new Size(100, 35);
            closeButton.Location = new Point((this.ClientSize.Width - closeButton.Width) / 2, y);
            closeButton.Click += (s, e) => this.Close();

            // Add controls
            this.Controls.Add(titleLabel);
            this.Controls.Add(statusTitle);
            this.Controls.Add(statusLabel);
            this.Controls.Add(remainingTitle);
            this.Controls.Add(remainingLabel);
            this.Controls.Add(usedTitle);
            this.Controls.Add(usedLabel);
            this.Controls.Add(maxTitle);
            this.Controls.Add(maxLabel);
            this.Controls.Add(resetTitle);
            this.Controls.Add(resetLabel);
            this.Controls.Add(closeButton);

            // Update timer (every second)
            updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Interval = 1000;
            updateTimer.Tick += UpdateDisplay;
            updateTimer.Start();

            // Initial update
            UpdateDisplay(null, null);
        }

        private void UpdateDisplay(object? sender, EventArgs? e)
        {
            // Get current values from app
            TimeSpan used = app.TotalUsageToday;
            int maxHours = app.MaxHours;
            bool enabled = app.IsEnabled;
            bool paused = app.IsPaused;

            TimeSpan remaining = TimeSpan.FromHours(maxHours) - used;
            
            // Status
            string status = enabled ? (paused ? "⏸️ PAUSED" : "▶️ RUNNING") : "⏹️ DISABLED";
            statusLabel.Text = status;
            statusLabel.ForeColor = enabled ? (paused ? Color.Orange : Color.Green) : Color.Gray;

            // Time Remaining
            if (remaining.TotalSeconds <= 0)
            {
                remainingLabel.Text = "00:00:00";
                remainingLabel.ForeColor = Color.Red;
            }
            else
            {
                int hours = (int)remaining.TotalHours;
                int minutes = remaining.Minutes;
                int seconds = remaining.Seconds;
                remainingLabel.Text = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
                
                // Color based on remaining time
                if (remaining.TotalMinutes <= 5)
                    remainingLabel.ForeColor = Color.Red;
                else if (remaining.TotalMinutes <= 15)
                    remainingLabel.ForeColor = Color.Orange;
                else
                    remainingLabel.ForeColor = Color.FromArgb(0, 150, 0);
            }

            // Used Today
            usedLabel.Text = $"{used.Hours:D2}:{used.Minutes:D2}:{used.Seconds:D2}";

            // Max Allowed
            maxLabel.Text = $"{maxHours} {(maxHours == 1 ? "hour" : "hours")}";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                updateTimer?.Stop();
                updateTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // Lock Screen Form
    public class LockScreenForm : Form
    {
        private Label messageLabel;
        private Label instructionLabel;
        private TextBox passwordTextBox;
        private Button unlockButton;
        private Button resetButton;
        private Label timeLabel;
        private Label usageLabel;
        private System.Windows.Forms.Timer clockTimer;
        private string adminPasswordHash;
        private TimeSpan currentUsage;
        private int maxHoursLimit;

        public LockScreenForm(string passwordHash, TimeSpan usage, int maxHours)
        {
            adminPasswordHash = passwordHash;
            currentUsage = usage;
            maxHoursLimit = maxHours;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Full screen lock
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.BackColor = Color.FromArgb(20, 20, 20);
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Prevent closing
            this.ControlBox = false;

            // Main message
            messageLabel = new Label();
            messageLabel.Text = "⏱️ TIME LIMIT REACHED";
            messageLabel.Font = new Font("Segoe UI", 32, FontStyle.Bold);
            messageLabel.ForeColor = Color.FromArgb(220, 50, 50);
            messageLabel.AutoSize = false;
            messageLabel.TextAlign = ContentAlignment.MiddleCenter;
            messageLabel.Dock = DockStyle.Top;
            messageLabel.Height = 100;
            messageLabel.Padding = new Padding(0, 50, 0, 0);

            // Time display
            timeLabel = new Label();
            timeLabel.Font = new Font("Segoe UI", 24, FontStyle.Regular);
            timeLabel.ForeColor = Color.White;
            timeLabel.AutoSize = false;
            timeLabel.TextAlign = ContentAlignment.MiddleCenter;
            timeLabel.Dock = DockStyle.Top;
            timeLabel.Height = 60;
            UpdateTime();

            // Clock timer
            clockTimer = new System.Windows.Forms.Timer();
            clockTimer.Interval = 1000;
            clockTimer.Tick += (s, e) => UpdateTime();
            clockTimer.Start();

            // Usage info
            usageLabel = new Label();
            usageLabel.Text = $"Used: {currentUsage.Hours:D2}:{currentUsage.Minutes:D2}:{currentUsage.Seconds:D2} / {maxHoursLimit:D2}:00:00";
            usageLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            usageLabel.ForeColor = Color.FromArgb(255, 180, 50);
            usageLabel.AutoSize = false;
            usageLabel.TextAlign = ContentAlignment.MiddleCenter;
            usageLabel.Dock = DockStyle.Top;
            usageLabel.Height = 50;

            // Instruction
            instructionLabel = new Label();
            instructionLabel.Text = "Your daily computer usage limit has been reached.\n\nEnter admin password to unlock:";
            instructionLabel.Font = new Font("Segoe UI", 14);
            instructionLabel.ForeColor = Color.White;
            instructionLabel.AutoSize = false;
            instructionLabel.TextAlign = ContentAlignment.MiddleCenter;
            instructionLabel.Height = 100;

            // Center panel
            Panel centerPanel = new Panel();
            centerPanel.Width = 500;
            centerPanel.Height = 260;
            centerPanel.BackColor = Color.FromArgb(40, 40, 40);

            // Password textbox
            passwordTextBox = new TextBox();
            passwordTextBox.Font = new Font("Segoe UI", 16);
            passwordTextBox.PasswordChar = '●';
            passwordTextBox.Width = 400;
            passwordTextBox.Height = 40;
            passwordTextBox.BackColor = Color.FromArgb(60, 60, 60);
            passwordTextBox.ForeColor = Color.White;
            passwordTextBox.BorderStyle = BorderStyle.FixedSingle;
            passwordTextBox.Location = new Point(50, 120);
            passwordTextBox.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    e.Handled = true;
                    UnlockButton_Click(s, e);
                }
            };

            // Unlock button
            unlockButton = new Button();
            unlockButton.Text = "Unlock";
            unlockButton.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            unlockButton.Width = 140;
            unlockButton.Height = 45;
            unlockButton.BackColor = Color.FromArgb(0, 120, 215);
            unlockButton.ForeColor = Color.White;
            unlockButton.FlatStyle = FlatStyle.Flat;
            unlockButton.FlatAppearance.BorderSize = 0;
            unlockButton.Location = new Point(50, 180);
            unlockButton.Click += UnlockButton_Click;

            // Reset button
            resetButton = new Button();
            resetButton.Text = "Reset Timer";
            resetButton.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            resetButton.Width = 140;
            resetButton.Height = 45;
            resetButton.BackColor = Color.FromArgb(200, 100, 0);
            resetButton.ForeColor = Color.White;
            resetButton.FlatStyle = FlatStyle.Flat;
            resetButton.FlatAppearance.BorderSize = 0;
            resetButton.Location = new Point(310, 180);
            resetButton.Click += ResetButton_Click;

            // Add to center panel
            centerPanel.Controls.Add(instructionLabel);
            instructionLabel.Location = new Point(0, 10);
            instructionLabel.Width = 500;
            centerPanel.Controls.Add(passwordTextBox);
            centerPanel.Controls.Add(unlockButton);
            centerPanel.Controls.Add(resetButton);

            // Position center panel
            this.Controls.Add(centerPanel);
            this.Resize += (s, e) =>
            {
                centerPanel.Location = new Point(
                    (this.ClientSize.Width - centerPanel.Width) / 2,
                    (this.ClientSize.Height - centerPanel.Height) / 2
                );
            };

            this.Controls.Add(messageLabel);
            this.Controls.Add(timeLabel);
            this.Controls.Add(usageLabel);

            // Initial position
            centerPanel.Location = new Point(
                (this.ClientSize.Width - centerPanel.Width) / 2,
                (this.ClientSize.Height - centerPanel.Height) / 2
            );

            // Prevent Alt+F4
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.Alt && e.KeyCode == Keys.F4)
                {
                    e.Handled = true;
                }
            };

            // Focus on password field
            this.Shown += (s, e) => passwordTextBox.Focus();
        }

        private void UpdateTime()
        {
            timeLabel.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy  HH:mm:ss");
        }

        private void UnlockButton_Click(object? sender, EventArgs e)
        {
            string enteredHash = HashPassword(passwordTextBox.Text);
            
            if (enteredHash == adminPasswordHash)
            {
                clockTimer.Stop();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                ShowWrongPasswordFeedback();
            }
        }

        private void ResetButton_Click(object? sender, EventArgs e)
        {
            string enteredHash = HashPassword(passwordTextBox.Text);
            
            if (enteredHash == adminPasswordHash)
            {
                if (MessageBox.Show("Reset the timer to zero and unlock?", "Confirm Reset",
                                  MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    clockTimer.Stop();
                    this.DialogResult = DialogResult.Retry; // Using Retry to signal reset
                    this.Close();
                }
            }
            else
            {
                ShowWrongPasswordFeedback();
            }
        }

        private void ShowWrongPasswordFeedback()
        {
            // Wrong password - shake effect
            Point original = this.Location;
            for (int i = 0; i < 3; i++)
            {
                this.Location = new Point(original.X + 10, original.Y);
                System.Threading.Thread.Sleep(50);
                this.Location = new Point(original.X - 10, original.Y);
                System.Threading.Thread.Sleep(50);
            }
            this.Location = original;

            passwordTextBox.Clear();
            passwordTextBox.BackColor = Color.FromArgb(150, 40, 40);
            Task.Delay(500).ContinueWith(t =>
            {
                if (!passwordTextBox.IsDisposed)
                {
                    passwordTextBox.Invoke((Action)(() =>
                    {
                        passwordTextBox.BackColor = Color.FromArgb(60, 60, 60);
                    }));
                }
            });
            passwordTextBox.Focus();
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                clockTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

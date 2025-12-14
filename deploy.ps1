# WindowsTimerLock Deployment Script
# Run this script as Administrator on the target Windows machine

# Configuration
$sourceExe = "C:\temp\WindowsTimerLock.exe"
$installDir = "C:\Program Files\WindowsTimerLock"
$exePath = "$installDir\WindowsTimerLock.exe"
$taskName = "WindowsTimerLock"

Write-Host "=== WindowsTimerLock Deployment Script ===" -ForegroundColor Cyan
Write-Host ""

# Check if running as Administrator
if (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    exit 1
}

# Step 1: Check if source file exists
Write-Host "[1/5] Checking source file..." -ForegroundColor Yellow
if (-not (Test-Path $sourceExe)) {
    Write-Host "ERROR: Source file not found: $sourceExe" -ForegroundColor Red
    exit 1
}
Write-Host "      Source file found: $sourceExe" -ForegroundColor Green

# Step 2: Create installation directory
Write-Host "[2/5] Creating installation directory..." -ForegroundColor Yellow
if (-not (Test-Path $installDir)) {
    New-Item -ItemType Directory -Path $installDir -Force | Out-Null
    Write-Host "      Created: $installDir" -ForegroundColor Green
} else {
    Write-Host "      Directory already exists: $installDir" -ForegroundColor Green
}

# Step 3: Copy executable
Write-Host "[3/5] Copying executable..." -ForegroundColor Yellow
Copy-Item -Path $sourceExe -Destination $exePath -Force
Write-Host "      Copied to: $exePath" -ForegroundColor Green

# Step 4: Set NTFS permissions to prevent deletion
Write-Host "[4/5] Setting NTFS permissions..." -ForegroundColor Yellow
try {
    # Get the ACL for the directory
    $acl = Get-Acl $installDir
    
    # Disable inheritance and copy existing permissions
    $acl.SetAccessRuleProtection($true, $true)
    
    # Remove all Users write/delete permissions
    $userRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
        "BUILTIN\Users",
        "Delete, DeleteSubdirectoriesAndFiles, Write, WriteData, AppendData, WriteExtendedAttributes, WriteAttributes",
        "ContainerInherit, ObjectInherit",
        "None",
        "Deny"
    )
    $acl.AddAccessRule($userRule)
    
    # Apply the ACL
    Set-Acl -Path $installDir -AclObject $acl
    
    # Also apply to the exe file
    $exeAcl = Get-Acl $exePath
    $exeAcl.SetAccessRuleProtection($true, $true)
    $exeAcl.AddAccessRule($userRule)
    Set-Acl -Path $exePath -AclObject $exeAcl
    
    Write-Host "      Permissions set - Users can only Read & Execute" -ForegroundColor Green
} catch {
    Write-Host "      WARNING: Could not set permissions: $_" -ForegroundColor Yellow
}

# Step 5: Create scheduled task to run at startup
Write-Host "[5/5] Creating startup scheduled task..." -ForegroundColor Yellow

# Remove existing task if it exists
$existingTask = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue
if ($existingTask) {
    Unregister-ScheduledTask -TaskName $taskName -Confirm:$false
    Write-Host "      Removed existing task" -ForegroundColor Yellow
}

# Create the scheduled task
$action = New-ScheduledTaskAction -Execute $exePath
$trigger = New-ScheduledTaskTrigger -AtStartup
$principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -LogonType ServiceAccount -RunLevel Highest
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable

Register-ScheduledTask -TaskName $taskName `
    -Action $action `
    -Trigger $trigger `
    -Principal $principal `
    -Settings $settings `
    -Description "Runs WindowsTimerLock at system startup" | Out-Null

Write-Host "      Scheduled task created: $taskName" -ForegroundColor Green

# Summary
Write-Host ""
Write-Host "=== Deployment Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Installation Path: $exePath" -ForegroundColor Cyan
Write-Host "Startup Task: $taskName" -ForegroundColor Cyan
Write-Host "Protection: Users cannot delete or modify the exe" -ForegroundColor Cyan
Write-Host ""
Write-Host "The application will run automatically on next system startup." -ForegroundColor Yellow
Write-Host "To test now, run: Start-ScheduledTask -TaskName '$taskName'" -ForegroundColor Yellow
Write-Host ""

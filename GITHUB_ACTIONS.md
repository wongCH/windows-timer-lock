# GitHub Actions CI/CD

This project uses GitHub Actions to automatically build the MSI installer on every push.

## Workflow: Build MSI Installer

**File:** `.github/workflows/build-msi.yml`

### Triggers

The workflow runs automatically when:
- Code is pushed to `main` or `master` branch
- Changes affect: `Program.cs`, `WindowsTimerLock.csproj`, `Installer.wxs`, or the workflow file
- Manually triggered via "Run workflow" button
- A new release is created

### What It Does

1. ✅ Checks out your code
2. ✅ Sets up .NET 10.0
3. ✅ Builds the Windows executable (`WindowsTimerLock.exe`)
4. ✅ Installs WiX Toolset v3.11
5. ✅ Compiles the MSI installer
6. ✅ Uploads artifacts (MSI + standalone EXE)
7. ✅ Attaches to releases (if creating a release)

### Downloading Build Artifacts

#### From GitHub Actions:

1. Go to your repository on GitHub
2. Click **Actions** tab
3. Click on the latest workflow run
4. Scroll to **Artifacts** section
5. Download:
   - `WindowsTimerLock-MSI-Installer` - MSI installer package
   - `WindowsTimerLock-EXE` - Standalone executable

#### From Releases:

When you create a release, the MSI and EXE are automatically attached.

1. Go to **Releases** page
2. Click on the release
3. Download from **Assets** section

## Manual Trigger

You can manually trigger the workflow without pushing code:

1. Go to **Actions** tab
2. Select **Build MSI Installer** workflow
3. Click **Run workflow** button
4. Select branch and click **Run workflow**

## Setup Instructions

### First Time Setup

1. **Push to GitHub:**
   ```bash
   git add .
   git commit -m "Add GitHub Actions workflow for MSI building"
   git push origin main
   ```

2. **Check Actions Tab:**
   - Go to your repo on GitHub
   - Click **Actions** tab
   - You should see the workflow running

3. **Download Artifacts:**
   - Wait for workflow to complete (~5-10 minutes)
   - Click on the workflow run
   - Download artifacts

### Creating a Release

To automatically attach MSI to a release:

```bash
# Create and push a tag
git tag v1.0.0
git push origin v1.0.0

# Then on GitHub:
# 1. Go to Releases
# 2. Click "Draft a new release"
# 3. Choose the tag (v1.0.0)
# 4. Fill in release notes
# 5. Click "Publish release"
```

The workflow will automatically build and attach the MSI and EXE to the release.

## Workflow Details

### Build Time
- Typical build time: 5-10 minutes
- .NET restore and build: ~2 minutes
- WiX install: ~1 minute
- MSI compilation: ~2 minutes

### Artifact Retention
- Artifacts are kept for **90 days**
- Release assets are kept permanently

### Caching
The workflow caches:
- NuGet packages (speeds up .NET restore)
- WiX installation (reused across builds)

## Troubleshooting

### Workflow Fails: "dotnet not found"
- Check .NET version in workflow matches project requirements
- Current: `dotnet-version: '10.0.x'`

### Workflow Fails: "candle.exe not found"
- WiX installation failed
- Check chocolatey logs in workflow output
- May need to update WiX version

### Workflow Fails: "EXE not found"
- Check `dotnet publish` output in workflow logs
- Verify output path matches: `output/win-x64/WindowsTimerLock.exe`

### MSI Size Too Large
- Current size: ~49 MB (includes .NET runtime)
- This is normal for self-contained applications
- Consider framework-dependent build if .NET is pre-installed on targets

### Build Succeeds but Can't Find Artifacts
- Check workflow completed successfully (green checkmark)
- Look for "Artifacts" section at bottom of workflow run page
- Artifacts expire after 90 days

## Local Testing

To test the workflow locally before pushing:

### Option 1: Act (GitHub Actions Local Runner)

```bash
# Install act
brew install act

# Run workflow locally
act push

# Run specific job
act -j build-msi
```

### Option 2: Manual Build on Windows

Transfer files to Windows and run:
```powershell
.\build-msi.ps1
```

## Customization

### Change Trigger Branches

Edit `.github/workflows/build-msi.yml`:
```yaml
on:
  push:
    branches: [ main, master, develop ]  # Add more branches
```

### Change Artifact Retention

Edit `.github/workflows/build-msi.yml`:
```yaml
- name: Upload MSI Artifact
  uses: actions/upload-artifact@v4
  with:
    retention-days: 30  # Change from 90 to 30 days
```

### Add Signing (Code Signing Certificate)

Add to workflow after MSI creation:
```yaml
- name: Sign MSI
  run: |
    signtool sign /f certificate.pfx /p ${{ secrets.CERT_PASSWORD }} /tr http://timestamp.digicert.com installer\WindowsTimerLock.msi
```

Store certificate password in GitHub Secrets.

## Cost

GitHub Actions is **free** for public repositories with:
- 2,000 minutes/month for private repos
- Unlimited for public repos

This workflow uses ~10 minutes per run.

## Status Badge

Add to README.md to show build status:

```markdown
![Build MSI](https://github.com/yourusername/timer/actions/workflows/build-msi.yml/badge.svg)
```

Replace `yourusername` with your GitHub username.

# NengJi Farm API - Publish & Restart Script
# Usage: PowerShell -ExecutionPolicy Bypass -File ".\publish-and-restart.ps1"

$ErrorActionPreference = "Stop"

$ProjectDir = "F:\Kuade-NengJiFarm\NenJi-API\WebAPI"
$PublishDir = "F:\NenJi-API"
$SiteName = "nsdsb"
$PoolName = "NengJiFarm"

Write-Host "================================"
Write-Host "  NengJi Farm API Deploy"
Write-Host "================================"
Write-Host ""

# 1. Build
Write-Host "[1/4] Building project..." -ForegroundColor Yellow
Set-Location $ProjectDir
dotnet build --configuration Release --nologo
if ($LASTEXITCODE -ne 0) { throw "Build failed" }
Write-Host "  OK Build success" -ForegroundColor Green
Write-Host ""

# 2. Stop Site
Write-Host "[2/4] Stopping IIS site..." -ForegroundColor Yellow
& "$env:SystemRoot\system32\inetsrv\appcmd.exe" stop apppool "$PoolName"
& "$env:SystemRoot\system32\inetsrv\appcmd.exe" stop site "$SiteName"
Start-Sleep -Seconds 2
Write-Host "  OK Site stopped" -ForegroundColor Green
Write-Host ""

# 3. Publish
Write-Host "[3/4] Publishing to $PublishDir ..." -ForegroundColor Yellow
dotnet publish --configuration Release --output "$PublishDir" --nologo
if ($LASTEXITCODE -ne 0) { throw "Publish failed" }

# Fix web.config
$webConfig = "$PublishDir\web.config"
$content = [System.IO.File]::ReadAllText($webConfig)
$needFix = $false

if (-not $content.Contains("WebDAVModule")) { $needFix = $true }
if (-not $content.Contains("ASPNETCORE_ENVIRONMENT")) { $needFix = $true }

if ($needFix) {
    Write-Host "  Fixing web.config..." -ForegroundColor Yellow
    $lines = @(
        '<?xml version="1.0" encoding="utf-8"?>',
        '<configuration>',
        '  <location path="." inheritInChildApplications="false">',
        '    <system.webServer>',
        '      <modules>',
        '        <remove name="WebDAVModule" />',
        '      </modules>',
        '      <handlers>',
        '        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />',
        '        <remove name="WebDAV" />',
        '      </handlers>',
        '      <aspNetCore processPath="dotnet" arguments=".\WebAPI.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="inprocess">',
        '        <environmentVariables>',
        '          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />',
        '        </environmentVariables>',
        '      </aspNetCore>',
        '    </system.webServer>',
        '  </location>',
        '</configuration>'
    )
    $newContent = $lines -join "`r`n"
    [System.IO.File]::WriteAllText($webConfig, $newContent)
    Write-Host "  OK web.config fixed" -ForegroundColor Green
} else {
    Write-Host "  OK web.config no changes needed" -ForegroundColor Green
}
Write-Host ""

# 4. Start Site
Write-Host "[4/4] Starting IIS site..." -ForegroundColor Yellow
& "$env:SystemRoot\system32\inetsrv\appcmd.exe" start apppool "$PoolName"
& "$env:SystemRoot\system32\inetsrv\appcmd.exe" start site "$SiteName"
Start-Sleep -Seconds 3
Write-Host "  OK Site started" -ForegroundColor Green
Write-Host ""

# Verify
Write-Host "Verifying..." -ForegroundColor Yellow
try {
    $r = Invoke-WebRequest -Uri "http://localhost/swagger/index.html" -UseBasicParsing -TimeoutSec 10
    Write-Host "  OK Site response: HTTP $($r.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "  FAIL Site not responding: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "================================"
Write-Host "  Deploy Complete"
Write-Host "================================"

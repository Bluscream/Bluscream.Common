param(
    [Parameter(Mandatory = $false)]
    [string]$Configuration = "Default",
    
    [Parameter(Mandatory = $false)]
    [string]$OutputPath = "bin",
    
    [Parameter(Mandatory = $false)]
    [switch]$Clean
)

# Define build configurations
$Configurations = @{
    "Default"     = @{
        USE_UNITY              = $false
        USE_VRCHAT             = $false
        USE_CHILLOUTVR         = $false
        USE_WINDOWS            = $false
        USE_SYSTEMDRAWING      = $false
        USE_SYSTEMMANAGEMENT   = $false
        USE_SYSTEMWINDOWSFORMS = $false
        USE_WINDOWSAPICODEPACK = $false
        USE_FNV1A              = $false
        USE_VDF                = $false
        USE_NEWTONSOFTJSON     = $false
        USE_SYSTEMTEXTJSON     = $false
        USE_NLOG               = $false
    }
    "All"         = @{
        USE_UNITY              = $true
        USE_VRCHAT             = $true
        USE_CHILLOUTVR         = $true
        USE_WINDOWS            = $true
        USE_SYSTEMDRAWING      = $true
        USE_SYSTEMMANAGEMENT   = $true
        USE_SYSTEMWINDOWSFORMS = $true
        USE_WINDOWSAPICODEPACK = $true
        USE_FNV1A              = $true
        USE_VDF                = $true
        USE_NEWTONSOFTJSON     = $true
        USE_SYSTEMTEXTJSON     = $true
        USE_NLOG               = $true
    }
    "Minimal"     = @{
        USE_UNITY              = $false
        USE_VRCHAT             = $false
        USE_CHILLOUTVR         = $false
        USE_WINDOWS            = $false
        USE_SYSTEMDRAWING      = $false
        USE_SYSTEMMANAGEMENT   = $false
        USE_SYSTEMWINDOWSFORMS = $false
        USE_WINDOWSAPICODEPACK = $false
        USE_FNV1A              = $false
        USE_VDF                = $false
        USE_NEWTONSOFTJSON     = $false
        USE_SYSTEMTEXTJSON     = $false
        USE_NLOG               = $false
    }
    "Core"        = @{
        USE_UNITY              = $false
        USE_VRCHAT             = $false
        USE_CHILLOUTVR         = $false
        USE_WINDOWS            = $false
        USE_SYSTEMDRAWING      = $false
        USE_SYSTEMMANAGEMENT   = $false
        USE_SYSTEMWINDOWSFORMS = $false
        USE_WINDOWSAPICODEPACK = $false
        USE_FNV1A              = $true
        USE_VDF                = $true
        USE_NEWTONSOFTJSON     = $true
        USE_SYSTEMTEXTJSON     = $true
        USE_NLOG               = $true
    }
    "UnityOnly"   = @{
        USE_UNITY              = $true
        USE_VRCHAT             = $true
        USE_CHILLOUTVR         = $true
        USE_WINDOWS            = $false
        USE_SYSTEMDRAWING      = $false
        USE_SYSTEMMANAGEMENT   = $false
        USE_SYSTEMWINDOWSFORMS = $false
        USE_WINDOWSAPICODEPACK = $false
        USE_FNV1A              = $true
        USE_VDF                = $true
        USE_NEWTONSOFTJSON     = $true
        USE_SYSTEMTEXTJSON     = $true
        USE_NLOG               = $true
    }
    "WindowsOnly" = @{
        USE_UNITY              = $false
        USE_VRCHAT             = $false
        USE_CHILLOUTVR         = $false
        USE_WINDOWS            = $true
        USE_SYSTEMDRAWING      = $true
        USE_SYSTEMMANAGEMENT   = $true
        USE_SYSTEMWINDOWSFORMS = $true
        USE_WINDOWSAPICODEPACK = $true
        USE_FNV1A              = $true
        USE_VDF                = $true
        USE_NEWTONSOFTJSON     = $true
        USE_SYSTEMTEXTJSON     = $true
        USE_NLOG               = $true
    }
    "NoUnity"     = @{
        USE_UNITY              = $false
        USE_VRCHAT             = $false
        USE_CHILLOUTVR         = $false
        USE_WINDOWS            = $true
        USE_SYSTEMDRAWING      = $true
        USE_SYSTEMMANAGEMENT   = $true
        USE_SYSTEMWINDOWSFORMS = $true
        USE_WINDOWSAPICODEPACK = $true
        USE_FNV1A              = $true
        USE_VDF                = $true
        USE_NEWTONSOFTJSON     = $true
        USE_SYSTEMTEXTJSON     = $true
        USE_NLOG               = $true
    }
}

# Check if configuration exists
if (-not $Configurations.ContainsKey($Configuration)) {
    Write-Error "Configuration '$Configuration' not found. Available configurations:"
    $Configurations.Keys | ForEach-Object { Write-Host "  - $_" }
    exit 1
}

$config = $Configurations[$Configuration]
Write-Host "Building with configuration: $Configuration" -ForegroundColor Green

# Build MSBuild properties string
$properties = @()
foreach ($key in $config.Keys) {
    $properties += "-p:$key=$($config[$key])"
}

# Clean if requested
if ($Clean) {
    Write-Host "Cleaning previous build..." -ForegroundColor Yellow
    dotnet clean
}

# Build the project
Write-Host "Building project..." -ForegroundColor Yellow
$buildArgs = @("build", "-c", "Release", "-o", $OutputPath) + $properties
Write-Host "dotnet $($buildArgs -join ' ')"
dotnet $buildArgs

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build completed successfully!" -ForegroundColor Green
    Write-Host "Output: $OutputPath" -ForegroundColor Cyan
}
else {
    Write-Error "Build failed!"
    exit 1
} 
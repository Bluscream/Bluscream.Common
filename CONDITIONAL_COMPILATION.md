# Conditional Compilation Guide

This project supports conditional compilation to enable/disable specific features and dependencies. This allows you to build smaller, more focused assemblies that only include the functionality you need.

## Available Features

The following features can be conditionally enabled/disabled:

- **Unity**: Unity-specific extensions and utilities (`UNITY_ENABLED`)
- **VRChat**: VRChat-specific functionality (`VRCHAT_ENABLED`)
- **ChilloutVR**: ChilloutVR-specific functionality (`CHILLOUTVR_ENABLED`)
- **Windows**: Windows-specific functionality (`WINDOWS_ENABLED`)
- **System.Drawing**: System.Drawing.Common functionality (`SYSTEMDRAWING_ENABLED`)
- **System.Management**: System.Management functionality (`SYSTEMMANAGEMENT_ENABLED`)
- **System.Windows.Forms**: System.Windows.Forms functionality (`SYSTEMWINDOWSFORMS_ENABLED`)
- **WindowsAPICodePack**: Windows API Code Pack functionality (`WINDOWSAPICODEPACK_ENABLED`)
- **Fnv1a**: Fnv1a hashing functionality (`FNV1A_ENABLED`)
- **Vdf**: VDF file parsing functionality (`VDF_ENABLED`)
- **Newtonsoft.Json**: Newtonsoft.Json serialization (`NEWTONSOFTJSON_ENABLED`)
- **System.Text.Json**: System.Text.Json serialization (`SYSTEMTEXTJSON_ENABLED`)
- **NLog**: NLog logging functionality (`NLOG_ENABLED`)

## Usage Methods

### Method 1: Using the PowerShell Build Script (Recommended)

Use the provided PowerShell script to build with predefined configurations:

```powershell
# Build with minimal features (default - all disabled)
.\tools\Build-Configurations.ps1

# Build with core libraries only (JSON, logging, etc.)
.\tools\Build-Configurations.ps1 -Configuration Core

# Build with all features enabled
.\tools\Build-Configurations.ps1 -Configuration All

# Build Unity-only version
.\tools\Build-Configurations.ps1 -Configuration UnityOnly

# Build Windows-only version (no Unity)
.\tools\Build-Configurations.ps1 -Configuration WindowsOnly

# Build without Unity features
.\tools\Build-Configurations.ps1 -Configuration NoUnity

# Clean build
.\tools\Build-Configurations.ps1 -Configuration All -Clean
```

### Method 2: Using dotnet CLI with MSBuild Properties

Build directly using dotnet CLI with specific properties:

```bash
# Build with minimal features (default - all disabled)
dotnet build

# Build with core libraries only
dotnet build -p:USE_FNV1A=true -p:USE_VDF=true -p:USE_NEWTONSOFTJSON=true -p:USE_SYSTEMTEXTJSON=true -p:USE_NLOG=true

# Build with Unity enabled
dotnet build -p:USE_UNITY=true

# Build with only Unity and VRChat enabled
dotnet build -p:USE_UNITY=true -p:USE_VRCHAT=true

# Build with all features enabled
dotnet build -p:USE_UNITY=true -p:USE_VRCHAT=true -p:USE_CHILLOUTVR=true -p:USE_WINDOWS=true -p:USE_SYSTEMDRAWING=true -p:USE_SYSTEMMANAGEMENT=true -p:USE_SYSTEMWINDOWSFORMS=true -p:USE_WINDOWSAPICODEPACK=true -p:USE_FNV1A=true -p:USE_VDF=true -p:USE_NEWTONSOFTJSON=true -p:USE_SYSTEMTEXTJSON=true -p:USE_NLOG=true
```

### Method 3: Using #if Preprocessor Directives in Code

You can also use preprocessor directives directly in your code:

```csharp
#if UNITY_ENABLED
using UnityEngine;

public static class UnityExtensions
{
    public static void DoSomething(this GameObject obj)
    {
        // Unity-specific code
    }
}
#endif

#if WINDOWS_ENABLED
using System.Windows.Forms;

public static class WindowsExtensions
{
    public static void ShowMessage(string message)
    {
        MessageBox.Show(message);
    }
}
#endif
```

## Predefined Build Configurations

| Configuration | Unity | VRChat | ChilloutVR | Windows | System.Drawing | System.Management | System.Windows.Forms | WindowsAPICodePack | Fnv1a | Vdf | Newtonsoft.Json | System.Text.Json | NLog |
|---------------|-------|--------|------------|---------|----------------|-------------------|---------------------|-------------------|-------|-----|----------------|------------------|------|
| Default | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| All | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Minimal | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Core | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ |
| UnityOnly | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ |
| WindowsOnly | ❌ | ❌ | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| NoUnity | ❌ | ❌ | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |

## How It Works

1. **MSBuild Properties**: The `.csproj` file defines conditional compilation symbols based on MSBuild properties
2. **Default Values**: All features are disabled by default - you must explicitly enable what you need
3. **Conditional Package References**: NuGet packages are only included when their corresponding feature is enabled
4. **Preprocessor Directives**: Code is wrapped with `#if` directives to conditionally compile sections
5. **Build Script**: The PowerShell script provides easy-to-use predefined configurations

## Benefits

- **Opt-in Approach**: All features are disabled by default - you only get what you explicitly enable
- **Smaller Assembly Size**: Only include the dependencies you need
- **Reduced Dependencies**: Avoid unnecessary NuGet packages
- **Platform-Specific Builds**: Create builds optimized for specific platforms
- **Faster Compilation**: Less code to compile when features are disabled
- **Cleaner Dependencies**: Avoid conflicts with platform-specific libraries
- **Security**: No unexpected dependencies or functionality included by default

## Adding New Conditional Features

To add a new conditional feature:

1. Add the property to the `.csproj` file:
   ```xml
   <DefineConstants Condition="'$(UseNewFeature)' == 'true'">$(DefineConstants);NEWFEATURE_ENABLED</DefineConstants>
   ```

2. Add conditional package references:
   ```xml
   <ItemGroup Condition="'$(UseNewFeature)' == 'true'">
     <PackageReference Include="NewFeature.Package" Version="1.0.0" />
   </ItemGroup>
   ```

3. Wrap your code with preprocessor directives:
   ```csharp
   #if NEWFEATURE_ENABLED
   // Your feature code here
   #endif
   ```

4. Add the configuration to the PowerShell script if needed.

## Notes

- When a feature is disabled, its code is completely excluded from the compiled assembly
- Dependencies are also excluded when their corresponding feature is disabled
- The build script provides a convenient way to manage multiple configurations
- You can create custom configurations by modifying the PowerShell script 
# Npgquery - Native Library Setup

## What this means

Npgquery is a .NET library, but the PostgreSQL parser it calls is native code from `libpg_query`. That means your app needs two pieces at runtime:

1. `Npgquery.dll`, the managed .NET assembly.
2. The platform-specific native parser library.

The native library file name depends on the operating system:

| Platform | Native file |
| --- | --- |
| Windows | `pg_query.dll` |
| Linux | `libpg_query.so` |
| macOS | `libpg_query.dylib` |

The .NET code imports the library by the logical name `pg_query`. At runtime, .NET and the operating system map that name to the correct file for the current platform.

## Should users need to install or build this separately?

No, not for the normal NuGet package experience.

The recommended model is that the Npgquery NuGet package includes the native `libpg_query` builds it supports under `runtimes/<rid>/native/`. That is standard practice for .NET packages that wrap native libraries. A first-time Npgquery user should be able to add the package, build their app, and have .NET copy the correct native file automatically.

If those runtime assets are missing for a user's platform, they will need to build or provide the native library themselves. That should be treated as a fallback or contributor workflow, not the default user experience.

The upstream `pganalyze/libpg_query` GitHub releases are source releases. They are not a general download location for ready-to-use `pg_query.dll`, `libpg_query.so`, or `libpg_query.dylib` files.

Npgquery's release workflow is responsible for producing those native assets. It builds Linux assets in containers, then uses hosted macOS and Windows runners for the platform-specific toolchains that cannot be containerized in the same way.

Use this rule of thumb:

| Scenario | What you need to do |
| --- | --- |
| You are installing Npgquery from NuGet | The package should include the matching native asset. No manual native setup should be needed. |
| You are maintaining or releasing Npgquery | Run the release workflow, which builds the supported native libraries and stages them under `Npgquery\runtimes\<rid>\native\` before packing. |
| You are running from this source repository | Use the checked-in Windows `pg_query.dll` for the examples/tests, or provide your own native library for other platforms. |
| You are targeting a platform not included in the package | Build `libpg_query` for that platform and copy it into your app output or package it under the correct RID. |

The checked-in `pg_query.dll` files in `Examples` and `Npgquery.Tests` are for local Windows example and test runs. They are not a complete cross-platform native distribution.

## Where the native library should go

The simplest app-local setup is to put the native library next to the executable or the app's main `.dll`:

```text
YourApp\
  YourApp.exe
  YourApp.dll
  Npgquery.dll
  pg_query.dll
```

On Linux, the same idea uses `libpg_query.so`:

```text
YourApp/
  YourApp
  YourApp.dll
  Npgquery.dll
  libpg_query.so
```

On macOS, use `libpg_query.dylib`:

```text
YourApp/
  YourApp
  YourApp.dll
  Npgquery.dll
  libpg_query.dylib
```

For NuGet packaging, use .NET runtime identifiers and place the native files under `runtimes/<rid>/native/`:

```text
Npgquery\
  runtimes\
    win-x64\
      native\
        pg_query.dll
    win-arm64\
      native\
        pg_query.dll
    linux-x64\
      native\
        libpg_query.so
    linux-arm64\
      native\
        libpg_query.so
    osx-x64\
      native\
        libpg_query.dylib
    osx-arm64\
      native\
        libpg_query.dylib
```

`Npgquery.csproj` is already configured to include files from `Npgquery\runtimes\**\*` when packing:

```xml
<None Include="runtimes\**\*" Pack="true" PackagePath="runtimes\" />
```

That project setting only packs files that exist. If the `runtimes` folder is empty or missing, the NuGet package will not include native binaries.

## Getting the native library

There are three practical paths.

### Option 1: Use the native assets included in Npgquery

This is the expected path for consumers. If the NuGet package includes the matching native asset for your platform, install the package and run the app normally.

For example, a Windows x64 app expects a package asset like this:

```text
runtimes\win-x64\native\pg_query.dll
```

A Linux x64 app expects:

```text
runtimes\linux-x64\native\libpg_query.so
```

### Option 2: Copy a native library into your app output

This is useful for local development, source builds, or platforms that are not covered by the NuGet package.

Some package managers may provide `libpg_query` on Linux or macOS. That can be useful for development, but application deployment is simpler and more predictable when the native library is copied with the app or included as a NuGet runtime asset.

Copy the native library into the same directory as your built app. For example, for a Windows debug build:

```powershell
Copy-Item .\pg_query.dll .\bin\Debug\net10.0\
```

If you publish your app, copy it into the publish folder:

```powershell
dotnet publish .\YourApp.csproj -c Release -r win-x64
Copy-Item .\pg_query.dll .\bin\Release\net10.0\win-x64\publish\
```

Use the matching file for the target platform. A Windows `.dll` will not work on Linux or macOS.

### Option 3: Build from source

Use this path when you are maintaining Npgquery, producing a package, or targeting a platform that is not included in the package.

1. Clone the upstream source:

   ```powershell
   git clone https://github.com/pganalyze/libpg_query.git
   ```

2. Check out the version you want to use.
3. Follow the build instructions in the upstream `libpg_query` repository for your platform.
4. Copy the resulting native library into either:
   - your app output folder, or
   - `Npgquery\runtimes\<rid>\native\` before creating a NuGet package.

## Release workflow

The GitHub Actions release workflow accepts a Npgquery package version and a pinned `libpg_query` tag or branch. The default native source ref is `18.0.0`.

The workflow does the release work in this order:

1. Builds Linux native libraries in Debian containers for `linux-x64` and `linux-arm64`.
2. Builds macOS native libraries on hosted macOS runners for `osx-x64` and `osx-arm64`, using explicit Intel and Apple Silicon runner labels.
3. Builds Windows native libraries with MSVC for `win-x64` and `win-arm64`.
4. Downloads all native artifacts into `Npgquery\runtimes\<rid>\native\`.
5. Builds and packs Npgquery on Windows so the `net472` target is supported.
6. Verifies that the `.nupkg` contains every expected native asset.
7. Installs the local package into a fresh console app and runs a parser smoke test on Windows, Linux, and macOS.
8. Publishes to NuGet only if the `publish` workflow input is enabled.

## Troubleshooting

### `DllNotFoundException`

.NET could not find the native library.

Check that:

- the native file is in the app output folder, or included as a NuGet runtime asset;
- the file name matches the platform: `pg_query.dll`, `libpg_query.so`, or `libpg_query.dylib`;
- the process architecture matches the native library architecture, such as x64 vs ARM64.

### `BadImageFormatException`

.NET found the file, but it is not loadable by the current process.

Common causes:

- using an x64 native library from an ARM64 process, or the reverse;
- using a Windows `.dll` on Linux or macOS;
- using a 32-bit binary in a 64-bit process.

### `EntryPointNotFoundException`

.NET loaded a native library, but it does not export the function Npgquery tried to call.

This usually means the native library version does not match the interop methods expected by Npgquery. Use the `libpg_query` version that Npgquery was built and tested against, or rebuild Npgquery and the native library together.

### Checking native dependencies

If the file exists but still will not load, inspect its native dependencies:

| Platform | Command |
| --- | --- |
| Windows | `dumpbin /dependents pg_query.dll` |
| Linux | `ldd libpg_query.so` |
| macOS | `otool -L libpg_query.dylib` |

On Windows, make sure the Microsoft Visual C++ Redistributable required by your native build is installed.

On Linux, make sure the target machine has a compatible C runtime, such as the expected `glibc` version for the binary you built.

On macOS, if the file was downloaded from the internet, remove quarantine if needed:

```bash
xattr -d com.apple.quarantine libpg_query.dylib
```

## PostgreSQL parser version

The PostgreSQL syntax supported by Npgquery is controlled by the `libpg_query` native library version you build or bundle. If you update the native library, verify the managed protobuf files and interop methods are still compatible.

## License and attribution

- Npgquery: MIT License
- libpg_query: 3-Clause BSD License
- PostgreSQL Parser: PostgreSQL License

When distributing applications using Npgquery, include the required license attributions for all bundled components.

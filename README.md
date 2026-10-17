# msdnus

A small Windows desktop app for keeping MSDN / Visual Studio subscription product keys in one
searchable, encrypted place.

> **Personal project, no warranty.** I wrote this for my own use. It is provided "as is",
> without warranty of any kind. I take no responsibility for lost keys, lost data or any
> other damage. Keep your own backup of your key exports.

## What it does

- Imports the `KeysExport.xml` file you download from the Visual Studio subscriptions portal.
- Search by product name, copy a key with one click.
- Several profiles (one per MSDN account), all unlocked by one PIN.
- Each profile is a LiteDB database encrypted at rest. The key is derived from the PIN with
  PBKDF2-SHA256 and a random per-vault salt, so the vault is not tied to one machine.
- Portable: `settings.json` and the `profile_*.litedb` files are stored next to `msdnus.exe`,
  so the folder can be synced (e.g. via Dropbox) and opened on another PC with the same PIN.
- Changing the PIN re-encrypts every profile into new files; `settings.json` is the single
  commit point, and the old files are deleted only after it is written.

## Stack

.NET 10, WinUI 3 (Windows App SDK), CommunityToolkit.Mvvm, LiteDB. Tests: xUnit v3,
FluentAssertions, NSubstitute.

## Build

Requires Windows and the .NET 10 SDK.

```powershell
dotnet build msdnus.sln -c Release -p:Platform=x64
dotnet test msdnus.Tests/msdnus.Tests.csproj -c Release
```

Portable single-file exe (self-contained, no runtime install needed):

```powershell
dotnet publish msdnus\msdnus.csproj -c Release -r win-x64 -p:Platform=x64 -p:UnpackagedRun=true -o publish\win-x64
```

More detail: [docs/PROJECT_OVERVIEW.md](docs/PROJECT_OVERVIEW.md).

# msdnus — MSDN Subscriptions Key Manager

> WinUI 3 desktop app that imports, stores, and searches Microsoft product keys exported from MSDN/Visual Studio Subscriptions.

## What It Does

msdnus lets holders of Visual Studio Subscriptions (MSDN) manage their product activation keys offline. The user imports a keys XML file (exported from `msdn.microsoft.com/subscriptions`) — either from the local filesystem or directly from OneDrive — and the app stores all keys in an encrypted local database. Keys can be searched by product name and copied to clipboard with a single tap. The entire database is locked behind a PIN/password that must be entered on each launch.

## How It Works

```
XML export (file / OneDrive)
        │
        ▼
  FileDataSource (XML deserialization → root graph, Stream-based)
        │
        ▼
  MainViewModel.SaveData()  (maps rootYourKeyProduct_Key → YourKeyDTO)
        │
        ▼
  DataTarget (LiteDB embedded DB — file encrypted with machine-bound password)
        │
        ▼
  MainPage ListView  (filtered by search text, copy-to-clipboard)
```

**Lock screen**: on launch and on manual lock, `IsIejimoMode = true` overlays a password prompt. Password is stored AES-GCM-encrypted in settings.

**Encryption**: `AesGcmCryptoService` (msdnus.Core) derives a 32-byte key via PBKDF2 from the machine name + application salt. The LiteDB database file is encrypted with a password derived from that key (`DeriveSecret("msdnusdb")`); the login PIN is protected with AES-GCM (random nonce per encryption).

**OneDrive import**: MSAL (`PublicClientApplication`, client ID `000000004017CE14`, `/consumers` authority) acquires a token with `Files.Read` scope, then a plain `HttpClient` call downloads `KeysExport.xml` from the user's OneDrive root via the Microsoft Graph REST endpoint (no Graph SDK dependency).

## Tech Stack

| Category | Technology |
|---|---|
| Runtime | .NET 10 (app: `net10.0-windows10.0.19041.0`; Core/Tests: `net10.0`) |
| UI framework | WinUI 3 via Windows App SDK 2.3 (stable) |
| MVVM toolkit | CommunityToolkit.Mvvm 8.4 (partial-property `[ObservableProperty]`, `[RelayCommand]`) |
| Embedded database | LiteDB 5 (file-level password encryption) |
| Cryptography | System.Security.Cryptography (AES-GCM, PBKDF2) |
| DI / hosting | Microsoft.Extensions.DependencyInjection + Hosting 10.0 |
| Cloud auth | MSAL (Microsoft.Identity.Client 4.86) |
| Cloud storage | Microsoft Graph REST via HttpClient (no SDK) |
| Testing | xUnit v3 + AutoFixture + FluentAssertions 7 + NSubstitute |
| Platforms | x86 / x64 / ARM64 (packaged MSIX; unpackaged dev run via `-p:UnpackagedRun=true`) |

## Project Structure

```
msdnus.sln
├── msdnus/                        — WinUI 3 app host
│   ├── App.xaml.cs                — DI container setup, MSAL init, window bootstrap
│   ├── MainPage.xaml / .cs        — primary key list view
│   ├── Settings.xaml / .cs        — password change + dark/light theme toggle
│   ├── About / Help / CoffeeCup   — informational + donation pages
│   ├── ViewModel/                 — MVVM ViewModels (one per page)
│   │   ├── MainViewModel.cs       — data load, search, import, lock/unlock, OneDrive
│   │   └── SettingsViewModel.cs   — password management, theme
│   └── Shared/                    — UI helpers
│       ├── SettingsProvider.cs    — roaming + local ApplicationData wrapper
│       ├── TextToastManager.cs    — in-app toast notifications
│       ├── ThemeAwareFrame.cs     — dynamic theme switching
│       └── converters/            — BoolToVisible, InverseBool, StringFormat
│
├── msdnus.Core/                   — platform-agnostic domain + data layer
│   ├── Model/
│   │   ├── IDataSource / FileDataSource   — XML deserialization
│   │   ├── IDataTarget / DataTarget       — Lex.Db persistence
│   │   ├── ISettingsProvider              — settings contract
│   │   ├── YourKey.cs                     — auto-generated XSD classes (MSDN XML schema)
│   │   └── YourKeyDTO.cs                  — app DTO + Lex.Db custom serializer (encrypts fields)
│   └── Shared/
│       ├── Utils.cs               — AES encryption/decryption, XML serialization helpers
│       └── NotifyTaskCompletion   — async-friendly INotifyPropertyChanged wrapper
│
└── msdnus.Tests/                  — MSTest unit tests for Core models
```

## Key Architectural Decisions

- **LiteDB instead of SQLite**: LiteDB is a single-file, pure-managed embedded database with built-in file encryption — no native runtime bridge and no per-field crypto plumbing needed. It replaced the abandoned Lex.Db during the 2026 modernization; legacy Lex.Db data is not migrated (the app detects the old database folder and asks the user to re-import the keys XML).

- **Machine-bound encryption key**: `AesGcmCryptoService` derives its key from the machine name + application salt via PBKDF2. The database file and the stored PIN cannot be decrypted on a different machine, providing at-rest protection without a user-managed keystore.

- **Settings are machine-local**: the PIN and flags live in `ApplicationData.LocalSettings` when packaged, or `%LOCALAPPDATA%\msdnus\settings.json` when unpackaged. Roaming would defeat the machine-binding of the key.

- **MSAL `000000004017CE14` (Microsoft Live app ID)**: This is the legacy "Microsoft account" OAuth client ID used for consumer accounts (`/consumers` authority). It avoids the need to register a private Azure AD application for a simple personal-use tool. If it stops working with modern MSAL/WAM, register a free Azure app with `Files.Read` delegated permission and swap the client ID.

- **Core is platform-agnostic**: `msdnus.Core` targets plain `net10.0` with no WinRT dependencies, so the test project runs on any OS (including Linux CI).

## Running the Project

**Prerequisites**: Windows 10 1809+ (build 17763+), .NET 10 SDK. Visual Studio 2022+ with the "Windows App SDK" workload is recommended but not required.

```bash
# build + tests (tests run on any OS)
dotnet build msdnus.sln -p:Platform=x64
dotnet test msdnus.Tests/msdnus.Tests.csproj

# dev inner loop: self-contained unpackaged build, runs without MSIX deployment
dotnet build msdnus/msdnus.csproj -p:Platform=x64 -p:UnpackagedRun=true
./msdnus/bin/x64/Debug/net10.0-windows10.0.19041.0/msdnus.exe

# unsigned MSIX package
dotnet build msdnus/msdnus.csproj -c Release -p:Platform=x64 -p:GenerateAppxPackageOnBuild=true -p:AppxPackageSigningEnabled=false
```

Or open `msdnus.sln` in Visual Studio and press F5. In unpackaged mode the app stores data in `%LOCALAPPDATA%\msdnus`; unhandled exceptions are logged to `crash.log` in the same folder.

**First run**: default password is `123`. Change it in Settings before importing real keys.

**Importing keys**:
1. Export keys from [my.visualstudio.com/Downloads](https://my.visualstudio.com/Downloads) → "Product Keys" → download XML.
2. In the app: tap the folder icon → select the XML file, or tap the OneDrive icon to import directly.

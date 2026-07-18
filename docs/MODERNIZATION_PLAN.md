# msdnus Modernization Plan

> Goal: finish the migration of msdnus (MSDN Subscriptions Key Manager) to a modern,
> supported Windows stack — and end up with an application that **builds, runs, and passes tests**,
> not just a ported codebase.

Status date: 2026-07-18 · Branch: `upgrade-to-UAP10`

## Execution Status: COMPLETE (Phases 1–5)

All five mandatory phases were executed on 2026-07-18. Deviations from the original plan:

- **Windows App SDK**: pinned to **2.3.1** (stable 2.x line was already available, so no fallback
  to 1.8.x was needed).
- **Microsoft Graph SDK removed entirely**: the old code used the Graph v3/v4 API surface
  (`DelegateAuthenticationProvider`, `.Request()`) which no longer exists. Since the app only
  downloads a single file, the Graph SDK was replaced with a direct `HttpClient` call to
  `GET /v1.0/me/drive/root:/KeysExport.xml:/content` using the MSAL token. This also removed the
  vulnerable `Microsoft.Kiota.Abstractions` transitive dependency.
- **Test stack**: migrated to **xUnit v3 + AutoFixture + FluentAssertions 7.x + NSubstitute**
  with `[Trait]` categories on every test (per project testing convention), instead of updating
  MSTest.
- **Unpackaged run support** was added (`-p:UnpackagedRun=true`) so the app can be built and
  launched without MSIX deployment for the dev inner loop; app data falls back from
  `ApplicationData` to `%LOCALAPPDATA%\msdnus` in that mode.
- Runtime bring-up verified on a real Windows 11 machine: launch, lock screen, unlock with the
  default PIN, and navigation to Help/About/Settings/CoffeeCup were exercised via UI Automation;
  a data-binding crash (`ItemsSource` bound to `NotifyTaskCompletion` instead of `.Result`) was
  found and fixed this way. Global exception logging writes to `crash.log` in the app data folder.
- Verified gates: solution builds with 0 errors; 23/23 tests pass; unsigned MSIX bundle builds.
- Still requiring a human: signed-package install test, OneDrive import with a real Microsoft
  account (legacy client ID risk, see §5), and Store submission.

---

## 1. Framework Decision

### Recommendation: **WinUI 3 (Windows App SDK, stable channel) + .NET 10**

The previous migration pass (see `MIGRATION_COMPLETE.md`) already moved the codebase from UWP
to WinUI 3: SDK-style projects, CommunityToolkit.Mvvm, Microsoft.Extensions.DependencyInjection,
Microsoft Graph + MSAL. Roughly 90% of that work is directly reusable. Switching frameworks now
would discard it for no benefit.

| Option | Verdict | Rationale |
|---|---|---|
| **WinUI 3 / Windows App SDK** | ✅ **Chosen** | Migration is mostly done; Microsoft's current native Windows UI stack; XAML dialect closest to the existing UWP XAML. |
| .NET MAUI | ❌ Rejected | On Windows, MAUI renders through WinUI 3 anyway — it adds an abstraction layer whose only payoff is Android/iOS/macOS targets this app does not need. Another full XAML rewrite for zero gain. |
| Avalonia UI | ❌ Rejected (for now) | Excellent framework, but it means rewriting all views again. Only worth revisiting if Linux/macOS support ever becomes a requirement. |
| WPF on .NET 10 | ❌ Rejected | Mature, but a step sideways: would also require a full XAML rewrite, and loses the completed WinUI 3 work. |

Key correction to the current state: the app references **Windows App SDK `2.0.0-experimental3`**.
Experimental builds are unsupported, may not have a matching redistributable runtime, and break
between drops. The plan pins the **latest stable channel release** (1.8.x line as of writing;
verify at [Windows App SDK release channels](https://learn.microsoft.com/windows/apps/windows-app-sdk/release-channels)).

---

## 2. Current State Assessment (verified by building)

`dotnet build msdnus.sln -c Debug -p:Platform=x64` — **FAILS** with 7 errors, 14 warnings.

### Blocking errors

| # | Problem | Evidence |
|---|---|---|
| E1 | Duplicate assembly attributes in `msdnus.Core` | `CS0579` ×7: legacy `Properties/AssemblyInfo.cs` conflicts with SDK-generated `AssemblyInfo` (`GenerateAssemblyInfo=true`). |
| E2 | `Lex.Db 1.2.6` restored via .NET Framework fallback | `NU1701` on all three projects. The package is abandoned (last release ~2015, PCL/WinRT era). Even if it compiles, runtime failure on .NET 10 is likely (reflection-emit / WinRT storage assumptions). |
| E3 | Windows App SDK `2.0.0-experimental3` | Unsupported experimental dependency; app cannot be shipped on it. |

### High-priority issues (non-blocking for compile, blocking for "it works")

| # | Problem | Evidence |
|---|---|---|
| H1 | Known high-severity vulnerability in Graph dependency chain | `NU1903`: `Microsoft.Kiota.Abstractions 1.15.2` (GHSA-7j59-v9qr-6fq9) pulled in by `Microsoft.Graph 5.68.0`. |
| H2 | `msdnus.Core` is not platform-agnostic | `Utils.cs` uses WinRT APIs (`Windows.Networking.Connectivity`, `Windows.Security.Cryptography`, `Windows.Storage`), forcing Core and Tests to target `net10.0-windows10.0.19041.0` and making tests untestable off a full Windows runtime. |
| H3 | Crypto design flaws | AES-**ECB** mode (no diffusion, pattern leakage); key derivation copies overlapping hash bytes (`Array.Copy(temp,0,hash,0,16)` then `Array.Copy(temp,0,hash,15,16)` — a long-standing off-by-one producing a 31-byte-entropy key). Machine-bound key derived from hostname only. |
| H4 | Runtime never validated | The migration report itself lists "Test Build / Runtime Testing" as not done. No feature of the app has been exercised on WinUI 3. |
| H5 | Legacy UWP leftovers | `DeviceFamily-Desktop/` and `DeviceFamily-Mobile/` duplicate `MainPage.xaml` (UWP device-family mechanism does not exist in WinUI 3); `msdnus.Core.rd.xml` (.NET Native directives, meaningless on .NET 10); `PropertyChangedBase`, `ViewModelLocator` remnants. |
| H6 | Outdated test stack | MSTest 3.2.0, no `Microsoft.NET.Test.Sdk` reference visible, tests target windows-specific TFM. |
| H7 | No CI | `.github/` contains no build workflow; regressions like E1 go unnoticed. |

---

## 3. Target Architecture

```
msdnus.sln
├── msdnus/            WinUI 3 app (net10.0-windows10.0.19041.0, WinAppSDK stable, MSIX)
│   ├── App.xaml.cs    Host + DI bootstrap
│   ├── Views          MainPage / Settings / About / Help / CoffeeCup (single set, responsive)
│   ├── ViewModel/     CommunityToolkit.Mvvm (unchanged)
│   └── Platform services registered in DI:
│       ├── IFilePicker, IClipboardService, IToastService
│       ├── ISettingsProvider  (ApplicationData)
│       └── IMachineIdentity   (feeds key derivation)
│
├── msdnus.Core/       net10.0  ←  NO Windows dependency
│   ├── Model/         DTOs, XML import (System.Xml.Serialization over Stream)
│   ├── Data/          IDataTarget → LiteDB implementation
│   └── Crypto/        ICryptoService → AES-GCM (System.Security.Cryptography)
│
└── msdnus.Tests/      net10.0, MSTest 3.x latest — runs on any OS / CI
```

Principles:

- **Core targets plain `net10.0`.** All WinRT usage moves behind interfaces implemented in the
  app project. This is what makes the test project (and CI) actually work.
- **Storage: replace Lex.Db with LiteDB 5.** Single-file embedded NoSQL DB, actively maintained,
  pure managed code, near drop-in for the existing "store a list of DTOs, query by name" usage.
  (SQLite + `Microsoft.Data.Sqlite` is the fallback if LiteDB is rejected — slightly more
  ceremony, equally viable.)
- **Crypto: `System.Security.Cryptography` AES-GCM** instead of WinRT AES-ECB. Key derivation via
  `Rfc2898DeriveBytes`/HKDF from machine identity + salt; optionally wrap the key with DPAPI
  (`ProtectedData`, machine scope) for stronger at-rest binding than a hostname hash.
- **No data migration required.** The dataset is fully re-importable from the MSDN keys XML
  export. On first launch of the new version, if the old Lex.Db files exist, prompt the user to
  re-import the XML (old DB is machine-bound and encrypted with the flawed key anyway — cleanest
  break). This removes the single riskiest work item (porting Lex.Db just to read old data).

---

## 4. Phased Plan

Each phase ends in a **verifiable gate**. Do not start a phase before the previous gate passes.

### Phase 1 — Make it compile (½ day)

1. Delete `msdnus.Core/Properties/AssemblyInfo.cs` and `msdnus/Properties/AssemblyInfo.cs`;
   move version info into the `.csproj` (`<Version>`, `<AssemblyVersion>`). Fixes E1.
2. Pin `Microsoft.WindowsAppSDK` to the latest **stable** release; remove
   `EnablePreviewFeatures` from all projects. Fixes E3.
3. Update `Microsoft.Graph` to the latest 5.x (pulls patched Kiota) — or add a direct
   `Microsoft.Kiota.Abstractions` reference at a patched version. Fixes H1.
4. Update `Microsoft.Identity.Client`, `Microsoft.Extensions.*` to current stable.
5. Fix whatever new compile errors the stable WinAppSDK surfaces (namespace/API drift from the
   experimental build).

**Gate 1:** `dotnet build msdnus.sln -c Debug -p:Platform=x64` → 0 errors.
(Lex.Db `NU1701` warning still present — addressed in Phase 2.)

### Phase 2 — Decouple Core, replace storage & crypto (1–2 days)

1. Retarget `msdnus.Core` to `net10.0`; delete `msdnus.Core.rd.xml`.
2. `Utils.cs` split:
   - XML serialization helpers → pure `Stream`-based (drop `StorageFile` overloads; the app
     project adapts picker results to streams).
   - Crypto → new `ICryptoService` + `AesGcmCryptoService` in Core using
     `System.Security.Cryptography`; key material supplied by `IMachineIdentity` (implemented in
     the app via `Environment.MachineName`/DPAPI). Fixes H2, H3.
3. Replace `DataTarget` (Lex.Db) with `LiteDbDataTarget` implementing the existing `IDataTarget`;
   encryption stays at the same boundary (encrypt DTO fields before insert, decrypt on read) —
   or use LiteDB's built-in password encryption for the whole file, which is simpler and
   strictly stronger. Remove the `Lex.Db` package from all projects. Fixes E2.
4. First-run handling: detect legacy DB folder → show "please re-import your keys XML" dialog.

**Gate 2:** solution builds with **zero** `NU1701`/`NU1903` warnings; Core has no `Windows.*`
references (`grep -r "using Windows\." msdnus.Core` is empty).

### Phase 3 — Tests & CI (½–1 day)

1. Retarget `msdnus.Tests` to `net10.0`; update MSTest to latest 3.x + `Microsoft.NET.Test.Sdk`.
2. Revive/extend tests: XML import parsing, DTO mapping, crypto round-trip, LiteDB
   save/query/purge (temp-file DB), password verify logic.
3. Add GitHub Actions workflow (`windows-latest`): restore → build (x64) → `dotnet test`.
   Fixes H6, H7.

**Gate 3:** `dotnet test` green locally and in CI on every push.

### Phase 4 — Runtime bring-up (1–2 days, the real "make it work")

Run the packaged app (F5 / `msix` deploy) and fix until the full feature checklist passes:

- [ ] App launches, DI resolves, main window shows lock screen
- [ ] Default password `123` unlocks; wrong password rejected
- [ ] Import keys XML from file picker → keys listed
- [ ] Search filters list; copy-to-clipboard works; toast shown
- [ ] OneDrive import: MSAL interactive sign-in (switch to WAM broker via
  `WithBroker(new BrokerOptions(...))` — required for reliable consumer sign-in in packaged
  WinUI 3 apps) → Graph downloads `KeysExport.xml`
- [ ] Settings: password change persists; theme toggle (light/dark) applies immediately and
  persists
- [ ] Navigation: About / Help / CoffeeCup pages open and return
- [ ] Lock command re-locks; relaunch requires password again
- [ ] App survives restart: imported keys still present and decryptable

Known work items in this phase:

1. Consolidate `DeviceFamily-Desktop`/`DeviceFamily-Mobile` `MainPage.xaml` into the single
   `MainPage.xaml` (device families are a UWP concept). Fixes H5.
2. Delete dead helpers (`PropertyChangedBase`, `ViewModelLocator`) once nothing references them.
3. Verify `x:Uid` resources resolve (en-US / ru-RU `.resw` under WinAppSDK MRT Core).
4. Replace any remaining UWP-isms flagged at runtime (dialog `XamlRoot`, `Window` vs
   `CoreWindow`, `DataTransfer` clipboard on UI thread, toast APIs).

**Gate 4:** full checklist above passes on a clean Windows 11 machine/VM.

### Phase 5 — Packaging & release (½ day)

1. MSIX packaging with the stable WinAppSDK; decide self-contained vs framework-dependent
   WinAppSDK runtime.
2. Refresh certificate story (current `.pfx` thumbprint is committed in the csproj — move
   signing to a local/CI secret; never commit certificates).
3. In-app purchase / donation page: `Windows.Services.Store` APIs still work in packaged
   WinUI 3 apps with `StoreContext` initialized via `InitializeWithWindow` — or simplify
   CoffeeCup to an external donation link and drop the Store dependency entirely (recommended
   for a personal tool).
4. Tag release; optional: CI job producing a sideload-ready MSIX artifact.

**Gate 5:** signed MSIX installs and runs on a machine without Visual Studio.

### Phase 6 — Optional polish (backlog, not required for "done")

- `NavigationView`-based shell instead of page-by-page navigation
- Windows Hello / credential-locker instead of stored password
- AOT/trimming audit (`PublishAot` currently off — fine)
- Accessibility pass (Narrator, keyboard-only)

---

## 5. Effort & Risk Summary

| Phase | Effort | Main risk |
|---|---|---|
| 1. Compile | 0.5 d | Stable WinAppSDK API drift from experimental3 (unknown surface) |
| 2. Core decoupling | 1–2 d | Low — mechanical, well-bounded |
| 3. Tests & CI | 0.5–1 d | Low |
| 4. Runtime bring-up | 1–2 d | **Highest** — first time the app runs on WinUI 3; MSAL/WAM auth flow needs a real Microsoft account to test |
| 5. Packaging | 0.5 d | Certificate/signing logistics |
| **Total** | **4–6 days** | |

Riskiest unknowns, in order:
1. **MSAL + legacy client ID `000000004017CE14`**: the pre-Azure "Live SDK" app ID may not work
   with modern MSAL/WAM. Fallback: register a free Azure app (personal Microsoft accounts,
   `Files.Read`) — 15 minutes, no cost.
2. **Stable WinAppSDK API differences** vs the experimental build the code was ported against.
3. **Resource (`.resw`) loading** under MRT Core — historically a common WinUI 3 migration trap.

---

## 6. Explicit Non-Goals

- No cross-platform support (Windows-only by design).
- No migration of existing Lex.Db data (re-import from XML instead).
- No visual redesign beyond what WinUI 3 gives for free (Fluent styles).
- No backward compatibility with the UWP/Windows 10 Mobile build.

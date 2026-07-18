# UI Redesign — Rationale and Decisions

Date: 2026-07-18 · Author: Claude (design brief: "make it beautiful and USABLE, your call")

## Starting Point

The pre-redesign UI was a direct port of a 2015 UWP layout:

- Five pages (Main, Settings, About, Help, CoffeeCup) with `Frame` navigation.
- The lock screen was a `SplitView` side pane — a hamburger-menu control abused as a modal
  barrier, dismissible with a swipe.
- A bottom `CommandBar` with seven cryptic icon buttons, half hidden in an overflow menu.
- Copy required hitting a small `AppBarButton` next to each key.
- Feedback went through **system** toast notifications that pop outside the app window.
- Windows 8-era brushes (`SystemControlBackgroundChromeMediumBrush`), fixed pixel margins,
  a `ListBox` inside a `ScrollViewer` (which silently kills UI virtualization).

## Design Premise

The application has exactly one job: **unlock → find a key → copy it**. Every design decision
below is judged against that path. A utility this small earns beauty through focus, not
decoration — remove everything that is not on the critical path, then polish what remains.

## Decisions

### 1. One page, no navigation
A search utility does not need a page stack. All five pages collapse into a single view:

| Was | Now | Why |
|---|---|---|
| Help page | Empty state with a direct "Import keys XML" call to action | The UI should explain itself exactly when the user needs it (first run), not behind a "?" button |
| About page (10 version-history entries) | Compact flyout: version, author, source link | Version history belongs in git, not in a UI page |
| Settings page | `ContentDialog` (theme toggle + PIN change) | Two controls do not justify a navigation destination |
| CoffeeCup (donation) | **Removed** | The Store IAP mechanism it relied on is dead; per the brief, dropped entirely |
| NavigationService, page ViewModels | Deleted | ~500 lines of infrastructure serving no user value |

### 2. Lock screen as a full-window overlay
The PIN gate is now a centered overlay (lock glyph, app name, PIN box, reset link) covering
the whole window. Rationale: a modal barrier must look and behave modal. The `SplitView` pane
looked like a menu, could be light-dismissed, and left the (empty) content area visible.
The PIN box is auto-focused on launch and on every re-lock; keys are simply not rendered
while locked.

### 3. Copy is the primary action — make the whole row a button
Each key row is one wide, monospaced button: click anywhere on it → key copied. No more
aiming at a 32-px icon. The key value is set in a monospace face and is the visually loudest
element of each card; product name is the card header; type and claimed date are dimmed
captions on the same row.

### 4. In-app feedback instead of system toasts
"Copied", "Imported 3606 products", "PINs do not match" now appear in an `InfoBar` docked at
the bottom of the window (auto-hides after 3 s, severity-colored). System toasts were the
wrong tool: they pop in the OS corner, outside the user's locus of attention, persist in the
Action Center, and require package identity. `TextToastManager` and the AppNotification
dependency are deleted.

### 5. Search is the centerpiece
- `AutoSuggestBox` with a search glyph at the top, filter-as-you-type (existing behavior kept).
- A live result counter ("3606 products") next to it — silent confirmation that import worked
  and the filter is doing something.
- Keyboard: `Ctrl+F` focuses search, `Esc` clears it, `Ctrl+O` imports a file, `Ctrl+L` locks.
  Focus lands in the search box immediately after unlock.
- Empty search **results** show nothing special (the counter reads "0"); the illustrated empty
  state is reserved for an empty **database** — different situations, different answers.

### 6. Fluent, but stock
Mica window backdrop, content extended into the title bar, rounded corner cards from the
default WinUI 3 `ItemContainer` look, Segoe Fluent icon glyphs, theme-aware brushes only —
no custom colors. A tool used twice a year should look native to the OS, not branded.
Dark/light theme toggle is preserved (in the settings dialog).

### 7. Data-flow simplification (invisible but load-bearing)
`NotifyTaskCompletion<List<T>>` (an async-binding wrapper from 2015, and the source of a
binding crash during migration) is replaced by a plain awaited property set:
`Products = await GetDataValueByNameAsync(...)`. The XAML binds to `Products` directly;
`ListView` virtualization works because the `ScrollViewer` wrapper is gone.

### 8. Localization kept
en-US / ru-RU `.resw` files are rewritten to match the new UI (≈25 strings instead of 77).
ViewModel messages go through a small `Loc` helper with hardcoded English fallbacks, so the
app degrades gracefully if resource loading fails (e.g., unpackaged runs).

## What Was Deliberately Not Done

- No NavigationView/шелл, no animations beyond stock control transitions — nothing to navigate to.
- No grouping/sorting options — alphabetical order plus instant search covers 3606 items.
- No custom accent colors or branding — see decision 6.
- Donation was removed rather than replaced with a link: the brief allowed removal, and a
  donation link inside a personal-use tool is noise.

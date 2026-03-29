# TrueBalancer -- Procon v2 Plugin

## Project Overview

TrueBalancer is a C# team balancing plugin for Procon v2 (Battlefield game server administration). It provides player-number balancing, skill-based team scrambling, and a balancing guard to prevent unfair team switching.

- **Language:** C#
- **License:** GPLv3
- **Original authors:** Panther (< v0.5), onegrizzlybeer (v0.5+)
- **Supported games:** BF3, BF4
- **Dependencies:** None beyond Procon v2 (runtime only)

## Architecture

The plugin uses a **partial class** pattern -- `TrueBalancer` is split across 7 files in `src/TrueBalancer/`:

| File | Responsibility |
|------|---------------|
| `src/TrueBalancer.cs` | Main entry point, fields, constructor, helper classes (CPlayerJoinInf, CPlayerScoreInf, CSquadScoreInf) |
| `src/TrueBalancer/Settings.cs` | Plugin metadata, GetDisplayPluginVariables, SetPluginVariable, command registration |
| `src/TrueBalancer/Events.cs` | Procon event handlers (OnListPlayers, OnServerInfo, OnPlayerTeamChange, etc.) |
| `src/TrueBalancer/Balancer.cs` | Core balancing logic (CompareTeams, startBalancing, DebugInfo) |
| `src/TrueBalancer/Scrambler.cs` | Skill-based team scrambler (StartScrambler, ScrambleNow, ScrambleRound) |
| `src/TrueBalancer/Guard.cs` | Balancing guard (team switch prevention, force move commands) |
| `src/TrueBalancer/Battlelog.cs` | Battlelog stats fetching (BattlelogClient, PlayerStats inner classes) |

## Code Style

Style is enforced by `.editorconfig` and checked via `dotnet format` in CI.

**Critical conventions:**
- **Use `String`, `Int32`, `Boolean`, `Double`** -- NOT `string`, `int`, `bool`, `double`. The codebase uses explicit System type names everywhere.
- **Allman brace style** -- opening brace on its own line
- **4 spaces** for indentation, LF line endings
- **Block-scoped namespaces** (not file-scoped)
- **`using` directives outside namespace**, System usings first

## Build & CI

- `TrueBalancer.csproj` at root is a **CI-only artifact** for `dotnet format`. It is NOT a real build file -- Procon v2 assemblies are unavailable for compilation.
- **CI workflow** (`.github/workflows/ci.yml`): runs on push to `master` and PRs. Checks `dotnet format whitespace` and `dotnet format style --exclude-diagnostics IDE1007`.
- **Release workflow** (`.github/workflows/release.yml`): triggered by `v*` tags. Packages `.cs` files from `src/` into a zip and creates a GitHub Release.

### Running style checks locally

```bash
dotnet restore
dotnet format whitespace --verify-no-changes
dotnet format style --verify-no-changes --severity warn --exclude-diagnostics IDE1007
```

## Branch Structure

- `master` -- current development, Procon v2 only
- `legacy` -- archived original single-file version

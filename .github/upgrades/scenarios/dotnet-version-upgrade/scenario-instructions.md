# .NET Version Upgrade

## Preferences
- **Flow Mode**: Automatic
- **Target Framework**: net10.0
- **Scope Focus**: Upgrade to newest .NET MAUI controls/runtime for .NET 10

## Source Control
- **Source Branch**: maui-mobile-support
- **Working Branch**: upgrade-maui-net10
- **Commit Strategy**: After Each Task
- **Branch Sync**: Auto (Merge)

## Upgrade Options
**Source**: .github/upgrades/scenarios/dotnet-version-upgrade/upgrade-options.md

### Strategy
- Upgrade Strategy: Top-Down

### Project Structure
- Package Management: Central Package Management (CPM)

### Compatibility
- Unsupported API Handling: Fix Inline

## Strategy
**Selected**: Top-Down (Application-First)
**Rationale**: 19-project modern .NET solution with mixed app/library/test surfaces and concentrated API risk is safer with app-first incremental slices.

### Execution Constraints
- Upgrade runnable entry-point projects before final shared-library/test consolidation.
- Keep each task slice buildable before advancing to the next major project group.
- Resolve API/source incompatibilities inline (no deferred stubs) as projects are upgraded.
- Run full-solution restore/build/test only after all project groups are on net10.0.

## Key Decisions Log
- 2026-06-26: User confirmed upgrade to .NET 10 with automatic execution and default git workflow.
- 2026-06-26: User requested to proceed without pauses and upgrade the full solution scope.

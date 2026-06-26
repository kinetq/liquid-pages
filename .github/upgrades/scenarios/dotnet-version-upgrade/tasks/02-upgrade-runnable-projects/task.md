# 02-upgrade-runnable-projects: Upgrade runnable app/sample entry points to net10.0

Upgrade all runnable entry-point projects (samples and host applications) to net10.0 and align their direct package references for .NET 10 compatibility. Focus on application-facing behavior changes first so runtime-impacting issues are exposed early.

This task includes the MAUI sample project and its related app bootstrap/control usage changes needed for .NET 10 compatibility, while preserving existing functionality expectations for each sample surface.

## Scope Inventory

### Projects in Scope
- `src/Kinetq.LiquidPages.Avalonia.Sample/Kinetq.LiquidPages.Avalonia.Sample.csproj`
- `src/Kinetq.LiquidPages.AspNetCore.Sample/Kinetq.LiquidPages.AspNetCore.Sample.csproj`
- `src/Kinetq.LiquidPages.EmbedIO.Sample/Kinetq.LiquidPages.EmbedIO.Sample.csproj`
- `src/Kinetq.LiquidPages.GenHTTP.Sample/Kinetq.LiquidPages.GenHTTP.Sample.csproj`
- `src/Kinetq.LiquidPages.RazorPages.Sample/Kinetq.LiquidPages.RazorPages.Sample.csproj`
- `src/Kinetq.LiquidPages.SimpleW.Sample/Kinetq.LiquidPages.SimpleW.Sample.csproj`
- `src/Kinetq.LiquidPages.SimpleW.Razor.Sample/Kinetq.LiquidPages.SimpleW.Razor.Sample.csproj`

### Distinct Concerns
- Target framework updates to net10.0 (and net10.0-windows for MAUI sample).
- Direct package alignment for upgraded app/sample projects.
- Source/behavior compatibility fixes, concentrated in the MAUI sample and SimpleW samples.

### Change Signals from Assessment
- MAUI sample has the highest risk surface (116 issues, including 109 source-incompatible API occurrences, 4 behavioral changes, and package upgrade recommendations).
- Other app/sample projects are mostly straightforward TFM changes with low issue counts (1-3 issues each), plus a small number of behavior/source compatibility incidents.
- App/sample projects have dependency relationships to shared libraries (`Kinetq.LiquidPages`, `Kinetq.LiquidPages.Router`, `Kinetq.LiquidPages.AspNetCore`, `Kinetq.LiquidPages.EmbedIO`, `Kinetq.LiquidPages.GenHTTP`, `Kinetq.LiquidPages.SimpleW`) that require ordered execution.

### Breakdown Decision
Task decomposition is required because this task spans 7 dependent runnable projects and fires the `multi-project-dependency-ordering` MUST hint from scenario breakdown rules.

**Done when**: All runnable app/sample projects target net10.0 with compatible package references, and compile-time/runtime-blocking API issues in these projects are resolved.

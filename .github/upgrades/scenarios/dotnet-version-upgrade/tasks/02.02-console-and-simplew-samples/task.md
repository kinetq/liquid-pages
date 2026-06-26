# 02.02-console-and-simplew-samples: Upgrade console-style sample hosts to net10.0

## Objective
Upgrade non-web sample hosts that depend on shared libraries, keeping runnable sample coverage aligned with net10.0 while preserving behavior.

## Scope
- src/Kinetq.LiquidPages.EmbedIO.Sample/Kinetq.LiquidPages.EmbedIO.Sample.csproj
- src/Kinetq.LiquidPages.GenHTTP.Sample/Kinetq.LiquidPages.GenHTTP.Sample.csproj
- src/Kinetq.LiquidPages.SimpleW.Sample/Kinetq.LiquidPages.SimpleW.Sample.csproj
- src/Kinetq.LiquidPages.SimpleW.Razor.Sample/Kinetq.LiquidPages.SimpleW.Razor.Sample.csproj

## Research Findings
- All four projects have mandatory `Project.0002` retargeting from `net9.0` to `net10.0`.
- `EmbedIO.Sample` and `GenHTTP.Sample` include `Microsoft.Extensions.Logging.Console` version `10.0.2` with an assessment recommendation to move to `10.0.9`.
- `SimpleW.Sample` and `SimpleW.Razor.Sample` contain source-incompatible calls to `TimeSpan.FromDays(int)` that require decimal/double literal usage for net10 compatibility.
- Assessment behavioral notes for `AddSimpleConsole(...)` are present in three projects; these calls remain compile-compatible but need runtime/build confirmation after retargeting.

## Steps
1. Retarget TFMs to net10.0 for all projects in scope.
2. Apply package reference updates indicated by restore/build diagnostics.
3. Resolve source/behavior compatibility findings that block compile for these entry points.
4. Build each project and validate dependency compatibility with current shared libraries.

## Done when
All in-scope console/SimpleW sample projects target net10.0 and compile without unresolved upgrade blockers.

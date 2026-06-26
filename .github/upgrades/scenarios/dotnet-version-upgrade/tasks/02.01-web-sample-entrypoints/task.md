# 02.01-web-sample-entrypoints: Upgrade ASP.NET Core sample entry points to net10.0

## Objective
Upgrade web entry-point projects to net10.0 with minimal package and API churn, establishing a low-risk first slice for runnable samples.

## Scope
- src/Kinetq.LiquidPages.AspNetCore.Sample/Kinetq.LiquidPages.AspNetCore.Sample.csproj
- src/Kinetq.LiquidPages.RazorPages.Sample/Kinetq.LiquidPages.RazorPages.Sample.csproj

## Research Findings
- Assessment for `AspNetCore.Sample` reports a mandatory TFM retarget only (`Project.0002`, net9.0 -> net10.0).
- Assessment for `RazorPages.Sample` reports mandatory TFM retarget plus one behavioral API issue (`Api.0003`) in `Program.cs` around exception handler usage.
- Dependency analysis shows `AspNetCore.Sample` references `Kinetq.LiquidPages.AspNetCore` and `Kinetq.LiquidPages`; retargeting the sample should stay compatible while shared libraries are upgraded in subsequent tasks.
- No central `Directory.Build.props` override was detected for these projects through dependency inspection, so project-local TFM properties are the expected edit points.

## Steps
1. Retarget project TFMs to net10.0.
2. Align direct package references if required by restore/build output.
3. Build both projects and resolve app-surface compile issues.

## Done when
Both web sample projects target net10.0 and build successfully with no unresolved blocking API/package issues.

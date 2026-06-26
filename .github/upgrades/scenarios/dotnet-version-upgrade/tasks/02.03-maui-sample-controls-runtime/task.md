# 02.03-maui-sample-controls-runtime: Upgrade MAUI sample project and control/runtime surface to net10.0

## Objective
Complete the high-risk MAUI sample migration slice by aligning target framework, MAUI-related dependencies, and application code to .NET 10 expectations.

## Scope
- src/Kinetq.LiquidPages.Avalonia.Sample/Kinetq.LiquidPages.Avalonia.Sample.csproj
- Associated MAUI app bootstrap and UI files in the same project

## Steps
1. Retarget MAUI sample TFM from net9.0-windows10.0.19041.0 to net10.0-windows.
2. Align MAUI/runtime-related package references for .NET 10 compatibility.
3. Resolve source-incompatible and behavioral API issues identified in assessment for this project.
4. Build the MAUI sample project and fix blocking compile/runtime-surface issues.

## Done when
The MAUI sample targets net10.0-windows, package references are compatible, and blocking API compatibility issues are resolved for successful build.

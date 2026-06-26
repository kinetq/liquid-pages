# .NET Version Upgrade Plan

## Overview

**Target**: Upgrade the full Kinetq.LiquidPages solution to .NET 10, including the MAUI sample and all shared libraries/samples/tests.
**Scope**: Large solution, 19 projects, modern .NET only (net8/net9), with assessment-flagged API behavior/source compatibility and package update work.

### Selected Strategy
**Top-Down (Application-First)** — Application and runnable sample projects are upgraded first, then shared libraries are finalized across the solution.
**Rationale**: The solution size and breadth (19 projects, 3 dependency levels, mixed app/library/test surfaces, and concentrated API risk in the MAUI sample) favor incremental, app-focused slices rather than a single high-blast-radius change.

### Application and Library Groups
Applications/samples/tests: `Kinetq.LiquidPages.Avalonia.Sample`, `Kinetq.LiquidPages.AspNetCore.Sample`, `Kinetq.LiquidPages.EmbedIO.Sample`, `Kinetq.LiquidPages.GenHTTP.Sample`, `Kinetq.LiquidPages.RazorPages.Sample`, `Kinetq.LiquidPages.SimpleW.Sample`, `Kinetq.LiquidPages.SimpleW.Razor.Sample`, and test projects.

Core/shared libraries: `Kinetq.LiquidPages`, `Kinetq.LiquidPages.AspNetCore`, `Kinetq.LiquidPages.EmbedIO`, `Kinetq.LiquidPages.GenHTTP`, `Kinetq.LiquidPages.SimpleW`, `Kinetq.LiquidPages.Router`, `Kinetq.LiquidPages.Extension`, `Kinetq.LiquidPages.Templates`.

## Tasks

### 01-prerequisites: Verify SDK and repository upgrade readiness
Validate that the local environment and repository configuration are ready for a net10.0 migration across all projects. This includes confirming .NET 10 SDK availability and checking solution-level SDK pinning so the upgrade can build consistently in local and CI environments.

This task also establishes the baseline state for dependency restore/build before broad project edits begin, reducing avoidable churn during the main migration tasks.

**Done when**: .NET 10 SDK requirements are validated, any required global.json adjustments are applied, and a baseline restore/build readiness check is complete.

---

### 02-upgrade-runnable-projects: Upgrade runnable app/sample entry points to net10.0
Upgrade all runnable entry-point projects (samples and host applications) to net10.0 and align their direct package references for .NET 10 compatibility. Focus on application-facing behavior changes first so runtime-impacting issues are exposed early.

This task includes the MAUI sample project and its related app bootstrap/control usage changes needed for .NET 10 compatibility, while preserving existing functionality expectations for each sample surface.

**Done when**: All runnable app/sample projects target net10.0 with compatible package references, and compile-time/runtime-blocking API issues in these projects are resolved.

---

### 03-upgrade-shared-libraries: Upgrade shared framework libraries and adapters to net10.0
Upgrade shared libraries consumed by multiple samples/tests to net10.0 and modernize package references where the assessment identified recommended or deprecated packages. Resolve source-incompatible and behavioral-change API issues that affect shared code paths.

This task is the stabilization pass for common code so downstream tests and sample projects can build against a consistent net10.0 library set.

**Done when**: Shared libraries target net10.0, package upgrades/deprecations in shared libraries are handled, and shared-code compile issues for net10.0 are resolved.

---

### 04-upgrade-test-projects: Upgrade and repair all test projects against net10.0
Move test projects to net10.0 and update test dependencies/framework packages to compatible versions. Repair broken test references and compilation issues introduced by framework/package/API changes from prior tasks.

This task ensures automated verification remains reliable and aligned with the upgraded production/sample code.

**Done when**: All test projects target net10.0, restore/build cleanly, and test dependency versions are compatible with net10.0.

---

### 05-solution-wide-validation: Run full solution validation and address residual warnings/issues
Execute full restore/build/test validation after all project upgrades are applied. Resolve remaining compiler/build issues and upgrade warnings in modified projects, including package-related warnings and any residual API compatibility issues.

Capture any intentionally deferred, non-blocking recommendations as explicit follow-ups so the upgraded baseline is clear and reproducible.

**Done when**: Full solution restore/build succeeds, tests pass (or failures are documented with root cause), and modified projects are warning-clean for the upgraded scope.

---

### 06-upgrade-documentation-and-finalization: Finalize upgrade records and completion artifacts
Update upgrade workflow artifacts with what changed, validation outcomes, and any remaining recommended follow-up work. Ensure scenario state reflects completed execution and provides a clear handoff for future maintenance.

This task closes the migration loop by making the upgrade auditable and easy to review.

**Done when**: Scenario task artifacts are complete, progress documentation is finalized, and workflow state reflects completed migration steps.

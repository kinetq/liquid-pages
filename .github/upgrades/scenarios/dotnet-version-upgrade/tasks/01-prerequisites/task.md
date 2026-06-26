# 01-prerequisites: Verify SDK and repository upgrade readiness

Validate that the local environment and repository configuration are ready for a net10.0 migration across all projects. This includes confirming .NET 10 SDK availability and checking solution-level SDK pinning so the upgrade can build consistently in local and CI environments.

This task also establishes the baseline state for dependency restore/build before broad project edits begin, reducing avoidable churn during the main migration tasks.

## Research Summary

- Verified .NET 10 SDK compatibility using `validate_dotnet_sdk_installation(net10.0)`.
- Verified there is no `global.json` pin requiring update using `validate_dotnet_sdk_in_globaljson(net10.0)`.
- Ran baseline restore/build checks on `Kinetq.LiquidPages.sln`.
- Installed required MAUI workload via `dotnet workload install maui` after readiness validation identified MAUI workload mismatch during baseline build.

## Baseline Findings

- SDK requirement for net10.0 is satisfied.
- No repository `global.json` pin blocks this migration.
- Baseline solution build currently reports MA003 from MAUI package/workload version alignment in current state; this is expected to be fully resolved as part of subsequent net10.0 project retargeting and package alignment tasks.

**Done when**: .NET 10 SDK requirements are validated, any required global.json adjustments are applied, and a baseline restore/build readiness check is complete.

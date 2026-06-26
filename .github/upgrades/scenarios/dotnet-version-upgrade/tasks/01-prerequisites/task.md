# 01-prerequisites: Verify SDK and repository upgrade readiness

Validate that the local environment and repository configuration are ready for a net10.0 migration across all projects. This includes confirming .NET 10 SDK availability and checking solution-level SDK pinning so the upgrade can build consistently in local and CI environments.

This task also establishes the baseline state for dependency restore/build before broad project edits begin, reducing avoidable churn during the main migration tasks.

**Done when**: .NET 10 SDK requirements are validated, any required global.json adjustments are applied, and a baseline restore/build readiness check is complete.

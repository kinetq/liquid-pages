# 03-upgrade-shared-libraries: Upgrade shared framework libraries and adapters to net10.0

Upgrade shared libraries consumed by multiple samples/tests to net10.0 and modernize package references where the assessment identified recommended or deprecated packages. Resolve source-incompatible and behavioral-change API issues that affect shared code paths.

This task is the stabilization pass for common code so downstream tests and sample projects can build against a consistent net10.0 library set.

**Done when**: Shared libraries target net10.0, package upgrades/deprecations in shared libraries are handled, and shared-code compile issues for net10.0 are resolved.

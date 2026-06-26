# 02-upgrade-runnable-projects: Upgrade runnable app/sample entry points to net10.0

Upgrade all runnable entry-point projects (samples and host applications) to net10.0 and align their direct package references for .NET 10 compatibility. Focus on application-facing behavior changes first so runtime-impacting issues are exposed early.

This task includes the MAUI sample project and its related app bootstrap/control usage changes needed for .NET 10 compatibility, while preserving existing functionality expectations for each sample surface.

**Done when**: All runnable app/sample projects target net10.0 with compatible package references, and compile-time/runtime-blocking API issues in these projects are resolved.

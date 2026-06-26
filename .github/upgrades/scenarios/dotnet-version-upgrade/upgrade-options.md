# Upgrade Options — Kinetq.LiquidPages.sln

Assessment: 19 SDK-style modern .NET projects (net8/net9) targeting net10.0; API breaking changes are flagged in multiple projects.

## Strategy

### Upgrade Strategy
Large modern-to-modern solution size (19 projects) favors incremental application-first migration to keep risk manageable.

| Value | Description |
|-------|-------------|
| **Top-Down** (selected) | Upgrade entry-point applications first, then consolidate shared libraries once app upgrades are complete. |
| All-at-Once | Upgrade all projects in one pass for fastest execution but with higher temporary breakage risk. |

## Project Structure

### Package Management
Multiple projects and no existing central package management make CPM a good fit for modern-to-modern alignment.

| Value | Description |
|-------|-------------|
| **Central Package Management (CPM)** (selected) | Add Directory.Packages.props and centralize package versions for consistency across projects. |
| Per-Project (defer CPM to post-migration) | Keep package versions in each project during migration and postpone CPM to later cleanup. |

## Compatibility

### Unsupported API Handling
Assessment flagged source-incompatible APIs for the target framework in some projects.

| Value | Description |
|-------|-------------|
| **Fix Inline** (selected) | Resolve all API changes in the same upgrade tasks, including complex changes, with no deferred stubs. |
| Defer Complex Changes | Fix simple changes now and defer complex API replacements into follow-up stub-resolution tasks. |

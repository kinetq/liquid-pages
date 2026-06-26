# 05-solution-wide-validation: Run full solution validation and address residual warnings/issues

Execute full restore/build/test validation after all project upgrades are applied. Resolve remaining compiler/build issues and upgrade warnings in modified projects, including package-related warnings and any residual API compatibility issues.

Capture any intentionally deferred, non-blocking recommendations as explicit follow-ups so the upgraded baseline is clear and reproducible.

**Done when**: Full solution restore/build succeeds, tests pass (or failures are documented with root cause), and modified projects are warning-clean for the upgraded scope.

# Runtime integration fork

This branch builds on Jint v4.16.0 and retains the upstream source and license.
OfficeIMO uses it for the native JavaScript interpreter in its isolated HTML worker.

The history metadata change adds `JsValue.IsLengthTrackingArrayBufferView()`. A host can
preserve a typed array or DataView's construction mode when serializing it, without
executing script, resizing its buffer, or reading private fields through reflection.
Bounds and detachment checks remain the caller's responsibility. The same API is
proposed against upstream `main` in [Jint #4146](https://github.com/sebastienros/jint/pull/4146); this branch carries its narrow 4.16
backport without adopting Jint 5's broader runtime surface.

`Host.CanExecuteJob()` lets an embedder reject queued interpreter work when its
execution context retires. Eligibility is checked for every job, including reactions
queued during a drain and late host-promise settlement. It does not interrupt the
currently executing script or cancel external operations. The default host permits
all jobs; OfficeIMO supplies its realm-lifetime policy.

History policy, frame messaging, resource budgets, navigation and rendering remain
in OfficeIMO. Validate the host API and the consuming worker before changing its
source pin. Fork builds are not official Jint releases; do not publish them under
upstream package identities.

```sh
dotnet test Jint.Tests.PublicInterface/Jint.Tests.PublicInterface.csproj -c Release -f net10.0 --filter FullyQualifiedName~HostBufferViewMetadataTests
```

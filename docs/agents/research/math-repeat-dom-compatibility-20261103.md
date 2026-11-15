# Owned repeat DOM structural compatibility

Repeat apply preparation now runs the bounded kind3/default-Math portion of
Engine `0E2870` on a private scratch clone. It reads actual serialized operands
only through the active outgoing payload lease and the existing synchronous
owning read chain. Snapshots cannot supply original-document authority.

All operation paths are resolved before any structural effects. A missing path
reports completed-but-incompatible without applying earlier rows. Insertions
select the current child ordinal; an append requires the exact current child
count. A failed append check still leaves its already-cloned, unattached payload
in the diagnostic scratch snapshot. Deletions detach the current child without
freeing its descendants. Scalar operations5/6/7 bind their paths but do not change
scratch values, matching the native structural preflight.

The empty route allocates no scratch tree. Kind4 roots, observer contexts,
operations1/3, unknown structural payloads and native allocation-failure behavior
are outside this bounded profile. Work/depth limits and cancellation are explicit
managed restrictions. Compatibility does not establish scalar application,
registry/tag containment, source registration or original receiver readiness.

Preparation retains only diagnostic copies of scratch nodes and ordered links.
Reads return independent arrays; disposal releases the retained arrays and
revokes reads. Exporter/ancestor disposal preserves the existing lease rules.
The old receipt is unconsumed and the original source/DOM is not mutated.
Original mutation grants, journal/rollback and successor issuance remain missing.

## Review and completion of this increment, 16 November 2026

The bounded component is retained as a separate Math change after the stable
.NET11 migration. This completes structural preflight and the prepared typed-tag
profile, not the remaining original-document receiver.

- Expanded kernel checks pass121 controls in Managed and fresh Windows Native AOT,
  including explicit refusal of unsupported operation codes, oversized paths,
  invalid payloads and foreign anchors without changing either input owner.
- Fresh owning runs pass886 controls in both modes. Their captures are byte-identical;
  independent NN/candidate/diff/unchanged/temporary-apply audits,48 lease checks and
  replay against the independently frozen3 November predictions all pass.
- Fixed the structural auditor's case-coverage gap: duplicate actual/lease cases
  could previously substitute for missing cases, and duplicate frozen rows could
  be silently discarded. It now requires each case0..5 exactly once and retains
  validation under Python `-O`. Thirteen corruption controls pass with and without
  optimization; CI runs them after each managed/native structural audit.
- The original3 November source and evidence hashes were checked before edits;
  that historical manifest is preserved. New logs, captures and native binaries
  are under `.artifacts/math-polish-*`. The existing desktop smoke failure and
  remaining original mutation/journal/successor work are not claimed as resolved.
- The full solution Release build with stable SDK11.0.100 and `-m:1` passes with
  zero warnings/errors; both static native instruction-pin auditors pass.

## Evidence and validation

The following records the original3 November validation, before the review above.

Local evidence is under `.artifacts/repeat-dom-compatibility-20261103/`:

- A read-only Ghidra batch exports `0E2870`, verifies every instruction byte
  against the pinned PE and preserves the project. The stdlib auditor
  `audit_repeat_dom_preflight.py` authenticates the whole engine, the complete
  code region,13 instruction sequences and the BE2 preflight binding.
- The DOM kernel probe passes109 controls in Managed and Windows Native AOT,
  including invalid paths/ordinals, retained parent identities, successful
  prefixes before incompatibility, scalar no-ops and cancellation/work limits.
- The owning real-model probe passes886 controls in both modes. Its six real
  repeat exports perform11 structural scratch changes. Captures are byte-equal.
- An independent Python object-tree implementation predicts all scratch node
  bytes/fields, ordered links, compatibility and counters. Local predictions
  were generated from a separately captured baseline before the final candidate
  compilation; the earlier exploratory candidate was already executed. Six
  deliberately corrupted captures are rejected.
- The existing48 lease checks and all five independent NN/candidate/diff/
  unchanged/temporary-apply auditors pass. Existing Actual and Unchanged arrays
  are exactly equal to the baseline. NumPy2.5.4 was installed in an isolated
  local `.artifacts` environment for the NN auditor.
- The existing math-prior CI script now also runs the independent structural
  replay in both modes. Remote CI has not been run here.
- The full solution Release build passes with zero warnings/errors. Managed
  net10.0 probes run with `--roll-forward Major` on installed runtime11;
  Native AOT probes use their net10.0 native runtime.

Native AOT is now available locally after installation of Visual Studio2026
Insiders/MSVC14.52.37025. This supersedes the earlier missing-linker limitation
recorded in the [subtree-tag increment](math-repeat-subtree-tags-20261103.md).

## .NET11 application publication

Full application AOT publication exposed missing compiled `App.axaml` resources,
also reproduced with a fresh intermediate directory. Ordinary full Build embeds
the resources; optimized AOT publish does not. SDK RC3's local publish targets
confirm that `UseOptimizedPublish=false` restores full Build ordering.
The project now sets that property, following the documented workaround in
[Avalonia issue22465](https://github.com/AvaloniaUI/Avalonia/issues/22465).
Initial failed publications and startup captures are retained.

The fixed Windows Native AOT publication succeeds. Its intermediate assembly
contains `!AvaloniaResources`, compiled XAML population methods and
`CompiledAvaloniaXaml`; the build reports compilation of `/App.axaml`.
An awaited native process starts successfully and reaches the existing desktop
pointer-resize assertion, then exits1. Thus the missing-XAML startup regression
is repaired, but full UI smoke still fails. ILC also emits two Avalonia.Win32
DirectComposition callback diagnostics about non-blittable Boolean parameters;
these were present in the failed publications as well and are not resolved here.

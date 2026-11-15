# Repeat DOM subtree tag remapping and SDK RC3

This is the historical 3 November checkpoint. The later
[structural compatibility checkpoint](math-repeat-dom-compatibility-20261103.md)
supersedes its missing-toolchain limitation. Stable SDK `11.0.100` and the
server's `net11.0` migration were committed separately on 16 November; see the
[current build requirements](../../engineering/build-and-packaging.md).

The prepared repeat DOM kernel now handles nonidentity tag mappings on inserted
element subtrees. It clones into the destination name owner, attaches at the
current child ordinal, maps the root tag, then visits descendant tags. Root
mapping preserves a value when the mapped result is -1; the descendant callback
writes a found result verbatim, including -1. An old value of -1 is skipped in
both branches. Mapping is applied once per attribute, without cascading through
the mapping table. The payload writer remains unchanged.

Native evidence is Engine `0E2D70` / `0E56F0`, writer V168/V128/V100 and the
descendant axis13 initializer/step `0DE8E0` / `0DF820`. The initializer starts at
the first child rather than the root itself; the step descends before yielding.
`audit_repeat_subtree_tags.py` pins the engine SHA256, three code regions,
14 instruction sequences, six bindings and the tag/query literals. It requires
the retained local SDK PE and never executes vendor code.

This remains a bounded prepared kernel. With a nonempty mapping, tag attributes
must be unique, typed INT64 values on elements; malformed/other scalar formats
are refused before cloning. Native general value conversions, text updates,
original application authority, source registration mappings, rollback and the
successor receiver are still incomplete. This increment does not change public
recognition admission or demonstrate handwriting accuracy. Cancellation during
an entered operation may still leave a partial clone/mutation; no new atomicity
or native failure-prefix equivalence is claimed.

Validation on Windows, 3 November 2026:

- The existing DOM probe passes 94 controls on .NET 11 RC3, including mapping
  collisions, nested and repeated IDs, both -1 branches, absent tags/mappings,
  INT64 extremes, unchanged source/other values, malformed/duplicate tags and
  precancellation. The net10.0 probe used `dotnet --roll-forward Major` because
  only runtime11 is installed. The existing math-prior CI script already runs
  this probe in Managed and Native AOT modes; remote CI has not been run here.
- The explicit port-check runner passes its three static steps with caching
  disabled. The impact planner parses1376 Python files without syntax errors;
  it finds no direct candidates for this C# file and is not a complete test map.
- Read-only Ghidra batches independently verify exported PE instruction bytes
  and unchanged project hashes. An exploratory axis batch fails correctly on
  missing function entry `0x1800de8d0`; it is retained as a failed batch. The
  descendant follow-up exports the two actual entries successfully. Tiny axis
  initializers are pinned directly from the PE rather than fabricated as Ghidra
  function entries.
- The full solution Release build passes with zero warnings/errors using
  `-p:PublishAot=false -p:SelfContained=false -m:1`.
- Native AOT probe publication stops at `Platform linker not found`; the local
  Visual Studio C++ toolchain is absent. Native parity is not established.
- Both ordinary and isolated desktop smoke runs fail at
  `Desktop pointer resize handle did not transform the stroke`. The cause and
  any relationship to the SDK update are unproved. UI smoke is not passed.

`global.json`, twelve workflows and build documentation now select SDK
`11.0.100-rc.3.26522.148`, already installed locally and listed on the
[official .NET11 download page](https://dotnet.microsoft.com/en-us/download/dotnet/11.0).
Desktop remains net11.0; libraries and server retain net10.0. The server's
standalone global.json continues to select its existing .NET10 SDK.

Local logs, requests and immutable export reports are under
`.artifacts/myscript-port-20261103/`. These local artifacts and vendor exports
are excluded from Git.

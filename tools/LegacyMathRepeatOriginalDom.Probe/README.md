# Repeat original DOM kernel controls

Pure owned-DOM handler/pre-resolution/scalar/subtree-tag controls, no NN/runtime inputs. Run managed with `dotnet run --project tools/LegacyMathRepeatOriginalDom.Probe -c Release`; publish the same project with `-r <RID> -p:PublishAot=true` for native parity. The friend assembly name is shared; keep output directories separate. When only .NET 11 is installed, run the built net10.0 DLL with `dotnet --roll-forward Major`.

The subtree controls cover root versus descendant `-1`, absent mappings/tags,
repeated values, one-pass noncascading replacements, INT64 extremes, untouched
attributes/text, source immutability, malformed/duplicate tags and cancellation.
The expanded probe passes121 controls, including structural scratch compatibility:
empty allocation-free route, prebound paths before edits, missing path/delete,
exact append ordinals, retained parent identities after insertion, detached
subtrees, scalar no-ops, compatible versus executed results and unchanged inputs
on cancellation/work exhaustion.
Unsupported operation codes, oversized paths, foreign anchors and invalid or
attribute-only insertion payloads are also refused without changing the inputs.
Local native evidence can be checked without executing the vendor engine:

```powershell
python -B tools/handwriting/audit_repeat_subtree_tags.py --output .artifacts/subtree-native-pins.json
python -B tools/handwriting/audit_repeat_dom_preflight.py --output .artifacts/preflight-native-pins.json
```

That auditor requires the retained SDK PE and a new output filename. The existing
`check-math-prior.ps1` runs these managed and native controls in CI. These controls
do not demonstrate the original tree mutation, journal, receiver or successor path.

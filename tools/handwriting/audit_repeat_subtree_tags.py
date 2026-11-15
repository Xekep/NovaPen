"""Pin the native root/descendant tag-remapping contracts without vendor execution.

Requires the locally retained engine PE, not Ghidra or a vendor runtime. This
checks native evidence only; managed behavior is covered by the DOM probe.
"""
import argparse
import hashlib
import json
import struct
from pathlib import Path

from export_port_batch import pe_image


def audit(binary):
    data, base, sections = pe_image(binary)
    expected = "d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c"
    if hashlib.sha256(data).hexdigest() != expected:
        raise ValueError("Unknown engine image")

    def read(address, size):
        rva = address - base
        for start, extent, offset in sections:
            if start <= rva and rva + size <= start + extent:
                return data[offset + rva - start:offset + rva - start + size]
        raise ValueError("Address outside a raw PE section")

    regions = []
    for start, end, digest in [
        (0x1800e2d70, 0x1800e3460, "34d3f886d16954a8d9bcce3c8c4a3b8bb99fd65b1d3cdc12dff516de7b3b13b3"),
        (0x1800e56f0, 0x1800e57a4, "01340bcbc53c0181818ebb0871482de1e41d6cde21b9d30295a997ee8cafa991"),
        (0x1800df820, 0x1800dfaa0, "cb80184eaf3861052b0eacf61bb88e5a66526638660ac76f898e562ef7231de9"),
    ]:
        if hashlib.sha256(read(start, end - start)).hexdigest() != digest:
            raise ValueError("Native remapping region changed")
        regions.append(dict(start=hex(start), end=hex(end), sha256=digest))

    # E2D70: only an element clone with nonempty mapping enters the root getter.
    # Missing root value and mapped -1 both jump over V128 to the visitor setup.
    # E56F0: skip a missing old value, but do NOT test the mapped value for -1.
    pins = {
        0x1800e3016: "803b03",                 # clone kind == 3
        0x1800e3045: "4c8b9068010000",         # V168 root INT64 getter
        0x1800e3052: "4883f8ff7474",           # old -1 -> skip root setter
        0x1800e3089: "4883f9ff743d",           # mapped -1 -> skip root setter
        0x1800e30b6: "4c8b9028010000",         # V128 root named setter
        0x1800e3112: "488d05d7250000",         # E56F0 callback
        0x1800e315a: "4c8b90e0010000",         # V1E0 visitor
        0x1800e5722: "e889e9ffff",             # E40B0 attribute INT64 conversion
        0x1800e572a: "4883f8ff7453",           # old -1 -> callback success
        0x1800e575b: "c644242004",             # produced INT64 tag
        0x1800e5763: "488b54c2084889542430",   # mapped value copied verbatim
        0x1800e5773: "4c8b9000010000",         # V100 attribute scalar setter
        0x1800de8d0: "498b4038488902b001c3",   # attribute axis starts at node+38
        0x1800de8e0: "498b4020488902b001c3",   # descendant axis starts at node+20, NOT self
    }
    for address, raw in pins.items():
        if read(address, len(bytes.fromhex(raw))) != bytes.fromhex(raw):
            raise ValueError("Instruction pin changed: " + hex(address))
    for slot, target in [(0x100, 0x1800eb9c0), (0x128, 0x1800eda40),
                         (0x168, 0x1800ee1e0), (0x1e0, 0x1800eaa20)]:
        if struct.unpack("<Q", read(0x180227080 + slot, 8))[0] != target:
            raise ValueError("Writer vtable binding changed")
    if struct.unpack("<Q", read(0x18021ebd0 + 13 * 8, 8))[0] != 0x1800de8e0:
        raise ValueError("Descendant initializer binding changed")
    if struct.unpack("<Q", read(0x18021ec90 + 13 * 8, 8))[0] != 0x1800df820:
        raise ValueError("Descendant step binding changed")
    if read(0x180226498, 4) != b"tag\0" or read(0x1802264a0, 8) != b".//@tag\0":
        raise ValueError("Native tag literals changed")
    return dict(status="passed", vendorExecution=False, binarySha256=expected,
                regions=regions, instructionPins=len(pins), bindings=6,
                scope="STATIC_ROOT_SENTINEL_AND_DESCENDANT_VERBATIM_MAPPING_NOT_ORIGINAL_APPLY")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--binary", type=Path, default=Path(__file__).resolve().parents[2] /
                        "research/myscript/sdk/build/x64/MyScriptEngine.dll")
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    report = audit(args.binary)
    with args.output.open("x", encoding="utf-8") as stream:
        json.dump(report, stream, indent=2)
        stream.write("\n")
    print("PASS native root/descendant tag-remapping pins; no vendor execution")


if __name__ == "__main__":
    main()

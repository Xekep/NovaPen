"""Static pins for the bounded Engine E2870 structural scratch preflight.

No SDK execution or decompiler dependency. General kind4 roots, observers,
allocation failure prefixes and original-document authority remain outside scope.
"""
import argparse
import hashlib
import json
import struct
from pathlib import Path

from export_port_batch import pe_image


def audit(binary):
    data, base, sections = pe_image(binary)
    binary_sha = 'd31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
    if hashlib.sha256(data).hexdigest() != binary_sha:
        raise ValueError('Unknown engine image')

    def read(address, count):
        rva = address - base
        for start, size, offset in sections:
            if start <= rva and rva + count <= start + size:
                return data[offset + rva - start:offset + rva - start + count]
        raise ValueError('Address outside raw PE section')

    start, end = 0x1800e2870, 0x1800e2d70
    digest = '10104e3d56789792d363c1a3afed16392e072aa4de741fc61d41aad16d19dbe7'
    if hashlib.sha256(read(start, end-start)).hexdigest() != digest:
        raise ValueError('Scratch preflight code changed')
    pins = {
        0x1800e2900: '448838',                 # empty route compatible=true
        0x1800e2b38: '48c744dd0000000000',     # null path row
        0x1800e2b41: '4532ff',                 # path miss -> incompatible
        0x1800e2b6d: 'e96effffff',             # path loop before mutation loop
        0x1800e2c00: '742e',                   # operation3/4 dispatch
        0x1800e2c12: '4c8b88e8000000',         # detach VE8, no free90
        0x1800e2c38: '4532ff',                 # missing delete -> incompatible
        0x1800e2c60: '41ffd2',                 # unchecked payload clone call
        0x1800e2ca8: '7513',                   # append ordinal != current count
        0x1800e2cb4: '4c8b90a8000000',         # exact append VA8
        0x1800e2cbd: '4532ff',                 # append mismatch -> incompatible
        0x1800e2cfb: '0fb69c2488000000',       # execution success separate from compatibility
        0x1800e2d5e: '448839',                 # compatibility output after cleanup
    }
    for address, value in pins.items():
        if read(address, len(bytes.fromhex(value))) != bytes.fromhex(value):
            raise ValueError('Instruction changed: ' + hex(address))
    if struct.unpack('<Q', read(0x1802265e8, 8))[0] != start:
        raise ValueError('BE2 preflight binding changed')
    return dict(PASS=True, vendorExecution=False, binarySha256=binary_sha,
                start=hex(start), end=hex(end), regionSha256=digest,
                instructionPins=len(pins), bindings=1,
                scope='STATIC_KIND3_STRUCTURAL_PREFLIGHT_NOT_ORIGINAL_MUTATION')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--binary', type=Path, default=Path(__file__).resolve().parents[2] /
                        'research/myscript/sdk/build/x64/MyScriptEngine.dll')
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    result = audit(args.binary)
    with args.output.open('x', encoding='utf-8') as stream:
        json.dump(result, stream, indent=2)
        stream.write('\n')
    print('PASS Engine E2870 structural preflight pins; no vendor execution')

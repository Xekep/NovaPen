"""Independent object-tree replay of Engine E2870's kind3 structural profile.

Does not import managed code or infer compatibility from expected recognition.
The source/captured operations are data, never mutation authority.
"""
import copy
import json
import sys
from pathlib import Path


def simulate(nodes, root, operations, payloads, original_order=None):
    source = {n['Id']: n for n in nodes}
    payload = {n['Id']: n for n in payloads}

    def ordering(rows):
        result = {n['Id']: dict(Attributes=[], Children=[]) for n in rows}
        for n in rows:
            if n['Parent']:
                result[n['Parent']]['Attributes' if n['Kind'] == 0 else 'Children'].append(n['Id'])
        return result

    oldlinks = ordering(nodes)
    if original_order:
        for row in original_order:
            oldlinks[row['Id']] = {k: row[k][:] for k in ('Attributes', 'Children')}
    plinks = ordering(payloads)
    scratch, links = [], {}

    def clone(rows, order, identity, parent=0):
        value = copy.deepcopy(rows[identity])
        new_id = len(scratch) + 1
        value.update(Id=new_id, Parent=parent, PayloadOrigin=0)
        scratch.append(value)
        links[new_id] = dict(Attributes=[], Children=[])
        if parent:
            links[parent]['Attributes' if value['Kind'] == 0 else 'Children'].append(new_id)
        for kind in ('Attributes', 'Children'):
            for child in order[identity][kind]:
                clone(rows, order, child, new_id)
        return new_id

    if not operations:
        return dict(Completed=True, Compatible=True, BoundPaths=0, VisitedOperations=0,
                    StructuralChanges=0, ScratchRoot=0, Nodes=[], Links=[])
    anchor = clone(source, oldlinks, root)
    parents = []
    compatible = True
    # Freeze all paths before any effects; a missing path prevents ALL effects.
    for op in operations:
        at = anchor
        for ordinal in op['Path']:
            children = links[at]['Children']
            if not 0 <= ordinal < len(children):
                at = None
                break
            at = children[ordinal]
        if at is None:
            compatible = False
            break
        parents.append(at)
    visited = changed = 0
    if compatible:
        for op, parent in zip(operations, parents):
            visited += 1
            if op['Operation'] not in (2, 4):
                continue
            children = links[parent]['Children']
            ordinal = op['Ordinal']
            if op['Operation'] == 4:
                if not 0 <= ordinal < len(children):
                    compatible = False
                    break
                removed = children.pop(ordinal)
                scratch[removed - 1]['Parent'] = 0
                changed += 1
            else:
                inserted = clone(payload, plinks, op['PayloadRoot'])
                if not 0 <= ordinal <= len(children):
                    compatible = False
                    break
                children.insert(ordinal, inserted)
                scratch[inserted - 1]['Parent'] = parent
                changed += 1
    return dict(Completed=True, Compatible=compatible, BoundPaths=len(parents),
                VisitedOperations=visited, StructuralChanges=changed, ScratchRoot=anchor,
                Nodes=scratch, Links=[dict(Id=i, **order) for i, order in links.items()])


def predict(capture):
    cases = []
    for actual in capture['Actual']:
        repeat = actual['Repeat']
        expected = simulate(repeat['OriginalBefore'], repeat['OriginalTree'][0]['Id'],
                            repeat['Dom'], repeat['FourthWriter'], repeat['OriginalTree'])
        cases.append(dict(Case=actual['Case'], Expected=expected))
    return dict(Scope='INDEPENDENT_STRUCTURAL_PREFLIGHT_ONLY', Cases=cases)


def freeze(capture, output):
    prediction = predict(capture)
    with output.open('x', encoding='utf-8') as stream:
        json.dump(prediction, stream, indent=2)
        stream.write('\n')
    print(f"FROZEN {len(prediction['Cases'])} structural scratch predictions from baseline operands")


def require(condition, message):
    if not condition:
        raise ValueError(message)


def validate(capture, frozen):
    # Do not let repeated cases stand in for missing inputs, or let dict
    # construction silently discard duplicate baseline predictions.
    expected_ids = list(range(6))
    require(sorted(case['Case'] for case in frozen['Cases']) == expected_ids,
            'Exactly six distinct frozen cases 0..5 are required')
    require([case['Case'] for case in capture['Actual']] == expected_ids,
            'Actual cases must cover 0..5 once, in source order')
    require([case['Case'] for case in capture['Leases']] == expected_ids,
            'Lease cases must cover 0..5 once, in source order')
    predictions = {case['Case']: case['Expected'] for case in frozen['Cases']}
    changes = 0
    for actual, lease in zip(capture['Actual'], capture['Leases']):
        case = actual['Case']
        expected = predictions[actual['Case']]
        require(lease['DomPreflight'] == expected, f'Scratch replay mismatch in case {case}')
        require(expected['Completed'] and expected['Compatible'], f'Incompatible fixture {case}')
        require(lease['Prepared']['DomPreflightCompleted'], f'Incomplete preflight {case}')
        require(lease['Prepared']['DomCompatible'] == expected['Compatible'], f'Compatibility mismatch {case}')
        require(lease['Prepared']['DomPreflightOperations'] == expected['VisitedOperations'],
                f'Operation count mismatch {case}')
        require(actual['Repeat']['OriginalBefore'] == actual['Repeat']['OriginalAfter'],
                f'Original tree changed in case {case}')
        require(actual['Repeat']['LateBefore'] == actual['Repeat']['LateAfter'],
                f'Late tree changed in case {case}')
        changes += expected['StructuralChanges']
    return changes


def audit(capture, frozen):
    changes = validate(capture, frozen)
    print(json.dumps(dict(PASS=True, cases=6, structuralChanges=changes,
                          scope='Independent scratch node/order/counter replay; no live original mutation')))


def negative_controls(capture):
    frozen = predict(capture)
    validate(capture, frozen)
    mutations = [
        lambda c, f: c['Leases'][0]['DomPreflight'].__setitem__('Compatible', False),
        lambda c, f: c['Leases'][0]['DomPreflight'].__setitem__('StructuralChanges', -1),
        lambda c, f: c['Leases'][0]['DomPreflight']['Nodes'][0].__setitem__('Name', 'corrupted'),
        lambda c, f: c['Leases'][0]['DomPreflight']['Links'][0]['Children'].reverse(),
        lambda c, f: c['Leases'][0]['Prepared'].__setitem__('DomPreflightCompleted', False),
        lambda c, f: c['Leases'][0]['Prepared'].__setitem__('DomPreflightOperations', -1),
        lambda c, f: c['Actual'][0]['Repeat']['OriginalAfter'].clear(),
        lambda c, f: c['Actual'].__setitem__(1, copy.deepcopy(c['Actual'][0])),
        lambda c, f: c['Leases'].__setitem__(1, copy.deepcopy(c['Leases'][0])),
        lambda c, f: (c['Actual'].__setitem__(1, copy.deepcopy(c['Actual'][0])),
                      c['Leases'].__setitem__(1, copy.deepcopy(c['Leases'][0]))),
        lambda c, f: c['Actual'].pop(),
        lambda c, f: f['Cases'].append(copy.deepcopy(f['Cases'][0])),
        lambda c, f: f['Cases'].__setitem__(1, copy.deepcopy(f['Cases'][0])),
    ]
    for index, mutate in enumerate(mutations):
        altered, predictions = copy.deepcopy(capture), copy.deepcopy(frozen)
        mutate(altered, predictions)
        try:
            validate(altered, predictions)
        except ValueError:
            continue
        raise ValueError(f'Corruption control {index} was accepted')
    print(json.dumps(dict(PASS=True, corruptionsRejected=len(mutations))))


if __name__ == '__main__':
    action, source, *targets = sys.argv[1:]
    capture = json.loads(Path(source).read_bytes())
    if action == 'freeze':
        freeze(capture, Path(targets[0]))
    elif action == 'audit':
        audit(capture, json.loads(Path(targets[0]).read_bytes()))
    elif action == 'check':
        audit(capture, predict(capture))
    elif action == 'negative':
        negative_controls(capture)
    else:
        raise SystemExit('freeze baseline.json predictions.json | audit candidate.json predictions.json | check capture.json | negative capture.json')

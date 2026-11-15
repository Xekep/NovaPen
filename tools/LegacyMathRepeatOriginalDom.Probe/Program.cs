using InkRecognition.Math;
int controls=0;
void Check(bool value){if(!value)throw new InvalidDataException($"Prepared DOM control {controls}");controls++;}
void Reject(Action body){try{body();}catch(Exception e)when(e is InvalidOperationException or NotSupportedException or OperationCanceledException){controls++;return;}throw new InvalidDataException("Expected prepared refusal");}
Math2Mode0OriginalArena Arena(params Math2Mode0MathFieldNode[] rows){var a=new Math2Mode0OriginalArena(Math2Mode0RepeatDomViews.PreparedNames());a.Import(rows);return a;}
Math2Mode0OriginalArena Original()=>Arena(new Math2Mode0MathFieldNode(1,3,"mathField",0,1),new(2,3,"A",1,2),new(3,3,"B",1,3),new(4,0,"tag",3,4,4,null,3));
var payload=Arena(new Math2Mode0MathFieldNode(1,3,"X",0,1),new(2,0,"tag",0,2,4,null,5),new(3,0,"absent",0,3,4,null,9));
var original=Original();var oldAttribute=original.Nodes[3];
var ops=new Math2RepeatDomOperand[]{new(2,[],0,payload,1),new(7,[1],0,payload,2),new(4,[],1,payload,0),new(6,[1],0,payload,3)};
var plan=Math2RepeatDomPlan.Resolve(original,original.Nodes[0],ops,new(default));
Check(ReferenceEquals(plan.Parents[1],original.Nodes[2]));Check(ReferenceEquals(plan.Parents[3],original.Nodes[2]));
plan.ApplyNext(new(default));Check(original.Children(1).Select(n=>n.Name.Text).SequenceEqual(new[]{"X","A","B"}));
// Native -1 mapping sentinel preserves OLD payload value rather than writing -1.
var sentinelOriginal=Original();var sentinelAttribute=sentinelOriginal.Nodes[3];
var sentinelPlan=Math2RepeatDomPlan.Resolve(sentinelOriginal,sentinelOriginal.Nodes[0],[new(7,[1],0,payload,2)],new(default));
sentinelPlan.ApplyNext(new(default),new Dictionary<long,long>{{5,-1}});Check(sentinelAttribute.Number==5&&ReferenceEquals(sentinelAttribute,sentinelOriginal.Nodes[3]));
plan.ApplyNext(new(default),new Dictionary<long,long>{{5,6}});Check(ReferenceEquals(oldAttribute,original.Nodes[3]));Check(oldAttribute.Number==6&&oldAttribute.Parent==3);
plan.ApplyNext(new(default));Check(original.Children(1).Select(n=>n.Name.Text).SequenceEqual(new[]{"X","B"}));Check(original.WriterFreed(original.Nodes[1]));Check(!original.WriterFreed(original.Nodes[2]));
plan.ApplyNext(new(default));Check(plan.Completed&&plan.Applied==4);Check(!original.WriterFreed(oldAttribute));Reject(()=>plan.ApplyNext(new(default)));
// Both missing-attribute6 and existing-attribute6 detach without allocator free.
var removal=Math2RepeatDomPlan.Resolve(original,original.Nodes[0],[new(6,[1],0,payload,2)],new(default));removal.ApplyNext(new(default));Check(oldAttribute.Parent==0&&!original.WriterFreed(oldAttribute));
var before=Original();using var canceled=new CancellationTokenSource();canceled.Cancel();
Reject(()=>Math2RepeatDomPlan.Resolve(before,before.Nodes[0],ops,new(canceled.Token)));Check(before.Nodes.Count==4&&before.Children(1).Count==2);
var pending=Math2RepeatDomPlan.Resolve(before,before.Nodes[0],ops,new(default));Reject(()=>pending.ApplyNext(new(canceled.Token)));Check(pending.Applied==0&&before.Nodes.Count==4);
var text=Arena(new Math2Mode0MathFieldNode(1,1,"text",0,1,1,"unsupported"));
Reject(()=>Math2RepeatDomPlan.Resolve(before,before.Nodes[0],[ops[0],new(7,[],0,text,1)],new(default)));Check(before.Nodes.Count==4);
Reject(()=>Math2RepeatDomPlan.Resolve(before,before.Nodes[0],[new(7,[9],0,payload,2)],new(default)));Check(before.Nodes.Count==4);
var exhausted=new Math2DomDiffWork(default){Visits=Math2DomDiffWork.MaximumVisits};Reject(()=>Math2RepeatDomPlan.Resolve(before,before.Nodes[0],ops,exhausted));Check(before.Nodes.Count==4);
// Native EB9C0 preserves tag4/tag7 widths; From enforces canonical widths.
foreach(var test in new (byte Tag,string? Text,long? Number)[]{(4,null,long.MinValue),(4,null,long.MaxValue),(7,null,int.MinValue),(7,null,int.MaxValue),(3,"",null),(2,"123456789012345",null),(1,"1234567890123456",null),(1,new string((char)0x00e9,8),null),(10,null,0)})
{
 var a=Original();var existing=a.Nodes[3];var v=Arena(new Math2Mode0MathFieldNode(1,0,"tag",0,1,test.Tag,test.Text,test.Number));
 var q=Math2RepeatDomPlan.Resolve(a,a.Nodes[0],[new(7,[1],0,v,1)],new(default));q.ApplyNext(new(default));
 Check(ReferenceEquals(existing,a.Nodes[3])&&existing.Tag==test.Tag&&existing.Text==(test.Tag==3?"":test.Text)&&existing.Number==test.Number);
}
foreach(var malformed in new (byte Tag,string? Text,long? Number)[]{(1,"",null),(1,"123456789012345",null),(2,"",null),(2,"1234567890123456",null),(7,null,(long)int.MaxValue+1),(7,null,(long)int.MinValue-1)})
{
 var a=Original();var v=Arena(new Math2Mode0MathFieldNode(1,0,"tag",0,1,malformed.Tag,malformed.Text,malformed.Number));
 Reject(()=>Math2RepeatDomPlan.Resolve(a,a.Nodes[0],[new(7,[1],0,v,1)],new(default)));Check(a.Nodes.Count==4&&a.Nodes[3].Tag==4&&a.Nodes[3].Number==3);
}
// E2D70 root remapping and E56F0 descendant remapping have different -1 rules.
// Expected values below were fixed from those two native branches, independently
// of the managed visitor. Duplicate OLD values must all map once, without cascade.
var subtree=Arena(new Math2Mode0MathFieldNode(1,3,"X",0,1),new(2,0,"tag",1,2,4,null,5),
 new(3,3,"nested",1,3),new(4,0,"tag",3,4,4,null,5),new(5,3,"deep",3,5),new(6,0,"tag",5,6,4,null,7),
 new(7,3,"unmatched",1,7),new(8,0,"tag",7,8,4,null,8),new(9,3,"sentinel",1,9),new(10,0,"tag",9,10,4,null,-1),
 new(11,0,"other",3,11,4,null,5),new(12,1,"text",3,12,2,"tag"));
var sourceBefore=subtree.Snapshot();
foreach(var test in new (Dictionary<long,long> Map,long[] Expected)[]{
 (new(){{5,6},{7,10}},[6,6,10,8,-1]),
 (new(){{5,-1},{7,-1},{-1,42}},[5,-1,-1,8,-1]),
 (new(){{5,7},{7,9}},[7,7,9,8,-1]),
 (new(){{99,100}},[5,5,7,8,-1]),
 (new(){{5,5}},[5,5,7,8,-1]),
 (new(),[5,5,7,8,-1]),
 (new(){{5,long.MaxValue},{7,long.MinValue}},[long.MaxValue,long.MaxValue,long.MinValue,8,-1])})
{
 var a=Original();var q=Math2RepeatDomPlan.Resolve(a,a.Nodes[0],[new(2,[],0,subtree,1)],new(default));q.ApplyNext(new(default),test.Map);
 var inserted=a.Children(1).First();Check(q.Completed&&inserted.Name.Text=="X"&&inserted.Parent==1);
 var tags=a.Nodes.Skip(4).Where(n=>n.Kind==0&&n.Name.Text=="tag").ToArray();
 Check(tags.Select(n=>n.Number!.Value).SequenceEqual(test.Expected));Check(tags.All(n=>n.Tag==4));
 Check(a.Nodes.Skip(4).Single(n=>n.Name.Text=="other").Number==5);
 Check(a.Nodes.Skip(4).Single(n=>n.Kind==1).Text=="tag");
 Check(subtree.Snapshot().SequenceEqual(sourceBefore));
}
// Missing root tag must not suppress the descendant visitor.
var noRootTag=Arena(new Math2Mode0MathFieldNode(1,3,"X",0,1),new(2,3,"nested",1,2),new(3,0,"tag",2,3,4,null,5));
var noRootOriginal=Original();var noRootPlan=Math2RepeatDomPlan.Resolve(noRootOriginal,noRootOriginal.Nodes[0],[new(2,[],0,noRootTag,1)],new(default));
noRootPlan.ApplyNext(new(default),new Dictionary<long,long>{{5,6}});Check(noRootOriginal.Nodes[^1].Number==6);
var rootSentinel=Arena(new Math2Mode0MathFieldNode(1,3,"X",0,1),new(2,0,"tag",1,2,4,null,-1),new(3,3,"nested",1,3),new(4,0,"tag",3,4,4,null,5));
var rootSentinelOriginal=Original();var rootSentinelPlan=Math2RepeatDomPlan.Resolve(rootSentinelOriginal,rootSentinelOriginal.Nodes[0],[new(2,[],0,rootSentinel,1)],new(default));
rootSentinelPlan.ApplyNext(new(default),new Dictionary<long,long>{{-1,42},{5,6}});
Check(rootSentinelOriginal.Nodes.Skip(4).Where(n=>n.Name.Text=="tag").Select(n=>n.Number).SequenceEqual(new long?[]{-1,6}));
// An unported descendant scalar is refused before the clone is attached.
var malformedTag=Arena(new Math2Mode0MathFieldNode(1,3,"X",0,1),new(2,3,"nested",1,2),new(3,0,"tag",2,3,7,null,5));
var badOriginal=Original();var badPlan=Math2RepeatDomPlan.Resolve(badOriginal,badOriginal.Nodes[0],[new(2,[],0,malformedTag,1)],new(default));
var badBefore=badOriginal.Snapshot();Reject(()=>badPlan.ApplyNext(new(default),new Dictionary<long,long>{{5,6}}));
Check(badPlan.Applied==0&&badOriginal.Snapshot().SequenceEqual(badBefore));
var duplicateTags=Arena(new Math2Mode0MathFieldNode(1,3,"X",0,1),new(2,0,"tag",1,2,4,null,5),new(3,0,"tag",1,3,4,null,7));
var duplicatePlan=Math2RepeatDomPlan.Resolve(badOriginal,badOriginal.Nodes[0],[new(2,[],0,duplicateTags,1)],new(default));
Reject(()=>duplicatePlan.ApplyNext(new(default),new Dictionary<long,long>{{5,6}}));Check(duplicatePlan.Applied==0&&badOriginal.Snapshot().SequenceEqual(badBefore));
var canceledOriginal=Original();var canceledPlan=Math2RepeatDomPlan.Resolve(canceledOriginal,canceledOriginal.Nodes[0],[new(2,[],0,subtree,1)],new(default));
Reject(()=>canceledPlan.ApplyNext(new(canceled.Token),new Dictionary<long,long>{{5,6}}));Check(canceledOriginal.Nodes.Count==4&&canceledPlan.Applied==0);
// E2870 is a structural scratch simulation, distinct from E2D70 live scalar application.
var preflightSource=Original();var preflightSourceBefore=preflightSource.Snapshot();var payloadBefore=payload.Snapshot();
var emptyPreflight=Math2RepeatDomPreflight.Run(preflightSource,preflightSource.Nodes[0],[],new(default));
Check(emptyPreflight.Completed&&emptyPreflight.Compatible&&emptyPreflight.Nodes.Length==0&&emptyPreflight.ScratchRoot==0);
var scalarPreflight=Math2RepeatDomPreflight.Run(preflightSource,preflightSource.Nodes[0],[new(7,[1],0,payload,2),new(6,[1],0,payload,2)],new(default));
Check(scalarPreflight.Compatible&&scalarPreflight.StructuralChanges==0&&scalarPreflight.VisitedOperations==2);
Check(scalarPreflight.Nodes.Single(n=>n.Name=="tag").Number==3); // No scalar set/remove in native preflight.
var missingPath=Math2RepeatDomPreflight.Run(preflightSource,preflightSource.Nodes[0],[new(2,[],0,payload,1),new(7,[9],0,payload,2)],new(default));
Check(missingPath.Completed&&!missingPath.Compatible&&missingPath.BoundPaths==1&&missingPath.VisitedOperations==0&&missingPath.StructuralChanges==0&&missingPath.Nodes.Length==4);
foreach(int invalid in new[]{-1,3,99})
{
 var q=Math2RepeatDomPreflight.Run(preflightSource,preflightSource.Nodes[0],[new(2,[],invalid,payload,1)],new(default));
 Check(q.Completed&&!q.Compatible&&q.VisitedOperations==1&&q.StructuralChanges==0&&q.Nodes.Length==5&&q.Nodes[^1].Parent==0);
}
var appendPreflight=Math2RepeatDomPreflight.Run(preflightSource,preflightSource.Nodes[0],[new(2,[],2,payload,1)],new(default));
Check(appendPreflight.Compatible&&appendPreflight.StructuralChanges==1&&appendPreflight.Links[0].Children.SequenceEqual(new[]{2,3,5}));
var missingDelete=Math2RepeatDomPreflight.Run(preflightSource,preflightSource.Nodes[0],[new(4,[],9,payload,0),new(2,[],0,payload,1)],new(default));
Check(!missingDelete.Compatible&&missingDelete.VisitedOperations==1&&missingDelete.StructuralChanges==0&&missingDelete.Nodes.Length==4);
var prefix=Math2RepeatDomPreflight.Run(preflightSource,preflightSource.Nodes[0],[new(2,[],0,payload,1),new(4,[],99,payload,0)],new(default));
Check(!prefix.Compatible&&prefix.VisitedOperations==2&&prefix.StructuralChanges==1&&prefix.Links[0].Children.SequenceEqual(new[]{5,2,3}));
var cachedSource=Arena(new Math2Mode0MathFieldNode(1,3,"field",0,1),new(2,3,"A",1,2),new(3,3,"B",1,3),new(4,3,"nested",3,4));
var cached=Math2RepeatDomPreflight.Run(cachedSource,cachedSource.Nodes[0],[new(2,[],0,payload,1),new(4,[1],0,payload,0)],new(default));
Check(cached.Compatible&&cached.StructuralChanges==2&&cached.Nodes[3].Parent==0&&cached.Links[2].Children.Length==0);
Check(cached.Links[0].Children.SequenceEqual(new[]{5,2,3}));
Reject(()=>Math2RepeatDomPreflight.Run(preflightSource,preflightSource.Nodes[0],[new(2,[],0,payload,1)],new(canceled.Token)));
var preflightExhausted=new Math2DomDiffWork(default){Visits=Math2DomDiffWork.MaximumVisits-3};
Reject(()=>Math2RepeatDomPreflight.Run(preflightSource,preflightSource.Nodes[0],[new(2,[],0,payload,1)],preflightExhausted));
Check(preflightSource.Snapshot().SequenceEqual(preflightSourceBefore)&&payload.Snapshot().SequenceEqual(payloadBefore));
// Unsupported profiles must fail explicitly, without changing either owner.
foreach(byte operation in new byte[]{0,1,3,8,255})
    Reject(()=>Math2RepeatDomPreflight.Run(preflightSource,preflightSource.Nodes[0],[new(operation,[],0,payload,1)],new(default)));
Reject(()=>Math2RepeatDomPreflight.Run(preflightSource,preflightSource.Nodes[0],[new(2,new int[65],0,payload,1)],new(default)));
Reject(()=>Math2RepeatDomPreflight.Run(preflightSource,preflightSource.Nodes[0],[new(2,[],0,payload,0)],new(default)));
Reject(()=>Math2RepeatDomPreflight.Run(preflightSource,preflightSource.Nodes[0],[new(2,[],0,payload,payload.Nodes.Count+1)],new(default)));
Reject(()=>Math2RepeatDomPreflight.Run(preflightSource,preflightSource.Nodes[0],[new(2,[],0,payload,2)],new(default))); // Attribute is not an insertable child.
Reject(()=>Math2RepeatDomPreflight.Run(preflightSource,payload.Nodes[0],[],new(default))); // Foreign anchor, same ID.
var negativePath=Math2RepeatDomPreflight.Run(preflightSource,preflightSource.Nodes[0],[new(4,[-1],0,payload,0)],new(default));
Check(negativePath.Completed&&!negativePath.Compatible&&negativePath.BoundPaths==0&&negativePath.VisitedOperations==0);
Check(preflightSource.Snapshot().SequenceEqual(preflightSourceBefore)&&payload.Snapshot().SequenceEqual(payloadBefore));
Console.WriteLine($"PASS {controls} prepared DOM application/subtree-tag/structural-preflight controls; no original apply authority.");

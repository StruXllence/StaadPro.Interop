using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StaadPro.Interop.Adapters.Interfaces;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Enums;
using StaadPro.Interop.Enums.Output;
using StaadPro.Interop.Models;
using StaadPro.Interop.Models.Output;
namespace PackageVerification
{
    // Exact managed native signatures used by the expanded Output reader; no STAAD activation or mutation.
    public static class OutputAdvancedFixture
    {
        public static void VerifyRatioTasks()
        {
            const string nativeMethod = "GetMemberSteelDesignRatio";
            var native = new RatioTaskNative();
            native.Responses[nativeMethod] = new Queue<object>(new object[] { true, false, true });
            var first = new Beam(17, new Node(1, 0, 0, 0), new Node(2, 2, 0, 0)) { UtilizationRatio = 0.1 };
            var second = new Beam(25, new Node(2, 2, 0, 0), new Node(3, 3, 0, 0)) { UtilizationRatio = 0.2 };
            var beams = new HashSet<Beam> { first, second };
            int[] ids = beams.Select(beam => beam.Id).ToArray();
            using (var wrapper = new OpenStaadWrapper(new Root { Output = native }))
            {
                IOSOutput output = wrapper.Output;
                Task<OutputQueryResult<double>> single = output.GetMemberSteelUtilizationRatioAsync(33);
                if (single.Status != TaskStatus.RanToCompletion || !single.Result.IsSuccessful ||
                    single.Result.EntityId != 33 || single.Result.Data != 0.75 || native.Calls.Count != 1)
                    throw new InvalidOperationException("Installed member-ID ratio task did not produce a settled, identified observation.");

                native.OnCall = operation => beams.Clear();
                Task<OutputBatchResult<double>> batch = output.GetMemberSteelUtilizationRatiosAsync(beams);
                if (batch.Status != TaskStatus.RanToCompletion || batch.Result.Items.Count != 2 ||
                    batch.Result.Items[0].Status != OutputQueryStatus.NativeFailure || batch.Result.Items[1].Data != 0.75 ||
                    !batch.Result.Items.Select(item => item.EntityId).SequenceEqual(ids.Select(id => (int?)id)))
                    throw new InvalidOperationException("Installed ratio batch task did not retain its captured order and item outcomes.");
                if (first.UtilizationRatio != 0.1 || second.UtilizationRatio != 0.2 ||
                    !native.Calls.SequenceEqual(new[] { nativeMethod, nativeMethod, nativeMethod }) ||
                    !native.Arguments.Select(args => (int)args.Single()).SequenceEqual(new[] { 33 }.Concat(ids)) ||
                    native.Threads.Any(thread => thread != Thread.CurrentThread.ManagedThreadId))
                    throw new InvalidOperationException("Installed ratio task calls must run sequentially on the caller thread with the captured IDs and preserve the beam caches.");

                var invalid = output.GetMemberSteelUtilizationRatioAsync(0);
                if (!invalid.IsFaulted || !(invalid.Exception?.InnerException is ArgumentOutOfRangeException argument) ||
                    argument.ParamName != "beamId" || native.Calls.Count != 3)
                    throw new InvalidOperationException("Installed scalar ratio task must validate beamId before native dispatch.");
            }
        }

        public sealed class Root
        {
            public object Output { get; set; }
            public object Geometry => null;
            public object Load => null;
            public int Major = 26;
            public object GetApplicationVersion(ref object major, ref object update, ref object revision, ref object build)
            { major=Major; update=0; revision=0; build=340; return Major + ".0.0.340"; }
        }
        public sealed class RatioTaskNative : Native
        {
            public object GetMemberSteelDesignRatio(int memberId, ref object ratio)
            {
                if (!(ratio is double sentinel) || !double.IsNaN(sentinel))
                    throw new InvalidOperationException("Expected a Double NaN steel ratio sentinel.");
                var response = R(nameof(GetMemberSteelDesignRatio), true, memberId);
                if (WriteOutputs) ratio = S("ratio", 0.75);
                return response;
            }
        }
        public class Native
        {
            public readonly List<string> Calls = new List<string>();
            public readonly List<object[]> Arguments = new List<object[]>();
            public readonly List<int> Threads = new List<int>();
            public readonly Dictionary<string, Queue<object>> Responses = new Dictionary<string, Queue<object>>();
            public readonly Dictionary<string, object> Scalars = new Dictionary<string, object>();
            public readonly Dictionary<string, Array> Arrays = new Dictionary<string, Array>();
            public Action<string> OnCall;
            public Func<string, Array, object> ReplaceArray;
            public Action<int[]> RewriteInputIds;
            public bool WriteOutputs = true;
            public double Interval = 0.25;
            public int SampleCount = 3;
            public object R(string name, object fallback, params object[] args)
            { Calls.Add(name); Arguments.Add(args); Threads.Add(Thread.CurrentThread.ManagedThreadId); OnCall?.Invoke(name); return Responses.TryGetValue(name,out var q)&&q.Count>0?q.Dequeue():fallback; }
            public object S(string role, object fallback) => Scalars.TryGetValue(role,out var value)?value:fallback;
            private void Fill(ref object output, string role)
            {
                if (!(output is double[] a) || a.Any(v=>!double.IsNaN(v))) throw new InvalidOperationException("Expected initialized Double NaN output for " + role);
                if (!WriteOutputs) return;
                if (ReplaceArray != null) { output=ReplaceArray(role,a); if(output is Array replacement)Arrays[role]=replacement; return; }
                for(int i=0;i<a.Length;i++) a[i]=i+1.25;
                Arrays[role]=a;
            }
            private void FillInPlace(object output,string role) { object same=output; Fill(ref same,role); }
            public object GetMemberSteelDesignResults(int memberId,ref object code,ref object status,ref object ratio,ref object allowable,ref object loadCase,
                ref object station,ref object clause,ref object section,ref object actions,ref object slenderness)
            {
                var raw=R(nameof(GetMemberSteelDesignResults),true,memberId);
                if(WriteOutputs){code=S("code","AISC");status=S("designStatus","PASS");ratio=S("ratio",0.75);allowable=S("allowable",1.0);loadCase=S("criticalCase",1);
                    station=S("criticalStation",0.5);clause=S("clause","H1-1a");section=S("section","W12X26");slenderness=S("slenderness",55.0);}
                Fill(ref actions,"designActions");return raw;
            }
            public object GetBucklingFactor(int mode,ref object factor)
            {
                if (!(factor is double sentinel) || !double.IsNaN(sentinel))
                    throw new InvalidOperationException("Expected a Double NaN buckling factor sentinel.");
                var response = R(nameof(GetBucklingFactor), true, mode);
                if (WriteOutputs) factor = S("bucklingFactor", 2.5);
                return response;
            }
            public object GetNoOfBucklingFactors()=>R(nameof(GetNoOfBucklingFactors),3);
            public object GetNoOfModesExtracted()=>R(nameof(GetNoOfModesExtracted),3);
            public object GetSteelDesignParameterBlockCount()=>R(nameof(GetSteelDesignParameterBlockCount),3);
            public object IsMultipleMemberSteelDesignResultsAvailable()=>R(nameof(IsMultipleMemberSteelDesignResultsAvailable),true);
            public object GetBucklingModeDisplacementAtNode(int mode,int node,ref object output){var r=R(nameof(GetBucklingModeDisplacementAtNode),1,mode,node);Fill(ref output,"buckling");return r;}
            public object GetModalDisplacementAtNode(int mode,int node,object output){var r=R(nameof(GetModalDisplacementAtNode),true,mode,node);FillInPlace(output,"modal");return r;}
            public object GetModeFrequency(int mode,ref object value){var r=R(nameof(GetModeFrequency),true,mode);if(WriteOutputs)value=S("frequency",2.5);return r;}
            public object GetModalMassParticipationFactors(int mode,ref object x,ref object y,ref object z){var r=R(nameof(GetModalMassParticipationFactors),true,mode);if(WriteOutputs){x=S("factorX",0.25);y=S("factorY",0.5);z=S("factorZ",0.75);}return r;}
            public object GetNLLoadStep(int loadCase)=>R(nameof(GetNLLoadStep),0.5,loadCase);
            public object GetIntermediateMemberTransDisplacements(int member,double station,int loadCase,ref object values){var r=R(nameof(GetIntermediateMemberTransDisplacements),true,member,station,loadCase);Fill(ref values,"relative");return r;}
            public object GetIntermediateMemberAbsTransDisplacements(int member,double station,int loadCase,ref object values){var r=R(nameof(GetIntermediateMemberAbsTransDisplacements),true,member,station,loadCase);Fill(ref values,"absolute");return r;}
            public object GetPMemberEndForces(int member,int end,int loadCase,ref object values,int axes){var r=R(nameof(GetPMemberEndForces),true,member,end,loadCase,axes);Fill(ref values,"physicalEnd");return r;}
            public object GetIntermediateMemberForcesAtDistance(int member,double station,int loadCase,ref object values){var r=R(nameof(GetIntermediateMemberForcesAtDistance),true,member,station,loadCase);Fill(ref values,"analyticalForces");return r;}
            public object GetPMemberIntermediateForcesAtDistance(int member,double station,int loadCase,ref object values){var r=R(nameof(GetPMemberIntermediateForcesAtDistance),true,member,station,loadCase);Fill(ref values,"physicalForces");return r;}
            public object GetMinMaxAxialForce(int member,int loadCase,ref object min,ref object minPos,ref object max,ref object maxPos){var r=R(nameof(GetMinMaxAxialForce),true,member,loadCase);if(WriteOutputs){min=S("minimum",-5.0);minPos=S("minimumPosition",0.25);max=S("maximum",4.0);maxPos=S("maximumPosition",1.5);}return r;}
            public object GetIntermediateDeflectionAtDistance(int member,double station,int loadCase,ref object y,ref object z){var r=R(nameof(GetIntermediateDeflectionAtDistance),true,member,station,loadCase);if(WriteOutputs){y=S("deflectionY",-0.25);z=S("deflectionZ",0.5);}return r;}
            public object GetAllSolidNormalStresses(int solid,int corner,int loadCase,ref object values){var r=R(nameof(GetAllSolidNormalStresses),true,solid,corner,loadCase);Fill(ref values,"normal");return r;}
            public object GetAllSolidShearStresses(int solid,int corner,int loadCase,ref object values){var r=R(nameof(GetAllSolidShearStresses),true,solid,corner,loadCase);Fill(ref values,"shear");return r;}
            public object GetAllSolidPrincipalStresses(int solid,int corner,int loadCase,ref object values){var r=R(nameof(GetAllSolidPrincipalStresses),true,solid,corner,loadCase);Fill(ref values,"principal");return r;}
            public object GetMatInfluenceAreas(object nodes,object yz,object zx,object xy){var r=R(nameof(GetMatInfluenceAreas),0,nodes);RewriteInputIds?.Invoke((int[])nodes);FillInPlace(yz,"matYZ");FillInPlace(zx,"matZX");FillInPlace(xy,"matXY");return r;}
            public object GetBasePressures(int loadCase,object nodes,object yz,object zx,object xy){var r=R(nameof(GetBasePressures),true,loadCase,nodes);RewriteInputIds?.Invoke((int[])nodes);FillInPlace(yz,"pressureYZ");FillInPlace(zx,"pressureZX");FillInPlace(xy,"pressureXY");return r;}
            public object GetTimeHistoryIntegrationStepInfo(ref double interval){var r=R(nameof(GetTimeHistoryIntegrationStepInfo),SampleCount);if(WriteOutputs)interval=Interval;return r;}
            public object GetTimeHistoryResponseAtTime(int reserved,int node,int dof,int kind,double time,ref double value){var r=R(nameof(GetTimeHistoryResponseAtTime),1,reserved,node,dof,kind,time);if(WriteOutputs)value=(double)S("timeValue",-0.125);return r;}
            public object GetTimeHistoryResponse(int reserved,int node,int dof,int kind,ref object values){var r=R(nameof(GetTimeHistoryResponse),1,reserved,node,dof,kind);Fill(ref values,"history");return r;}
        }
    }
}

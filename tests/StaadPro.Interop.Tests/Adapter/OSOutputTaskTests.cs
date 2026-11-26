using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;
using PackageVerification;
using StaadPro.Interop.Adapters.Interfaces;
using StaadPro.Interop.Adapters.Models;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Enums;
using StaadPro.Interop.Enums.Output;
using StaadPro.Interop.Models;
using StaadPro.Interop.Models.Output;

namespace StaadPro.Interop.Tests.Adapter
{
    [TestFixture, NonParallelizable]
    public sealed class OSOutputTaskTests
    {
        private static Beam Beam(int id=17)=>new Beam(id,new Node(1,0,0,0),new Node(2,2,0,0));
        private static OpenStaadWrapper Wrap(OutputWorkflowFixture.Native native,int version=26)=>
            new OpenStaadWrapper(new OutputWorkflowFixture.Root{Output=native,Major=version});
        private static void Response(OutputWorkflowFixture.Native native,string method,object value)=>
            native.Responses[method]=new Queue<object>(new[]{value});
        private static Task Call(IOSOutput output,bool forces,Beam beam=null)=>forces
            ?(Task)output.GetMemberEndForcesAsync("Case",17,1,MemberEnd.Start)
            :output.GetMemberSteelUtilizationRatioAsync(beam??Beam());
        private sealed class ObservedContext:SynchronizationContext
        {
            public int Posts,Sends;
            public override void Post(SendOrPostCallback callback,object state){Posts++;throw new ApplicationException("Unexpected dispatch.");}
            public override void Send(SendOrPostCallback callback,object state){Sends++;throw new ApplicationException("Unexpected dispatch.");}
        }
        [TestCase(24,MemberEnd.Start)]
        [TestCase(24,MemberEnd.End)]
        [TestCase(25,MemberEnd.Start)]
        [TestCase(25,MemberEnd.End)]
        [TestCase(26,MemberEnd.Start)]
        [TestCase(26,MemberEnd.End)]
        public void ForceTaskPreservesTitleIdentityLocalFrameNativeArgumentsAndOwnedValues(int version,MemberEnd end)
        {
            var native=new OutputWorkflowFixture.Native();using(var wrapper=Wrap(native,version))
            {
                string title="  Case"+Environment.NewLine+"Δ";
                var task=wrapper.Output.GetMemberEndForcesAsync(title,17,41,end);
                Assert.That(task.Status,Is.EqualTo(TaskStatus.RanToCompletion));
                Assert.That(native.Calls,Is.EqualTo(new[]{"GetMemberEndForces"}));
                Assert.That(native.Arguments.Single(),Is.EqualTo(new object[]{17,(int)end,41,0}));
                Assert.That(native.Threads.Single(),Is.EqualTo(Thread.CurrentThread.ManagedThreadId));
                var result=task.GetAwaiter().GetResult();
                Assert.That(result.IsSuccessful,Is.True);Assert.That(result.EntityId,Is.EqualTo(17));
                Assert.That(result.LoadCaseId,Is.EqualTo(41));Assert.That(result.MemberEnd,Is.EqualTo(end));
                Assert.That(result.Data.LoadCaseTitle,Is.EqualTo(title));
                Assert.That(result.Data.MemberId,Is.EqualTo(17));Assert.That(result.Data.CoordinateSystem,Is.EqualTo(ResultCoordinateSystem.Local));
                Assert.That(result.Data.Components.Fx,Is.EqualTo(1.25));Assert.That(result.Data.Components.Mz,Is.EqualTo(6.25));
                Assert.That(result.Observations,Has.Count.EqualTo(1));
                Assert.That(result.NativeBooleanReturn,Is.True);Assert.That(result.NativeReturnCode,Is.Null);
                native.Arrays["endForces"].SetValue(999d,0);
                var editable=result.Data.ToMemberForces();editable.Fx=888;
                Assert.That(result.Data.Components.Fx,Is.EqualTo(1.25));
                Assert.That(editable.LoadCaseTitle,Is.EqualTo(title));
            }
        }
        [TestCase(0d,OutputQueryStatus.Success)]
        [TestCase(0.75d,OutputQueryStatus.Success)]
        [TestCase(double.MaxValue,OutputQueryStatus.Success)]
        [TestCase(-999d,OutputQueryStatus.MemberNotDesigned)]
        [TestCase(-1d,OutputQueryStatus.AnalysisNotPerformed)]
        public void RatioTaskRetainsNativeOutcomeWithoutPopulatingManagedCache(double ratio,OutputQueryStatus expected)
        {
            var native=new OutputWorkflowFixture.Native();native.Scalars["ratio"]=ratio;
            var beam=Beam();beam.UtilizationRatio=0.123;int notifications=0;
            beam.PropertyChanged+=(sender,args)=>notifications++;
            using(var wrapper=Wrap(native))
            {
                var task=wrapper.Output.GetMemberSteelUtilizationRatioAsync(beam);
                Assert.That(task.Status,Is.EqualTo(TaskStatus.RanToCompletion));
                var result=task.GetAwaiter().GetResult();
                Assert.That(result.Status,Is.EqualTo(expected));Assert.That(result.EntityId,Is.EqualTo(17));
                Assert.That(beam.UtilizationRatio,Is.EqualTo(0.123));Assert.That(notifications,Is.Zero);
                Assert.That(native.Calls,Is.EqualTo(new[]{"GetMemberSteelDesignRatio"}));
                Assert.That(native.Arguments.Single(),Is.EqualTo(new object[]{17}));
                Assert.That(native.Threads.Single(),Is.EqualTo(Thread.CurrentThread.ManagedThreadId));
                if(expected==OutputQueryStatus.Success)Assert.That(result.Data,Is.EqualTo(ratio));
                else Assert.Throws<InvalidOperationException>(()=>{var ignored=result.Data;});
            }
        }
        [TestCase(true)]
        [TestCase(false)]
        public void NativeFalseIsACompletedUnsuccessfulObservationNotAFaultedTask(bool forces)
        {
            foreach(object status in new object[]{false,0,(short)0,0L})
            {
                var native=new OutputWorkflowFixture.Native{ReplaceArray=(role,array)=>null};
                native.Scalars["ratio"]="invalid";
                Response(native,forces?"GetMemberEndForces":"GetMemberSteelDesignRatio",status);
                using(var wrapper=Wrap(native))
                {
                    if(forces)
                    {
                        var task=wrapper.Output.GetMemberEndForcesAsync("",17,1,MemberEnd.End);
                        Assert.That(task.Status,Is.EqualTo(TaskStatus.RanToCompletion));
                        var result=task.GetAwaiter().GetResult();Assert.That(result.Status,Is.EqualTo(OutputQueryStatus.NativeFailure));
                        Assert.That(result.EntityId,Is.EqualTo(17));Assert.That(result.MemberEnd,Is.EqualTo(MemberEnd.End));
                        Assert.Throws<InvalidOperationException>(()=>{var ignored=result.Data;});
                    }
                    else
                    {
                        var task=wrapper.Output.GetMemberSteelUtilizationRatioAsync(Beam());
                        Assert.That(task.Status,Is.EqualTo(TaskStatus.RanToCompletion));
                        var result=task.GetAwaiter().GetResult();Assert.That(result.Status,Is.EqualTo(OutputQueryStatus.NativeFailure));
                        Assert.Throws<InvalidOperationException>(()=>{var ignored=result.Data;});
                    }
                }
                Assert.That(native.Calls,Has.Count.EqualTo(1));
            }
        }
        [TestCase(true)]
        [TestCase(false)]
        public void UndocumentedNativeStatusesFaultTheReturnedTaskWithoutRetry(bool forces)
        {
            foreach(object status in new object[]{null,2,1d,1m,"1",uint.MaxValue})
            {
                var native=new OutputWorkflowFixture.Native();
                Response(native,forces?"GetMemberEndForces":"GetMemberSteelDesignRatio",status);
                using(var wrapper=Wrap(native))
                {
                    Task task=null;Assert.DoesNotThrow(()=>task=Call(wrapper.Output,forces));
                    Assert.That(task.Status,Is.EqualTo(TaskStatus.Faulted));
                    Assert.Throws<InvalidOperationException>(()=>task.GetAwaiter().GetResult());
                }
                Assert.That(native.Calls,Has.Count.EqualTo(1));
            }
        }
        [TestCase(true)]
        [TestCase(false)]
        public void ExactIntegerSuccessAndVariantTrueConventionsMatchSynchronousReaders(bool forces)
        {
            foreach(object status in new object[]{true,1,-1,(short)1,1L})
            {
                var native=new OutputWorkflowFixture.Native();
                Response(native,forces?"GetMemberEndForces":"GetMemberSteelDesignRatio",status);
                using(var wrapper=Wrap(native))
                {
                    if(forces)
                    {
                        var result=wrapper.Output.GetMemberEndForcesAsync("Case",17,1,MemberEnd.Start).GetAwaiter().GetResult();
                        Assert.That(result.IsSuccessful,Is.True);
                        Assert.That(result.NativeBooleanReturn,Is.EqualTo(status is bool?(bool?)true:null));
                        Assert.That(result.NativeReturnCode,Is.EqualTo(status is bool?null:(int?)Convert.ToInt32(status)));
                    }
                    else
                    {
                        var result=wrapper.Output.GetMemberSteelUtilizationRatioAsync(Beam()).GetAwaiter().GetResult();
                        Assert.That(result.IsSuccessful,Is.True);
                        Assert.That(result.NativeBooleanReturn,Is.Null);
                        Assert.That(result.NativeReturnCode,Is.Null,"Legacy reader acknowledgement is not invented.");
                    }
                }
            }
        }
        [Test]
        public void InvalidTitleIdsEndAndBeamProduceSettledFaultsBeforeNativeAccess()
        {
            var native=new OutputWorkflowFixture.Native();using(var wrapper=Wrap(native))
            {
                var title=wrapper.Output.GetMemberEndForcesAsync(null,17,1,MemberEnd.Start);
                Assert.That(title.IsFaulted,Is.True);
                Assert.That(Assert.Throws<ArgumentNullException>(()=>title.GetAwaiter().GetResult()).ParamName,Is.EqualTo("loadCaseTitle"));
                Assert.Throws<ArgumentException>(()=>wrapper.Output.GetMemberEndForcesAsync("bad"+(char)0+"label",17,1,MemberEnd.Start).GetAwaiter().GetResult());
                foreach(int id in new[]{0,-1,int.MinValue})
                {
                    Assert.Throws<ArgumentOutOfRangeException>(()=>wrapper.Output.GetMemberEndForcesAsync("Case",id,1,MemberEnd.Start).GetAwaiter().GetResult());
                    Assert.Throws<ArgumentOutOfRangeException>(()=>wrapper.Output.GetMemberEndForcesAsync("Case",17,id,MemberEnd.Start).GetAwaiter().GetResult());
                    Assert.Throws<ArgumentOutOfRangeException>(()=>wrapper.Output.GetMemberSteelUtilizationRatioAsync(Beam(id)).GetAwaiter().GetResult());
                }
                Assert.Throws<ArgumentOutOfRangeException>(()=>wrapper.Output.GetMemberEndForcesAsync("Case",17,1,(MemberEnd)99).GetAwaiter().GetResult());
                var missing=wrapper.Output.GetMemberSteelUtilizationRatioAsync((Beam)null);
                Assert.That(missing.IsFaulted,Is.True);
                Assert.Throws<ArgumentNullException>(()=>missing.GetAwaiter().GetResult());
                Assert.That(native.Calls,Is.Empty);
            }
        }
        [Test]
        public void ForceTaskRequiresFullyWrittenFiniteSixDoubleOutputAndCopiesNonzeroBounds()
        {
            foreach(object value in new object[]{null,new double[5],new double[7],new double[1,6],new float[6],new object[6],
                new[]{0d,0,0,0,0,double.NaN},new[]{0d,0,0,0,0,double.PositiveInfinity}})
            {
                var native=new OutputWorkflowFixture.Native{ReplaceArray=(role,array)=>value};
                using(var wrapper=Wrap(native))
                {
                    var fault=wrapper.Output.GetMemberEndForcesAsync("Case",17,1,MemberEnd.Start);
                    Assert.That(fault.IsFaulted,Is.True);
                    Assert.Throws<InvalidOperationException>(()=>fault.GetAwaiter().GetResult());
                }
            }
            var unwritten=new OutputWorkflowFixture.Native{WriteOutputs=false};
            using(var wrapper=Wrap(unwritten))
                Assert.Throws<InvalidOperationException>(()=>wrapper.Output.GetMemberEndForcesAsync("Case",17,1,MemberEnd.Start).GetAwaiter().GetResult());
            var values=Array.CreateInstance(typeof(double),new[]{6},new[]{-4});
            for(int i=0;i<6;i++)values.SetValue(i-2d,i-4);
            var nativeCopied=new OutputWorkflowFixture.Native{ReplaceArray=(role,array)=>values};
            using(var wrapper=Wrap(nativeCopied))
            {
                var result=wrapper.Output.GetMemberEndForcesAsync("Case",17,1,MemberEnd.Start).GetAwaiter().GetResult();
                values.SetValue(999d,-4);Assert.That(result.Data.Components.Fx,Is.EqualTo(-2));Assert.That(result.Data.Components.Mz,Is.EqualTo(3));
            }
        }
        [Test]
        public void RatioTaskQualifiesWrittenFiniteValuesAndRejectsUndocumentedNegatives()
        {
            foreach(object value in new object[]{null,1,"1",1m,double.NaN,double.PositiveInfinity,double.NegativeInfinity,-2d})
            {
                var native=new OutputWorkflowFixture.Native();native.Scalars["ratio"]=value;
                using(var wrapper=Wrap(native))
                {
                    var fault=wrapper.Output.GetMemberSteelUtilizationRatioAsync(Beam());
                    Assert.That(fault.IsFaulted,Is.True);
                    Assert.Throws<InvalidOperationException>(()=>fault.GetAwaiter().GetResult());
                }
            }
            var unwritten=new OutputWorkflowFixture.Native{WriteOutputs=false};
            using(var wrapper=Wrap(unwritten))
                Assert.Throws<InvalidOperationException>(()=>wrapper.Output.GetMemberSteelUtilizationRatioAsync(Beam()).GetAwaiter().GetResult());
            var single=new OutputWorkflowFixture.Native();single.Scalars["ratio"]=0.75f;
            using(var wrapper=Wrap(single))
                Assert.That(wrapper.Output.GetMemberSteelUtilizationRatioAsync(Beam()).GetAwaiter().GetResult().Data,Is.EqualTo(0.75));
        }
        [Test]
        public void RatioTaskUsesCapturedIdentityRejectsChangesAndIgnoresGeometryAndCache()
        {
            var native=new OutputWorkflowFixture.Native();var beam=Beam();beam.StartNode.X=double.NaN;beam.UtilizationRatio=0.4;
            using(var wrapper=Wrap(native))
            {
                Assert.That(wrapper.Output.GetMemberSteelUtilizationRatioAsync(beam).GetAwaiter().GetResult().Data,Is.EqualTo(0.75));
                native.OnCall=op=>beam.Id=99;
                var fault=wrapper.Output.GetMemberSteelUtilizationRatioAsync(beam);
                Assert.That(fault.IsFaulted,Is.True);
                Assert.Throws<InvalidOperationException>(()=>fault.GetAwaiter().GetResult());
                Assert.That(native.Arguments.Last(),Is.EqualTo(new object[]{17}));
                Assert.That(beam.UtilizationRatio,Is.EqualTo(0.4));
            }
        }
        [TestCase(true)]
        [TestCase(false)]
        public void StaleDisposedAndReplacedDuringQueryBindingsFaultTasks(bool forces)
        {
            var native=new OutputWorkflowFixture.Native();var root=new OutputWorkflowFixture.Root{Output=native};
            var wrapper=new OpenStaadWrapper(root);var stale=wrapper.Output;
            root.Output=new OutputWorkflowFixture.Native();wrapper.RefreshComObjects();
            var fault=Call(stale,forces);Assert.That(fault.IsFaulted,Is.True);
            Assert.Throws<InvalidOperationException>(()=>fault.GetAwaiter().GetResult());Assert.That(native.Calls,Is.Empty);
            var current=wrapper.Output;wrapper.Dispose();
            Assert.Throws<InvalidOperationException>(()=>Call(current,forces).GetAwaiter().GetResult());
            var freshNative=new OutputWorkflowFixture.Native();root.Output=freshNative;
            using(var fresh=new OpenStaadWrapper(root))
            {
                freshNative.OnCall=op=>{root.Output=new OutputWorkflowFixture.Native();fresh.RefreshComObjects();};
                var changed=Call(fresh.Output,forces);Assert.That(changed.IsFaulted,Is.True);
                Assert.Throws<InvalidOperationException>(()=>changed.GetAwaiter().GetResult());
                Assert.That(freshNative.Calls,Has.Count.EqualTo(1));
            }
        }
        [TestCase(true)]
        [TestCase(false)]
        public void OriginalComAndMissingSignatureErrorsFaultWithoutConversionOrFallback(bool forces)
        {
            var native=new OutputWorkflowFixture.Native();var original=new COMException("native",unchecked((int)0x80010108));
            native.OnCall=op=>throw original;
            using(var wrapper=Wrap(native))
            {
                Task task=null;Assert.DoesNotThrow(()=>task=Call(wrapper.Output,forces));
                Assert.That(task.IsFaulted,Is.True);
                Assert.That(Assert.Throws<COMException>(()=>task.GetAwaiter().GetResult()),Is.SameAs(original));
                Assert.That(native.Calls,Has.Count.EqualTo(1));
                native.OnCall=null;Assert.DoesNotThrow(()=>Call(wrapper.Output,forces).GetAwaiter().GetResult());
            }
            using(var missing=new OpenStaadWrapper(new OutputWorkflowFixture.Root{Output=new object()}))
                Assert.Throws<RuntimeBinderException>(()=>Call(missing.Output,forces).GetAwaiter().GetResult());
        }
        [TestCase(true)]
        [TestCase(false)]
        public void TaskShapeDoesNotPostOrSendNativeWorkToAnySynchronizationContext(bool forces)
        {
            var native=new OutputWorkflowFixture.Native();using(var wrapper=Wrap(native))
            {
                var prior=SynchronizationContext.Current;var observed=new ObservedContext();
                try
                {
                    SynchronizationContext.SetSynchronizationContext(observed);
                    var task=Call(wrapper.Output,forces);
                    Assert.That(task.Status,Is.EqualTo(TaskStatus.RanToCompletion));
                    Assert.That(native.Calls,Has.Count.EqualTo(1));
                    Assert.That(native.Threads.Single(),Is.EqualTo(Thread.CurrentThread.ManagedThreadId));
                    task.GetAwaiter().GetResult();
                    Assert.That(observed.Posts,Is.Zero);Assert.That(observed.Sends,Is.Zero);
                }
                finally{SynchronizationContext.SetSynchronizationContext(prior);}
            }
        }
        [Test]
        public void BothTaskContractsShipFullMatchingXmlAndNamedResultShapes()
        {
            var methods=new[]{
                typeof(IOSOutput).GetMethod("GetMemberEndForcesAsync",new[]{typeof(string),typeof(int),typeof(int),typeof(MemberEnd)}),
                typeof(IOSOutput).GetMethod("GetMemberSteelUtilizationRatioAsync",new[]{typeof(Beam)})};
            var returns=new[]{typeof(Task<OutputQueryResult<IdentifiedMemberForces>>),typeof(Task<OutputQueryResult<double>>)};
            var names=new[]{new[]{"loadCaseTitle","memberId","loadCaseId","memberEnd"},new[]{"beam"}};
            var xml=XDocument.Load(Path.ChangeExtension(typeof(IOSOutput).Assembly.Location,".xml"));
            for(int i=0;i<2;i++)
            {
                var method=methods[i];Assert.That(method.ReturnType,Is.EqualTo(returns[i]));
                Assert.That(method.GetParameters().Select(p=>p.Name),Is.EqualTo(names[i]));
                Assert.That(method.GetParameters().Any(p=>p.IsOptional||p.ParameterType.IsByRef),Is.False);
                string suffix="."+method.Name+"("+string.Join(",",method.GetParameters().Select(p=>p.ParameterType.FullName))+")";
                var contract=xml.Descendants("member").Single(e=>(string)e.Attribute("name")=="M:"+typeof(IOSOutput).FullName+suffix);
                var concrete=xml.Descendants("member").Single(e=>(string)e.Attribute("name")=="M:"+typeof(OSOutputAdapter).FullName+suffix);
                Assert.That(string.Concat(contract.Elements()),Is.EqualTo(string.Concat(concrete.Elements())));
                foreach(string tag in new[]{"summary","param","returns","remarks","exception","example"})Assert.That(contract.Element(tag),Is.Not.Null);
                Assert.That(contract.Descendants().Any(e=>e.Name=="include"||e.Name=="inheritdoc"),Is.False);
            }
        }
    }
}

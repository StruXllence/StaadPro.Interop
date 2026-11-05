using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using StaadPro.Interop.Core.Adapters.Interfaces;
using StaadPro.Interop.Core.Adapters.Models;
using StaadPro.Interop.Core.Enums.View;
using StaadPro.Interop.Core.Models;
using StaadPro.Interop.Core.Models.View;
using Native = PackageVerification.ViewFoundationFixture.Native;
using Root = PackageVerification.ViewFoundationFixture.Root;

namespace StaadPro.Interop.Core.Tests.Adapter
{
    [TestFixture]
    public class OSViewIncrementalRotationTests
    {
        private static readonly string[] Names = { "RotateDown", "RotateLeft", "RotateRight", "SpinLeft", "SpinRight" };
        public static IEnumerable<TestCaseData> Methods => Names.Select(n => new TestCaseData(n));
        public static IEnumerable<TestCaseData> FiniteAngles
        {
            get
            {
                foreach (string name in Names)
                    foreach (double angle in new[] { 0.0, -0.0, 15.25, -15.25, 360.0, -720.0, double.Epsilon, double.MaxValue, double.MinValue })
                        yield return new TestCaseData(name, angle);
            }
        }
        public static IEnumerable<TestCaseData> NonfiniteAngles
        {
            get
            {
                foreach (string name in Names)
                    foreach (double angle in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
                        yield return new TestCaseData(name, angle);
            }
        }
        private static ViewOperationResult Invoke(IOSView view, string name, double angle)
        {
            try { return (ViewOperationResult)typeof(IOSView).GetMethod(name).Invoke(view, new object[] { angle }); }
            catch (TargetInvocationException e) { ExceptionDispatchInfo.Capture(e.InnerException).Throw(); throw; }
        }

        [TestCaseSource(nameof(FiniteAngles))]
        public void EachFiniteSignedAngleIsBoxedAsDoubleAndForwardedBitForBit(string name, double angle)
        {
            var native = new Native();
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                var result = Invoke((OSViewAdapter)wrapper.View, name, angle);
                Assert.That(native.Calls, Is.EqualTo(new[] { name }));
                Assert.That(BitConverter.DoubleToInt64Bits(native.RotationAngles.Single()), Is.EqualTo(BitConverter.DoubleToInt64Bits(angle)));
                Assert.That(native.Threads.Single(), Is.EqualTo(Thread.CurrentThread.ManagedThreadId));
                Assert.That(result.Status, Is.EqualTo(ViewOperationStatus.InvokedWithoutAcknowledgement));
                Assert.That(result.IsSuccessful, Is.False);
                Assert.That(result.NativeReturnCode, Is.Null);
            }
        }

        [TestCaseSource(nameof(NonfiniteAngles))]
        public void NonfiniteInputNamesTheOriginalAngleParameterAndNeverDispatches(string name, double angle)
        {
            var native = new Native();
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                var error = Assert.Throws<ArgumentOutOfRangeException>(() => Invoke(wrapper.View, name, angle));
                Assert.That(error.ParamName, Is.EqualTo("angle"));
                Assert.That(native.Calls, Is.Empty);
                Assert.That(native.RotationAngles, Is.Empty);
            }
        }

        [TestCaseSource(nameof(Methods))]
        public void RepeatedRequestsAreNotCollapsedAndNegativeZeroIsNotNormalized(string name)
        {
            var native = new Native();
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                double negativeZero = BitConverter.Int64BitsToDouble(long.MinValue);
                foreach (double angle in new[] { 15.0, -15.0, negativeZero }) Invoke(wrapper.View, name, angle);
                Assert.That(native.Calls, Is.EqualTo(new[] { name, name, name }));
                Assert.That(native.RotationAngles.Take(2), Is.EqualTo(new[] { 15.0, -15.0 }));
                Assert.That(BitConverter.DoubleToInt64Bits(native.RotationAngles.Last()), Is.EqualTo(long.MinValue));
            }
        }

        [TestCaseSource(nameof(Methods))]
        public void ExactMainSourceParameterNameTypeAndDefaultMatchOnBothSurfaces(string name)
        {
            foreach (var owner in new[] { typeof(IOSView), typeof(OSViewAdapter) })
            {
                var method = owner.GetMethod(name);
                var parameters = method.GetParameters();
                Assert.That(parameters.Length, Is.EqualTo(1));
                Assert.That(parameters[0].Name, Is.EqualTo("angle"));
                Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(double)));
                Assert.That(parameters[0].IsOptional, Is.False);
                Assert.That(method.IsGenericMethod, Is.False);
            }
        }

        [Test]
        public void NewRotationsAndExistingOrientationShareOneAdapterGate()
        {
            int active = 0, maximum = 0;
            var native = new Native();
            native.OnCall = () => { int current = Interlocked.Increment(ref active); maximum = Math.Max(maximum, current); Thread.Sleep(15); Interlocked.Decrement(ref active); };
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                var tasks = Names.Select(name => Task.Run(() => Invoke(wrapper.View, name, 1.0))).ToList<Task>();
                tasks.Add(Task.Run(() => wrapper.View.ShowFront()));
                Task.WaitAll(tasks.ToArray());
                Assert.That(maximum, Is.EqualTo(1));
                Assert.That(native.Calls, Is.EquivalentTo(Names.Concat(new[] { "ShowFront" })));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;

namespace PackageVerification
{
    public static class ViewFoundationFixture
    {
        public sealed class Root
        {
            public object View { get; set; }
            public int Major = 26;
            public object GetApplicationVersion(ref object major, ref object update, ref object revision, ref object build)
            { major = Major; update = 0; revision = 0; build = 340; return Major + ".00.00.340"; }
        }
        public sealed class Native
        {
            public readonly List<string> Calls = new List<string>();
            public readonly List<int> Threads = new List<int>();
            public readonly List<double> RotationAngles = new List<double>();
            public readonly List<int> ShownMemberIds = new List<int>();
            public readonly List<int> HiddenMemberIds = new List<int>();
            public readonly List<int> HiddenPlateIds = new List<int>();
            public readonly List<int> HiddenSolidIds = new List<int>();
            public readonly List<int> HiddenSurfaceIds = new List<int>();
            public readonly List<int[]> ShownMemberBatches = new List<int[]>();
            public readonly List<int[]> HiddenMemberBatches = new List<int[]>();
            public Action OnCall;
            public void ShowFront()
            { Record("ShowFront"); }
            public void ShowBack()
            { Record("ShowBack"); }
            public void ShowLeft()
            { Record("ShowLeft"); }
            public void ShowRight()
            { Record("ShowRight"); }
            public void ShowPlan()
            { Record("ShowPlan"); }
            public void ShowBottom()
            { Record("ShowBottom"); }
            public void ShowIsometric()
            { Record("ShowIsometric"); }
            public void RotateUp(object angleDegrees)
            {
                if (!(angleDegrees is double))
                    throw new ArgumentException("RotateUp must receive a boxed Double.", nameof(angleDegrees));
                RotationAngles.Add((double)angleDegrees);
                Record("RotateUp");
            }
            public void RotateDown(object angle) => RecordAngle("RotateDown", angle);
            public void RotateLeft(object angle) => RecordAngle("RotateLeft", angle);
            public void RotateRight(object angle) => RecordAngle("RotateRight", angle);
            public void SpinLeft(object angle) => RecordAngle("SpinLeft", angle);
            public void SpinRight(object angle) => RecordAngle("SpinRight", angle);
            public void ZoomAll() => Record("ZoomAll");
            public void ZoomExtentsMainView() => Record("ZoomExtentsMainView");
            public void ShowAllMembers() => Record("ShowAllMembers");
            public void HideAllMembers() => Record("HideAllMembers");
            public void ShowMember(object memberId)
            {
                if (!(memberId is int))
                    throw new ArgumentException("ShowMember must receive a boxed Int32.", nameof(memberId));
                ShownMemberIds.Add((int)memberId);
                Record("ShowMember");
            }

            public void HideMember(object memberId)
            {
                if (!(memberId is int)) throw new ArgumentException("HideMember requires a boxed Int32.", nameof(memberId));
                HiddenMemberIds.Add((int)memberId);
                Record("HideMember");
            }
            public void ShowMembers(object count, object memberIds) => RecordMembers("ShowMembers", count, memberIds, ShownMemberBatches);
            public void HideMembers(object count, object memberIds) => RecordMembers("HideMembers", count, memberIds, HiddenMemberBatches);

            public void HidePlate(object plateId) => RecordEntity("HidePlate", plateId, HiddenPlateIds);
            public void HideSolid(object solidId) => RecordEntity("HideSolid", solidId, HiddenSolidIds);
            public void HideSurface(object surfaceId) => RecordEntity("HideSurface", surfaceId, HiddenSurfaceIds);
            private void RecordEntity(string operation, object entityId, List<int> ids)
            {
                if (!(entityId is int)) throw new ArgumentException("Native visibility requires a boxed Int32.", nameof(entityId));
                ids.Add((int)entityId);
                Record(operation);
            }
            private void RecordMembers(string operation, object count, object memberIds, List<int[]> batches)
            {
                var ids = memberIds as int[];
                if (!(count is int) || ids == null || (int)count != ids.Length)
                    throw new ArgumentException("Native member lists require a matching boxed Int32 count and Int32 array.");
                if (ids.Rank != 1 || ids.GetLowerBound(0) != 0)
                    throw new ArgumentException("Native member lists must be one-dimensional and zero-based.");
                batches.Add(ids);
                Record(operation);
            }

            private void RecordAngle(string operation, object angle)
            {
                if (!(angle is double)) throw new ArgumentException("Native rotation requires a boxed Double.", nameof(angle));
                RotationAngles.Add((double)angle);
                Record(operation);
            }
            private void Record(string nativeFunction)
            { Calls.Add(nativeFunction); Threads.Add(Thread.CurrentThread.ManagedThreadId); OnCall?.Invoke(); }
        }
    }
}

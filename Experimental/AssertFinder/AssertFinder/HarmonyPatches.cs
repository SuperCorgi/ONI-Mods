using System;
using System.Diagnostics;
using Harmony;

namespace AssertFinder
{
    internal class HarmonyPatches
    {
        [HarmonyPatch(typeof(Debug), "Assert", new Type[] { typeof(bool) })]
        internal static class Debug_Assert_Patch
        {
            internal static void Prefix(bool condition)
            {
                if (!condition)
                {
                    StackTrace stack = new StackTrace();
                    if (stack.FrameCount > 2)
                    {
                        string callName = stack.GetFrame(2).GetMethod().FullDescription();
                        string stackTrace = Environment.StackTrace;
                        Debug.LogWarning($"[AssertFinder] The following method called an Assert that is about to fail:\n{callName}\nStack Trace:\n{stackTrace}");
                    }
                }
            }
        }

    }
}

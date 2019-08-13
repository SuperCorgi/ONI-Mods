using System;
using System.Diagnostics;
using Harmony;

namespace AssertFinder
{
    internal class HarmonyPatches
    {
        [HarmonyPatch(typeof(DebugUtil), "Assert", new Type[] { typeof(bool)})]
        internal static class DebugUtil_Assert_Patch
        {
            internal static void Prefix(bool test)
            {
                if (!test)
                {
                    StackTrace stack = new StackTrace();
                    if(stack.FrameCount > 2)
                    {
                        string callName = stack.GetFrame(2).GetMethod().FullDescription();
                        Debug.LogWarning($"[AssertFinder] The following method called an Assert that is about to fail:\n{callName}");
                    }
                }
            }
        }
    }
}

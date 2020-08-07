using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSerialization;
using UnityEngine;
using System.Runtime.Serialization;
namespace PressurizedPipes
{
    public interface IVersion
    {
        string Version
        {
            get;
            set;
        }
        void OnVersionChange();


    }
    public static class PressurizedModManager
    {
        internal static readonly string[] previousVersions = new string[] { "" };
        internal static readonly string CurrentVersion = "1.2.1";

        internal static void SetVersion(IVersion obj)
        {
            obj.Version = CurrentVersion;
        }

        internal static void VersionChangedCheck(IVersion obj)
        {
            string version = obj.Version;
            if(version != CurrentVersion)
            {
                obj.OnVersionChange();
            }
        }

        


    }
}

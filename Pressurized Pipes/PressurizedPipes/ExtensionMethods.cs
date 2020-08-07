using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Harmony;
namespace PressurizedPipes
{
    public static class ExtensionMethods
    {
        private static readonly FieldInfo maxMass = AccessTools.Field(typeof(ConduitFlow), "MaxMass");

        public static float MaxMass(this ConduitFlow manager)
        {
            if(manager == null)
                throw new System.ArgumentNullException("manager");
            return (float)maxMass.GetValue(manager);
        }
    }
}

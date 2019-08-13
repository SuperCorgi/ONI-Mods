using UnityEngine;

namespace MultiIO
{
    public static class PortIconColors
    {
        public static Color StandardInput = Color.white;
        public static Color StandardOutput = new Color(107f / 255f, 211f / 255f, 132f / 255f);
        public static Color SecondaryColor = new Color(251f / 255f, 176f / 255f, 59 / 255f);
        public static Color Oxygen = Assets.SubstanceTable.GetSubstance(SimHashes.Oxygen).conduitColour;
        public static Color Hydrogen = Assets.SubstanceTable.GetSubstance(SimHashes.Hydrogen).conduitColour;
        public static Color CarbonDioxidie = Assets.SubstanceTable.GetSubstance(SimHashes.CarbonDioxide).conduitColour;
        public static Color NaturalGas = Assets.SubstanceTable.GetSubstance(SimHashes.Methane).conduitColour;
        public static Color ChlorineGas = Assets.SubstanceTable.GetSubstance(SimHashes.ChlorineGas).conduitColour;
        public static Color Water = Assets.SubstanceTable.GetSubstance(SimHashes.Water).conduitColour;
        public static Color Polluted = Assets.SubstanceTable.GetSubstance(SimHashes.DirtyWater).conduitColour;
        public static Color Petroleum = Assets.SubstanceTable.GetSubstance(SimHashes.Petroleum).conduitColour;
        public static Color Coolant = Assets.SubstanceTable.GetSubstance(SimHashes.SuperCoolant).conduitColour;
        public static Color SulfurGas = Assets.SubstanceTable.GetSubstance(SimHashes.SulfurGas).conduitColour;
        public static Color CrudeOil = Assets.SubstanceTable.GetSubstance(SimHashes.CrudeOil).conduitColour;
        public static Color MoltenGlass = Assets.SubstanceTable.GetSubstance(SimHashes.MoltenGlass).conduitColour;
        public static Color Helium = Assets.SubstanceTable.GetSubstance(SimHashes.Helium).conduitColour;
    }

}
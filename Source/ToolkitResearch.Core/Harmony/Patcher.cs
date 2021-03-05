using System.Reflection;
using JetBrains.Annotations;
using Verse;

namespace SirRandoo.ToolkitResearch.Harmony
{
    [UsedImplicitly]
    [StaticConstructorOnStartup]
    public static class Patcher
    {
        static Patcher()
        {
            new HarmonyLib.Harmony("sirrandoo.tkresearch").PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}

using System.Reflection;
using JetBrains.Annotations;
using Verse;

namespace SirRandoo.ToolkitResearch.Compat
{
    [UsedImplicitly]
    [StaticConstructorOnStartup]
    public static class Patcher
    {
        static Patcher()
        {
            new HarmonyLib.Harmony("com.sirrandoo.tkresearch.compat").PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}

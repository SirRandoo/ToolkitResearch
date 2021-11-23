using System.Reflection;
using HarmonyLib;
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
            new Harmony("com.sirrandoo.tkresearch.compat").PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}

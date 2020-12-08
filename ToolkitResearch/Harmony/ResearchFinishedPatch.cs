using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using SirRandoo.ToolkitResearch.Windows;
using Verse;

namespace SirRandoo.ToolkitResearch.Harmony
{
    [HarmonyPatch]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public static class ResearchFinishedPatch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(ResearchManager), nameof(ResearchManager.FinishProject));
        }

        [SuppressMessage("ReSharper", "RedundantAssignment")]
        public static void Prefix(ref bool doCompletionDialog)
        {
            doCompletionDialog = false;
        }

        public static void Postfix()
        {
            ToolkitResearch.StartNewPoll();
        }
    }
}

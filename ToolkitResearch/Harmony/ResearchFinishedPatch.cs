using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
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
            doCompletionDialog = Settings.ShowResearchDialog && Find.WindowStack != null;
        }

        public static void Postfix(ResearchProjectDef proj)
        {
            ToolkitResearch.StartNewPoll(proj);
        }
    }
}

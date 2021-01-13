using System;
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
            yield return AccessTools.Method(typeof(ResearchManager), "FinishProject");
        }

        [SuppressMessage("ReSharper", "RedundantAssignment")]
        public static void Prefix(ref bool doCompletionDialog)
        {
            doCompletionDialog = Settings.ShowResearchDialog && Find.WindowStack != null;
        }

        [HarmonyPriority(800)]
        public static void Postfix(ResearchProjectDef proj)
        {
            try
            {
                ToolkitResearch.StartNewPoll(proj);
            }
            catch (Exception e)
            {
                Log.Error(
                    $"[ToolkitResearch] You shouldn't be seeing this error.\n\nProject null? {proj == null}\nProject name: {proj?.label ?? "Unknown"}\nException type: {e.GetType().Name}\nException message: {e.Message}\nFull stacktrace is as follows:\n{e}"
                );
            }
        }

        public static Exception Cleanup(Exception ex)
        {
            Log.Message($"[ToolkitResearch] Failed to patch ResearchManager.FinishProject! Stacktrace: {ex}");
            return null;
        }

        // TODO: Remove finalizer once this weird crashing is diagnosed.
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static Exception Finalizer(Exception __exception, ResearchProjectDef proj)
        {
            if(__exception != null)
            {
                Log.Error(
                    $"[ToolkitResearch] You shouldn't be seeing this error.\n\nProject null? {proj == null}\nProject name: {proj?.label ?? "Unknown"}\nException type: {__exception?.GetType().Name ?? "None"}\nException message: {__exception?.Message ?? "None"}\nFull stacktrace is as follows:\n{__exception}"
                );
            }

            return null;
        }
    }
}

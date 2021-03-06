﻿using System;
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
        private static bool _tkPollsActive;

        public static bool Prepare()
        {
            _tkPollsActive = ModLister.GetActiveModWithIdentifier("sirrandoo.tkcore.polls") != null;
            return true;
        }

        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(ResearchManager), "FinishProject");
        }

        [SuppressMessage("ReSharper", "RedundantAssignment")]
        public static void Prefix(ref bool doCompletionDialog)
        {
            doCompletionDialog = Settings.ShowResearchDialog && Find.WindowStack != null && !_tkPollsActive;
        }

        [HarmonyPriority(800)]
        public static void Postfix(ResearchProjectDef proj)
        {
            if (Settings._pollsDisabled)
            {
                return;
            }
            
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
            if (ex != null)
            {
                Log.Message($"[ToolkitResearch] Failed to patch ResearchManager.FinishProject! Stacktrace: {ex}");
            }

            return null;
        }
    }
}

// MIT License
// 
// Copyright (c) 2021 SirRandoo
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using SirRandoo.ToolkitPolls;
using SirRandoo.ToolkitResearch.Helpers;
using SirRandoo.ToolkitResearch.Models;
using UnityEngine;
using Verse;

namespace SirRandoo.ToolkitResearch.Compat
{
    [HarmonyPatch]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public static class TkPolls
    {
        private static MethodBase _startNewPollMethod;

        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return _startNewPollMethod ??= AccessTools.Method(typeof(ResearchVoteHandler), "StartNewPoll");
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static bool Prefix([NotNull] Poll poll, [NotNull] ResearchVoteHandler __instance)
        {
            var builder = new PollSetupBuilder();

            foreach (Choice choice in poll.Choices)
            {
                builder.WithChoice(
                    choice.Project.label?.CapitalizeFirst() ?? choice.Project.defName,
                    () =>
                    {
                        Current.Game.researchManager.currentProj = choice.Project;
                        __instance.DiscardPoll();
                    },
                    choice.Project.description
                );
            }

            if (poll.CompletedProject != null)
            {
                builder.WithCoverDelegate(
                    r =>
                    {
                        ResearchProjectDef proj = poll.CompletedProject;
                        SettingsHelper.DrawLabel(
                            r,
                            "ToolkitResearch.Windows.Poll.ResearchComplete".Translate(proj.label),
                            TextAnchor.UpperLeft
                        );
                    }
                );
            }


            __instance.CurrentPoll = new Poll
            {
                Choices = new List<Choice>(), CoverTimer = 99999f, Timer = 99999f, ResultsTimer = 99999f
            };
            builder.WithTitle("ToolkitResearch.Windows.Poll.PollTitle".TranslateSimple(), "#95d0fc");
            ToolkitPolls.ToolkitPolls.SchedulePoll(builder);
            return false;
        }
    }
}

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
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using SirRandoo.ToolkitPolls;
using SirRandoo.ToolkitResearch.Helpers;
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
            yield return _startNewPollMethod ??= AccessTools.Method(typeof(ToolkitResearch), "StartNewPoll");
        }

        public static bool Prefix([CanBeNull] ResearchProjectDef project)
        {
            if (!UnityData.IsInMainThread)
            {
                Log.Message("[ToolkitResearch] Research project finished in another thread!");
                return true;
            }

            var builder = new PollBuilder();
            foreach (ResearchProjectDef proj in DefDatabase<ResearchProjectDef>.AllDefs.Where(p => p.CanStartNow)
               .InRandomOrder()
               .Take(PollSettings.MaxChoices))
            {
                builder.WithChoice(
                    proj.label?.CapitalizeFirst() ?? proj.defName,
                    () => Current.Game.researchManager.currentProj = proj
                );
            }

            if (project != null)
            {
                builder.WithCoverDrawer(
                    r =>
                    {
                        ResearchProjectDef proj = project;
                        SettingsHelper.DrawLabel(
                            r,
                            "ToolkitResearch.Windows.Poll.ResearchComplete".Translate(proj.label)
                        );
                    }
                );
            }

            builder.WithTitle("ToolkitResearch.Windows.Poll.PollTitle".TranslateSimple(), ColorLibrary.LightBlue);
            ToolkitPolls.ToolkitPolls.SchedulePoll(builder.Build());

            return false;
        }
    }
}

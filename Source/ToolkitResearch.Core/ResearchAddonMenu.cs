using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using SirRandoo.ToolkitResearch.Helpers;
using SirRandoo.ToolkitResearch.Windows;
using ToolkitCore.Interfaces;
using Verse;

namespace SirRandoo.ToolkitResearch
{
    [UsedImplicitly]
    [StaticConstructorOnStartup]
    public class ResearchAddonMenu : IAddonMenu
    {
        private static readonly List<FloatMenuOption> Options;

        static ResearchAddonMenu()
        {
            Options = new List<FloatMenuOption>
            {
                new FloatMenuOption(
                    "ToolkitResearch.AddonMenu.Settings".TranslateSimple(),
                    () => Find.WindowStack.Add(new ResearchSettingsWindow())
                ),
                new FloatMenuOption(
                    "ToolkitResearch.AddonMenu.AbandonProject".TranslateSimple(),
                    () =>
                    {
                        Find.ResearchManager.currentProj = null;
                        Current.Game?.GetComponent<ResearchVoteHandler>()?.DiscardPoll();
                    }
                ),
                new FloatMenuOption(
                    "ToolkitResearch.AddonMenu.TogglePollGen".TranslateSimple(),
                    () =>
                    {
                        Settings.PollsDisabled = !Settings.PollsDisabled;

                        Messages.Message(
                            Settings.PollsDisabled
                                ? "ToolkitResearch.Messages.PollGenDisabled".TranslateSimple()
                                : "ToolkitResearch.Messages.PollGenEnabled".TranslateSimple(),
                            MessageTypeDefOf.TaskCompletion
                        );
                    }
                )
            };
        }

        public List<FloatMenuOption> MenuOptions()
        {
            return Options;
        }
    }
}

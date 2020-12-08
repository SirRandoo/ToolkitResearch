using System.Collections.Generic;
using JetBrains.Annotations;
using SirRandoo.ToolkitResearch.Helpers;
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
                    () => SettingsHelper.OpenSettingsMenuFor(LoadedModManager.GetMod<ToolkitResearch>())
                ),
                new FloatMenuOption(
                    "ToolkitResearch.AddonMenu.AbandonProject".TranslateSimple(),
                    () =>
                    {
                        Find.ResearchManager.currentProj = null;
                        ToolkitResearch.StartNewPoll();
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

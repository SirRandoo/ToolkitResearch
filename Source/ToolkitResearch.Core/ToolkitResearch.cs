using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using JetBrains.Annotations;
using RimWorld;
using SirRandoo.ToolkitResearch.ModCompat;
using SirRandoo.ToolkitResearch.Windows;
using UnityEngine;
using Verse;

namespace SirRandoo.ToolkitResearch
{
    [UsedImplicitly]
    public class ToolkitResearch : Mod
    {
        public ToolkitResearch(ModContentPack content) : base(content)
        {
            GetSettings<Settings>();
            Instance = this;
        }
        public static ToolkitResearch Instance { get; private set; }

        [NotNull]
        public override string SettingsCategory()
        {
            return Content.Name;
        }

        public override void WriteSettings()
        {
            Settings.MaximumOptions = Mathf.Clamp(Settings.MaximumOptions, 2, 20);
            Settings.Duration = Mathf.Clamp(Settings.Duration, 60, 600);
            Settings.CompletedDuration = Mathf.Clamp(Settings.CompletedDuration, 0, 600);
            Settings.ResultsDuration = Mathf.Clamp(Settings.ResultsDuration, 0, 600);

            base.WriteSettings();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Find.WindowStack.TryRemove(HugsLib.Active ? HugsLib.SettingsWindow : typeof(Dialog_ModSettings), false);

            Find.WindowStack.Add(new ResearchSettingsWindow());
        }

        [NotNull]
        internal static IEnumerable<ResearchProjectDef> GetNextChoices()
        {
            List<ResearchProjectDef> projects = DefDatabase<ResearchProjectDef>.AllDefs
               .Where(p => !Settings.LimitToTechLevel || (int)p.techLevel <= (int)Find.FactionManager.OfPlayer.def.techLevel)
               .Where(p => !p.IsFinished && p.CanStartNow)
               .ToList();

            if (Settings.TieredMode)
            {
                int lowestUncompleted = projects.Min(r => (int)r.techLevel);
                projects.RemoveAll(r => (int)r.techLevel > lowestUncompleted);
            }

            var container = new List<ResearchProjectDef>();

            while (container.Count < Settings.MaximumOptions)
            {
                if (!projects.TryRandomElementByWeight(CalculateWeight, out ResearchProjectDef project))
                {
                    break;
                }

                container.Add(project);
                projects.Remove(project);
            }

            return container;
        }

        private static float CalculateWeight([NotNull] ResearchProjectDef project)
        {
            float weight = project.modContentPack?.IsCoreMod == true ? 0.1f : 0.15f;

            weight += project.CostFactor(Current.Game.InitData?.playerFaction?.def?.techLevel ?? TechLevel.Undefined);
            weight += project.ProgressPercent * 0.25f;

            return weight;
        }
    }
}

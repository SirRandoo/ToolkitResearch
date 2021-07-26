using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
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
        }

        [NotNull]
        public override string SettingsCategory()
        {
            return Content.Name;
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.Draw(inRect);
        }

        [NotNull]
        internal static IEnumerable<ResearchProjectDef> GetNextChoices()
        {
            List<ResearchProjectDef> projects = DefDatabase<ResearchProjectDef>.AllDefs
               .Where(
                    p => !Settings.LimitToTechLevel
                         || (int) p.techLevel <= (int) Find.FactionManager.OfPlayer.def.techLevel
                )
               .Where(p => !p.IsFinished && p.CanStartNow)
               .ToList();

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

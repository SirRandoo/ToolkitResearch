using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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

        public override string SettingsCategory()
        {
            return nameof(ToolkitResearch);
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.Draw(inRect);
        }
        
        internal static void StartNewPoll()
        {
            Find.WindowStack?.Add(new ResearchPollDialog());
        }
    }
}

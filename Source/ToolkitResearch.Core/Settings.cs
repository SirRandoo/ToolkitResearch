using Verse;

namespace SirRandoo.ToolkitResearch
{
    public class Settings : ModSettings
    {
        public static int MaximumOptions = 4;
        public static int Duration = 280;
        public static bool OptionsInChat;
        public static bool TieredMode;
        public static bool LimitToTechLevel;
        public static int CompletedDuration = 10;
        public static int ResultsDuration = 10;
        internal static bool PollsDisabled = false;
        internal static float PollX;
        internal static float PollY;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref MaximumOptions, "polls.maxOptions", 4);
            Scribe_Values.Look(ref Duration, "polls.duration", 280);
            Scribe_Values.Look(ref ResultsDuration, "polls.resultsDuration", 10);
            Scribe_Values.Look(ref CompletedDuration, "polls.completedDuration", 10);
            Scribe_Values.Look(ref OptionsInChat, "polls.sendToChat");
            Scribe_Values.Look(ref LimitToTechLevel, "behavior.techLevelLimit");
            Scribe_Values.Look(ref TieredMode, "behavior.tiered");
            Scribe_Values.Look(ref PollX, "polls.x");
            Scribe_Values.Look(ref PollY, "polls.y");
        }
    }
}

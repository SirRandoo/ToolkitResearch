using System;
using SirRandoo.ToolkitResearch.Helpers;
using UnityEngine;
using Verse;

namespace SirRandoo.ToolkitResearch
{
    public class Settings : ModSettings
    {
        public static int MaximumOptions = 4;
        private static string _maximumOptionsBuffer = MaximumOptions.ToString();
        public static int Duration = 280;
        private static string _durationBuffer = Duration.ToString();
        public static bool OptionsInChat;
        public static bool ShowResearchDialog;
        public static bool LimitToTechLevel;
        public static int CompletedDuration = 10;
        public static int ResultsDuration = 10;
        private static string _completedDurationBuffer;
        private static string _resultsDurationBuffer;
        internal static bool PollsDisabled = false;
        internal static float PollX;
        internal static float PollY;

        public static void Draw(Rect canvas)
        {
            var listing = new Listing_Standard();

            listing.Begin(canvas);

            (Rect optionsLabel, Rect optionsField) = listing.GetRectAsForm();
            SettingsHelper.DrawLabel(optionsLabel, "ToolkitResearch.Settings.Options.Label".TranslateSimple());
            Widgets.TextFieldNumeric(optionsField, ref MaximumOptions, ref _maximumOptionsBuffer, 2f);
            listing.DrawDescription("ToolkitResearch.Settings.Options.Description".TranslateSimple());

            (Rect durationLabel, Rect durationField) = listing.GetRectAsForm();
            SettingsHelper.DrawLabel(durationLabel, "ToolkitResearch.Settings.Duration.Label".TranslateSimple());
            Widgets.TextFieldNumeric(durationField, ref Duration, ref _durationBuffer, 60);
            listing.DrawDescription("ToolkitResearch.Settings.Duration.Description".TranslateSimple());

            (Rect completedLabel, Rect completedField) = listing.GetRectAsForm();
            SettingsHelper.DrawLabel(
                completedLabel,
                "ToolkitResearch.Settings.CompletedDuration.Label".TranslateSimple()
            );
            Widgets.TextFieldNumeric(completedField, ref CompletedDuration, ref _completedDurationBuffer);
            listing.DrawDescription("ToolkitResearch.Settings.CompletedDuration.Description".TranslateSimple());

            (Rect resultsLabel, Rect resultsField) = listing.GetRectAsForm();
            SettingsHelper.DrawLabel(resultsLabel, "ToolkitResearch.Settings.ResultsDuration.Label".TranslateSimple());
            Widgets.TextFieldNumeric(resultsField, ref ResultsDuration, ref _resultsDurationBuffer);
            listing.DrawDescription("ToolkitResearch.Settings.ResultsDuration.Description".TranslateSimple());

            listing.CheckboxLabeled(
                "ToolkitResearch.Settings.OptionsInChat.Label".TranslateSimple(),
                ref OptionsInChat
            );
            listing.DrawDescription("ToolkitResearch.Settings.OptionsInChat.Description".TranslateSimple());

            listing.CheckboxLabeled(
                "ToolkitResearch.Settings.LimitToTechLevel.Label".TranslateSimple(),
                ref LimitToTechLevel
            );
            listing.DrawDescription("ToolkitResearch.Settings.LimitToTechLevel.Description".TranslateSimple());

            listing.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref MaximumOptions, "polls.maxOptions", 4);
            Scribe_Values.Look(ref Duration, "polls.duration", 280);
            Scribe_Values.Look(ref ResultsDuration, "polls.resultsDuration", 10);
            Scribe_Values.Look(ref CompletedDuration, "polls.completedDuration", 10);
            Scribe_Values.Look(ref OptionsInChat, "polls.sendToChat");
            Scribe_Values.Look(ref LimitToTechLevel, "behavior.techLevelLimit");
            Scribe_Values.Look(ref PollX, "polls.x");
            Scribe_Values.Look(ref PollY, "polls.y");
        }
    }
}

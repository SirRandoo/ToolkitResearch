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
        private static bool OptionsInChat;

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
            
            listing.CheckboxLabeled("ToolkitResearch.Settings.OptionsInChat.Label".TranslateSimple(), ref OptionsInChat);
            listing.DrawDescription("ToolkitResearch.Settings.OptionsInChat.Description".TranslateSimple());
            
            listing.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref MaximumOptions, "polls.maxOptions", 4);
            Scribe_Values.Look(ref Duration, "polls.duration", 280);
        }
    }
}

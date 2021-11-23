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

using System;
using SirRandoo.ToolkitResearch.Helpers;
using UnityEngine;
using Verse;

namespace SirRandoo.ToolkitResearch.Windows
{
    public class ResearchSettingsWindow : FakeSettingsWindow
    {
        private static readonly bool ToolkitPollsActive = ModLister.GetActiveModWithIdentifier("sirrandoo.tkcore.polls") != null;
        private string _completedDurationBuffer = Settings.CompletedDuration.ToString();
        private bool _completedDurationBufferValid = true;
        private string _completedDurationDescription;
        private string _completedDurationLabel;
        private string _durationBuffer = Settings.Duration.ToString();
        private bool _durationBufferValid = true;
        private string _durationDescription;
        private string _durationLabel;
        private string _limitToTechLevelDescription;
        private string _limitToTechLevelLabel;
        private string _limitToTechTierDescription;
        private string _limitToTechTierLabel;
        private string _maximumOptionsBuffer = Settings.MaximumOptions.ToString();
        private bool _maximumOptionsBufferValid = true;
        private string _maximumOptionsDescription;
        private string _maximumOptionsLabel;
        private string _optionsInChatDescription;
        private string _optionsInChatLabel;
        private string _resultsDurationBuffer = Settings.ResultsDuration.ToString();
        private bool _resultsDurationBufferValid = true;
        private string _resultsDurationDescription;
        private string _resultsDurationLabel;

        private string _maximumOptionsError;
        private string _durationError;
        private string _resultsDurationError;
        private string _completedDurationError;
        private string _invalidMaximumOptionsText;
        private string _invalidDurationText;
        private string _invalidResultsDurationText;
        private string _invalidCompletedDurationText;
        private string _toolkitPollsText;
        private string _toolkitPollsOverrideText;

        public ResearchSettingsWindow() : base(ToolkitResearch.Instance)
        {
        }

        protected override void GetTranslations()
        {
            _maximumOptionsLabel = "ToolkitResearch.Settings.Options.Label".TranslateSimple();
            _maximumOptionsDescription = "ToolkitResearch.Settings.Options.Description".TranslateSimple();
            _durationLabel = "ToolkitResearch.Settings.Duration.Label".TranslateSimple();
            _durationDescription = "ToolkitResearch.Settings.Duration.Description".TranslateSimple();
            _completedDurationLabel = "ToolkitResearch.Settings.CompletedDuration.Label".TranslateSimple();
            _completedDurationDescription = "ToolkitResearch.Settings.CompletedDuration.Description".TranslateSimple();
            _resultsDurationLabel = "ToolkitResearch.Settings.ResultsDuration.Label".TranslateSimple();
            _resultsDurationDescription = "ToolkitResearch.Settings.ResultsDuration.Description".TranslateSimple();
            _optionsInChatLabel = "ToolkitResearch.Settings.OptionsInChat.Label".TranslateSimple();
            _optionsInChatDescription = "ToolkitResearch.Settings.OptionsInChat.Description".TranslateSimple();
            _limitToTechLevelLabel = "ToolkitResearch.Settings.LimitToTechLevel.Label".TranslateSimple();
            _limitToTechLevelDescription = "ToolkitResearch.Settings.LimitToTechLevel.Description".TranslateSimple();
            _limitToTechTierLabel = "ToolkitResearch.Settings.LimitToTechTier.Label".TranslateSimple();
            _limitToTechTierDescription = "ToolkitResearch.Settings.LimitToTechTier.Description".TranslateSimple();

            _toolkitPollsText = "ToolkitResearch.Messages.ToolkitPolls".TranslateSimple();
            _toolkitPollsOverrideText = "ToolkitResearch.Messages.Overriden".TranslateSimple();
            _invalidDurationText = "ToolkitResearch.Messages.InvalidDuration".TranslateSimple();
            _invalidMaximumOptionsText = "ToolkitResearch.Messages.InvalidMaximumOptions".TranslateSimple();
            _invalidResultsDurationText = "ToolkitResearch.Messages.InvalidResultsDuration".TranslateSimple();
            _invalidCompletedDurationText = "ToolkitResearch.Messages.InvalidCompletedDuration".TranslateSimple();
        }

        public override void WindowUpdate()
        {
            base.WindowUpdate();

            _durationError = Settings.Duration < 60 || Settings.Duration > 600 ? _invalidDurationText : null;
            _maximumOptionsError = Settings.MaximumOptions < 2 || Settings.MaximumOptions > 20 ? _invalidMaximumOptionsText : null;
            _resultsDurationError = Settings.ResultsDuration < 0 || Settings.ResultsDuration > 600 ? _invalidResultsDurationText : null;
            _completedDurationError = Settings.CompletedDuration < 0 || Settings.CompletedDuration > 600 ? _invalidCompletedDurationText : null;
        }

        protected override void DrawSettings(Rect region)
        {
            var listing = new Listing_Standard();

            listing.Begin(region);

            (Rect optionsLabel, Rect optionsField) = listing.GetRectAsForm();
            SettingsHelper.DrawLabel(optionsLabel, _maximumOptionsLabel);
            listing.DrawDescription(_maximumOptionsDescription);
            DrawMaximumOptionsField(optionsField);

            (Rect durLabel, Rect durationField) = listing.GetRectAsForm();
            SettingsHelper.DrawLabel(durLabel, _durationLabel);
            listing.DrawDescription(_durationDescription);
            DrawDurationField(durationField);

            (Rect completedLabel, Rect completedField) = listing.GetRectAsForm();
            SettingsHelper.DrawLabel(completedLabel, _completedDurationLabel);
            listing.DrawDescription(_completedDurationDescription);
            DrawCompletedDurationField(completedField);

            (Rect resultsLabel, Rect resultsField) = listing.GetRectAsForm();
            SettingsHelper.DrawLabel(resultsLabel, _resultsDurationLabel);
            listing.DrawDescription(_resultsDurationDescription);
            DrawResultDurationField(resultsField);

            if (ToolkitPollsActive)
            {
                (Rect chatLabel, Rect chatField) = listing.GetRectAsForm();
                SettingsHelper.DrawLabel(chatLabel, _optionsInChatLabel);
                SettingsHelper.DrawLabel(chatField, _toolkitPollsOverrideText.ColorTagged(Color.grey).Tagged("i"), TextAnchor.MiddleRight);
            }
            else
            {
                listing.CheckboxLabeled(_optionsInChatLabel, ref Settings.OptionsInChat);
            }

            listing.DrawDescription(_optionsInChatDescription);

            listing.CheckboxLabeled(_limitToTechLevelLabel, ref Settings.LimitToTechLevel);
            listing.DrawDescription(_limitToTechLevelDescription);

            listing.CheckboxLabeled(_limitToTechTierLabel, ref Settings.TieredMode);
            listing.DrawDescription(_limitToTechTierDescription);

            listing.End();
        }

        private void DrawResultDurationField(Rect region)
        {
            if (ToolkitPollsActive)
            {
                SettingsHelper.DrawLabel(region, _toolkitPollsOverrideText.ColorTagged(Color.grey).Tagged("i"), TextAnchor.MiddleRight);
                TooltipHandler.TipRegion(region, _toolkitPollsText);
                return;
            }

            SettingsHelper.DrawAugmentedNumberEntry(region, ref _resultsDurationBuffer, ref Settings.ResultsDuration, ref _resultsDurationBufferValid);

            if (!_resultsDurationError.NullOrEmpty())
            {
                SettingsHelper.DrawFieldButton(region, Widgets.CheckboxOffTex, _resultsDurationError);
            }

            if (!_resultsDurationBufferValid)
            {
                SettingsHelper.DrawFieldButton(region, Widgets.CheckboxOffTex, $"{_resultsDurationBuffer} is not a valid integer");
            }
        }

        private void DrawCompletedDurationField(Rect region)
        {
            if (ToolkitPollsActive)
            {
                SettingsHelper.DrawLabel(region, _toolkitPollsOverrideText.ColorTagged(Color.grey).Tagged("i"), TextAnchor.MiddleRight);
                TooltipHandler.TipRegion(region, _toolkitPollsText);
                return;
            }

            SettingsHelper.DrawAugmentedNumberEntry(region, ref _completedDurationBuffer, ref Settings.CompletedDuration, ref _completedDurationBufferValid);

            if (!_completedDurationError.NullOrEmpty())
            {
                SettingsHelper.DrawFieldButton(region, Widgets.CheckboxOffTex, _completedDurationError);
            }

            if (!_completedDurationBufferValid)
            {
                SettingsHelper.DrawFieldButton(region, Widgets.CheckboxOffTex, $"{_completedDurationBuffer} is not a valid integer");
            }
        }

        private void DrawDurationField(Rect region)
        {
            if (ToolkitPollsActive)
            {
                SettingsHelper.DrawLabel(region, _toolkitPollsOverrideText.ColorTagged(Color.grey).Tagged("i"), TextAnchor.MiddleRight);
                TooltipHandler.TipRegion(region, _toolkitPollsText);
                return;
            }

            SettingsHelper.DrawAugmentedNumberEntry(region, ref _durationBuffer, ref Settings.Duration, ref _durationBufferValid);

            if (!_durationError.NullOrEmpty())
            {
                SettingsHelper.DrawFieldButton(region, Widgets.CheckboxOffTex, _durationError);
            }

            if (!_durationBufferValid)
            {
                SettingsHelper.DrawFieldButton(region, Widgets.CheckboxOffTex, $"{_durationBuffer} is not a valid integer");
            }
        }

        private void DrawMaximumOptionsField(Rect region)
        {
            if (ToolkitPollsActive)
            {
                SettingsHelper.DrawLabel(region, _toolkitPollsOverrideText.ColorTagged(Color.grey).Tagged("i"), TextAnchor.MiddleRight);
                TooltipHandler.TipRegion(region, _toolkitPollsText);
                return;
            }

            SettingsHelper.DrawAugmentedNumberEntry(region, ref _maximumOptionsBuffer, ref Settings.MaximumOptions, ref _maximumOptionsBufferValid);

            if (!_maximumOptionsError.NullOrEmpty())
            {
                SettingsHelper.DrawFieldButton(region, Widgets.CheckboxOffTex, _maximumOptionsError);
            }

            if (!_maximumOptionsBufferValid)
            {
                SettingsHelper.DrawFieldButton(region, Widgets.CheckboxOffTex, $"{_maximumOptionsBuffer} is not a valid integer");
            }
        }
    }
}

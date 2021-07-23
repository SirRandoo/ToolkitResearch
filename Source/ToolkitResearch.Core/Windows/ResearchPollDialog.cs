using System.Linq;
using SirRandoo.ToolkitResearch.Helpers;
using SirRandoo.ToolkitResearch.Models;
using UnityEngine;
using Verse;

namespace SirRandoo.ToolkitResearch.Windows
{
    [StaticConstructorOnStartup]
    public class ResearchPollDialog : Window
    {
        private static readonly Gradient TimerGradient;
        private string _completeTitleText;
        private string _pollTitleText;
        private string _resultsTitleText;
        private ResearchVoteHandler _voteHandler;

        static ResearchPollDialog()
        {
            TimerGradient = new Gradient();

            var colorKey = new GradientColorKey[2];
            colorKey[0].color = Color.green;
            colorKey[0].time = 0.0f;
            colorKey[1].color = Color.red;
            colorKey[1].time = 1.0f;

            var alphaKey = new GradientAlphaKey[2];
            alphaKey[0].alpha = 1.0f;
            alphaKey[0].time = 0.0f;
            alphaKey[1].alpha = 1.0f;
            alphaKey[1].time = 1.0f;

            TimerGradient.SetKeys(colorKey, alphaKey);
        }

        public ResearchPollDialog()
        {
            GetTranslations();
            doCloseX = true;
            draggable = true;
            closeOnCancel = false;
            closeOnAccept = false;
            focusWhenOpened = false;
            closeOnClickedOutside = false;
            absorbInputAroundWindow = false;
            preventCameraMotion = false;
        }

        public override Vector2 InitialSize =>
            new Vector2(310, 150 + Text.SmallFontHeight * Mathf.Min(Settings.MaximumOptions + 1, 8));

        private void GetTranslations()
        {
            _pollTitleText = "ToolkitResearch.Windows.Poll.PollTitle".TranslateSimple()
               .ColorTagged(ColorLibrary.LightBlue)
               .Tagged("b");
            _resultsTitleText = "ToolkitResearch.Windows.Poll.ResultsTitle".TranslateSimple()
               .ColorTagged(ColorLibrary.LightBlue)
               .Tagged("b");
            _completeTitleText = "ToolkitResearch.Windows.Poll.CompleteTitle".TranslateSimple()
               .ColorTagged(ColorLibrary.LightBlue)
               .Tagged("b");
        }

        public override void PreOpen()
        {
            base.PreOpen();

            _voteHandler = Current.Game.GetComponent<ResearchVoteHandler>();

            if (_voteHandler != null)
            {
                if (_voteHandler.CurrentPoll.State == PollState.None)
                {
                    _voteHandler.CurrentPoll.Transition();
                }

                return;
            }

            Log.Error("[ToolkitResearch] Could not get vote handler!".ColorTagged(ColorLibrary.Salmon));
            Close();
        }

        public override void DoWindowContents(Rect region)
        {
            if (Event.current.type == EventType.Layout)
            {
                return;
            }

            GUI.BeginGroup(region);

            var pollRect = new Rect(0f, 0f, region.width, region.height - Text.LineHeight);
            var timerRect = new Rect(0f, pollRect.y + pollRect.height, region.width, Text.LineHeight);

            GUI.BeginGroup(pollRect);
            switch (_voteHandler.CurrentPoll.State)
            {
                case PollState.Cover:
                    optionalTitle = _completeTitleText;
                    _voteHandler.CurrentPoll.DrawCover(pollRect);
                    break;
                case PollState.Poll:
                    optionalTitle = _pollTitleText;
                    _voteHandler.CurrentPoll.DrawPoll(pollRect);
                    break;
                case PollState.Results:
                    optionalTitle = _resultsTitleText;
                    _voteHandler.CurrentPoll.DrawResults(pollRect);
                    break;
                case PollState.Done:
                    Close();
                    break;
            }

            GUI.EndGroup();

            GUI.BeginGroup(timerRect);
            DrawTimer(timerRect.AtZero());
            GUI.EndGroup();

            GUI.EndGroup();
        }

        private void DrawTimer(Rect region)
        {
            var progress = 0f;

            switch (_voteHandler.CurrentPoll.State)
            {
                case PollState.Cover:
                    progress = Mathf.Clamp(_voteHandler.CurrentPoll.CoverTimer / Settings.CompletedDuration, 0f, 100f);
                    break;
                case PollState.Poll:
                    progress = Mathf.Clamp(_voteHandler.CurrentPoll.Timer / Settings.Duration, 0f, 100f);
                    break;
                case PollState.Results:
                    progress = Mathf.Clamp(_voteHandler.CurrentPoll.ResultsTimer / Settings.ResultsDuration, 0f, 100f);
                    break;
            }

            GUI.color = TimerGradient.Evaluate(1f - progress);
            Widgets.FillableBar(region, progress, Texture2D.whiteTexture, null, true);
            GUI.color = Color.white;
        }

        protected override void SetInitialSizeAndPosition()
        {
            float defaultY = UI.screenHeight / 5f - InitialSize.y / 5f;
            float height = _voteHandler?.CurrentPoll.Choices.Count * Text.SmallFontHeight + 1f ?? InitialSize.y;
            float width = (_voteHandler?.CurrentPoll.Choices.Max(c => c.LabelWidth) ?? InitialSize.x) + 20f;

            windowRect = new Rect(
                Mathf.Clamp(Settings.PollX, 0f, UI.screenWidth - width),
                Settings.PollY <= 0 ? defaultY : Mathf.Clamp(Settings.PollY, 0f, UI.screenHeight - height),
                width,
                height
            ).Rounded();
        }

        public override void Close(bool doCloseSound = true)
        {
            base.Close(doCloseSound);

            _voteHandler.CurrentPoll.Timer = -10f;
            _voteHandler.CurrentPoll.CoverTimer = -10f;
            _voteHandler.CurrentPoll.ResultsTimer = -10f;

            while (_voteHandler.CurrentPoll.State != PollState.Results)
            {
                _voteHandler.CurrentPoll.Transition();
            }
        }

        public override void PostClose()
        {
            base.PostClose();

            Settings.PollX = windowRect.x;
            Settings.PollY = windowRect.y;
            LoadedModManager.GetMod<ToolkitResearch>()?.WriteSettings();
        }
    }
}

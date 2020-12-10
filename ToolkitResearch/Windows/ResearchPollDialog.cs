using System;
using System.Collections.Generic;
using System.Linq;
using SirRandoo.ToolkitResearch.Helpers;
using SirRandoo.ToolkitResearch.Models;
using ToolkitCore;
using UnityEngine;
using Verse;

namespace SirRandoo.ToolkitResearch.Windows
{
    internal enum WindowState { Layout, Research, Poll, Results }

    [StaticConstructorOnStartup]
    public class ResearchPollDialog : Window
    {
        private static readonly Gradient TimerGradient;
        private readonly List<PollItem> _choices = new List<PollItem>();
        private Rect _completedRect;
        private string _completeText;
        private string _completeTitleText;
        private string _loadingText;
        private int _marker;
        private bool _notified;
        private Rect _pollRect;
        private string _pollTitleText;
        private IEnumerator<ResearchProjectDef> _projectIndexer;
        private string _resultsTitleText;
        private Vector2 _scrollPos;
        private WindowState _state = WindowState.Layout;
        private float _timer = 1;
        private Rect _timerRect;
        private float _totalVotes;
        private Rect _viewPort;
        private readonly bool _wasProjectCompleted;

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

        public ResearchPollDialog(Def project = null)
        {
            GetTranslations(project);
            _wasProjectCompleted = project != null;
            optionalTitle = _loadingText;

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
            new Vector2(290, 150 + Text.SmallFontHeight * Mathf.Min(Settings.MaximumOptions + 1, 8));

        private void GetTranslations(Def project = null)
        {
            _pollTitleText = "ToolkitResearch.Windows.Poll.PollTitle".TranslateSimple();
            _resultsTitleText = "ToolkitResearch.Windows.Poll.ResultsTitle".TranslateSimple();
            _completeTitleText = "ToolkitResearch.Windows.Poll.CompleteTitle".TranslateSimple();
            _loadingText = "ToolkitResearch.Windows.Poll.LoadingNextOptions".TranslateSimple();

            _completeText = project != null
                ? (string) "ToolkitResearch.Windows.Poll.ResearchComplete".Translate(project.label)
                : "";
        }

        public override void PreOpen()
        {
            base.PreOpen();

            var voteHandler = Current.Game.GetComponent<ResearchVoteHandler>();

            if (voteHandler == null)
            {
                Log.Error("[ToolkitResearch] Could not get vote handler!".ColorTagged(ColorLibrary.Salmon));
                Close();
                return;
            }

            voteHandler.RegisterPoll(this);
            _projectIndexer = GetProjects().GetEnumerator();
        }

        public override void DoWindowContents(Rect canvas)
        {
            if (Event.current.type == EventType.Layout)
            {
                if (_state == WindowState.Layout)
                {
                    CalculateRegions(canvas);
                }

                return;
            }

            GUI.BeginGroup(canvas);

            switch (_state)
            {
                case WindowState.Layout: // Indexing poll options OR timer is stuck
                    SettingsHelper.DrawLabel(canvas, _loadingText);
                    break;
                case WindowState.Research:
                    DrawCompletedResearch();
                    DrawTimer();
                    break;
                case WindowState.Poll:
                    DrawResearchPoll();
                    DrawTimer();
                    break;
                case WindowState.Results:
                    DrawResearchPoll(true);
                    DrawTimer();
                    break;
            }

            GUI.EndGroup();
        }

        private void DrawResearchPoll(bool drawWinner = false)
        {
            var listing = new Listing_Standard();
            listing.Begin(_pollRect);
            listing.BeginScrollView(_pollRect, ref _scrollPos, ref _viewPort);
            for (var index = 0; index < _choices.Count; index++)
            {
                PollItem choice = _choices[index];
                Rect line = listing.GetRect(Text.LineHeight);

                float chance = choice.VoteCount > 0 ? choice.VoteCount / _totalVotes : 0f;
                bool winner = drawWinner && choice.Project == Find.ResearchManager?.currentProj;
                Rect progressRect = new Rect(line.x, line.y, line.width * (winner ? 1f : chance), line.height * 0.95f)
                   .Rounded();

                if (winner)
                {
                    Widgets.DrawHighlightSelected(progressRect);
                }
                else
                {
                    Widgets.DrawLightHighlight(progressRect);
                }

                SettingsHelper.DrawLabel(line, $"[{choice.IdLabel}] {choice.Project!.LabelCap}: {choice.VoteCountLabel}");
            }

            listing.End();
            listing.EndScrollView(ref _viewPort);
        }

        private void DrawTimer()
        {
            if (_timer <= 0)
            {
                return;
            }

            GUI.BeginGroup(_timerRect);
            var progress = 0f;

            if (_timer > 0)
            {
                switch (_state)
                {
                    case WindowState.Research:
                        progress = _timer / Settings.CompletedDuration;
                        break;
                    case WindowState.Poll:
                        progress = _timer / Settings.Duration;
                        break;
                    case WindowState.Results:
                        progress = _timer / Settings.ResultsDuration;
                        break;
                }
            }

            GUI.color = TimerGradient.Evaluate(1f - progress);
            Widgets.FillableBar(_timerRect.AtZero(), progress, Texture2D.whiteTexture, null, true);
            GUI.color = Color.white;

            GUI.EndGroup();
        }

        private void DrawCompletedResearch()
        {
            GUI.BeginGroup(_completedRect);
            SettingsHelper.DrawLabel(_completedRect, _completeText, TextAnchor.UpperLeft);
            GUI.EndGroup();
        }

        public override void WindowUpdate()
        {
            base.WindowUpdate();

            if (_projectIndexer != null) // TODO: This will block the timer from updating while it's in effect.
            {
                try
                {
                    AdvanceIndexer();
                }
                catch (Exception)
                {
                    _projectIndexer = null;
                }

                if (_projectIndexer == null)
                {
                    Notify_AllChoicesGathered();
                }

                return;
            }

            int currentTime = Mathf.FloorToInt(Time.unscaledTime);
            if (currentTime <= _marker)
            {
                return;
            }

            _marker = currentTime;
            _timer -= 1;

            if (_timer <= 0)
            {
                UpdateState();
            }
        }

        private void UpdateState()
        {
            switch (_state)
            {
                case WindowState.Layout:
                    optionalTitle = _completeTitleText.ColorTagged(ColorLibrary.LightGreen).Tagged("b");
                    _state = WindowState.Research;
                    _timer = Settings.CompletedDuration;

                    if (!_wasProjectCompleted)
                    {
                        // ReSharper disable once TailRecursiveCall
                        UpdateState();
                    }

                    return;
                case WindowState.Research:
                    optionalTitle = _pollTitleText.ColorTagged(ColorLibrary.LightGreen).Tagged("b");
                    _state = WindowState.Poll;
                    _timer = Settings.Duration;

                #if DEBUG
                    _timer = 10;
                #endif

                    return;
                case WindowState.Poll:
                    ChooseWinner();
                    optionalTitle = _resultsTitleText.ColorTagged(ColorLibrary.LightGreen).Tagged("b");
                    _state = WindowState.Results;
                    _timer = Settings.ResultsDuration;
                    return;
                case WindowState.Results:
                    Close();
                    return;
            }
        }

        private void CalculateRegions(Rect canvas)
        {
            _completedRect = new Rect(0f, 0f, canvas.width, canvas.height - Text.SmallFontHeight);
            _pollRect = _completedRect;
            _viewPort = new Rect(0f, 0f, _completedRect.width - 16f, _choices.Count * Text.SmallFontHeight);
            _timerRect = new Rect(0f, canvas.height - Text.SmallFontHeight, canvas.width, Text.SmallFontHeight);
        }

        private void Notify_AllChoicesGathered()
        {
            if (!Settings.OptionsInChat || _notified || TwitchWrapper.Client == null)
            {
                return;
            }

            TwitchWrapper.SendChatMessage(_pollTitleText);

            for (var index = 0; index < _choices.Count; index++)
            {
                PollItem choice = _choices[index];
                TwitchWrapper.SendChatMessage($"[{index + 1}] {choice.Project.LabelCap}");
            }

            _notified = true;
        }

        private void AdvanceIndexer()
        {
            for (var j = 0; j < 10; j++)
            {
                if (!_projectIndexer.MoveNext())
                {
                    _projectIndexer = null;
                    break;
                }

                ResearchProjectDef project = _projectIndexer.Current;

                if (project == null)
                {
                    continue;
                }

                if (_choices.Count < Settings.MaximumOptions)
                {
                    _choices.Add(new PollItem(_choices.Count + 1) {Project = project});
                }
                else
                {
                    _projectIndexer = null;
                    break;
                }
            }
        }

        public override void PreClose()
        {
            if (!_choices.NullOrEmpty() && Find.ResearchManager?.currentProj == null)
            {
                ChooseWinner();
            }

            base.PreClose();
        }

        private void ChooseWinner()
        {
            int winningVotes = _choices.Max(i => i.Voters.Count);
            PollItem winner = _choices.Where(i => i.Voters.Count == winningVotes).RandomElement();
            Find.ResearchManager.currentProj = winner?.Project;
        }

        protected override void SetInitialSizeAndPosition()
        {
            windowRect = new Rect(
                UI.screenWidth - InitialSize.x,
                UI.screenHeight / 2f - InitialSize.y / 2f,
                InitialSize.x,
                InitialSize.y
            );
        }

        public void ProcessVote(string username, int vote)
        {
            PollItem item = _choices.FirstOrDefault(c => c.Id == vote);

            if (item == null)
            {
                return;
            }

            foreach (PollItem poll in _choices)
            {
                poll.Voters.Remove(username);
            }

            item.Voters.Add(username);
            Notify_ChoicesDirty();
        }

        private void Notify_ChoicesDirty()
        {
            _totalVotes = _choices.Sum(c => c.VoteCount);
        }

        public bool IsProcessingVotes()
        {
            return _projectIndexer == null && _timer > 0;
        }

        private static IEnumerable<ResearchProjectDef> GetProjects()
        {
            IEnumerable<ResearchProjectDef> projects = DefDatabase<ResearchProjectDef>.AllDefs.InRandomOrder();

            foreach (ResearchProjectDef project in projects)
            {
                if (!project.CanStartNow)
                {
                    continue;
                }

                yield return project;
            }
        }
    }
}

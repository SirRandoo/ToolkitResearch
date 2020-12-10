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
    [StaticConstructorOnStartup]
    public class ResearchPollDialog : Window
    {
        private static readonly Gradient TimerGradient;
        private readonly List<PollItem> _choices = new List<PollItem>();
        private string _loading;
        private int _marker;
        private bool _notified;
        private IEnumerator<ResearchProjectDef> _projectIndexer;
        private Vector2 _scrollPos;
        private float _timer = Settings.Duration;
        private string _title;

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

            optionalTitle = _title;

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
            new Vector2(325, 150 + Text.SmallFontHeight * Mathf.Min(Settings.MaximumOptions + 1, 8));

        private void GetTranslations()
        {
            _title = "ToolkitResearch.Windows.Poll.Title".TranslateSimple();
            _loading = "ToolkitResearch.Windows.Poll.Loading".TranslateSimple();
        }

        public override void PreOpen()
        {
            base.PreOpen();

            Current.Game.GetComponent<ResearchVoteHandler>()?.RegisterPoll(this);
            _projectIndexer = GetProjects().GetEnumerator();
        }

        public override void DoWindowContents(Rect inRect)
        {
            GUI.BeginGroup(inRect);

            var listing = new Listing_Standard();
            var contentRect = new Rect(0f, 0f, inRect.width, inRect.height - Text.SmallFontHeight);
            var viewPort = new Rect(0f, 0f, inRect.width - 16f, _choices.Count * Text.SmallFontHeight);

            listing.Begin(inRect);
            listing.BeginScrollView(contentRect, ref _scrollPos, ref viewPort);

            if (_projectIndexer == null)
            {
                for (var index = 0; index < _choices.Count; index++)
                {
                    PollItem choice = _choices[index];
                    Rect line = listing.GetRect(Text.LineHeight);

                    if (index % 2 == 1)
                    {
                        Widgets.DrawLightHighlight(line);
                    }

                    SettingsHelper.DrawLabel(line, $"[{index + 1}] {choice.Project.LabelCap}: {choice.Voters.Count:N0}");
                }
            }
            else
            {
                SettingsHelper.DrawLabel(contentRect, _loading, TextAnchor.MiddleCenter);
            }

            listing.End();
            listing.EndScrollView(ref viewPort);
            var footerRect = new Rect(0f, inRect.height - Text.LineHeight, inRect.width, Text.LineHeight);

            float progress = _timer / Settings.Duration;
            GUI.color = TimerGradient.Evaluate(1f - progress);
            Widgets.FillableBar(footerRect, progress, Texture2D.whiteTexture, null, true);
            GUI.color = Color.white;

            GUI.EndGroup();
        }

        public override void WindowUpdate()
        {
            base.WindowUpdate();

            if (_projectIndexer != null)
            {
                try
                {
                    AdvanceIndexer();
                }
                catch (Exception)
                {
                    _projectIndexer = null;
                }

                Notify_ChoicesComplete();
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
                Close();
            }
        }

        private void Notify_ChoicesComplete()
        {
            if (_notified || TwitchWrapper.Client == null)
            {
                return;
            }

            TwitchWrapper.SendChatMessage(_title);

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
                    _choices.Add(new PollItem {Project = project});
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
            if (_choices.NullOrEmpty())
            {
                base.PreClose();
                return;
            }
            
            int winningVotes = _choices.Max(i => i.Voters.Count);
            PollItem winner = _choices.Where(i => i.Voters.Count == winningVotes).RandomElement();
            Find.ResearchManager.currentProj = winner?.Project;

            base.PreClose();
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
            try
            {
                PollItem item = _choices[vote - 1];
                
                foreach (PollItem poll in _choices)
                {
                    poll.Voters.Remove(username);
                }
                
                item.Voters.Add(username);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public bool IsProcessingVotes()
        {
            return _projectIndexer == null;
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

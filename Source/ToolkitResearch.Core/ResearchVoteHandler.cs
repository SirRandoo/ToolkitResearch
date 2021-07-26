using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SirRandoo.ToolkitResearch.Models;
using SirRandoo.ToolkitResearch.Windows;
using ToolkitCore;
using TwitchLib.Client.Models.Interfaces;
using UnityEngine;
using Verse;

namespace SirRandoo.ToolkitResearch
{
    [UsedImplicitly]
    public class ResearchVoteHandler : TwitchInterfaceBase
    {
        private readonly ConcurrentQueue<Vote> _votes = new ConcurrentQueue<Vote>();
        private ResearchProjectDef _lastProject;
        private float _marker;

        public ResearchVoteHandler(Game game) { }

        public Poll CurrentPoll { get; set; }

        public override void ParseMessage(ITwitchMessage twitchMessage)
        {
            if (CurrentPoll == null || CurrentPoll.Choices.Count <= 0)
            {
                return;
            }

            string message = twitchMessage.Message;

            if (message.StartsWith("#"))
            {
                message = message.Substring(1);
            }

            if (!int.TryParse(message, out int vote))
            {
                return;
            }

            _votes.Enqueue(new Vote {Viewer = twitchMessage.Username.ToLowerInvariant(), Index = vote});
        }

        public void DiscardPoll()
        {
            CurrentPoll = null;
        }

        public override void GameComponentTick()
        {
            if (Find.ResearchManager.currentProj != null || CurrentPoll != null)
            {
                return;
            }

            var poll = new Poll
            {
                CompletedProject =
                    _lastProject != null && Find.ResearchManager.GetProgress(_lastProject) >= 100
                        ? _lastProject
                        : null,
                Timer = Settings.Duration,
                ResultsTimer = Settings.ResultsDuration,
                CoverTimer = Settings.CompletedDuration,
                Choices = new List<Choice>()
            };

            foreach (ResearchProjectDef project in ToolkitResearch.GetNextChoices())
            {
                var choice = new Choice
                {
                    Label = project.label?.CapitalizeFirst() ?? project.defName,
                    OnChosen = () => Find.ResearchManager.currentProj = project,
                    Project = project,
                    Tooltip = project.description,
                    Votes = new List<string>()
                };

                choice.Initialize();
                poll.Choices.Add(choice);
            }

            if (poll.Choices.Count == 1)
            {
                Find.ResearchManager.currentProj = poll.Choices.FirstOrDefault()?.Project;
            }

            StartNewPoll(poll);
        }

        private void StartNewPoll([NotNull] Poll poll)
        {
        #if DEBUG
            poll.Timer = 10;
            poll.CoverTimer = 10;
            poll.ResultsTimer = 10;
        #endif

            CurrentPoll = poll;
            Find.WindowStack.Add(new ResearchPollDialog());
            _lastProject = null;

            TwitchWrapper.SendChatMessage("ToolkitResearch.Messages.VoteMessage".TranslateSimple());
            for (var index = 0; index < poll.Choices.Count; index++)
            {
                Choice choice = poll.Choices[index];
                TwitchWrapper.SendChatMessage($"[{index + 1}] {choice.Label}");
            }
        }

        public override void GameComponentUpdate()
        {
            if (Find.ResearchManager.currentProj != null)
            {
                _lastProject = Find.ResearchManager.currentProj;
            }

            if (CurrentPoll == null)
            {
                return;
            }

            while (!_votes.IsEmpty)
            {
                if (!_votes.TryDequeue(out Vote vote))
                {
                    break;
                }

                CurrentPoll.UnregisterVote(vote.Viewer);
                CurrentPoll.RegisterVote(vote.Index, vote.Viewer);
            }

            if (_marker <= 0)
            {
                _marker = Time.unscaledTime;
            }

            TickPoll();
        }

        private void TickPoll()
        {
            switch (CurrentPoll.State)
            {
                case PollState.Cover:
                    CurrentPoll.CoverTimer -= Time.unscaledTime - _marker;

                    if (CurrentPoll.CoverTimer <= 0)
                    {
                        CurrentPoll.Transition();
                    }

                    break;
                case PollState.Poll:
                    CurrentPoll.Timer -= Time.unscaledTime - _marker;

                    if (CurrentPoll.Timer <= 0)
                    {
                        CurrentPoll.Transition();
                    }

                    break;
                case PollState.Results:
                    CurrentPoll.ResultsTimer -= Time.unscaledTime - _marker;

                    if (CurrentPoll.ResultsTimer <= 0)
                    {
                        CurrentPoll.Transition();
                        CurrentPoll.Conclude();
                        CurrentPoll = null;
                    }

                    break;
            }

            _marker = Time.unscaledTime;
        }

        public override void ExposeData()
        {
            Scribe_Defs.Look(ref _lastProject, "lastProject");
        }
    }
}

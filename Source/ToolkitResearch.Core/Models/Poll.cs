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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using SettingsHelper = SirRandoo.ToolkitResearch.Helpers.SettingsHelper;

namespace SirRandoo.ToolkitResearch.Models
{
    internal enum PollState { None, Cover, Poll, Results, Done }

    public class Poll
    {
        private Choice _winner;
        private float _totalVotes;
        private string _completedText;

        public float Timer { get; set; }
        public float CoverTimer { get; set; }
        public float ResultsTimer { get; set; }
        internal PollState State { get; private set; } = PollState.None;
        public List<Choice> Choices { get; set; }
        public ResearchProjectDef CompletedProject { get; set; }

        public void DrawCover(Rect region)
        {
            SettingsHelper.DrawLabel(
                region,
                _completedText ??= "ToolkitResearch.Windows.Poll.ResearchComplete".Translate(CompletedProject.label),
                TextAnchor.UpperLeft
            );
        }

        public void DrawPoll(Rect region)
        {
            GameFont cache = Text.Font;
            Text.Font = GameFont.Small;
            var listing = new Listing_Standard();
            listing.Begin(region);

            for (var index = 0; index < Choices.Count; index++)
            {
                Choice choice = Choices[index];
                Rect lineRect = listing.GetRect(Text.LineHeight);

                var indexRect = new Rect(0f, lineRect.y, 25f, lineRect.height);
                var nameRect = new Rect(
                    indexRect.x + indexRect.width + 2f,
                    lineRect.y,
                    lineRect.width - 39f,
                    lineRect.height
                );
                var votesRect = new Rect(lineRect.x + lineRect.width - 10f, lineRect.y, 10f, lineRect.height);

                choice.DrawBar(lineRect, choice.Votes.Count / _totalVotes);

                SettingsHelper.DrawLabel(indexRect, $"#{index + 1}");
                choice.Draw(nameRect);
                SettingsHelper.DrawLabel(votesRect, choice.Votes.Count.ToString());
            }

            listing.End();
            Text.Font = cache;
        }

        public void DrawResults(Rect region)
        {
            _winner ??= GetWinningChoice();

            GameFont cache = Text.Font;
            Text.Font = GameFont.Small;
            var listing = new Listing_Standard();
            listing.Begin(region);

            for (var index = 0; index < Choices.Count; index++)
            {
                Choice choice = Choices[index];
                Rect lineRect = listing.GetRect(Text.LineHeight);

                var indexRect = new Rect(0f, lineRect.y, 25f, lineRect.height);
                var nameRect = new Rect(
                    indexRect.x + indexRect.width + 2f,
                    lineRect.y,
                    lineRect.width - 24f,
                    lineRect.height
                );
                var votesRect = new Rect(lineRect.x + lineRect.width - 10f, lineRect.y, 10f, lineRect.height);

                SettingsHelper.DrawLabel(indexRect, $"#{index + 1}");
                choice.Draw(nameRect);
                SettingsHelper.DrawLabel(votesRect, choice.Votes.Count.ToString());

                if (choice != _winner)
                {
                    continue;
                }

                var barRect = new Rect(
                    0f,
                    lineRect.y,
                    Mathf.FloorToInt(lineRect.width * (_totalVotes <= 0 ? 1f : choice.Votes.Count / _totalVotes)),
                    lineRect.height - 4f
                );
                Widgets.DrawHighlightSelected(barRect);
            }

            listing.End();
            Text.Font = cache;
        }

        public void RegisterVote(int id, string viewer)
        {
            try
            {
                Choice choice = Choices[id - 1];

                choice.RegisterVote(viewer);
                _totalVotes = Choices.Sum(c => c.Votes.Count);
            }
            catch (IndexOutOfRangeException)
            {
                // Ignored
            }
        }

        public void UnregisterVote(string viewer)
        {
            foreach (Choice choice in Choices)
            {
                choice.UnregisterVote(viewer);
            }

            _totalVotes = Choices.Sum(c => c.Votes.Count);
        }

        private Choice GetWinningChoice()
        {
            if (_winner != null)
            {
                return _winner;
            }

            int winningVotes = Choices.Max(c => c.Votes.Count);
            _winner ??= Choices.Where(c => c.Votes.Count == winningVotes).RandomElement();

            return _winner;
        }

        public void Transition()
        {
            switch (State)
            {
                case PollState.None when CompletedProject == null:
                    State = PollState.Poll;
                    break;
                case PollState.None:
                    State = PollState.Cover;
                    break;
                case PollState.Cover:
                    State = PollState.Poll;
                    break;
                case PollState.Poll:
                    State = PollState.Results;
                    break;
                case PollState.Results:
                    State = PollState.Done;
                    break;
            }
        }

        public void Conclude()
        {
            GetWinningChoice()?.OnChosen.Invoke();
        }
    }
}

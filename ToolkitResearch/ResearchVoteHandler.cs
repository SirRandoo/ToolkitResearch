using System.Collections.Concurrent;
using System.Linq;
using JetBrains.Annotations;
using SirRandoo.ToolkitResearch.Windows;
using ToolkitCore;
using TwitchLib.Client.Models.Interfaces;
using Verse;

namespace SirRandoo.ToolkitResearch
{
    [UsedImplicitly]
    public class ResearchVoteHandler : TwitchInterfaceBase
    {
        private readonly ConcurrentStack<ResearchPollDialog> _pollDialogs = new ConcurrentStack<ResearchPollDialog>();

        public ResearchVoteHandler(Game game) { }

        public override void FinalizeInit()
        {
            if (Find.ResearchManager == null || Find.WindowStack == null)
            {
                return;
            }

            if (Find.ResearchManager.currentProj == null)
            {
                ToolkitResearch.StartNewPoll();
            }
        }

        public override void ParseMessage(ITwitchMessage twitchMessage)
        {
            if (_pollDialogs.Count <= 0)
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

            foreach (ResearchPollDialog dialog in _pollDialogs.Where(d => d.IsProcessingVotes()))
            {
                dialog.RegisterVote(twitchMessage.Username.ToLowerInvariant(), vote);
            }
        }

        public void RegisterPoll(ResearchPollDialog researchPollDialog)
        {
            _pollDialogs.Push(researchPollDialog);
        }
    }
}

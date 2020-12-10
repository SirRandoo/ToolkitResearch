using System.Collections.Generic;
using System.Collections.ObjectModel;
using Verse;

namespace SirRandoo.ToolkitResearch.Models
{
    public class PollItem
    {
        public PollItem()
        {
            Voters.CollectionChanged += (sender, args) =>
            {
                VoteCount = Voters.Count;
                VoteCountLabel = VoteCount.ToString("N0");
            };
        }

        public ResearchProjectDef Project { get; set; }
        public ObservableCollection<string> Voters { get; set; } = new ObservableCollection<string>();
        public int VoteCount { get; set; }
        public string VoteCountLabel { get; set; }
    }
}

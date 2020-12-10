using System.Collections.Generic;
using System.Collections.ObjectModel;
using Verse;

namespace SirRandoo.ToolkitResearch.Models
{
    public class PollItem
    {
        public PollItem()
        {
            Voters.CollectionChanged += (sender, args) => VoteCount = Voters.Count;
        }

        public ResearchProjectDef Project { get; set; }
        public ObservableCollection<string> Voters { get; set; } = new ObservableCollection<string>();
        public int VoteCount { get; set; }
    }
}

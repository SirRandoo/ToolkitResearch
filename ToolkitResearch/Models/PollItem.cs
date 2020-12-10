using System.Collections.ObjectModel;
using Verse;

namespace SirRandoo.ToolkitResearch.Models
{
    public class PollItem
    {
        public PollItem(int id)
        {
            Id = id;
            IdLabel = id.ToString("N0");

            Voters.CollectionChanged += (sender, args) =>
            {
                VoteCount = Voters.Count;
                VoteCountLabel = VoteCount.ToString("N0");
            };
        }

        public int Id { get; }
        public string IdLabel { get; }
        public ResearchProjectDef Project { get; set; }
        public ObservableCollection<string> Voters { get; } = new ObservableCollection<string>();
        public int VoteCount { get; private set; }
        public string VoteCountLabel { get; private set; }
    }
}

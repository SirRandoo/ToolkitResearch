using System.Collections.ObjectModel;
using Verse;

namespace SirRandoo.ToolkitResearch.Models
{
    public class PollItem
    {
        private int _voteCount;

        public PollItem(int id)
        {
            Id = id;
            IdLabel = id.ToString("N0");

            Voters.CollectionChanged += (sender, args) =>
            {
                VoteCount = Voters.Count;
            };
        }

        public int Id { get; }
        public string IdLabel { get; }
        public ResearchProjectDef Project { get; set; }
        public ObservableCollection<string> Voters { get; } = new ObservableCollection<string>();

        public int VoteCount
        {
            get => _voteCount;
            private set
            {
                _voteCount = value;
                VoteCountLabel = _voteCount.ToString("N0");
            }
        }

        public string VoteCountLabel { get; private set; } = "0";
    }
}

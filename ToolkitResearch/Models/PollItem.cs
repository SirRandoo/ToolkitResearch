using System.Collections.ObjectModel;
using SirRandoo.ToolkitResearch.Helpers;
using Verse;

namespace SirRandoo.ToolkitResearch.Models
{
    public class PollItem
    {
        private int _voteCount;

        public PollItem(int id)
        {
            Id = id;
            IdLabel = $"#{id:N0}".Tagged("b");

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

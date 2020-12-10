using System.Collections.ObjectModel;
using SirRandoo.ToolkitResearch.Helpers;
using Verse;

namespace SirRandoo.ToolkitResearch.Models
{
    public class PollItem
    {
        private ResearchProjectDef _project;
        private int _voteCount;

        public PollItem(int id)
        {
            Id = id;
            IdLabel = $"#{id:N0}".Tagged("b");
            IdWidth = Text.CalcSize(IdLabel).x;

            Voters.CollectionChanged += (sender, args) => VoteCount = Voters.Count;

            VoteCount = 0;
        }

        public int Id { get; }
        public string IdLabel { get; }
        public float IdWidth { get; }

        public ResearchProjectDef Project
        {
            get => _project;
            set
            {
                _project = value;
                ProjectWidth = Text.CalcSize(_project.LabelCap).x;
            }
        }

        public float ProjectWidth { get; private set; }

        public ObservableCollection<string> Voters { get; } = new ObservableCollection<string>();

        public int VoteCount
        {
            get => _voteCount;
            private set
            {
                _voteCount = value;
                VoteCountLabel = _voteCount.ToString("N0");
                VoteCountWidth = Text.CalcSize(VoteCountLabel).x;
            }
        }

        public string VoteCountLabel { get; private set; }
        public float VoteCountWidth { get; private set; }
    }
}

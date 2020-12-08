using System.Collections.Generic;
using Verse;

namespace SirRandoo.ToolkitResearch.Models
{
    public class PollItem
    {
        public ResearchProjectDef Project { get; set; }
        public List<string> Voters { get; set; } = new List<string>();
    }
}

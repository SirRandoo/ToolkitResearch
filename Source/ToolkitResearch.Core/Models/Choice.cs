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
using SirRandoo.ToolkitResearch.Helpers;
using UnityEngine;
using Verse;

namespace SirRandoo.ToolkitResearch.Models
{
    public class Choice
    {
        private float _lastStep;
        public ResearchProjectDef Project { get; set; }
        public string Label { get; set; }
        public string Tooltip { get; set; }
        public Action OnChosen { get; set; }
        public List<string> Votes { get; set; }
        public float LabelWidth { get; private set; }

        public void Initialize()
        {
            Label = Project.label?.CapitalizeFirst() ?? Project.defName.CapitalizeFirst();

            GameFont cache = Text.Font;
            Text.Font = GameFont.Small;
            LabelWidth = Text.CalcSize(Label).x;
            Text.Font = cache;

            Tooltip = Project.description;
        }

        public void Draw(Rect region)
        {
            SettingsHelper.DrawLabel(region, Label);

            if (Tooltip.NullOrEmpty())
            {
                return;
            }

            Widgets.DrawHighlightIfMouseover(region);
            TooltipHandler.TipRegion(region, Tooltip);
        }

        public void DrawBar(Rect region, float percentage)
        {
            float difference = Mathf.Abs(_lastStep - percentage);

            if (difference > 0.0f)
            {
                _lastStep = Mathf.SmoothStep(_lastStep, percentage, 0.2f);
            }

            var barRect = new Rect(region.x, region.y, Mathf.FloorToInt(region.width * _lastStep), region.height - 4f);

            GUI.color = new Color(0.57f, 0.27f, 1f, 0.5f);
            GUI.DrawTexture(barRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        public void RegisterVote(string viewer)
        {
            Votes.RemoveAll(v => v.Equals(viewer, StringComparison.InvariantCultureIgnoreCase));
            Votes.Add(viewer);
        }

        public void UnregisterVote(string viewer)
        {
            Votes.RemoveAll(v => v.Equals(viewer, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}

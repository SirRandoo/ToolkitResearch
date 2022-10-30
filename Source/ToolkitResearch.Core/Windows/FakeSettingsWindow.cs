﻿// MIT License
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

using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using SirRandoo.ToolkitResearch.Helpers;
using SirRandoo.ToolkitResearch.ModCompat;
using UnityEngine;
using Verse;

namespace SirRandoo.ToolkitResearch.Windows
{
    public class FakeSettingsWindow : Window
    {
        private readonly Mod _mod;
        private bool _hasSettings;
        private FloatMenu _noSettingsFloatMenu;
        private string _selectModText;
        private FloatMenu _settingsFloatMenu;

        public FakeSettingsWindow(Mod mod)
        {
            _mod = mod;
            forcePause = true;
            doCloseX = true;
            doCloseButton = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
        }

        public override Vector2 InitialSize => new Vector2(900f, 700f);

        public override void DoWindowContents(Rect region)
        {
            var headerRect = new Rect(0f, 0f, region.width, 35f);
            var settingsRect = new Rect(0f, 40f, region.width, region.height - 40f - CloseButSize.y);

            GUI.BeginGroup(region);

            GUI.BeginGroup(headerRect);

            if (HugsLib.Active)
            {
                DrawHugsLibHeader(headerRect);
            }
            else
            {
                DrawHeader(headerRect);
            }

            GUI.EndGroup();

            GUI.BeginGroup(settingsRect);
            DrawSettings(settingsRect.AtZero());
            GUI.EndGroup();

            GUI.EndGroup();
        }

        private void DrawHugsLibHeader(Rect region)
        {
            var labelRect = new Rect(0f, 0f, region.width, 32f);
            SettingsHelper.DrawLabel(labelRect, "HugsLib_setting_mod_name_title".Translate(_mod.SettingsCategory()), fontScale: GameFont.Medium);

            Color cache = GUI.color;
            GUI.color = new Color(0.3f, 0.3f, 0.3f);
            Widgets.DrawLineHorizontal(0f, 32f, region.width);
            GUI.color = cache;
        }

        private void DrawHeader(Rect region)
        {
            var btnRect = new Rect(0f, 0f, 150f, region.height);
            var labelRect = new Rect(167f, 0f, region.width - 150f - 17f, region.height);

            if (Widgets.ButtonText(btnRect, _selectModText))
            {
                Find.WindowStack.Add(_hasSettings ? _settingsFloatMenu : _noSettingsFloatMenu);
            }

            SettingsHelper.DrawLabel(labelRect, _mod.SettingsCategory(), fontScale: GameFont.Medium);
        }

        protected virtual void DrawSettings(Rect region) { }

        protected virtual void GetTranslations() { }

        public override void PostOpen()
        {
            var modSettings = new List<FloatMenuOption>();
            foreach (Mod handle in LoadedModManager.ModHandles)
            {
                if (handle.SettingsCategory().NullOrEmpty())
                {
                    continue;
                }

                _hasSettings = true;
                modSettings.Add(new FloatMenuOption(handle.SettingsCategory(), () => DisplayMod(handle)));
            }

            _selectModText = "SelectMod".TranslateSimple();

            if (!_hasSettings)
            {
                _noSettingsFloatMenu = new FloatMenu(
                    new List<FloatMenuOption>
                    {
                        new FloatMenuOption("NoConfigurableMods".TranslateSimple(), null)
                    }
                );
            }

            _settingsFloatMenu = new FloatMenu(modSettings);
            GetTranslations();
        }

        public override void PreClose()
        {
            base.PreClose();
            _mod.WriteSettings();
        }

        private void DisplayMod(Mod handle)
        {
            var window = new Dialog_ModSettings(handle);

            Find.WindowStack.TryRemove(this, false);
            Find.WindowStack.Add(window);
        }
    }
}

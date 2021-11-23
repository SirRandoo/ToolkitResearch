using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace SirRandoo.ToolkitResearch.Helpers
{
    public static class SettingsHelper
    {
        private static readonly FieldInfo SelectedModField = AccessTools.Field(typeof(Dialog_ModSettings), "selMod");

        private static readonly GameFont[] GameFonts = Enum.GetNames(typeof(GameFont))
           .Select(f => (GameFont)Enum.Parse(typeof(GameFont), f))
           .OrderByDescending(f => (int)f)
           .ToArray();

        public static bool DrawFieldButton(Rect region, Texture2D icon, [CanBeNull] string tooltip = null)
        {
            var buttonRegion = new Rect(
                region.x + region.width - region.height + 6f,
                region.y + 6f,
                region.height - 12f,
                region.height - 12f
            );
            Widgets.ButtonImage(buttonRegion, icon);

            if (!tooltip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(buttonRegion, tooltip);
            }

            bool wasClicked = Event.current.type == EventType.Used && Input.GetMouseButtonDown(0);
            bool shouldTrigger = Mouse.IsOver(buttonRegion) && wasClicked;

            if (!shouldTrigger)
            {
                return false;
            }

            GUIUtility.keyboardControl = 0;
            return true;
        }

        public static bool DrawClearButton(Rect canvas)
        {
            var region = new Rect(canvas.x + canvas.width - 16f, canvas.y, 16f, canvas.height);
            Widgets.ButtonText(region, "×", false);

            bool clicked = Mouse.IsOver(region) && Event.current.type == EventType.Used && Input.GetMouseButtonDown(0);

            if (!clicked)
            {
                return false;
            }

            GUIUtility.keyboardControl = 0;
            return true;
        }

        public static void DrawPriceField(Rect canvas, ref int price)
        {
            const float buttonWidth = 50f;

            var reduceRect = new Rect(canvas.x, canvas.y, buttonWidth, canvas.height);
            var raiseRect = new Rect(canvas.x + canvas.width - buttonWidth, canvas.y, buttonWidth, canvas.height);
            var fieldRect = new Rect(canvas.x + buttonWidth + 2f, canvas.y, canvas.width - buttonWidth * 2 - 4f, canvas.height);
            var buffer = price.ToString();
            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool control = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);


            if (control && shift)
            {
                buffer = DrawControlShiftPriceBtns(ref price, reduceRect, buffer, raiseRect);
            }
            else if (control)
            {
                buffer = DrawControlPriceBtns(ref price, reduceRect, buffer, raiseRect);
            }
            else if (shift)
            {
                buffer = DrawShiftPriceBtns(ref price, reduceRect, buffer, raiseRect);
            }
            else
            {
                buffer = DrawBasePriceBtns(ref price, reduceRect, buffer, raiseRect);
            }


            Widgets.TextFieldNumeric(fieldRect, ref price, ref buffer);
        }

        private static string DrawControlShiftPriceBtns(ref int price, Rect reduceRect, string buffer, Rect raiseRect)
        {
            if (Widgets.ButtonText(reduceRect, "-1000"))
            {
                price -= 1000;
                buffer = price.ToString();
            }

            if (Widgets.ButtonText(raiseRect, "+1000"))
            {
                price += 1000;
                buffer = price.ToString();
            }

            return buffer;
        }

        private static string DrawControlPriceBtns(ref int price, Rect reduceRect, string buffer, Rect raiseRect)
        {
            if (Widgets.ButtonText(reduceRect, "-100"))
            {
                price -= 100;
                buffer = price.ToString();
            }

            if (Widgets.ButtonText(raiseRect, "+100"))
            {
                price += 100;
                buffer = price.ToString();
            }

            return buffer;
        }

        private static string DrawShiftPriceBtns(ref int price, Rect reduceRect, string buffer, Rect raiseRect)
        {
            if (Widgets.ButtonText(reduceRect, "-10"))
            {
                price -= 10;
                buffer = price.ToString();
            }

            if (Widgets.ButtonText(raiseRect, "+10"))
            {
                price += 10;
                buffer = price.ToString();
            }

            return buffer;
        }

        private static string DrawBasePriceBtns(ref int price, Rect reduceRect, string buffer, Rect raiseRect)
        {
            if (Widgets.ButtonText(reduceRect, "-1"))
            {
                price -= 1;
                buffer = price.ToString();
            }

            if (Widgets.ButtonText(raiseRect, "+1"))
            {
                price += 1;
                buffer = price.ToString();
            }

            return buffer;
        }

        public static bool WasLeftClicked(this Rect region)
        {
            return WasMouseButtonClicked(region, 0);
        }

        public static bool WasRightClicked(this Rect region)
        {
            return WasMouseButtonClicked(region, 1);
        }

        public static bool WasMouseButtonClicked(this Rect region, int mouseButton)
        {
            if (!Mouse.IsOver(region))
            {
                return false;
            }

            Event current = Event.current;
            bool was = current.button == mouseButton;

            switch (current.type)
            {
                case EventType.Used when was:
                case EventType.MouseDown when was:
                    current.Use();
                    return true;
                default:
                    return false;
            }
        }

        public static Rect ShiftLeft(this Rect region, float padding = 5f)
        {
            return new Rect(region.x - region.width - padding, region.y, region.width, region.height);
        }

        public static Rect ShiftRight(this Rect region, float padding = 5f)
        {
            return new Rect(region.x + region.width + padding, region.y, region.width, region.height);
        }

        public static bool IsRegionVisible(this Rect region, Rect scrollView, Vector2 scrollPos)
        {
            return (region.y >= scrollPos.y || region.y + region.height - 1f >= scrollPos.y) && region.y <= scrollPos.y + scrollView.height;
        }

        public static void DrawColored(this Texture2D t, Rect region, Color color)
        {
            Color old = GUI.color;

            GUI.color = color;
            GUI.DrawTexture(region, t);
            GUI.color = old;
        }

        public static void DrawLabel(Rect region, string text, TextAnchor anchor = TextAnchor.MiddleLeft, GameFont fontScale = GameFont.Small, bool vertical = false)
        {
            Text.Anchor = anchor;
            Text.Font = fontScale;

            if (vertical)
            {
                region.y += region.width;
                GUIUtility.RotateAroundPivot(-90f, region.position);
            }

            Widgets.Label(region, text);

            if (vertical)
            {
                GUI.matrix = Matrix4x4.identity;
            }

            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }

        public static bool DrawTabButton(
            Rect region,
            string text,
            TextAnchor anchor = TextAnchor.MiddleLeft,
            GameFont fontScale = GameFont.Small,
            bool vertical = false,
            bool selected = false
        )
        {
            Text.Anchor = anchor;
            Text.Font = fontScale;

            if (vertical)
            {
                region.y += region.width;
                GUIUtility.RotateAroundPivot(-90f, region.position);
            }

            GUI.color = selected ? new Color(0.46f, 0.49f, 0.5f) : new Color(0.21f, 0.23f, 0.24f);
            Widgets.DrawHighlight(region);
            GUI.color = Color.white;

            if (!selected && Mouse.IsOver(region))
            {
                Widgets.DrawLightHighlight(region);
            }

            Widgets.Label(region, text);
            bool pressed = Widgets.ButtonInvisible(region);

            if (vertical)
            {
                GUI.matrix = Matrix4x4.identity;
            }

            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            return pressed;
        }

        public static void DrawColoredLabel(
            Rect region,
            string text,
            Color color,
            TextAnchor anchor = TextAnchor.MiddleLeft,
            GameFont fontScale = GameFont.Small,
            bool vertical = false
        )
        {
            GUI.color = color;
            DrawLabel(region, text, anchor, fontScale, vertical);
            GUI.color = Color.white;
        }

        public static void DrawFittedLabel(Rect region, string text, TextAnchor anchor = TextAnchor.MiddleLeft, GameFont maxScale = GameFont.Small, bool vertical = false)
        {
            Text.Anchor = anchor;

            if (vertical)
            {
                region.y += region.width;
                GUIUtility.RotateAroundPivot(-90f, region.position);
            }

            var maxFontScale = (int)maxScale;
            foreach (GameFont f in GameFonts)
            {
                if ((int)f > maxFontScale)
                {
                    continue;
                }

                Text.Font = f;

                if (Text.CalcSize(text).x <= region.width)
                {
                    break;
                }
            }

            Widgets.Label(region, text);

            if (vertical)
            {
                GUI.matrix = Matrix4x4.identity;
            }

            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }

        public static Tuple<Rect, Rect> ToForm(this Rect region, float factor = 0.8f)
        {
            var left = new Rect(region.x, region.y, region.width * factor - 2f, region.height);

            return new Tuple<Rect, Rect>(left.Rounded(), new Rect(left.x + left.width + 2f, left.y, region.width - left.width - 2f, left.height).Rounded());
        }

        public static Tuple<Rect, Rect> GetRectAsForm(this Listing listing, float factor = 0.8f)
        {
            return listing.GetRect(Text.LineHeight).ToForm(factor);
        }

        public static string Tagged(this string s, string tag)
        {
            return $"<{tag}>{s}</{tag}>";
        }

        public static string ColorTagged(this string s, string hex)
        {
            if (!hex.StartsWith("#"))
            {
                hex = $"#{hex}";
            }

            return $@"<color=""{hex}"">{s}</color>";
        }

        public static string ColorTagged(this string s, Color color)
        {
            return ColorTagged(s, ColorUtility.ToHtmlStringRGB(color));
        }

        public static void TipRegion(this Rect region, string tooltip)
        {
            TooltipHandler.TipRegion(region, tooltip);
            Widgets.DrawHighlightIfMouseover(region);
        }

        public static void OpenSettingsMenuFor(Mod modInstance)
        {
            var settings = new Dialog_ModSettings();
            SelectedModField.SetValue(settings, modInstance);

            Find.WindowStack.Add(settings);
        }

        public static Rect TrimLeft(this Rect region, float amount)
        {
            return new Rect(region.x + amount, region.y, region.width - amount, region.height);
        }

        public static Rect WithWidth(this Rect region, float width)
        {
            return new Rect(region.x, region.y, width, region.height);
        }

        public static void DrawDescription([NotNull] this Listing listing, string description, Color color, TextAnchor anchor = TextAnchor.UpperLeft)
        {
            GameFont fontCache = Text.Font;
            GUI.color = color;
            Text.Font = GameFont.Tiny;
            float height = Text.CalcHeight(description, listing.ColumnWidth * 0.7f);
            DrawLabel(listing.GetRect(height).TrimLeft(10f).WithWidth(listing.ColumnWidth * 0.7f).Rounded(), description, anchor, GameFont.Tiny);
            GUI.color = Color.white;
            Text.Font = fontCache;

            listing.Gap(6f);
        }

        public static void DrawDescription([NotNull] this Listing listing, string description, TextAnchor anchor)
        {
            DrawDescription(listing, description, new Color(0.72f, 0.72f, 0.72f), anchor);
        }

        public static void DrawDescription([NotNull] this Listing listing, string description)
        {
            DrawDescription(listing, description, new Color(0.72f, 0.72f, 0.72f));
        }

        public static void DrawGroupHeader(this Listing listing, string heading, bool gapPrefix = true)
        {
            if (gapPrefix)
            {
                listing.Gap(Mathf.CeilToInt(Text.LineHeight * 1.25f));
            }

            DrawLabel(listing.GetRect(Text.LineHeight), heading, TextAnchor.LowerLeft, GameFont.Tiny);
            listing.GapLine(6f);
        }

        [ContractAnnotation("=> true,newContent:notnull; => false,newContent:null")]
        public static bool DrawTextField(Rect region, string content, out string newContent)
        {
            string text = Widgets.TextField(region, content);

            newContent = !text.Equals(content) ? text : null;
            return newContent != null;
        }

        public static void DrawAugmentedNumberEntry(Rect region, ref string buffer, ref int value, ref bool bufferValid)
        {
            GUI.backgroundColor = bufferValid ? Color.white : Color.red;

            if (DrawTextField(region, buffer, out string newBuffer))
            {
                buffer = newBuffer;

                if (int.TryParse(buffer, out int result))
                {
                    value = result;
                    bufferValid = true;
                }
                else
                {
                    bufferValid = false;
                }
            }

            GUI.backgroundColor = Color.white;
        }

        public static void DrawAugmentedNumberEntry(Rect region, ref string buffer, ref float value, ref bool bufferValid)
        {
            GUI.backgroundColor = bufferValid ? Color.white : Color.red;

            if (DrawTextField(region, buffer, out string newBuffer))
            {
                buffer = newBuffer;

                if (float.TryParse(buffer, out float result))
                {
                    value = result;
                    bufferValid = true;
                }
                else
                {
                    bufferValid = false;
                }
            }

            GUI.backgroundColor = Color.white;
        }
    }
}

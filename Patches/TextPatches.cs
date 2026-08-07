using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;

namespace StardewChaos.Patches
{
    public static class TextPatches
    {
        public static bool AaaaActive = false;
        public static bool IsDrawingChaosHud = false;

        public static void Apply(Harmony harmony)
        {
            var spriteTextType = AccessTools.TypeByName("StardewValley.BellsAndWhistles.SpriteText");
            if (spriteTextType != null)
            {
                foreach (var method in spriteTextType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
                {
                    if (method.Name == "drawString" && method.GetParameters().Length >= 2)
                    {
                        var secondParam = method.GetParameters()[1];
                        if (secondParam.ParameterType == typeof(string))
                        {
                            harmony.Patch(method, prefix: new HarmonyMethod(typeof(TextPatches), nameof(DrawString_Prefix)));
                        }
                    }
                }
            }

            var clickableMenuType = AccessTools.TypeByName("StardewValley.Menus.IClickableMenu");
            if (clickableMenuType != null)
            {
                foreach (var method in clickableMenuType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
                {
                    if (method.Name == "drawHoverText" && method.GetParameters().Length >= 3)
                    {
                        var firstParam = method.GetParameters()[1];
                        if (firstParam.ParameterType == typeof(string))
                        {
                            harmony.Patch(method, prefix: new HarmonyMethod(typeof(TextPatches), nameof(DrawHoverText_String_Prefix)));
                        }
                        else if (firstParam.ParameterType == typeof(StringBuilder))
                        {
                            harmony.Patch(method, prefix: new HarmonyMethod(typeof(TextPatches), nameof(DrawHoverText_StringBuilder_Prefix)));
                        }
                    }
                    if (method.Name == "drawToolTip")
                    {
                        harmony.Patch(method, prefix: new HarmonyMethod(typeof(TextPatches), nameof(DrawToolTip_Prefix)));
                    }
                }
            }

            var spriteBatchType = typeof(SpriteBatch);
            foreach (var method in spriteBatchType.GetMethods())
            {
                if (method.Name == "DrawString" && method.GetParameters().Length >= 1)
                {
                    var firstParam = method.GetParameters()[0];
                    var secondParam = method.GetParameters()[1];
                    if (firstParam.ParameterType == typeof(SpriteFont) && secondParam.ParameterType == typeof(string))
                    {
                        harmony.Patch(method,
                            prefix: new HarmonyMethod(typeof(TextPatches), nameof(DrawStringSpriteFont_Prefix)));
                    }
                }
            }
        }

        static void DrawString_Prefix(ref string s)
        {
            if (!AaaaActive || IsDrawingChaosHud || string.IsNullOrEmpty(s)) return;
            s = new string('A', s.Length);
        }

        static void DrawHoverText_String_Prefix(ref string text)
        {
            if (!AaaaActive || IsDrawingChaosHud || string.IsNullOrEmpty(text)) return;
            text = new string('A', text.Length);
        }

        static void DrawHoverText_StringBuilder_Prefix(ref StringBuilder text)
        {
            if (!AaaaActive || IsDrawingChaosHud || text == null || text.Length == 0) return;
            for (int i = 0; i < text.Length; i++) text[i] = 'A';
        }

        static void DrawToolTip_Prefix(ref string hoverText, ref string hoverTitle)
        {
            if (!AaaaActive || IsDrawingChaosHud) return;
            if (!string.IsNullOrEmpty(hoverText)) hoverText = new string('A', hoverText.Length);
            if (!string.IsNullOrEmpty(hoverTitle)) hoverTitle = new string('A', hoverTitle.Length);
        }

        static void DrawStringSpriteFont_Prefix(SpriteFont spriteFont, ref string text)
        {
            if (!AaaaActive || IsDrawingChaosHud || string.IsNullOrEmpty(text)) return;
            text = new string('A', text.Length);
        }
    }
}
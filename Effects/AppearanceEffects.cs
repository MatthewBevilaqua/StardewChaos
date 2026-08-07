using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;

namespace StardewChaos
{
    public static class AppearanceEffects
    {
        public static void ScrambleCharacter()
        {
            var p = Game1.player;

            int skin = Game1.random.Next(24);
            p.changeSkinColor(skin);

            int lastHair = Farmer.GetAllHairstyleIndices().Count > 0
                ? Farmer.GetAllHairstyleIndices().Count - 1
                : 15;
            int hair = Game1.random.Next(lastHair + 1);
            p.changeHairStyle(hair);

            int accessory = Game1.random.Next(-1, 30);
            p.changeAccessory(accessory);

            var hairColor = new Color(
                Game1.random.Next(256), Game1.random.Next(256), Game1.random.Next(256));
            p.changeHairColor(hairColor);

            var eyeColor = new Color(
                Game1.random.Next(256), Game1.random.Next(256), Game1.random.Next(256));
            p.changeEyeColor(eyeColor);

            var pantsColor = new Color(
                Game1.random.Next(256), Game1.random.Next(256), Game1.random.Next(256));
            p.changePantsColor(pantsColor);

            var shirtIds = GetValidShirtIds();
            if (shirtIds.Count > 0)
                p.changeShirt(shirtIds[Game1.random.Next(shirtIds.Count)]);

            var pantsIds = GetValidPantsIds();
            if (pantsIds.Count > 0)
                p.changePantStyle(pantsIds[Game1.random.Next(pantsIds.Count)]);

            int shoeColor = Game1.random.Next(12);
            p.changeShoeColor(shoeColor.ToString());

            int hatCount = (FarmerRenderer.hatsTexture?.Height ?? 0) / 80 * 12;
            if (hatCount > 0)
            {
                int hat = Game1.random.Next(-1, hatCount);
                p.changeHat(hat);
            }

            Game1.playSound("shwip");
        }

        public static void ChangeGender()
        {
            var p = Game1.player;
            int current = (int)p.netGender.Value;
            int newGender = current == 0 ? 1 : 0;
            p.netGender.Value = (Gender)newGender;
            Game1.playSound("dirtyHit");
        }

        private static List<string> GetValidShirtIds()
        {
            var ids = new List<string>();
            if (Game1.shirtData != null)
            {
                foreach (var kvp in Game1.shirtData)
                    ids.Add(kvp.Key);
            }
            return ids;
        }

        private static List<string> GetValidPantsIds()
        {
            var ids = new List<string>();
            if (Game1.pantsData != null)
            {
                foreach (var kvp in Game1.pantsData)
                    ids.Add(kvp.Key);
            }
            return ids;
        }
    }
}
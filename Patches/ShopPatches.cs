using System;
using System.Collections.Generic;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace StardewChaos.Patches
{
    public static class ShopPatches
    {
        public static float BuyPriceMultiplier = 1f;

        public static void Apply(Harmony harmony)
        {
            ModEntry.Instance?.Monitor?.Log("ShopPatches: will apply buy price multiplier via MenuChanged event", LogLevel.Info);
        }

        internal static void ApplyPriceMultiplier(ShopMenu shop)
        {
            if (BuyPriceMultiplier == 1f) return;
            try
            {
                var dict = shop.itemPriceAndStock;
                if (dict == null) return;

                var keys = new List<ISalable>(dict.Keys);
                foreach (var key in keys)
                {
                    var stock = dict[key];
                    if (stock == null) continue;

                    int originalPrice = stock.Price;
                    if (originalPrice <= 0) continue;

                    int newPrice = (int)Math.Round(originalPrice * BuyPriceMultiplier);
                    if (newPrice < 1) newPrice = 1;

                    stock.Price = newPrice;
                    dict[key] = stock;
                }
            }
            catch (Exception ex)
            {
                ModEntry.Instance?.Monitor?.Log($"ShopPatches: error modifying shop prices: {ex.Message}", LogLevel.Warn);
            }
        }
    }
}
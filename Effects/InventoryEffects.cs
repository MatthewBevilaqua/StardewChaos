using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;

namespace StardewChaos
{
    public static class InventoryEffects
    {
        private static readonly string[] JunkItemIds = { "168", "169", "565", "167", "153" };

        public static void JunkInventory()
        {
            var p = Game1.player;
            var items = p.Items;
            bool added = false;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == null)
                {
                    string junkId = JunkItemIds[Game1.random.Next(JunkItemIds.Length)];
                    var item = ItemRegistry.Create("(O)" + junkId);
                    items[i] = item;
                    added = true;
                }
            }
            if (added)
                Game1.playSound("dirtyHit");
            else
                Game1.playSound("cancel");
        }

        public static void ClearHotbar()
        {
            var p = Game1.player;
            var items = p.Items;
            int hotbarSize = Math.Min(12, items.Count);
            if (hotbarSize == 0) return;

            var hotbarItems = new List<Item>();
            for (int i = 0; i < hotbarSize; i++)
            {
                if (items[i] != null)
                    hotbarItems.Add(items[i]);
                items[i] = null;
            }

            int dest = hotbarSize;
            foreach (var it in hotbarItems)
            {
                while (dest < items.Count && items[dest] != null) dest++;
                if (dest < items.Count)
                {
                    items[dest] = it;
                }
                else
                {
                    for (int i = 0; i < hotbarSize; i++)
                    {
                        if (items[i] == null) { items[i] = it; break; }
                    }
                }
            }

            for (int i = 0; i < hotbarSize && i < items.Count; i++)
            {
                if (items[i] != null)
                {
                    int empty = -1;
                    for (int j = hotbarSize; j < items.Count; j++)
                    {
                        if (items[j] == null) { empty = j; break; }
                    }
                    if (empty >= 0)
                    {
                        items[empty] = items[i];
                        items[i] = null;
                    }
                }
            }

            p.CurrentToolIndex = 0;
            Game1.playSound("shwip");
        }

        public static void DeleteHeldItem()
        {
            var p = Game1.player;
            int idx = p.CurrentToolIndex;
            if (idx >= 0 && idx < p.Items.Count && p.Items[idx] != null)
            {
                p.Items[idx] = null;
                Game1.playSound("cancel");
            }
        }

        public static void ShippingBinRobbed()
        {
            var farm = Game1.getFarm();
            if (farm == null) return;
            farm.lastItemShipped = null;
            farm.getShippingBin(Game1.player).Clear();
            Game1.playSound("cancel");
        }
    }
}
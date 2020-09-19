using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using LichItems.ItemAPI;
using MonoMod.RuntimeDetour;
using Dungeonator;
using System.Reflection;

namespace LichItems
{
    public class TrophyOfTheJammed : PassiveItem
    {
        public static void Init()
        {
            string itemName = "Trophy of the Jammed";
            string resourceName = "LichItems/Resources/Trophy_of_the_Jammed";
            GameObject obj = new GameObject(itemName);
            var item = obj.AddComponent<TrophyOfTheJammed>();
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);
            string shortDesc = "Dances with Jammed";
            string longDesc = "Long ago, beyond the power of the curse, gungeon.\nA relic given to the conquered hero.";
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "spapi");
            item.quality = ItemQuality.D;
            item.AddPassiveStatModifier(PlayerStats.StatType.Curse, 1, StatModifier.ModifyMethod.ADDITIVE);
            Hook hook = new Hook(
                typeof(Dungeon).GetMethod("SpawnCurseReaper", BindingFlags.Public | BindingFlags.Instance),
                typeof(TrophyOfTheJammed).GetMethod("SpawnCurseReaperHook")
            );
            Hook hook2 = new Hook(
                typeof(ShopItemController).GetProperty("ModifiedPrice", BindingFlags.Public | BindingFlags.Instance).GetGetMethod(),
                typeof(TrophyOfTheJammed).GetMethod("GetModifiedPriceHook")
             );
        }

        public static void SpawnCurseReaperHook(Action<Dungeon> orig, Dungeon self)
        {
            if(!IsFlagSetAtAll(typeof(TrophyOfTheJammed)) || PlayerStats.GetTotalCurse() >= 20)
            {
                orig(self);
            }
        }

        public static int GetModifiedPriceHook(Func<ShopItemController, int> orig, ShopItemController self)
        {
            int price = orig(self);
            if (self.IsResourcefulRatKey)
            {
                return price;
            }
            if (self.CurrencyType == ShopItemController.ShopCurrencyType.META_CURRENCY)
            {
                return price;
            }
            if (self.CurrencyType == ShopItemController.ShopCurrencyType.KEYS)
            {
                return price;
            }
            if (self.OverridePrice != null)
            {
                return price;
            }
            if (self.PrecludeAllDiscounts)
            {
                return price;
            }
            if (IsFlagSetAtAll(typeof(TrophyOfTheJammed)))
            {
                bool hasCurse = false;
                if (self.item is Gun)
                {
                    Gun gun = self.item as Gun;
                    for (int i = 0; i < gun.passiveStatModifiers.Length; i++)
                    {
                        if (gun.passiveStatModifiers[i].statToBoost == PlayerStats.StatType.Curse && gun.passiveStatModifiers[i].amount > 0f)
                        {
                            hasCurse = true;
                            break;
                        }
                    }
                }
                else if (self.item is PlayerItem)
                {
                    PlayerItem playerItem = self.item as PlayerItem;
                    for (int j = 0; j < playerItem.passiveStatModifiers.Length; j++)
                    {
                        if (playerItem.passiveStatModifiers[j].statToBoost == PlayerStats.StatType.Curse && playerItem.passiveStatModifiers[j].amount > 0f)
                        {
                            hasCurse = true;
                            break;
                        }
                    }
                }
                else if (self.item is PassiveItem)
                {
                    PassiveItem passiveItem = self.item as PassiveItem;
                    for (int k = 0; k < passiveItem.passiveStatModifiers.Length; k++)
                    {
                        if (passiveItem.passiveStatModifiers[k].statToBoost == PlayerStats.StatType.Curse && passiveItem.passiveStatModifiers[k].amount > 0f)
                        {
                            hasCurse = true;
                            break;
                        }
                    }
                }
                if (hasCurse)
                {
                    return Mathf.RoundToInt((float)price * 0.8f);
                }
            }
            return price;
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            IncrementFlag(player, typeof(TrophyOfTheJammed));
        }

        public override DebrisObject Drop(PlayerController player)
        {
            DecrementFlag(player, typeof(TrophyOfTheJammed));
            return base.Drop(player);
        }
    }
}

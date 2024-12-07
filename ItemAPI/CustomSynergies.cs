using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoMod.RuntimeDetour;
using HarmonyLib;

namespace LichItems.ItemAPI
{
    public static class CustomSynergies
    {
        public static AdvancedSynergyEntry Add(string name, CustomSynergyType synergyType, List<string> mandatoryConsoleIDs, List<string> optionalConsoleIDs = null, bool ignoreLichEyeBullets = true)
        {
            if (mandatoryConsoleIDs == null || mandatoryConsoleIDs.Count == 0) { ETGModConsole.Log($"Synergy {name} has no mandatory items/guns."); return null; }

            List<int>
                itemIDs = [],
                gunIDs = [],
                optItemIDs = [],
                optGunIDs = [];

            var synergyId = name.ToID().ToUpperInvariant();
            var key = $"#SPAPI_{synergyId}";

            foreach (var id in mandatoryConsoleIDs)
            {
                var pickup = Gungeon.Game.Items[id];

                if (pickup && pickup is Gun)
                    gunIDs.Add(pickup.PickupObjectId);

                else if (pickup && pickup is PlayerItem or PassiveItem)
                    itemIDs.Add(pickup.PickupObjectId);
            }

            if (optionalConsoleIDs != null)
            {
                foreach (var id in optionalConsoleIDs)
                {
                    var pickup = Gungeon.Game.Items[id];

                    if (pickup && pickup is Gun)
                        optGunIDs.Add(pickup.PickupObjectId);

                    else if (pickup && pickup is PlayerItem or PassiveItem)
                        optItemIDs.Add(pickup.PickupObjectId);
                }
            }

            var entry = new AdvancedSynergyEntry()
            {
                NameKey = key,
                MandatoryItemIDs = itemIDs,
                MandatoryGunIDs = gunIDs,
                OptionalItemIDs = optItemIDs,
                OptionalGunIDs = optGunIDs,
                bonusSynergies = [synergyType],
                statModifiers = [],
                IgnoreLichEyeBullets = ignoreLichEyeBullets,
                ActiveWhenGunUnequipped = true
            };
            ETGMod.Databases.Strings.Synergy.Set(key, name);

            Add(entry);
            return entry;
        }

        public static void Add(AdvancedSynergyEntry synergyEntry)
        {
            GameManager.Instance.SynergyManager.synergies = GameManager.Instance.SynergyManager.synergies.AddToArray(synergyEntry);
        }

        public static bool HasMTGConsoleID(this PlayerController player, string consoleID)
        {
            if (!Gungeon.Game.Items.ContainsID(consoleID)) return false;
            return player.HasPickupID(Gungeon.Game.Items[consoleID].PickupObjectId);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LichItems.ItemAPI;
using UnityEngine;
using MonoMod.RuntimeDetour;
using System.Reflection;
using BepInEx;
using HarmonyLib;

namespace LichItems
{
    [BepInPlugin(GUID, "Lich Items", "1.0.7")]
    [BepInDependency(ETGModMainBehaviour.GUID)]
    public class LichModule : BaseUnityPlugin
    {
        public const string GUID = "spapi.etg.lichitems";

        public void Awake()
        {
            ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
        }

        public void GMStart(GameManager gm)
        {
            new Harmony(GUID).PatchAll();

            ItemBuilder.Init();
            CrossChamber.Init();
            LichsBookItem.Init();
            LichsGun.Init();

            CustomSynergies.Add("Master of the Gungeon", LichsBookItem.MasterOfTheGungeonSynergy, ["spapi:lichs_gun", "spapi:lichs_book", "lichs_eye_bullets"]);
            CustomSynergies.Add("Crossfire", CrossChamber.CrossfireSynergy, ["spapi:cross_chamber", "magnum"]);

            ETGMod.StartGlobalCoroutine(DelayedStartCR());
        }

        private IEnumerator DelayedStartCR()
        {
            yield return null;

            var obj = BraveResources.Load("PlayerLich") as GameObject;

            if (obj == null || obj.GetComponent<PlayerController>() is not PlayerController player)
                yield break;

            foreach (tk2dSpriteAnimationClip clip in player.spriteAnimator.Library.clips)
            {
                foreach (tk2dSpriteAnimationFrame frame in clip.frames)
                {
                    if (string.IsNullOrEmpty(frame.eventAudio))
                        continue;

                    if (frame.eventAudio != "Play_FS" && frame.eventAudio != "Play_CHR_boot_stairs_01")
                        continue;

                    frame.eventAudio = "";
                }
            }
        }
    }
}

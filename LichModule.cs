using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexandria.ItemAPI;
using UnityEngine;
using MonoMod.RuntimeDetour;
using System.Reflection;
using BepInEx;
using HarmonyLib;

namespace LichItems
{
    [BepInPlugin(GUID, "Lich Items", "1.0.8")]
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

            CrossChamber.Init();
            LichsBookItem.Init();
            LichsGun.Init();

            CustomSynergies.Add("Master of the Gungeon", ["spapi:lichs_gun", "spapi:lichs_book", "lichs_eye_bullets"]).bonusSynergies = [LichsBookItem.MasterOfTheGungeonSynergy];
            CustomSynergies.Add("Crossfire", ["spapi:cross_chamber", "magnum"]).bonusSynergies = [CrossChamber.CrossfireSynergy];

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

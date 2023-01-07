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

namespace LichItems
{
    [BepInPlugin("spapi.etg.lichitems", "Lich Items", "1.0.1")]
    [BepInDependency(ETGModMainBehaviour.GUID)]
    public class LichModule : BaseUnityPlugin
    {
        public void Awake()
        {
            ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
        }

        public void GMStart(GameManager gm)
        {
            ItemBuilder.Init();
            CrossChamber.Init();
            LichsBookItem.Init();
            LichsGun.Init();
            CustomSynergies.Add("Master of the Gungeon", new List<string> { "spapi:lichs_gun", "spapi:lichs_book", "lichs_eye_bullets" });
            CustomSynergies.Add("Crossfire", new List<string> { "spapi:cross_chamber", "magnum" });
            AdvancedGunBehaviour.Setup();
            ETGMod.StartGlobalCoroutine(DelayedStartCR());
        }

        private IEnumerator DelayedStartCR()
        {
            yield return null;
            GameObject obj = BraveResources.Load("PlayerLich") as GameObject;
            if (obj != null)
            {
                PlayerController player = obj.GetComponent<PlayerController>();
                if (player != null)
                {
                    GameObject dustup = new GameObject("InvisibleInstantDestroyDustup");
                    dustup.SetActive(false);
                    FakePrefab.MarkAsFakePrefab(dustup);
                    DontDestroyOnLoad(dustup);
                    dustup.AddComponent<InstantDestroyDustup>();
                    player.OverrideDustUp = dustup;
                    foreach (tk2dSpriteAnimationClip clip in player.spriteAnimator.Library.clips)
                    {
                        foreach (tk2dSpriteAnimationFrame frame in clip.frames)
                        {
                            if (!string.IsNullOrEmpty(frame.eventAudio) && (frame.eventAudio == "Play_FS" || frame.eventAudio == "Play_CHR_boot_stairs_01"))
                            {
                                frame.eventAudio = "";
                            }
                        }
                    }
                }
            }
        }

        private class InstantDestroyDustup : MonoBehaviour
        {
            public void Start()
            {
                Destroy(gameObject);
            }
        }
    }
}

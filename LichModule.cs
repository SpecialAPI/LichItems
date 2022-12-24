using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LichItems.ItemAPI;
using UnityEngine;
using MonoMod.RuntimeDetour;
using System.Reflection;
using CustomCharacters;

namespace LichItems
{
    public class LichModule : ETGModule
    {
        public override void Init()
        {
        }

        public override void Start()
        {
            ItemBuilder.Init();
            CrossChamber.Init();
            LichsBookItem.Init();
            LichsGun.Init();
            CustomSynergies.Add("Master of Gungeon", new List<string> { "spapi:lichs_gun", "spapi:lichs_book", "lichs_eye_bullets" });
            CustomSynergies.Add("Bullet = peace", new List<string> { "spapi:cross_chamber", "magnum" });
            Hook getNicknamehook = new Hook(
                    typeof(StringTableManager).GetMethod("GetTalkingPlayerNick", BindingFlags.NonPublic | BindingFlags.Static),
                    typeof(LichModule).GetMethod("GetTalkingPlayerNickHook")
                );

            Hook getNamehook = new Hook(
                typeof(StringTableManager).GetMethod("GetTalkingPlayerName", BindingFlags.NonPublic | BindingFlags.Static),
                typeof(LichModule).GetMethod("GetTalkingPlayerNameHook")
            );

            Hook getValueHook = new Hook(
                typeof(dfLanguageManager).GetMethod("GetValue", BindingFlags.Public | BindingFlags.Instance),
                typeof(LichModule).GetMethod("GetValueHook")
            );
            ETGModConsole.Log("ADVANCED SYNERGIES AREN'T NULL: " + (ETGMod.Databases.Items["Cross Chamber"].GetComponent<AdvancedCompanionItem>().AdvancedSynergies != null));
            ETGMod.StartGlobalCoroutine(this.DelayedStartCR());
        }

        public static string GetTalkingPlayerNickHook(Func<string> orig)
        {
            PlayerController talkingPlayer = GetTalkingPlayer();
            if (talkingPlayer.IsThief)
            {
                return "#THIEF_NAME";
            }
            if (talkingPlayer.GetComponent<CustomCharacter>() != null)
            {
                if (talkingPlayer.GetComponent<CustomCharacter>().data != null)
                {
                    return "#PLAYER_NICK_" + talkingPlayer.GetComponent<CustomCharacter>().data.nameShort.ToUpper();
                }
            }
            return orig();
        }

        public static string GetValueHook(Func<dfLanguageManager, string, string> orig, dfLanguageManager self, string key)
        {
            if (characterDeathNames.Contains(key))
            {
                if (GameManager.Instance.PrimaryPlayer != null && GameManager.Instance.PrimaryPlayer.GetComponent<CustomCharacter>() != null && GameManager.Instance.PrimaryPlayer.GetComponent<CustomCharacter>().data != null)
                {
                    return GameManager.Instance.PrimaryPlayer.GetComponent<CustomCharacter>().data.name;
                }
            }
            return orig(self, key);
        }

        public static string GetTalkingPlayerNameHook(Func<string> orig)
        {
            PlayerController talkingPlayer = GetTalkingPlayer();
            if (talkingPlayer.IsThief)
            {
                return "#THIEF_NAME";
            }
            if (talkingPlayer.GetComponent<CustomCharacter>() != null)
            {
                if (talkingPlayer.GetComponent<CustomCharacter>().data != null)
                {
                    return "#PLAYER_NAME_" + talkingPlayer.GetComponent<CustomCharacter>().data.nameShort.ToUpper();
                }
            }
            return orig();
        }

        private static PlayerController GetTalkingPlayer()
        {
            List<TalkDoerLite> allNpcs = StaticReferenceManager.AllNpcs;
            for (int i = 0; i < allNpcs.Count; i++)
            {
                if (allNpcs[i])
                {
                    if (!allNpcs[i].IsTalking || !allNpcs[i].TalkingPlayer || GameManager.Instance.HasPlayer(allNpcs[i].TalkingPlayer))
                    {
                        if (allNpcs[i].IsTalking && allNpcs[i].TalkingPlayer)
                        {
                            return allNpcs[i].TalkingPlayer;
                        }
                    }
                }
            }
            return GameManager.Instance.PrimaryPlayer;
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
                    UnityEngine.Object.DontDestroyOnLoad(dustup);
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

        public override void Exit()
        {
        }

        public static List<string> characterDeathNames = new List<string>
        {
            "#CHAR_ROGUE_SHORT",
            "#CHAR_CONVICT_SHORT",
            "#CHAR_ROBOT_SHORT",
            "#CHAR_MARINE_SHORT",
            "#CHAR_GUIDE_SHORT",
            "#CHAR_CULTIST_SHORT",
            "#CHAR_BULLET_SHORT",
            "#CHAR_PARADOX_SHORT",
            "#CHAR_GUNSLINGER_SHORT"
        };

        private class InstantDestroyDustup : MonoBehaviour
        {
            public void Start()
            {
                Destroy(base.gameObject);
            }
        }
    }
}

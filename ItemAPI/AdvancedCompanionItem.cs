using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using LichItems.ItemAPI;
using Gungeon;
using DirectionType = DirectionalAnimation.DirectionType;
using AnimationType = LichItems.ItemAPI.CompanionBuilder.AnimationType;
using MonoMod.RuntimeDetour;
using Dungeonator;
using System.Reflection;

namespace LichItems
{
    public class AdvancedCompanionItem : CompanionItem
    {
        public AdvancedCompanionItem()
        {
            SacrificeGunDuration = 30f;
            m_lastActiveSynergyTransformation = -1;
        }
        private void CreateCompanion(PlayerController owner)
        {
            if (PreventRespawnOnFloorLoad)
            {
                return;
            }
            if (BabyGoodMimicOrbitalOverridden)
            {
                GameObject extantCompanion = PlayerOrbitalItem.CreateOrbital(owner, (!OverridePlayerOrbitalItem.OrbitalFollowerPrefab) ? OverridePlayerOrbitalItem.OrbitalPrefab.gameObject : OverridePlayerOrbitalItem.OrbitalFollowerPrefab.gameObject, OverridePlayerOrbitalItem.OrbitalFollowerPrefab, null);
                SetExtantCompanion(extantCompanion);
                return;
            }
            string guid = CompanionGuid;
            m_lastActiveSynergyTransformation = -1;
            if (UsesAlternatePastPrefab && GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST)
            {
                guid = CompanionPastGuid;
            }
            else
            {
                if (UseAdvancedSynergies && AdvancedSynergies.Length > 0)
                {
                    for (int i = 0; i < AdvancedSynergies.Length; i++)
                    {
                        if ((AdvancedSynergies[i].UseStringSynergyDetectionInstead && LichsBookItem.PlayerHasActiveSynergy(m_owner, AdvancedSynergies[i].RequiredStringSynergy)) || (!AdvancedSynergies[i].UseStringSynergyDetectionInstead &&
                            owner.HasActiveBonusSynergy(AdvancedSynergies[i].RequiredSynergy, false)))
                        {
                            guid = AdvancedSynergies[i].SynergyCompanionGuid;
                            m_lastActiveSynergyTransformation = i;
                        }
                    }
                }
                else if (Synergies.Length > 0)
                {
                    for (int i = 0; i < Synergies.Length; i++)
                    {
                        if (owner.HasActiveBonusSynergy(Synergies[i].RequiredSynergy, false))
                        {
                            guid = Synergies[i].SynergyCompanionGuid;
                            m_lastActiveSynergyTransformation = i;
                        }
                    }
                }
            }
            AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(guid);
            Vector3 vector = owner.transform.position;
            if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER)
            {
                vector += new Vector3(1.125f, -0.3125f, 0f);
            }
            GameObject extantCompanion2 = UnityEngine.Object.Instantiate<GameObject>(orLoadByGuid.gameObject, vector, Quaternion.identity);
            SetExtantCompanion(extantCompanion2);
            CompanionController orAddComponent = ExtantCompanion.GetOrAddComponent<CompanionController>();
            orAddComponent.Initialize(owner);
            if (orAddComponent.specRigidbody)
            {
                PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(orAddComponent.specRigidbody, null, false);
            }
            if (orAddComponent.companionID == CompanionController.CompanionIdentifier.BABY_GOOD_MIMIC)
            {
                GameStatsManager.Instance.SetFlag(GungeonFlags.ITEMSPECIFIC_GOT_BABY_MIMIC, true);
            }
        }

        private void DestroyCompanion()
        {
            if (!ExtantCompanion)
            {
                return;
            }
            Destroy(ExtantCompanion);
            SetExtantCompanion(null);
        }

        public override void Update()
        {
            base.Update();
            if (!Dungeon.IsGenerating && m_owner && UseAdvancedSynergies && AdvancedSynergies.Length > 0)
            {
                if (!UsesAlternatePastPrefab || GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.CHARACTER_PAST)
                {
                    bool flag = false;
                    for (int i = AdvancedSynergies.Length - 1; i >= 0; i--)
                    {
                        if ((AdvancedSynergies[i].UseStringSynergyDetectionInstead && LichsBookItem.PlayerHasActiveSynergy(m_owner, AdvancedSynergies[i].RequiredStringSynergy)) || (!AdvancedSynergies[i].UseStringSynergyDetectionInstead &&
                            m_owner.HasActiveBonusSynergy(AdvancedSynergies[i].RequiredSynergy, false)))
                        {
                            if (m_lastActiveSynergyTransformation != i)
                            {
                                DestroyCompanion();
                                CreateCompanion(m_owner);
                            }
                            flag = true;
                            break;
                        }
                    }
                    if (!flag && m_lastActiveSynergyTransformation != -1)
                    {
                        DestroyCompanion();
                        CreateCompanion(m_owner);
                    }
                }
            }
        }

        public void SetExtantCompanion(GameObject companion)
        {
            extantCompanionInfo.SetValue(this, companion);
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            DestroyCompanion();
            player.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Combine(player.OnNewFloorLoaded, new Action<PlayerController>(HandleNewFloor));
            CreateCompanion(player);
        }

        private void HandleNewFloor(PlayerController obj)
        {
            DestroyCompanion();
            if (!PreventRespawnOnFloorLoad)
            {
                CreateCompanion(obj);
            }
        }

        public override DebrisObject Drop(PlayerController player)
        {
            DestroyCompanion();
            player.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Remove(player.OnNewFloorLoaded, new Action<PlayerController>(HandleNewFloor));
            return base.Drop(player);
        }

        public override void OnDestroy()
        {
            if (m_owner != null)
            {
                PlayerController owner = m_owner;
                owner.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Remove(owner.OnNewFloorLoaded, new Action<PlayerController>(HandleNewFloor));
            }
            DestroyCompanion();
            base.OnDestroy();
        }

        public bool UseAdvancedSynergies;
        [SerializeField]
        public AdvancedCompanionTransformSynergy[] AdvancedSynergies = new AdvancedCompanionTransformSynergy[0];
        private int m_lastActiveSynergyTransformation;
        private static FieldInfo extantCompanionInfo = typeof(CompanionItem).GetField("m_extantCompanion", BindingFlags.NonPublic | BindingFlags.Instance);
    }
}
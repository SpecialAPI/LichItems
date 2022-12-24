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
            this.SacrificeGunDuration = 30f;
            this.m_lastActiveSynergyTransformation = -1;
        }
        private void CreateCompanion(PlayerController owner)
        {
            if (this.PreventRespawnOnFloorLoad)
            {
                return;
            }
            if (this.BabyGoodMimicOrbitalOverridden)
            {
                GameObject extantCompanion = PlayerOrbitalItem.CreateOrbital(owner, (!this.OverridePlayerOrbitalItem.OrbitalFollowerPrefab) ? this.OverridePlayerOrbitalItem.OrbitalPrefab.gameObject : this.OverridePlayerOrbitalItem.OrbitalFollowerPrefab.gameObject, this.OverridePlayerOrbitalItem.OrbitalFollowerPrefab, null);
                this.SetExtantCompanion(extantCompanion);
                return;
            }
            string guid = this.CompanionGuid;
            this.m_lastActiveSynergyTransformation = -1;
            if (this.UsesAlternatePastPrefab && GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST)
            {
                guid = this.CompanionPastGuid;
            }
            else
            {
                if (this.UseAdvancedSynergies && this.AdvancedSynergies.Length > 0)
                {
                    for (int i = 0; i < this.AdvancedSynergies.Length; i++)
                    {
                        if ((this.AdvancedSynergies[i].UseStringSynergyDetectionInstead && LichsBookItem.PlayerHasActiveSynergy(this.m_owner, this.AdvancedSynergies[i].RequiredStringSynergy)) || (!this.AdvancedSynergies[i].UseStringSynergyDetectionInstead &&
                            owner.HasActiveBonusSynergy(this.AdvancedSynergies[i].RequiredSynergy, false)))
                        {
                            guid = this.AdvancedSynergies[i].SynergyCompanionGuid;
                            this.m_lastActiveSynergyTransformation = i;
                        }
                    }
                }
                else if (this.Synergies.Length > 0)
                {
                    for (int i = 0; i < this.Synergies.Length; i++)
                    {
                        if (owner.HasActiveBonusSynergy(this.Synergies[i].RequiredSynergy, false))
                        {
                            guid = this.Synergies[i].SynergyCompanionGuid;
                            this.m_lastActiveSynergyTransformation = i;
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
            this.SetExtantCompanion(extantCompanion2);
            CompanionController orAddComponent = this.ExtantCompanion.GetOrAddComponent<CompanionController>();
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
            if (!this.ExtantCompanion)
            {
                return;
            }
            Destroy(this.ExtantCompanion);
            this.SetExtantCompanion(null);
        }

        public override void Update()
        {
            base.Update();
            if (!Dungeon.IsGenerating && this.m_owner && this.UseAdvancedSynergies && this.AdvancedSynergies.Length > 0)
            {
                if (!this.UsesAlternatePastPrefab || GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.CHARACTER_PAST)
                {
                    bool flag = false;
                    for (int i = this.AdvancedSynergies.Length - 1; i >= 0; i--)
                    {
                        if ((this.AdvancedSynergies[i].UseStringSynergyDetectionInstead && LichsBookItem.PlayerHasActiveSynergy(this.m_owner, this.AdvancedSynergies[i].RequiredStringSynergy)) || (!this.AdvancedSynergies[i].UseStringSynergyDetectionInstead &&
                            this.m_owner.HasActiveBonusSynergy(this.AdvancedSynergies[i].RequiredSynergy, false)))
                        {
                            if (this.m_lastActiveSynergyTransformation != i)
                            {
                                this.DestroyCompanion();
                                this.CreateCompanion(this.m_owner);
                            }
                            flag = true;
                            break;
                        }
                    }
                    if (!flag && this.m_lastActiveSynergyTransformation != -1)
                    {
                        this.DestroyCompanion();
                        this.CreateCompanion(this.m_owner);
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
            this.DestroyCompanion();
            player.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Combine(player.OnNewFloorLoaded, new Action<PlayerController>(this.HandleNewFloor));
            this.CreateCompanion(player);
        }

        private void HandleNewFloor(PlayerController obj)
        {
            this.DestroyCompanion();
            if (!this.PreventRespawnOnFloorLoad)
            {
                this.CreateCompanion(obj);
            }
        }

        public override DebrisObject Drop(PlayerController player)
        {
            this.DestroyCompanion();
            player.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Remove(player.OnNewFloorLoaded, new Action<PlayerController>(this.HandleNewFloor));
            return base.Drop(player);
        }

        public override void OnDestroy()
        {
            if (this.m_owner != null)
            {
                PlayerController owner = this.m_owner;
                owner.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Remove(owner.OnNewFloorLoaded, new Action<PlayerController>(this.HandleNewFloor));
            }
            this.DestroyCompanion();
            base.OnDestroy();
        }

        public bool UseAdvancedSynergies;
        [SerializeField]
        public AdvancedCompanionTransformSynergy[] AdvancedSynergies = new AdvancedCompanionTransformSynergy[0];
        private int m_lastActiveSynergyTransformation;
        private static FieldInfo extantCompanionInfo = typeof(CompanionItem).GetField("m_extantCompanion", BindingFlags.NonPublic | BindingFlags.Instance);
    }
}
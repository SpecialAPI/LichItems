using Dungeonator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LichItems.ItemAPI
{
    public class AdvancedTransformGunSynergyProcessor : MonoBehaviour
    {
        private void Awake()
        {
            m_gun = base.GetComponent<Gun>();
        }

        private void Update()
        {
            if (Dungeon.IsGenerating || Dungeon.ShouldAttemptToLoadFromMidgameSave)
            {
                return;
            }
            if (m_gun && m_gun.CurrentOwner is PlayerController)
            {
                PlayerController playerController = m_gun.CurrentOwner as PlayerController;
                if (!m_gun.enabled)
                {
                    return;
                }
                if (LichsBookItem.PlayerHasActiveSynergy(playerController, SynergyToCheck) && !m_transformed)
                {
                    m_transformed = true;
                    m_gun.TransformToTargetGun(PickupObjectDatabase.GetById(SynergyGunId) as Gun);
                    AdvancedGunBehaviour synergyBehaviour = PickupObjectDatabase.GetById(SynergyGunId).GetComponent<AdvancedGunBehaviour>();
                    if (synergyBehaviour != null)
                    {
                        AdvancedGunBehaviour behav = gameObject.GetOrAddComponent<AdvancedGunBehaviour>();
                        behav.preventNormalFireAudio = synergyBehaviour.preventNormalFireAudio;
                        behav.preventNormalReloadAudio = synergyBehaviour.preventNormalReloadAudio;
                        behav.overrideNormalFireAudio = synergyBehaviour.overrideNormalFireAudio;
                        behav.overrideReloadSwitchGroup = synergyBehaviour.overrideReloadSwitchGroup;
                        behav.overrideNormalReloadAudio = synergyBehaviour.overrideNormalReloadAudio;
                        behav.usesOverrideHeroSwordCooldown = synergyBehaviour.usesOverrideHeroSwordCooldown;
                        behav.overrideHeroSwordCooldown = synergyBehaviour.overrideHeroSwordCooldown;
                    }
                    else
                    {
                        if (GetComponent<AdvancedGunBehaviour>() != null)
                        {
                            Destroy(GetComponent<AdvancedGunBehaviour>());
                        }
                    }
                    if (ShouldResetAmmoAfterTransformation)
                    {
                        m_gun.ammo = ResetAmmoCount;
                    }
                }
                else if (!LichsBookItem.PlayerHasActiveSynergy(playerController, SynergyToCheck) && m_transformed)
                {
                    m_transformed = false;
                    m_gun.TransformToTargetGun(PickupObjectDatabase.GetById(NonSynergyGunId) as Gun);
                    AdvancedGunBehaviour normalBehaviour = PickupObjectDatabase.GetById(NonSynergyGunId).GetComponent<AdvancedGunBehaviour>();
                    if (normalBehaviour != null)
                    {
                        AdvancedGunBehaviour behav = gameObject.GetOrAddComponent<AdvancedGunBehaviour>();
                        behav.preventNormalFireAudio = normalBehaviour.preventNormalFireAudio;
                        behav.preventNormalReloadAudio = normalBehaviour.preventNormalReloadAudio;
                        behav.overrideNormalFireAudio = normalBehaviour.overrideNormalFireAudio;
                        behav.overrideReloadSwitchGroup = normalBehaviour.overrideReloadSwitchGroup;
                        behav.overrideNormalReloadAudio = normalBehaviour.overrideNormalReloadAudio;
                        behav.usesOverrideHeroSwordCooldown = normalBehaviour.usesOverrideHeroSwordCooldown;
                        behav.overrideHeroSwordCooldown = normalBehaviour.overrideHeroSwordCooldown;
                    }
                    else
                    {
                        if (GetComponent<AdvancedGunBehaviour>() != null)
                        {
                            Destroy(GetComponent<AdvancedGunBehaviour>());
                        }
                    }
                    if (ShouldResetAmmoAfterTransformation)
                    {
                        m_gun.ammo = ResetAmmoCount;
                    }
                }
            }
            else if (m_gun && !m_gun.CurrentOwner && m_transformed)
            {
                m_transformed = false;
                m_gun.TransformToTargetGun(PickupObjectDatabase.GetById(NonSynergyGunId) as Gun);
                if (ShouldResetAmmoAfterTransformation)
                {
                    m_gun.ammo = ResetAmmoCount;
                }
            }
            ShouldResetAmmoAfterTransformation = false;
        }

        public string SynergyToCheck;
        public int NonSynergyGunId;
        public int SynergyGunId;
        private Gun m_gun;
        private bool m_transformed;
        public bool ShouldResetAmmoAfterTransformation;
        public int ResetAmmoCount;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LichItems.ItemAPI
{
    public class AdvancedTransformGunSynergyProcessor : MonoBehaviour
    {

        private void Awake()
        {
            this.m_gun = base.GetComponent<Gun>();
        }

        private void Update()
        {
            if (Dungeon.IsGenerating || Dungeon.ShouldAttemptToLoadFromMidgameSave)
            {
                return;
            }
            if (this.m_gun && this.m_gun.CurrentOwner is PlayerController)
            {
                PlayerController playerController = this.m_gun.CurrentOwner as PlayerController;
                if (!this.m_gun.enabled)
                {
                    return;
                }
                if (LichsBookItem.PlayerHasActiveSynergy(playerController, this.SynergyToCheck) && !this.m_transformed)
                {
                    this.m_transformed = true;
                    this.m_gun.TransformToTargetGun(PickupObjectDatabase.GetById(this.SynergyGunId) as Gun);
                    AdvancedGunBehaviour synergyBehaviour = PickupObjectDatabase.GetById(this.SynergyGunId).GetComponent<AdvancedGunBehaviour>();
                    if (synergyBehaviour != null)
                    {
                        AdvancedGunBehaviour behav = this.gameObject.GetOrAddComponent<AdvancedGunBehaviour>();
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
                        if (this.GetComponent<AdvancedGunBehaviour>() != null)
                        {
                            Destroy(this.GetComponent<AdvancedGunBehaviour>());
                        }
                    }
                    if (this.ShouldResetAmmoAfterTransformation)
                    {
                        this.m_gun.ammo = this.ResetAmmoCount;
                    }
                }
                else if (!LichsBookItem.PlayerHasActiveSynergy(playerController, this.SynergyToCheck) && this.m_transformed)
                {
                    this.m_transformed = false;
                    this.m_gun.TransformToTargetGun(PickupObjectDatabase.GetById(this.NonSynergyGunId) as Gun);
                    AdvancedGunBehaviour normalBehaviour = PickupObjectDatabase.GetById(this.NonSynergyGunId).GetComponent<AdvancedGunBehaviour>();
                    if (normalBehaviour != null)
                    {
                        AdvancedGunBehaviour behav = this.gameObject.GetOrAddComponent<AdvancedGunBehaviour>();
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
                        if (this.GetComponent<AdvancedGunBehaviour>() != null)
                        {
                            Destroy(this.GetComponent<AdvancedGunBehaviour>());
                        }
                    }
                    if (this.ShouldResetAmmoAfterTransformation)
                    {
                        this.m_gun.ammo = this.ResetAmmoCount;
                    }
                }
            }
            else if (this.m_gun && !this.m_gun.CurrentOwner && this.m_transformed)
            {
                this.m_transformed = false;
                this.m_gun.TransformToTargetGun(PickupObjectDatabase.GetById(this.NonSynergyGunId) as Gun);
                if (this.ShouldResetAmmoAfterTransformation)
                {
                    this.m_gun.ammo = this.ResetAmmoCount;
                }
            }
            this.ShouldResetAmmoAfterTransformation = false;
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

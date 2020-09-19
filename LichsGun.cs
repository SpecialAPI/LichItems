using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gungeon;
using UnityEngine;
using LichItems.ItemAPI;
using Dungeonator;

namespace LichItems
{
    class LichsGun
    {
        public static void Init()
        {
            Gun gun = ETGMod.Databases.Items.NewGun("Lich's Gun", "lichsgun");
            Game.Items.Rename("outdated_gun_mods:lich's_gun", "spapi:lichs_gun");
            GunExt.SetShortDescription(gun, "Bullet Curve");
            GunExt.SetLongDescription(gun, "This is Gun from Gungeon Master.\nLooking into the muzzle, you can sometimes hear a scream of anger from a loaded bullet.");
            GunExt.SetupSprite(gun, null, "lichsgun_idle_001", 10);
            GunExt.SetAnimationFPS(gun, gun.shootAnimation, 12);
            GunExt.AddProjectileModuleFrom(gun, "klobb", true, false);
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
            gun.DefaultModule.angleVariance = 0;
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.MEDIUM_BULLET;
            InputGuidedProjectile projectile = CopyFields<InputGuidedProjectile>(UnityEngine.Object.Instantiate((PickupObjectDatabase.GetById(183) as Gun).DefaultModule.projectiles[0]));
            projectile.trackingSpeed = 240f;
            projectile.dumbfireTime = -1f;
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;
            projectile.baseData.damage = 6f;
            projectile.shouldRotate = true;
            projectile.name = "LichsGun_Projectile";
            projectile.baseData.range = 20f;
            gun.reloadClipLaunchFrame = 0;
            gun.DefaultModule.cooldownTime = 0.1f;
            gun.DefaultModule.numberOfShotsInClip = 6;
            for (int i = 0; i < gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).frames.Length; i++)
            {
                gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).frames[i].triggerEvent = true;
                tk2dSpriteDefinition def = gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).frames[i].spriteCollection.spriteDefinitions[gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).frames[i].spriteId];
                Vector2 offset = new Vector2(((float)fireOffsets[i].x) / 16f, ((float)fireOffsets[i].y) / 16f);
                LichsBookItem.MakeOffset(def, offset);
            }
            for (int i = 0; i < gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.reloadAnimation).frames.Length; i++)
            {
                gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.reloadAnimation).frames[i].triggerEvent = true;
                tk2dSpriteDefinition def = gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.reloadAnimation).frames[i].spriteCollection.spriteDefinitions[gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.reloadAnimation).frames[i].spriteId];
                Vector2 offset = new Vector2(((float)reloadOffsets[i].x) / 16f, ((float)reloadOffsets[i].y) / 16f);
                LichsBookItem.MakeOffset(def, offset);
            }
            tk2dSpriteAnimationClip reloadClip = gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.reloadAnimation);
            reloadClip.frames = new tk2dSpriteAnimationFrame[0];
            foreach (string text in reloadFrames)
            {
                tk2dSpriteAnimationFrame frame = new tk2dSpriteAnimationFrame { spriteCollection = ETGMod.Databases.Items.WeaponCollection, spriteId = ETGMod.Databases.Items.WeaponCollection.GetSpriteIdByName(text) };
                reloadClip.frames = reloadClip.frames.Concat(new tk2dSpriteAnimationFrame[] { frame }).ToArray();
            }
            tk2dSpriteAnimationClip shootClip = gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation);
            shootClip.frames = new tk2dSpriteAnimationFrame[0];
            foreach (string text in shootFrames)
            {
                tk2dSpriteAnimationFrame frame = new tk2dSpriteAnimationFrame { spriteCollection = ETGMod.Databases.Items.WeaponCollection, spriteId = ETGMod.Databases.Items.WeaponCollection.GetSpriteIdByName(text) };
                shootClip.frames = shootClip.frames.Concat(new tk2dSpriteAnimationFrame[] { frame }).ToArray();
            }
            VFXPool muzzleFlashVfx = new VFXPool { type = VFXPoolType.All };
            VFXComplex complex = new VFXComplex();
            VFXObject vfxObj = new VFXObject
            {
                alignment = VFXAlignment.Fixed,
                attached = false,
                orphaned = true,
                persistsOnDeath = false,
                destructible = true,
                usesZHeight = true,
                zHeight = -0.25f
            };
            List<int> muzzleFlashIds = new List<int>
            {
                VFXCollection.GetSpriteIdByName("lichsgun_vfx_001"),
                VFXCollection.GetSpriteIdByName("lichsgun_vfx_002"),
                VFXCollection.GetSpriteIdByName("lichsgun_vfx_003"),
                VFXCollection.GetSpriteIdByName("lichsgun_vfx_004")
            };
            GameObject muzzleFlashVfxObject = new GameObject("LichsGunMuzzleflash");
            muzzleFlashVfxObject.AddComponent<tk2dSprite>().SetSprite(VFXCollection, muzzleFlashIds[0]);
            muzzleFlashVfxObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(muzzleFlashVfxObject);
            UnityEngine.Object.DontDestroyOnLoad(muzzleFlashVfxObject);
            tk2dSpriteAnimator animator = muzzleFlashVfxObject.AddComponent<tk2dSpriteAnimator>();
            SpriteBuilder.AddAnimation(animator, muzzleFlashVfxObject.GetComponent<tk2dBaseSprite>().Collection, muzzleFlashIds, "flash", tk2dSpriteAnimationClip.WrapMode.Once).fps = 16;
            animator.playAutomatically = true;
            animator.DefaultClipId = animator.GetClipIdByName("flash");
            SpriteAnimatorKiller killer = muzzleFlashVfxObject.AddComponent<SpriteAnimatorKiller>();
            killer.fadeTime = -1f;
            killer.delayDestructionTime = -1f;
            killer.animator = animator;
            foreach (int id in muzzleFlashIds)
            {
                LichsBookItem.ConstructOffsetsFromAnchor(VFXCollection.spriteDefinitions[id], tk2dBaseSprite.Anchor.MiddleLeft);
            }
            vfxObj.effect = muzzleFlashVfxObject;
            complex.effects = new VFXObject[] { vfxObj };
            muzzleFlashVfx.effects = new VFXComplex[] { complex };
            gun.gunSwitchGroup = "SAA";
            gun.muzzleFlashEffects = muzzleFlashVfx;
            gun.reloadTime = 1.1f;
            gun.SetBaseMaxAmmo(200);
            gun.quality = PickupObject.ItemQuality.D;
            gun.barrelOffset.transform.localPosition = new Vector3(1.1875f, 0.5625f, 0f);
            gun.gunClass = GunClass.PISTOL;
            ETGMod.Databases.Items.Add(gun, null, "ANY");
            InitSynergyForm(gun);
        }

        public static void InitSynergyForm(Gun original)
        {
            Gun gun = ETGMod.Databases.Items.NewGun("Synergy Lich's Gun", "lichsgun2");
            Game.Items.Rename("outdated_gun_mods:synergy_lich's_gun", "spapi:lichs_gun+master_of_the_gungeon");
            GunExt.SetShortDescription(gun, "Bullet Curve");
            GunExt.SetLongDescription(gun, "This is Gun from Gungeon Master.\nLooking into the muzzle, you can sometimes hear a scream of anger from a loaded bullet.");
            GunExt.SetupSprite(gun, null, "lichsgun2_idle_001", 10);
            GunExt.SetAnimationFPS(gun, gun.shootAnimation, 12);
            GunExt.AddProjectileModuleFrom(gun, "klobb", true, false);
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
            gun.DefaultModule.angleVariance = 0;
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.MEDIUM_BULLET;
            InputGuidedProjectile projectile = CopyFields<InputGuidedProjectile>(UnityEngine.Object.Instantiate((PickupObjectDatabase.GetById(183) as Gun).DefaultModule.projectiles[0]));
            projectile.trackingSpeed = 240f;
            projectile.dumbfireTime = -1f;
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;
            projectile.baseData.damage = 8f;
            projectile.shouldRotate = true;
            projectile.name = "LichsGun2_Projectile";
            projectile.baseData.range = 30f;
            gun.reloadClipLaunchFrame = 0;
            gun.DefaultModule.cooldownTime = 0.1f;
            gun.DefaultModule.numberOfShotsInClip = 17;
            for (int i = 0; i < gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).frames.Length; i++)
            {
                gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).frames[i].triggerEvent = true;
                tk2dSpriteDefinition def = gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).frames[i].spriteCollection.spriteDefinitions[gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).frames[i].spriteId];
                Vector2 offset = new Vector2(((float)synergyFireOffsets[i].x) / 16f, ((float)synergyFireOffsets[i].y) / 16f);
                LichsBookItem.MakeOffset(def, offset);
            }
            for (int i = 0; i < gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.reloadAnimation).frames.Length; i++)
            {
                gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.reloadAnimation).frames[i].triggerEvent = true;
                tk2dSpriteDefinition def = gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.reloadAnimation).frames[i].spriteCollection.spriteDefinitions[gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.reloadAnimation).frames[i].spriteId];
                Vector2 offset = new Vector2(((float)synergyReloadOffsets[i].x) / 16f, ((float)synergyReloadOffsets[i].y) / 16f);
                LichsBookItem.MakeOffset(def, offset);
            }
            gun.gunSwitchGroup = "Colt1851";
            gun.gunHandedness = GunHandedness.HiddenOneHanded;
            gun.muzzleFlashEffects = original.muzzleFlashEffects;
            gun.reloadTime = 1.1f;
            gun.SetBaseMaxAmmo(200);
            gun.quality = PickupObject.ItemQuality.SPECIAL;
            gun.barrelOffset.transform.localPosition = new Vector3(1.1875f, 0.5625f, 0f);
            gun.gunClass = GunClass.PISTOL;
            ETGMod.Databases.Items.Add(gun, null, "ANY");
            AdvancedTransformGunSynergyProcessor processor = original.gameObject.AddComponent<AdvancedTransformGunSynergyProcessor>();
            processor.NonSynergyGunId = original.PickupObjectId;
            processor.SynergyGunId = gun.PickupObjectId;
            processor.SynergyToCheck = "Master of Gungeon";
        }

        public static tk2dSpriteCollectionData VFXCollection
        {
            get
            {
                return (PickupObjectDatabase.GetById(95) as Gun).clipObject.GetComponent<tk2dBaseSprite>().Collection;
            }
        }

        public static T CopyFields<T>(Projectile sample2) where T : Projectile
        {
            T sample = sample2.gameObject.AddComponent<T>();
            sample.PossibleSourceGun = sample2.PossibleSourceGun;
            sample.SpawnedFromOtherPlayerProjectile = sample2.SpawnedFromOtherPlayerProjectile;
            sample.PlayerProjectileSourceGameTimeslice = sample2.PlayerProjectileSourceGameTimeslice;
            sample.BulletScriptSettings = sample2.BulletScriptSettings;
            sample.damageTypes = sample2.damageTypes;
            sample.allowSelfShooting = sample2.allowSelfShooting;
            sample.collidesWithPlayer = sample2.collidesWithPlayer;
            sample.collidesWithProjectiles = sample2.collidesWithProjectiles;
            sample.collidesOnlyWithPlayerProjectiles = sample2.collidesOnlyWithPlayerProjectiles;
            sample.projectileHitHealth = sample2.projectileHitHealth;
            sample.collidesWithEnemies = sample2.collidesWithEnemies;
            sample.shouldRotate = sample2.shouldRotate;
            sample.shouldFlipVertically = sample2.shouldFlipVertically;
            sample.shouldFlipHorizontally = sample2.shouldFlipHorizontally;
            sample.ignoreDamageCaps = sample2.ignoreDamageCaps;
            sample.baseData = sample2.baseData;
            sample.AppliesPoison = sample2.AppliesPoison;
            sample.PoisonApplyChance = sample2.PoisonApplyChance;
            sample.healthEffect = sample2.healthEffect;
            sample.AppliesSpeedModifier = sample2.AppliesSpeedModifier;
            sample.SpeedApplyChance = sample2.SpeedApplyChance;
            sample.speedEffect = sample2.speedEffect;
            sample.AppliesCharm = sample2.AppliesCharm;
            sample.CharmApplyChance = sample2.CharmApplyChance;
            sample.charmEffect = sample2.charmEffect;
            sample.AppliesFreeze = sample2.AppliesFreeze;
            sample.FreezeApplyChance = sample2.FreezeApplyChance;
            sample.freezeEffect = (sample2.freezeEffect);
            sample.AppliesFire = sample2.AppliesFire;
            sample.FireApplyChance = sample2.FireApplyChance;
            sample.fireEffect = (sample2.fireEffect);
            sample.AppliesStun = sample2.AppliesStun;
            sample.StunApplyChance = sample2.StunApplyChance;
            sample.AppliedStunDuration = sample2.AppliedStunDuration;
            sample.AppliesBleed = sample2.AppliesBleed;
            sample.bleedEffect = (sample2.bleedEffect);
            sample.AppliesCheese = sample2.AppliesCheese;
            sample.CheeseApplyChance = sample2.CheeseApplyChance;
            sample.cheeseEffect = (sample2.cheeseEffect);
            sample.BleedApplyChance = sample2.BleedApplyChance;
            sample.CanTransmogrify = sample2.CanTransmogrify;
            sample.ChanceToTransmogrify = sample2.ChanceToTransmogrify;
            sample.TransmogrifyTargetGuids = sample2.TransmogrifyTargetGuids;
            sample.BossDamageMultiplier = sample2.BossDamageMultiplier;
            sample.SpawnedFromNonChallengeItem = sample2.SpawnedFromNonChallengeItem;
            sample.TreatedAsNonProjectileForChallenge = sample2.TreatedAsNonProjectileForChallenge;
            sample.hitEffects = sample2.hitEffects;
            sample.CenterTilemapHitEffectsByProjectileVelocity = sample2.CenterTilemapHitEffectsByProjectileVelocity;
            sample.wallDecals = sample2.wallDecals;
            sample.persistTime = sample2.persistTime;
            sample.angularVelocity = sample2.angularVelocity;
            sample.angularVelocityVariance = sample2.angularVelocityVariance;
            sample.spawnEnemyGuidOnDeath = sample2.spawnEnemyGuidOnDeath;
            sample.HasFixedKnockbackDirection = sample2.HasFixedKnockbackDirection;
            sample.FixedKnockbackDirection = sample2.FixedKnockbackDirection;
            sample.pierceMinorBreakables = sample2.pierceMinorBreakables;
            sample.objectImpactEventName = sample2.objectImpactEventName;
            sample.enemyImpactEventName = sample2.enemyImpactEventName;
            sample.onDestroyEventName = sample2.onDestroyEventName;
            sample.additionalStartEventName = sample2.additionalStartEventName;
            sample.IsRadialBurstLimited = sample2.IsRadialBurstLimited;
            sample.MaxRadialBurstLimit = sample2.MaxRadialBurstLimit;
            sample.AdditionalBurstLimits = sample2.AdditionalBurstLimits;
            sample.AppliesKnockbackToPlayer = sample2.AppliesKnockbackToPlayer;
            sample.PlayerKnockbackForce = sample2.PlayerKnockbackForce;
            sample.HasDefaultTint = sample2.HasDefaultTint;
            sample.DefaultTintColor = sample2.DefaultTintColor;
            sample.IsCritical = sample2.IsCritical;
            sample.BlackPhantomDamageMultiplier = sample2.BlackPhantomDamageMultiplier;
            sample.PenetratesInternalWalls = sample2.PenetratesInternalWalls;
            sample.neverMaskThis = sample2.neverMaskThis;
            sample.isFakeBullet = sample2.isFakeBullet;
            sample.CanBecomeBlackBullet = sample2.CanBecomeBlackBullet;
            sample.TrailRenderer = sample2.TrailRenderer;
            sample.CustomTrailRenderer = sample2.CustomTrailRenderer;
            sample.ParticleTrail = sample2.ParticleTrail;
            sample.DelayedDamageToExploders = sample2.DelayedDamageToExploders;
            sample.OnHitEnemy = sample2.OnHitEnemy;
            sample.OnWillKillEnemy = sample2.OnWillKillEnemy;
            sample.OnBecameDebris = sample2.OnBecameDebris;
            sample.OnBecameDebrisGrounded = sample2.OnBecameDebrisGrounded;
            sample.IsBlackBullet = sample2.IsBlackBullet;
            sample.statusEffectsToApply = sample2.statusEffectsToApply;
            sample.AdditionalScaleMultiplier = sample2.AdditionalScaleMultiplier;
            sample.ModifyVelocity = sample2.ModifyVelocity;
            sample.CurseSparks = sample2.CurseSparks;
            sample.PreMoveModifiers = sample2.PreMoveModifiers;
            sample.OverrideMotionModule = sample2.OverrideMotionModule;
            sample.Shooter = sample2.Shooter;
            sample.Owner = sample2.Owner;
            sample.Speed = sample2.Speed;
            sample.Direction = sample2.Direction;
            sample.DestroyMode = sample2.DestroyMode;
            sample.Inverted = sample2.Inverted;
            sample.LastVelocity = sample2.LastVelocity;
            sample.ManualControl = sample2.ManualControl;
            sample.ForceBlackBullet = sample2.ForceBlackBullet;
            sample.IsBulletScript = sample2.IsBulletScript;
            sample.OverrideTrailPoint = sample2.OverrideTrailPoint;
            sample.SkipDistanceElapsedCheck = sample2.SkipDistanceElapsedCheck;
            sample.ImmuneToBlanks = sample2.ImmuneToBlanks;
            sample.ImmuneToSustainedBlanks = sample2.ImmuneToSustainedBlanks;
            sample.ForcePlayerBlankable = sample2.ForcePlayerBlankable;
            sample.IsReflectedBySword = sample2.IsReflectedBySword;
            sample.LastReflectedSlashId = sample2.LastReflectedSlashId;
            sample.TrailRendererController = sample2.TrailRendererController;
            sample.braveBulletScript = sample2.braveBulletScript;
            sample.TrapOwner = sample2.TrapOwner;
            sample.SuppressHitEffects = sample2.SuppressHitEffects;
            UnityEngine.Object.Destroy(sample2);
            return sample;
        }

        public static List<string> shootFrames = new List<string>
        {
            "lichsgun_fire_001",
            "lichsgun_fire_002",
            "lichsgun_fire_003",
            "lichsgun_fire_004",
            "lichsgun_idle_001"
        };
        public static List<string> reloadFrames = new List<string>
        {
            "lichsgun_reload_001",
            "lichsgun_reload_001",
            "lichsgun_reload_002",
            "lichsgun_reload_003",
            "lichsgun_reload_002",
            "lichsgun_reload_003",
            "lichsgun_reload_004"
        };
        public static List<IntVector2> fireOffsets = new List<IntVector2>
        {
            new IntVector2(0, 0),
            new IntVector2(-3, 1),
            new IntVector2(-2, 0),
            new IntVector2(1, -1)
        };
        public static List<IntVector2> reloadOffsets = new List<IntVector2>
        {
            new IntVector2(1, -2),
            new IntVector2(-3, -1),
            new IntVector2(-3, 0),
            new IntVector2(-1, 1)
        };
        public static List<IntVector2> synergyFireOffsets = new List<IntVector2>
        {
            new IntVector2(-1, 0),
            new IntVector2(-3, 1),
            new IntVector2(-2, 0),
            new IntVector2(1, -1)
        };
        public static List<IntVector2> synergyReloadOffsets = new List<IntVector2>
        {
            new IntVector2(0, 3),
            new IntVector2(0, 0),
            new IntVector2(0, 0),
            new IntVector2(0, 0)
        };
    }

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
                    if (this.ShouldResetAmmoAfterTransformation)
                    {
                        this.m_gun.ammo = this.ResetAmmoCount;
                    }
                }
                else if (!LichsBookItem.PlayerHasActiveSynergy(playerController, this.SynergyToCheck) && this.m_transformed)
                {
                    this.m_transformed = false;
                    this.m_gun.TransformToTargetGun(PickupObjectDatabase.GetById(this.NonSynergyGunId) as Gun);
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

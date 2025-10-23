using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gungeon;
using UnityEngine;
using Alexandria.ItemAPI;
using Dungeonator;
using Alexandria.SoundAPI;


namespace LichItems
{
    public class LichsGun
    {
        public static void Init()
        {
            var gun = ETGMod.Databases.Items.NewGun("Lich's Gun", "lichsgun");
            Game.Items.Rename("outdated_gun_mods:lich's_gun", "spapi:lichs_gun");
            gun.SetShortDescription("The Freeshooter");
            gun.SetLongDescription("The gun of the Gungeon master. The bullets of this gun can be guided after being fired.");
            gun.SetupSprite(null, "lichsgun_idle_001", 10);
            gun.SetAnimationFPS(gun.shootAnimation, 12);

            gun.AddProjectileModuleFrom("klobb", true, false);
            var projectile = CopyFields<InputGuidedProjectile>(UnityEngine.Object.Instantiate((PickupObjectDatabase.GetById(183) as Gun).DefaultModule.projectiles[0]));
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);

            projectile.baseData.damage = 5f;
            projectile.shouldRotate = true;
            projectile.name = "LichsGun_Projectile";
            projectile.baseData.range = 18f;
            projectile.baseData.speed = 22f;
            projectile.trackingSpeed = 500f;
            projectile.dumbfireTime = -1f;
            GunTools.SetProjectileSpriteRight(projectile, "lichsgun_projectile_001", 6, 6, false, tk2dBaseSprite.Anchor.MiddleCenter, overrideProjectileToCopyFrom: (PickupObjectDatabase.GetById(183) as Gun).DefaultModule.projectiles[0]);
            gun.DefaultModule.projectiles[0] = projectile;

            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
            gun.DefaultModule.angleVariance = 0;
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.MEDIUM_BULLET;
            gun.DefaultModule.cooldownTime = 0.1f;
            gun.DefaultModule.numberOfShotsInClip = 6;

            gun.reloadClipLaunchFrame = 0;
            gun.gunSwitchGroup = "SAA";
            gun.reloadTime = 1.1f;
            gun.SetBaseMaxAmmo(350);
            gun.quality = PickupObject.ItemQuality.SPECIAL;
            gun.barrelOffset.transform.localPosition = new Vector3(1.1875f, 0.5625f, 0f);
            gun.gunClass = GunClass.PISTOL;
            gun.InfiniteAmmo = true;

            var anim = gun.GetComponent<tk2dSpriteAnimator>();
            var shootAnim = anim.GetClipByName(gun.shootAnimation);
            var reloadAnim = anim.GetClipByName(gun.reloadAnimation);

            for (int i = 0; i < shootAnim.frames.Length; i++)
            {
                var frame = shootAnim.frames[i];
                var def = frame.spriteCollection.spriteDefinitions[frame.spriteId];
                var offset = new Vector2(fireOffsets[i].x / 16f, fireOffsets[i].y / 16f);

                GunTools.MakeOffset(def, offset);
            }

            for (int i = 0; i < reloadAnim.frames.Length; i++)
            {
                var frame = reloadAnim.frames[i];
                var def = frame.spriteCollection.spriteDefinitions[frame.spriteId];
                var offset = new Vector2(reloadOffsets[i].x / 16f, reloadOffsets[i].y / 16f);

                GunTools.MakeOffset(def, offset);
            }

            shootAnim.frames = new tk2dSpriteAnimationFrame[shootFrames.Count];
            for (var i = 0; i < shootFrames.Count; i++)
            {
                var sprite = shootFrames[i];

                shootAnim.frames[i] = new()
                {
                    spriteCollection = ETGMod.Databases.Items.WeaponCollection,
                    spriteId = ETGMod.Databases.Items.WeaponCollection.GetSpriteIdByName(sprite)
                };
            }

            reloadAnim.frames = new tk2dSpriteAnimationFrame[reloadFrames.Count];
            for (var i = 0; i < reloadFrames.Count; i++)
            {
                var sprite = reloadFrames[i];

                reloadAnim.frames[i] = new()
                {
                    spriteCollection = ETGMod.Databases.Items.WeaponCollection,
                    spriteId = ETGMod.Databases.Items.WeaponCollection.GetSpriteIdByName(sprite)
                };
            }

            var muzzleFlashIds = new List<int>
            {
                VFXCollection.GetSpriteIdByName("lichsgun_vfx_001"),
                VFXCollection.GetSpriteIdByName("lichsgun_vfx_002"),
                VFXCollection.GetSpriteIdByName("lichsgun_vfx_003"),
                VFXCollection.GetSpriteIdByName("lichsgun_vfx_004")
            };

            var muzzleFlashVfxObject = new GameObject("LichsGunMuzzleflash");
            muzzleFlashVfxObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(muzzleFlashVfxObject);
            UnityEngine.Object.DontDestroyOnLoad(muzzleFlashVfxObject);

            muzzleFlashVfxObject.AddComponent<tk2dSprite>().SetSprite(VFXCollection, muzzleFlashIds[0]);

            var animator = muzzleFlashVfxObject.AddComponent<tk2dSpriteAnimator>();
            SpriteBuilder.AddAnimation(animator, muzzleFlashVfxObject.GetComponent<tk2dBaseSprite>().Collection, muzzleFlashIds, "flash", tk2dSpriteAnimationClip.WrapMode.Once).fps = 16;
            animator.playAutomatically = true;
            animator.DefaultClipId = animator.GetClipIdByName("flash");

            var killer = muzzleFlashVfxObject.AddComponent<SpriteAnimatorKiller>();
            killer.fadeTime = -1f;
            killer.delayDestructionTime = -1f;
            killer.animator = animator;

            foreach (int id in muzzleFlashIds)
                GunTools.ConstructOffsetsFromAnchor(VFXCollection.spriteDefinitions[id], tk2dBaseSprite.Anchor.MiddleLeft);

            gun.muzzleFlashEffects = new()
            {
                type = VFXPoolType.All,
                effects =
                [
                    new()
                    {
                        effects =
                        [
                            new()
                            {
                                effect = muzzleFlashVfxObject,
                                alignment = VFXAlignment.Fixed,
                                attached = false,
                                orphaned = true,
                                persistsOnDeath = false,
                                destructible = true,
                                usesZHeight = true,
                                zHeight = -0.25f
                            }
                        ]
                    }
                ]
            };

            ETGMod.Databases.Items.Add(gun, null, "ANY");
            InitSynergyForm(gun);
        }

        public static void InitSynergyForm(Gun original)
        {
            var gun = ETGMod.Databases.Items.NewGun("Synergy Lich's Gun", "lichsgun2");
            Game.Items.Rename("outdated_gun_mods:synergy_lich's_gun", "spapi:lichs_gun+master_of_the_gungeon");
            gun.SetShortDescription("The Freeshooter");
            gun.SetLongDescription("The gun of the Gungeon master. The bullets of this gun can be guided after being fired.");
            gun.SetupSprite(null, "lichsgun2_idle_001", 10);
            gun.SetAnimationFPS(gun.shootAnimation, 12);

            gun.AddProjectileModuleFrom("klobb", true, false);
            var projectile = CopyFields<InputGuidedProjectile>(UnityEngine.Object.Instantiate((PickupObjectDatabase.GetById(183) as Gun).DefaultModule.projectiles[0]));
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);

            projectile.baseData.damage = 3f;
            projectile.shouldRotate = true;
            projectile.name = "LichsGun2_Projectile";
            projectile.baseData.range = 100f;
            projectile.baseData.speed = 14f;
            projectile.dumbfireTime = -1f;
            GunTools.SetProjectileSpriteRight(projectile, "lichsgun_projectile_001", 6, 6, false, tk2dBaseSprite.Anchor.MiddleCenter, overrideProjectileToCopyFrom: (PickupObjectDatabase.GetById(183) as Gun).DefaultModule.projectiles[0]);
            gun.DefaultModule.projectiles[0] = projectile;

            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Burst;
            gun.DefaultModule.angleVariance = 0;
            gun.DefaultModule.burstShotCount = 17;
            gun.DefaultModule.burstCooldownTime = 0.06f;
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.MEDIUM_BULLET;
            gun.DefaultModule.cooldownTime = 0.1f;
            gun.DefaultModule.numberOfShotsInClip = 17;

            gun.reloadClipLaunchFrame = 0;
            gun.doesScreenShake = false;
            gun.quality = PickupObject.ItemQuality.EXCLUDED;
            gun.barrelOffset.transform.localPosition = new Vector3(1.1875f, 0.5625f, 0f);
            gun.gunClass = GunClass.PISTOL;
            gun.gunSwitchGroup = "Hammer";
            gun.gunHandedness = GunHandedness.HiddenOneHanded;
            gun.muzzleFlashEffects = new VFXPool() { type = VFXPoolType.None, effects = [] };
            gun.reloadTime = 1.1f;
            gun.SetBaseMaxAmmo(700);

            gun.gunSwitchGroup = "SPAPI_LichsGunSynergy";
            SoundManager.AddCustomSwitchData("WPN_Guns", "SPAPI_LichsGunSynergy", "Play_WPN_Gun_Shot_01", "Play_WPN_h4mmer_shot_01");
            SoundManager.AddCustomSwitchData("WPN_Guns", "SPAPI_LichsGunSynergy", "Play_WPN_gun_reload_01", "Play_BOSS_lichC_morph_01");

            ETGMod.Databases.Items.Add(gun, null, "ANY");

            var processor = original.gameObject.AddComponent<TransformGunSynergyProcessor>();
            processor.NonSynergyGunId = original.PickupObjectId;
            processor.SynergyGunId = gun.PickupObjectId;
            processor.SynergyToCheck = LichsBookItem.MasterOfTheGungeonSynergy;
        }

        public static tk2dSpriteCollectionData VFXCollection => (PickupObjectDatabase.GetById(95) as Gun).clipObject.GetComponent<tk2dBaseSprite>().Collection;

        public static T CopyFields<T>(Projectile sample2) where T : Projectile
        {
            var sample = sample2.gameObject.AddComponent<T>();

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

        public static List<string> shootFrames =
        [
            "lichsgun_fire_001",
            "lichsgun_fire_002",
            "lichsgun_fire_003",
            "lichsgun_fire_004",
            "lichsgun_idle_001"
        ];

        public static List<string> reloadFrames =
        [
            "lichsgun_reload_001",
            "lichsgun_reload_001",
            "lichsgun_reload_002",
            "lichsgun_reload_003",
            "lichsgun_reload_002",
            "lichsgun_reload_003",
            "lichsgun_reload_004"
        ];

        public static List<IntVector2> fireOffsets =
        [
            new(0, 0),
            new(-3, 1),
            new(-2, 0),
            new(1, -1)
        ];

        public static List<IntVector2> reloadOffsets =
        [
            new(1, -2),
            new(-3, -1),
            new(-3, 0),
            new(-1, 1)
        ];
    }
}

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
            GunExt.SetShortDescription(gun, "The Freeshooter");
            GunExt.SetLongDescription(gun, "This is Gun from Gungeon Master.");
            GunExt.SetupSprite(gun, null, "lichsgun_idle_001", 10);
            GunExt.SetAnimationFPS(gun, gun.shootAnimation, 12);
            GunExt.AddProjectileModuleFrom(gun, "klobb", true, false);
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
            gun.DefaultModule.angleVariance = 0;
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.MEDIUM_BULLET;
            InputGuidedProjectile projectile = CopyFields<InputGuidedProjectile>(UnityEngine.Object.Instantiate((PickupObjectDatabase.GetById(183) as Gun).DefaultModule.projectiles[0]));
            projectile.trackingSpeed = 500f;
            projectile.dumbfireTime = -1f;
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;
            projectile.baseData.damage = 5f;
            projectile.shouldRotate = true;
            projectile.name = "LichsGun_Projectile";
            projectile.baseData.range = 18f;
            projectile.baseData.speed = 22f;
            SetProjectileSpriteRight(projectile, "lichsgun_projectile_001", 6, 6, false, tk2dBaseSprite.Anchor.MiddleCenter, true, false, null, null, null, null, (PickupObjectDatabase.GetById(183) as Gun).DefaultModule.projectiles[0]);
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
            gun.SetBaseMaxAmmo(350);
            gun.quality = PickupObject.ItemQuality.SPECIAL;
            gun.barrelOffset.transform.localPosition = new Vector3(1.1875f, 0.5625f, 0f);
            gun.gunClass = GunClass.PISTOL;
            ETGMod.Databases.Items.Add(gun, null, "ANY");
            InitSynergyForm(gun);
        }

        /// <summary>
        /// Clones a tk2dSpriteDefinition.
        /// </summary>
        /// <param name="other">The original tk2dSpriteDefinition.</param>
        /// <returns>The new tk2dSpriteDefinition.</returns>
        public static tk2dSpriteDefinition CopyDefinitionFrom(tk2dSpriteDefinition other)
        {
            tk2dSpriteDefinition result = new tk2dSpriteDefinition
            {
                boundsDataCenter = new Vector3
                {
                    x = other.boundsDataCenter.x,
                    y = other.boundsDataCenter.y,
                    z = other.boundsDataCenter.z
                },
                boundsDataExtents = new Vector3
                {
                    x = other.boundsDataExtents.x,
                    y = other.boundsDataExtents.y,
                    z = other.boundsDataExtents.z
                },
                colliderConvex = other.colliderConvex,
                colliderSmoothSphereCollisions = other.colliderSmoothSphereCollisions,
                colliderType = other.colliderType,
                colliderVertices = other.colliderVertices,
                collisionLayer = other.collisionLayer,
                complexGeometry = other.complexGeometry,
                extractRegion = other.extractRegion,
                flipped = other.flipped,
                indices = other.indices,
                material = new Material(other.material),
                materialId = other.materialId,
                materialInst = new Material(other.materialInst),
                metadata = other.metadata,
                name = other.name,
                normals = other.normals,
                physicsEngine = other.physicsEngine,
                position0 = new Vector3
                {
                    x = other.position0.x,
                    y = other.position0.y,
                    z = other.position0.z
                },
                position1 = new Vector3
                {
                    x = other.position1.x,
                    y = other.position1.y,
                    z = other.position1.z
                },
                position2 = new Vector3
                {
                    x = other.position2.x,
                    y = other.position2.y,
                    z = other.position2.z
                },
                position3 = new Vector3
                {
                    x = other.position3.x,
                    y = other.position3.y,
                    z = other.position3.z
                },
                regionH = other.regionH,
                regionW = other.regionW,
                regionX = other.regionX,
                regionY = other.regionY,
                tangents = other.tangents,
                texelSize = new Vector2
                {
                    x = other.texelSize.x,
                    y = other.texelSize.y
                },
                untrimmedBoundsDataCenter = new Vector3
                {
                    x = other.untrimmedBoundsDataCenter.x,
                    y = other.untrimmedBoundsDataCenter.y,
                    z = other.untrimmedBoundsDataCenter.z
                },
                untrimmedBoundsDataExtents = new Vector3
                {
                    x = other.untrimmedBoundsDataExtents.x,
                    y = other.untrimmedBoundsDataExtents.y,
                    z = other.untrimmedBoundsDataExtents.z
                }
            };
            List<Vector2> uvs = new List<Vector2>();
            foreach (Vector2 vector in other.uvs)
            {
                uvs.Add(new Vector2
                {
                    x = vector.x,
                    y = vector.y
                });
            }
            result.uvs = uvs.ToArray();
            List<Vector3> colliderVertices = new List<Vector3>();
            foreach (Vector3 vector in other.colliderVertices)
            {
                colliderVertices.Add(new Vector3
                {
                    x = vector.x,
                    y = vector.y,
                    z = vector.z
                });
            }
            result.colliderVertices = colliderVertices.ToArray();
            return result;
        }

        public static void SetProjectileSpriteRight(Projectile proj, string name, int pixelWidth, int pixelHeight, bool lightened = true, tk2dBaseSprite.Anchor anchor = tk2dBaseSprite.Anchor.LowerLeft, bool anchorChangesCollider = true,
            bool fixesScale = false, int? overrideColliderPixelWidth = null, int? overrideColliderPixelHeight = null, int? overrideColliderOffsetX = null, int? overrideColliderOffsetY = null, Projectile overrideProjectileToCopyFrom = null)
        {
            try
            {
                proj.GetAnySprite().spriteId = ETGMod.Databases.Items.ProjectileCollection.inst.GetSpriteIdByName(name);
                tk2dSpriteDefinition def = SetupDefinitionForProjectileSprite(name, proj.GetAnySprite().spriteId, pixelWidth, pixelHeight, lightened, overrideColliderPixelWidth, overrideColliderPixelHeight, overrideColliderOffsetX,
                    overrideColliderOffsetY, overrideProjectileToCopyFrom);
                LichsBookItem.ConstructOffsetsFromAnchor(def, anchor, def.position3, fixesScale, anchorChangesCollider);
                proj.GetAnySprite().scale = new Vector3(1f, 1f, 1f);
                proj.transform.localScale = new Vector3(1f, 1f, 1f);
                proj.GetAnySprite().transform.localScale = new Vector3(1f, 1f, 1f);
                proj.AdditionalScaleMultiplier = 1f;
            }
            catch (Exception ex)
            {
                ETGModConsole.Log("Ooops! Seems like something got very, Very, VERY wrong. Here's the exception:");
                ETGModConsole.Log(ex.ToString());
            }
        }

        public static tk2dSpriteDefinition SetupDefinitionForProjectileSprite(string name, int id, int pixelWidth, int pixelHeight, bool lightened = true, int? overrideColliderPixelWidth = null, int? overrideColliderPixelHeight = null,
            int? overrideColliderOffsetX = null, int? overrideColliderOffsetY = null, Projectile overrideProjectileToCopyFrom = null)
        {
            if (overrideColliderPixelWidth == null)
            {
                overrideColliderPixelWidth = pixelWidth;
            }
            if (overrideColliderPixelHeight == null)
            {
                overrideColliderPixelHeight = pixelHeight;
            }
            if (overrideColliderOffsetX == null)
            {
                overrideColliderOffsetX = 0;
            }
            if (overrideColliderOffsetY == null)
            {
                overrideColliderOffsetY = 0;
            }
            float thing = 16;
            float thing2 = 16;
            float trueWidth = (float)pixelWidth / thing;
            float trueHeight = (float)pixelHeight / thing;
            float colliderWidth = (float)overrideColliderPixelWidth.Value / thing2;
            float colliderHeight = (float)overrideColliderPixelHeight.Value / thing2;
            float colliderOffsetX = (float)overrideColliderOffsetX.Value / thing2;
            float colliderOffsetY = (float)overrideColliderOffsetY.Value / thing2;
            tk2dSpriteDefinition def = CopyDefinitionFrom(ETGMod.Databases.Items.ProjectileCollection.inst.spriteDefinitions[(overrideProjectileToCopyFrom ??
                    (PickupObjectDatabase.GetById(lightened ? 47 : 12) as Gun).DefaultModule.projectiles[0]).GetAnySprite().spriteId]);
            def.boundsDataCenter = new Vector3(trueWidth / 2f, trueHeight / 2f, 0f);
            def.boundsDataExtents = new Vector3(trueWidth, trueHeight, 0f);
            def.untrimmedBoundsDataCenter = new Vector3(trueWidth / 2f, trueHeight / 2f, 0f);
            def.untrimmedBoundsDataExtents = new Vector3(trueWidth, trueHeight, 0f);
            def.texelSize = new Vector2(1 / 16f, 1 / 16f);
            def.position0 = new Vector3(0f, 0f, 0f);
            def.position1 = new Vector3(0f + trueWidth, 0f, 0f);
            def.position2 = new Vector3(0f, 0f + trueHeight, 0f);
            def.position3 = new Vector3(0f + trueWidth, 0f + trueHeight, 0f);
            def.colliderVertices[0].x = colliderOffsetX;
            def.colliderVertices[0].y = colliderOffsetY;
            def.colliderVertices[1].x = colliderWidth;
            def.colliderVertices[1].y = colliderHeight;
            def.name = name;
            ETGMod.Databases.Items.ProjectileCollection.inst.spriteDefinitions[id] = def;
            return def;
        }

        public static void InitSynergyForm(Gun original)
        {
            Gun gun = ETGMod.Databases.Items.NewGun("Synergy Lich's Gun", "lichsgun2");
            Game.Items.Rename("outdated_gun_mods:synergy_lich's_gun", "spapi:lichs_gun+master_of_the_gungeon");
            GunExt.SetShortDescription(gun, "The Freeshooter");
            GunExt.SetLongDescription(gun, "This is Gun from Gungeon Master.");
            GunExt.SetupSprite(gun, null, "lichsgun2_idle_001", 10);
            GunExt.SetAnimationFPS(gun, gun.shootAnimation, 12);
            GunExt.AddProjectileModuleFrom(gun, "klobb", true, false);
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Burst;
            gun.DefaultModule.angleVariance = 0;
            gun.DefaultModule.burstShotCount = 17;
            gun.DefaultModule.burstCooldownTime = 0.06f;
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.MEDIUM_BULLET;
            InputGuidedProjectile projectile = CopyFields<InputGuidedProjectile>(UnityEngine.Object.Instantiate((PickupObjectDatabase.GetById(183) as Gun).DefaultModule.projectiles[0]));
            projectile.dumbfireTime = -1f;
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;
            projectile.baseData.damage = 3f;
            projectile.shouldRotate = true;
            projectile.name = "LichsGun2_Projectile";
            projectile.baseData.range = 100f;
            projectile.baseData.speed = 14f;
            SetProjectileSpriteRight(projectile, "lichsgun_projectile_001", 6, 6, false, tk2dBaseSprite.Anchor.MiddleCenter, true, false, null, null, null, null, (PickupObjectDatabase.GetById(183) as Gun).DefaultModule.projectiles[0]);
            gun.reloadClipLaunchFrame = 0;
            gun.DefaultModule.cooldownTime = 0.1f;
            gun.DefaultModule.numberOfShotsInClip = 17;
            gun.doesScreenShake = false;
            AdvancedGunBehaviour behav = gun.gameObject.AddComponent<AdvancedGunBehaviour>();
            behav.preventNormalReloadAudio = true;
            behav.overrideNormalReloadAudio = "Play_BOSS_lichC_morph_01";
            gun.gunSwitchGroup = "Hammer";
            gun.gunHandedness = GunHandedness.HiddenOneHanded;
            gun.muzzleFlashEffects = new VFXPool() { type = VFXPoolType.None, effects = new VFXComplex[0] };
            gun.reloadTime = 1.1f;
            gun.SetBaseMaxAmmo(700);
            gun.quality = PickupObject.ItemQuality.EXCLUDED;
            gun.barrelOffset.transform.localPosition = new Vector3(1.1875f, 0.5625f, 0f);
            gun.gunClass = GunClass.PISTOL;
            ETGMod.Databases.Items.Add(gun, null, "ANY");
            AdvancedTransformGunSynergyProcessor processor = original.gameObject.AddComponent<AdvancedTransformGunSynergyProcessor>();
            processor.NonSynergyGunId = original.PickupObjectId;
            processor.SynergyGunId = gun.PickupObjectId;
            processor.SynergyToCheck = "Master of the Gungeon";
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
}

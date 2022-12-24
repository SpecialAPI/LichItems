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

namespace LichItems
{
    public class CrossChamber
    {
        public static void Init()
        {
            try
            {
                string itemName = "Cross Chamber";
                string resourceName = "LichItems/Resources/Cross_Chamber";
                GameObject obj = new GameObject(itemName);
                var item = obj.AddComponent<AdvancedCompanionItem>();
                ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);
                string shortDesc = "Baby Tombstoner";
                string longDesc = "A relic that summons a Lich personal guard.";
                ItemBuilder.SetupItem(item, shortDesc, longDesc, "spapi");
                item.quality = PickupObject.ItemQuality.SPECIAL;
                item.CompanionGuid = "Cross_Chamber";
                item.Synergies = new CompanionTransformSynergy[0];
                item.passiveStatModifiers = new StatModifier[0];
                item.UseAdvancedSynergies = true;
                item.AdvancedSynergies = new AdvancedCompanionTransformSynergy[]
                {
                    new AdvancedCompanionTransformSynergy()
                    {
                      SynergyCompanionGuid = "Synergy_Cross_Chamber",
                      UseStringSynergyDetectionInstead = true,
                      RequiredStringSynergy = "Bullet = peace"
                    }
                };
                BuildPrefab();
                BuildSynergyPrefab();
            }
            catch (Exception ex)
            {
                ETGModConsole.Log("Something bad happened when creating Cross Chamber. Here's the Exception:");
                ETGModConsole.Log(ex.ToString());
            }
        }

        public static AIShooter SetupBasicAIShooter(GameObject go, int gunid, Transform gunattachpoint, GameObject handobject)
        {
            AIShooter aishoot = go.GetOrAddComponent<AIShooter>();
            aishoot.volley = null;
            aishoot.equippedGunId = gunid;
            aishoot.shouldUseGunReload = true;
            aishoot.volleyShootPosition = null;
            aishoot.volleyShellCasing = null;
            aishoot.volleyShellTransform = null;
            aishoot.volleyShootVfx = null;
            aishoot.usesOctantShootVFX = true;
            aishoot.bulletName = "default";
            aishoot.customShootCooldownPeriod = 1f;
            aishoot.doesScreenShake = false;
            aishoot.rampBullets = false;
            aishoot.rampStartHeight = 2f;
            aishoot.rampTime = 1f;
            aishoot.gunAttachPoint = gunattachpoint;
            aishoot.bulletScriptAttachPoint = null;
            aishoot.overallGunAttachOffset = IntVector2.Zero;
            aishoot.flippedGunAttachOffset = IntVector2.Zero;
            aishoot.handObject = handobject.GetComponent<PlayerHandController>();
            aishoot.AllowTwoHands = true;
            aishoot.ForceGunOnTop = false;
            aishoot.IsReallyBigBoy = false;
            aishoot.BackupAimInMoveDirection = false;
            aishoot.PostProcessProjectile = null;
            return aishoot;
        }

        public static void BuildPrefab()
        {
            if (prefab == null && !CompanionBuilder.companionDictionary.ContainsKey("Cross_Chamber"))
            {
                prefab = CompanionBuilder.BuildPrefab("Cross Chamber", "Cross_Chamber", "LichItems/Resources/CrossChamber/IdleRight/tomb_idle_right_001", new IntVector2(0, 0), new IntVector2(14, 16));
                var companion = prefab.AddComponent<CompanionController>();
                PixelCollider collider = new PixelCollider();
                collider.ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual;
                collider.CollisionLayer = CollisionLayer.PlayerHitBox;
                collider.ManualWidth = 14;
                collider.ManualHeight = 16;
                collider.ManualOffsetX = 0;
                collider.ManualOffsetY = 0;
                KnockbackDoer knockback = companion.gameObject.GetOrAddComponent<KnockbackDoer>();
                knockback.weight = 100f;
                companion.aiActor.IsNormalEnemy = false;
                companion.CanInterceptBullets = true;
                companion.specRigidbody.PrimaryPixelCollider.CollisionLayer = CollisionLayer.PlayerCollider;
                companion.specRigidbody.PixelColliders.Add(collider);
                companion.gameObject.AddComponent<IgnoreEnemyCollisions>();
                companion.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox));
                companion.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyCollider));
                companion.specRigidbody.CollideWithOthers = true;
                companion.aiActor.CollisionDamage = 0f;
                companion.aiActor.MovementSpeed = 3f;
                companion.aiActor.CanDropCurrency = false;
                companion.healthHaver.PreventAllDamage = false;
                prefab.AddAnimation("idle_right", "LichItems/Resources/CrossChamber/IdleRight", 4, AnimationType.Idle, DirectionType.TwoWayHorizontal);
                prefab.AddAnimation("idle_left", "LichItems/Resources/CrossChamber/IdleLeft", 4, AnimationType.Idle, DirectionType.TwoWayHorizontal);
                prefab.AddAnimation("run_right", "LichItems/Resources/CrossChamber/MoveRight", 10, AnimationType.Move, DirectionType.TwoWayHorizontal);
                prefab.AddAnimation("run_left", "LichItems/Resources/CrossChamber/MoveLeft", 10, AnimationType.Move, DirectionType.TwoWayHorizontal);
                prefab.AddAnimation("hit_left", "LichItems/Resources/CrossChamber/HitLeft", 6, AnimationType.Hit, DirectionType.TwoWayHorizontal).wrapMode = tk2dSpriteAnimationClip.WrapMode.Once;
                prefab.AddAnimation("hit_right", "LichItems/Resources/CrossChamber/HitRight", 6, AnimationType.Hit, DirectionType.TwoWayHorizontal).wrapMode = tk2dSpriteAnimationClip.WrapMode.Once;
                BehaviorSpeculator component = CrossChamber.prefab.GetComponent<BehaviorSpeculator>();
                component.MovementBehaviors.Add(new CompanionFollowPlayerBehavior
                {
                    IdleAnimations = new string[]
                    {
                        "idle"
                    },
                    DisableInCombat = false
                });
            }
        }
        public static void BuildSynergyPrefab()
        {
            if (synergyPrefab == null && !CompanionBuilder.companionDictionary.ContainsKey("Synergy_Cross_Chamber"))
            {
                synergyPrefab = CompanionBuilder.BuildPrefab("Synergy Cross Chamber", "Synergy_Cross_Chamber", "LichItems/Resources/CrossChamber/IdleRight/tomb_idle_right_001", new IntVector2(0, 0), new IntVector2(14, 16));
                var companion = synergyPrefab.AddComponent<CompanionController>();
                PixelCollider collider = new PixelCollider();
                collider.ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual;
                collider.CollisionLayer = CollisionLayer.PlayerHitBox;
                collider.ManualWidth = 14;
                collider.ManualHeight = 16;
                collider.ManualOffsetX = 0;
                collider.ManualOffsetY = 0;
                KnockbackDoer knockback = companion.gameObject.GetOrAddComponent<KnockbackDoer>();
                knockback.weight = 100f;
                companion.aiActor.IsNormalEnemy = false;
                companion.CanInterceptBullets = true;
                companion.specRigidbody.PrimaryPixelCollider.CollisionLayer = CollisionLayer.PlayerCollider;
                companion.specRigidbody.PixelColliders.Add(collider);
                companion.gameObject.AddComponent<IgnoreEnemyCollisions>();
                companion.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox));
                companion.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyCollider));
                companion.specRigidbody.CollideWithOthers = true;
                companion.aiActor.CollisionDamage = 0f;
                companion.aiActor.MovementSpeed = 3f;
                companion.aiActor.CanDropCurrency = false;
                companion.healthHaver.PreventAllDamage = false;
                GameObject gunAttachPointObject = new GameObject("GunAttachPoint");
                FakePrefab.MarkAsFakePrefab(gunAttachPointObject);
                UnityEngine.Object.DontDestroyOnLoad(gunAttachPointObject);
                Transform gunAttachPoint = gunAttachPointObject.transform;
                gunAttachPoint.parent = synergyPrefab.transform;
                gunAttachPoint.localPosition = new Vector3(-0.0625f, 0.3125f, 0f);
                GameObject hand = SpriteBuilder.SpriteFromResource("LichItems/Resources/CrossChamber/Hand/hand_001", new GameObject("SynergyCrossChamberHand"));
                FakePrefab.MarkAsFakePrefab(hand);
                UnityEngine.Object.DontDestroyOnLoad(hand);
                LichsBookItem.ConstructOffsetsFromAnchor(hand.GetComponent<tk2dBaseSprite>().GetCurrentSpriteDef(), tk2dBaseSprite.Anchor.MiddleCenter);
                PlayerHandController handController = hand.AddComponent<PlayerHandController>();
                handController.ForceRenderersOff = false;
                handController.attachPoint = null;
                handController.handHeightFromGun = 0.05f;
                SetupBasicAIShooter(synergyPrefab, 38 /*38 is the id of magnum*/, gunAttachPoint, hand);
                synergyPrefab.AddAnimation("idle_right", "LichItems/Resources/CrossChamber/IdleRight", 4, AnimationType.Idle, DirectionType.TwoWayHorizontal);
                synergyPrefab.AddAnimation("idle_left", "LichItems/Resources/CrossChamber/IdleLeft", 4, AnimationType.Idle, DirectionType.TwoWayHorizontal);
                synergyPrefab.AddAnimation("run_right", "LichItems/Resources/CrossChamber/MoveRight", 10, AnimationType.Move, DirectionType.TwoWayHorizontal);
                synergyPrefab.AddAnimation("run_left", "LichItems/Resources/CrossChamber/MoveLeft", 10, AnimationType.Move, DirectionType.TwoWayHorizontal);
                synergyPrefab.AddAnimation("hit_left", "LichItems/Resources/CrossChamber/HitLeft", 6, AnimationType.Hit, DirectionType.TwoWayHorizontal).wrapMode = tk2dSpriteAnimationClip.WrapMode.Once;
                synergyPrefab.AddAnimation("hit_right", "LichItems/Resources/CrossChamber/HitRight", 6, AnimationType.Hit, DirectionType.TwoWayHorizontal).wrapMode = tk2dSpriteAnimationClip.WrapMode.Once;
                BehaviorSpeculator component = CrossChamber.synergyPrefab.GetComponent<BehaviorSpeculator>();
                component.MovementBehaviors.Add(new CompanionFollowPlayerBehavior
                {
                    IdleAnimations = new string[]
                    {
                        "idle"
                    },
                    DisableInCombat = false
                });
                component.TargetBehaviors = new List<TargetBehaviorBase>
                {
                    new TargetPlayerBehavior
                    {
                        Radius = 35f,
                        LineOfSight = true,
                        ObjectPermanence = true,
                        SearchInterval = 0.25f,
                        PauseOnTargetSwitch = false,
                        PauseTime = 0.25f
                    }
                };
                component.AttackBehaviors = new List<AttackBehaviorBase>
                {
                    new ShootGunBehavior()
                    {
                        GroupCooldownVariance = -1f,
                        LineOfSight = true,
                        WeaponType = WeaponType.AIShooterProjectile,
                        OverrideBulletName = "default",
                        BulletScript = null,
                        FixTargetDuringAttack = false,
                        StopDuringAttack = false,
                        LeadAmount = 0f,
                        LeadChance = 1f,
                        RespectReload = true,
                        MagazineCapacity = 6,
                        ReloadSpeed = 2f,
                        EmptiesClip = false,
                        SuppressReloadAnim = false,
                        TimeBetweenShots = -1f,
                        PreventTargetSwitching = false,
                        OverrideAnimation = string.Empty,
                        OverrideDirectionalAnimation = string.Empty,
                        HideGun = false,
                        UseLaserSight = false,
                        UseGreenLaser = false,
                        PreFireLaserTime = -1f,
                        AimAtFacingDirectionWhenSafe = false
                    }
                };
                AIBulletBank bulletBank = synergyPrefab.GetOrAddComponent<AIBulletBank>();
                bulletBank.Bullets = new List<AIBulletBank.Entry>
                {
                    new AIBulletBank.Entry()
                    {
                        Name = "default",
                        OverrideProjectile = true,
                        ProjectileData = new ProjectileData()
                        {
                            damage = 10f,
                            speed = 10f,
                            range = 30f,
                            force = 5f,
                            damping = 0f,
                            UsesCustomAccelerationCurve = false,
                            AccelerationCurve = null,
                            CustomAccelerationCurveDuration = 0f,
                            IgnoreAccelCurveTime = 0f,
                            onDestroyBulletScript = null
                        },
                        BulletObject = null
                    }
                };
                bulletBank.useDefaultBulletIfMissing = false;
                bulletBank.transforms = new List<Transform>
                {
                    synergyPrefab.transform
                };
                bulletBank.rampBullets = false;
                bulletBank.rampStartHeight = 0f;
                bulletBank.rampTime = 0f;
                bulletBank.OverrideGun = null;
                bulletBank.OnProjectileCreated = null;
                bulletBank.OnProjectileCreatedWithSource = null;
                bulletBank.FixedPlayerPosition = null;
            }
        }


        public class IgnoreEnemyCollisions : BraveBehaviour
        {
            public void Start()
            {
                base.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox));
                base.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyCollider));
                base.healthHaver.OnDamaged += this.HandleDamaged;
            }

            private void HandleDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
            {
                base.healthHaver.FullHeal();
            }
        }
        public static GameObject prefab;
        public static GameObject synergyPrefab;

    }
}

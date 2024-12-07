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
        public static GameObject prefab;
        public static GameObject synergyPrefab;

        public static CustomSynergyType CrossfireSynergy;

        public static void Init()
        {
            var itemName = "Cross Chamber";
            var resourceName = "LichItems/Resources/Cross_Chamber";

            var obj = new GameObject(itemName);
            var item = obj.AddComponent<CompanionItem>();
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            var shortDesc = "Marbled Minion";
            var longDesc = "A relic that summons one of the Lich's personal guards. While unable to attack, it can still block bullets for the owner.";
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "spapi");

            CrossfireSynergy = ETGModCompatibility.ExtendEnum<CustomSynergyType>(LichModule.GUID, "CROSSFIRE");

            item.quality = PickupObject.ItemQuality.SPECIAL;
            item.CompanionGuid = "Cross_Chamber";
            item.Synergies =
            [
                new()
                {
                    RequiredSynergy = CrossfireSynergy,
                    SynergyCompanionGuid = "Synergy_Cross_Chamber"
                }
            ];

            BuildPrefab();
            BuildSynergyPrefab();
        }

        public static AIShooter SetupBasicAIShooter(GameObject go, int gunid, Transform gunattachpoint, GameObject handobject)
        {
            var shoot = go.GetOrAddComponent<AIShooter>();

            shoot.equippedGunId = gunid;
            shoot.shouldUseGunReload = true;
            shoot.customShootCooldownPeriod = 1f;
            shoot.gunAttachPoint = gunattachpoint;
            shoot.handObject = handobject.GetComponent<PlayerHandController>();
            shoot.AllowTwoHands = true;

            return shoot;
        }

        public static void BuildPrefab()
        {
            if (prefab != null || CompanionBuilder.companionDictionary.ContainsKey("Cross_Chamber"))
                return;

            prefab = CompanionBuilder.BuildPrefab("Cross Chamber", "Cross_Chamber", "LichItems/Resources/CrossChamber/IdleRight/tomb_idle_right_001", new IntVector2(0, 0), new IntVector2(14, 16));

            var companion = prefab.AddComponent<CompanionController>();
            companion.CanInterceptBullets = true;

            var rigidbody = companion.specRigidbody;
            rigidbody.PixelColliders.Add(new PixelCollider
            {
                ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual,
                CollisionLayer = CollisionLayer.PlayerHitBox,
                ManualWidth = 14,
                ManualHeight = 16,
                ManualOffsetX = 0,
                ManualOffsetY = 0
            });
            rigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox));
            rigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyCollider));
            rigidbody.CollideWithOthers = true;

            var knockback = companion.gameObject.GetOrAddComponent<KnockbackDoer>();
            knockback.weight = 100f;

            var actor = companion.aiActor;
            actor.IsNormalEnemy = false;
            actor.CollisionDamage = 0f;
            actor.MovementSpeed = 3f;
            actor.CanDropCurrency = false;

            companion.gameObject.AddComponent<IgnoreEnemyCollisions>();
            companion.healthHaver.PreventAllDamage = false;

            prefab.AddAnimation("idle_right", "LichItems/Resources/CrossChamber/IdleRight", 4, AnimationType.Idle, DirectionType.TwoWayHorizontal);
            prefab.AddAnimation("idle_left", "LichItems/Resources/CrossChamber/IdleLeft", 4, AnimationType.Idle, DirectionType.TwoWayHorizontal);
            prefab.AddAnimation("run_right", "LichItems/Resources/CrossChamber/MoveRight", 10, AnimationType.Move, DirectionType.TwoWayHorizontal);
            prefab.AddAnimation("run_left", "LichItems/Resources/CrossChamber/MoveLeft", 10, AnimationType.Move, DirectionType.TwoWayHorizontal);
            prefab.AddAnimation("hit_left", "LichItems/Resources/CrossChamber/HitLeft", 6, AnimationType.Hit, DirectionType.TwoWayHorizontal).wrapMode = tk2dSpriteAnimationClip.WrapMode.Once;
            prefab.AddAnimation("hit_right", "LichItems/Resources/CrossChamber/HitRight", 6, AnimationType.Hit, DirectionType.TwoWayHorizontal).wrapMode = tk2dSpriteAnimationClip.WrapMode.Once;

            var behavior = prefab.GetComponent<BehaviorSpeculator>();
            behavior.MovementBehaviors.Add(new CompanionFollowPlayerBehavior
            {
                IdleAnimations = ["idle"],
                DisableInCombat = false
            });
        }

        public static void BuildSynergyPrefab()
        {
            if (synergyPrefab != null || CompanionBuilder.companionDictionary.ContainsKey("Synergy_Cross_Chamber"))
                return;

            synergyPrefab = CompanionBuilder.BuildPrefab("Synergy Cross Chamber", "Synergy_Cross_Chamber", "LichItems/Resources/CrossChamber/IdleRight/tomb_idle_right_001", new IntVector2(0, 0), new IntVector2(14, 16));
            var companion = synergyPrefab.AddComponent<CompanionController>();

            var rigidbody = companion.specRigidbody;
            rigidbody.PrimaryPixelCollider.CollisionLayer = CollisionLayer.PlayerCollider;
            rigidbody.PixelColliders.Add(new PixelCollider
            {
                ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual,
                CollisionLayer = CollisionLayer.PlayerHitBox,
                ManualWidth = 14,
                ManualHeight = 16,
                ManualOffsetX = 0,
                ManualOffsetY = 0
            });
            rigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox));
            rigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyCollider));
            rigidbody.CollideWithOthers = true;

            var knockback = companion.gameObject.GetOrAddComponent<KnockbackDoer>();
            knockback.weight = 100f;

            var actor = companion.aiActor;
            actor.IsNormalEnemy = false;
            actor.CollisionDamage = 0f;
            actor.MovementSpeed = 3f;
            actor.CanDropCurrency = false;

            companion.CanInterceptBullets = true;
            companion.gameObject.AddComponent<IgnoreEnemyCollisions>();
            companion.healthHaver.PreventAllDamage = false;

            var gunAttachPointObject = new GameObject("GunAttachPoint");
            FakePrefab.MarkAsFakePrefab(gunAttachPointObject);
            UnityEngine.Object.DontDestroyOnLoad(gunAttachPointObject);

            var gunAttachPoint = gunAttachPointObject.transform;
            gunAttachPoint.parent = synergyPrefab.transform;
            gunAttachPoint.localPosition = new Vector3(-0.0625f, 0.3125f, 0f);

            var hand = SpriteBuilder.SpriteFromResource("LichItems/Resources/CrossChamber/Hand/hand_001", new GameObject("SynergyCrossChamberHand"));
            FakePrefab.MarkAsFakePrefab(hand);
            UnityEngine.Object.DontDestroyOnLoad(hand);
            SpriteBuilder.ConstructOffsetsFromAnchor(hand.GetComponent<tk2dBaseSprite>().GetCurrentSpriteDef(), tk2dBaseSprite.Anchor.MiddleCenter);

            var handController = hand.AddComponent<PlayerHandController>();
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

            var behavior = synergyPrefab.GetComponent<BehaviorSpeculator>();
            behavior.MovementBehaviors.Add(new CompanionFollowPlayerBehavior
            {
                IdleAnimations = ["idle"],
                DisableInCombat = false
            });

            behavior.TargetBehaviors =
            [
                new TargetPlayerBehavior
                {
                    Radius = 35f,
                    LineOfSight = true,
                    ObjectPermanence = true,
                    SearchInterval = 0.25f,
                    PauseOnTargetSwitch = false,
                    PauseTime = 0.25f
                }
            ];

            behavior.AttackBehaviors =
            [
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
            ];

            var bulletBank = synergyPrefab.GetOrAddComponent<AIBulletBank>();
            bulletBank.Bullets =
            [
                new()
                {
                    Name = "default",
                    OverrideProjectile = true,
                    ProjectileData = new ProjectileData()
                    {
                        damage = 5f,
                        speed = 10f,
                        range = 60f,
                        force = 10f,
                        damping = 0f,
                        UsesCustomAccelerationCurve = false,
                        AccelerationCurve = null,
                        CustomAccelerationCurveDuration = 0f,
                        IgnoreAccelCurveTime = 0f,
                        onDestroyBulletScript = null
                    },
                    BulletObject = null
                }
            ];
            bulletBank.useDefaultBulletIfMissing = false;
            bulletBank.transforms = [synergyPrefab.transform];
        }


        public class IgnoreEnemyCollisions : BraveBehaviour
        {
            public void Start()
            {
                specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox));
                specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyCollider));
                healthHaver.OnDamaged += HandleDamaged;
            }

            private void HandleDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
            {
                healthHaver.FullHeal();
            }
        }
    }
}
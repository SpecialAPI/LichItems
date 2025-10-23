using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using Gungeon;

namespace LichItems
{
    public class LichsBookItem : PlayerItem
    {
        public GameObject LichsBookPrefab;
        private GameObject m_instanceBook;
        public tk2dSpriteAnimation replacementLibrary;
        public tk2dSprite replacementHandSprite;
        private PlayerController m_lastPlayer;
        private bool m_transformed;
        private readonly List<StatModifier> synergyModifiers = [];

        public static CustomSynergyType MasterOfTheGungeonSynergy;

        public static void Init()
        {
            var itemName = "Lich's Book";
            var resourceName = "LichItems/Resources/lichsbook_item_001";

            var obj = new GameObject(itemName);
            var item = obj.AddComponent<LichsBookItem>();
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            var shortDesc = "Reload Spell";
            var longDesc = "The book of the Gungoen master. Place to create a zone of instant reload.";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "spapi");
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 600);

            item.consumable = false;
            item.quality = ItemQuality.C;
            Game.Items.Rename("spapi:lich's_book", "spapi:lichs_book");

            var shadow = SpriteBuilder.SpriteFromResource("LichItems/Resources/lichsbook_shadow_001");
            shadow.SetActive(false);
            FakePrefab.MarkAsFakePrefab(shadow);
            DontDestroyOnLoad(shadow);

            var sprite = shadow.GetComponent<tk2dBaseSprite>();
            GunTools.ConstructOffsetsFromAnchor(sprite.GetCurrentSpriteDef(), tk2dBaseSprite.Anchor.LowerCenter);

            var shadowShader = GameManager.Instance.RewardManager.A_Chest.gameObject.transform.Find("Shadow").gameObject.GetComponent<tk2dSprite>().renderer.material.shader;
            sprite.GetCurrentSpriteDef().material.shader = shadowShader;
            sprite.GetCurrentSpriteDef().materialInst.shader = shadowShader;

            var book = SpriteBuilder.SpriteFromResource("LichItems/Resources/lichsbook_depoy_001");
            book.SetActive(false);
            FakePrefab.MarkAsFakePrefab(book);
            DontDestroyOnLoad(book);

            var animator = book.gameObject.AddComponent<tk2dSpriteAnimator>();
            var ids = new List<int>
            {
                book.GetComponent<tk2dBaseSprite>().spriteId,
                SpriteBuilder.AddSpriteToCollection("LichItems/Resources/lichsbook_depoy_002", book.GetComponent<tk2dBaseSprite>().Collection),
                SpriteBuilder.AddSpriteToCollection("LichItems/Resources/lichsbook_depoy_003", book.GetComponent<tk2dBaseSprite>().Collection),
                SpriteBuilder.AddSpriteToCollection("LichItems/Resources/lichsbook_depoy_004", book.GetComponent<tk2dBaseSprite>().Collection),
                SpriteBuilder.AddSpriteToCollection("LichItems/Resources/lichsbook_depoy_005", book.GetComponent<tk2dBaseSprite>().Collection),
                SpriteBuilder.AddSpriteToCollection("LichItems/Resources/lichsbook_depoy_006", book.GetComponent<tk2dBaseSprite>().Collection),
                SpriteBuilder.AddSpriteToCollection("LichItems/Resources/lichsbook_depoy_007", book.GetComponent<tk2dBaseSprite>().Collection),
                SpriteBuilder.AddSpriteToCollection("LichItems/Resources/lichsbook_depoy_008", book.GetComponent<tk2dBaseSprite>().Collection),
            };

            foreach (int id in ids)
                GunTools.ConstructOffsetsFromAnchor(book.GetComponent<tk2dBaseSprite>().Collection.spriteDefinitions[id], tk2dBaseSprite.Anchor.LowerCenter);

            SpriteBuilder.AddAnimation(animator, book.GetComponent<tk2dBaseSprite>().Collection, ids, "idle", tk2dSpriteAnimationClip.WrapMode.Loop).fps = 10;
            animator.DefaultClipId = animator.GetClipIdByName("idle");
            animator.playAutomatically = true;
            book.AddComponent<LichsBook>().shadowPrefab = shadow;
            item.LichsBookPrefab = book;

            MasterOfTheGungeonSynergy = ETGModCompatibility.ExtendEnum<CustomSynergyType>(Plugin.MOD_GUID, "MASTER_OF_THE_GUNGEON");
            BuildLibrary(item);
        }

        public static tk2dSpriteDefinition CopyDefinitionFrom(tk2dSpriteDefinition other)
        {
            var result = new tk2dSpriteDefinition
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

        private static void BuildLibrary(LichsBookItem targetBook)
        {
            var spriteNames = new List<string>();
            var resources = ResourceExtractor.GetResourceNames();

            var spriteDirectory = "LichItems/Resources/InfinilichTransformation";
            for (int i = 0; i < resources.Length; i++)
            {
                var res = resources[i];

                if (!res.StartsWith(spriteDirectory.Replace('/', '.'), StringComparison.OrdinalIgnoreCase))
                    continue;

                spriteNames.Add(res);
            }

            var sprites = new List<Texture2D>();

            foreach (var name in spriteNames)
                sprites.Add(ResourceExtractor.GetTextureFromResource(name));

            var library = Instantiate((PickupObjectDatabase.GetById(163) as BulletArmorItem).knightLibrary);
            DontDestroyOnLoad(library);
            var orig = library.clips[0].frames[0].spriteCollection;
            var copyCollection = Instantiate(orig);

            var copydef = CopyDefinitionFrom((PickupObjectDatabase.GetById(607) as BankMaskItem).OverrideHandSprite.GetCurrentSpriteDef());
            copydef.name = "knight_hand_001";
            var handId = SpriteBuilder.AddSpriteToCollection(copydef, copyCollection);

            DontDestroyOnLoad(copyCollection);

            for (var i = 0; i < sprites.Count; i++)
            {
                var tex = sprites[i];
                var def = copyCollection.GetSpriteDefinition(tex.name);

                if (def == null)
                    continue;

                def.ReplaceTexture(tex);
                def.name = def.name.Replace("knight", "inflich");
                GunTools.MakeOffset(def, new Vector2(-0.0625f, 0f), false);
            }

            foreach (var clip in library.clips)
            {
                for (var i = 0; i < clip.frames.Length; i++)
                    clip.frames[i].spriteCollection = copyCollection;
            }

            foreach (var clip in library.clips)
            {
                foreach (var frame in clip.frames)
                {
                    if (string.IsNullOrEmpty(frame.eventAudio))
                        continue;

                    if(frame.eventAudio != "Play_FS" && frame.eventAudio != "Play_CHR_boot_stairs_01")
                        continue;

                    frame.eventAudio = "";
                }
            }

            var spriteObj = new GameObject("OverrideHandSprite");
            spriteObj.SetActive(false);
            DontDestroyOnLoad(spriteObj);

            var sprite = spriteObj.AddComponent<tk2dSprite>();
            sprite.SetSprite(copyCollection, handId);
            targetBook.replacementHandSprite = sprite;
            targetBook.replacementLibrary = library;
        }

        public override void Update()
        {
            base.Update();
            ProcessInfinilichStatus(LastOwner, false);
        }

        public override void OnDestroy()
        {
            if (m_transformed)
                ProcessInfinilichStatus(null, true);

            base.OnDestroy();
        }

        private void RevealAllRooms(PlayerController player)
        {
            if (Minimap.Instance == null)
                return;

            Minimap.Instance.RevealAllRooms(true);
        }

        private void ProcessInfinilichStatus(PlayerController player, bool forceDisable = false)
        {
            var shouldTransform = player && player.HasActiveBonusSynergy(MasterOfTheGungeonSynergy) && !forceDisable;

            if (shouldTransform && !m_transformed)
            {
                m_lastPlayer = player;

                if (!player)
                    return;

                m_transformed = true;

                if (Minimap.Instance != null)
                    Minimap.Instance.RevealAllRooms(true);

                player.OnNewFloorLoaded += RevealAllRooms;
                player.carriedConsumables.InfiniteKeys = true;
                player.OverrideAnimationLibrary = replacementLibrary;
                player.SetOverrideShader(ShaderCache.Acquire(player.LocalShaderName));

                if (player.characterIdentity == PlayableCharacters.Eevee)
                    player.GetComponent<CharacterAnimationRandomizer>().AddOverrideAnimLibrary(replacementLibrary);

                player.ChangeHandsToCustomType(replacementHandSprite.Collection, replacementHandSprite.spriteId);

                var mod1 = StatModifier.Create(PlayerStats.StatType.AdditionalItemCapacity, StatModifier.ModifyMethod.ADDITIVE, 1);
                player.ownerlessStatModifiers.Add(mod1);
                player.stats.RecalculateStats(player, false, false);
                synergyModifiers.Add(mod1);
            }
            else if (m_transformed && !shouldTransform)
            {
                m_transformed = false;

                if (!m_lastPlayer)
                    return;

                m_lastPlayer.OnNewFloorLoaded -= RevealAllRooms;
                m_lastPlayer.carriedConsumables.InfiniteKeys = false;
                m_lastPlayer.OverrideAnimationLibrary = null;
                m_lastPlayer.ClearOverrideShader();

                if (m_lastPlayer.characterIdentity == PlayableCharacters.Eevee)
                    m_lastPlayer.GetComponent<CharacterAnimationRandomizer>().RemoveOverrideAnimLibrary(replacementLibrary);

                m_lastPlayer.RevertHandsToBaseType();

                foreach (StatModifier mod in synergyModifiers)
                    m_lastPlayer.ownerlessStatModifiers.Remove(mod);

                m_lastPlayer.stats.RecalculateStats(m_lastPlayer, false, false);
                synergyModifiers.Clear();

                m_lastPlayer = null;
            }
        }

        public override bool CanBeUsed(PlayerController user)
        {
            return !m_instanceBook && base.CanBeUsed(user);
        }

        public override void DoEffect(PlayerController user)
        {
            m_instanceBook = Instantiate(LichsBookPrefab, user.CenterPosition.ToVector3ZisY(0f), Quaternion.identity, null);
        }
    }
}

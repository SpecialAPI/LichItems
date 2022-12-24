using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Linq;
using System.Text;
using MonoMod.RuntimeDetour;

namespace LichItems
{
    public class AdvancedGunBehaviour : BraveBehaviour, IGunInheritable, ILevelLoadedListener
    {
        /// <summary>
        /// Update() is called every tick when the gun is the player's current gun or is dropped.
        /// </summary>
        protected virtual void Update()
        {
            if (Player != null)
            {
                if (!everPickedUpByPlayer)
                {
                    everPickedUpByPlayer = true;
                }
            }
            if (Owner != null)
            {
                if (!everPickedUp)
                {
                    everPickedUp = true;
                }
            }
            if (Owner != null && !pickedUpLast)
            {
                OnPickup(Owner);
                pickedUpLast = true;
            }
            if (Owner == null && pickedUpLast)
            {
                if (lastOwner != null)
                {
                    OnPostDrop(lastOwner);
                    lastOwner = null;
                }
                pickedUpLast = false;
            }
            if (lastOwner != Owner)
            {
                lastOwner = Owner;
            }
            if (gun != null && !gun.IsReloading && !hasReloaded)
            {
                hasReloaded = true;
                if (Owner != null)
                {
                    OnReloadEnded(Owner as PlayerController, gun);
                }
            }
            gun.PreventNormalFireAudio = preventNormalFireAudio;
            gun.OverrideNormalFireAudioEvent = overrideNormalFireAudio;
        }

        /// <summary>
        /// Inherits data from another gun. Inherit the variables you want to be saved here!
        /// </summary>
        /// <param name="source">The source gun.</param>
        public virtual void InheritData(Gun source)
        {
            AdvancedGunBehaviour component = source.GetComponent<AdvancedGunBehaviour>();
            if (component != null)
            {
                preventNormalFireAudio = component.preventNormalFireAudio;
                preventNormalReloadAudio = component.preventNormalReloadAudio;
                overrideNormalReloadAudio = component.overrideNormalReloadAudio;
                overrideNormalFireAudio = component.overrideNormalFireAudio;
                everPickedUpByPlayer = component.everPickedUpByPlayer;
                everPickedUp = component.everPickedUp;
                usesOverrideHeroSwordCooldown = component.usesOverrideHeroSwordCooldown;
                overrideHeroSwordCooldown = component.overrideHeroSwordCooldown;
                overrideReloadSwitchGroup = component.overrideReloadSwitchGroup;
            }
        }

        public static bool ReloadHook(Func<Gun, bool> orig, Gun self)
        {
            AdvancedGunBehaviour behav = self.GetComponent<AdvancedGunBehaviour>();
            if (behav != null)
            {
                if (behav.preventNormalReloadAudio)
                {
                    AkSoundEngine.SetSwitch("WPN_Guns", "Kthulu", self.gameObject);
                }
                else if (!string.IsNullOrEmpty(behav.overrideReloadSwitchGroup))
                {
                    AkSoundEngine.SetSwitch("WPN_Guns", behav.overrideReloadSwitchGroup, self.gameObject);
                }
            }
            bool result = orig(self);
            if (behav != null && (behav.preventNormalReloadAudio || !string.IsNullOrEmpty(behav.overrideReloadSwitchGroup)))
            {
                AkSoundEngine.SetSwitch("WPN_Guns", self.gunSwitchGroup, self.gameObject);
                if (result && behav.preventNormalReloadAudio && !string.IsNullOrEmpty(behav.overrideNormalReloadAudio))
                {
                    AkSoundEngine.PostEvent(self.GetComponent<AdvancedGunBehaviour>().overrideNormalReloadAudio, self.gameObject);
                }
            }
            return result;
        }

        /// <summary>
        /// Saves the data of the gun to a list. Save the variables you want to be saved here!
        /// </summary>
        /// <param name="data">The list.</param>
        /// <param name="dataIndex">DataIndex. You don't need to use this argument.</param>
        public virtual void MidGameSerialize(List<object> data, int dataIndex)
        {
            data.Add(preventNormalFireAudio);
            data.Add(preventNormalReloadAudio);
            data.Add(overrideNormalReloadAudio);
            data.Add(overrideNormalFireAudio);
            data.Add(everPickedUpByPlayer);
            data.Add(everPickedUp);
            data.Add(usesOverrideHeroSwordCooldown);
            data.Add(overrideHeroSwordCooldown);
            data.Add(overrideReloadSwitchGroup);
        }

        /// <summary>
        /// Sets the data of the gun to the contents of a list. Set the variables you want to be saved here!
        /// </summary>
        /// <param name="data">The list.</param>
        /// <param name="dataIndex">DataIndex. Add a number equal to the amount of your data to it.</param>
        public virtual void MidGameDeserialize(List<object> data, ref int dataIndex)
        {
            preventNormalFireAudio = (bool)data[dataIndex];
            preventNormalReloadAudio = (bool)data[dataIndex + 1];
            overrideNormalReloadAudio = (string)data[dataIndex + 2];
            overrideNormalFireAudio = (string)data[dataIndex + 3];
            everPickedUpByPlayer = (bool)data[dataIndex + 4];
            everPickedUp = (bool)data[dataIndex + 5];
            usesOverrideHeroSwordCooldown = (bool)data[dataIndex + 6];
            overrideHeroSwordCooldown = (float)data[dataIndex + 7];
            overrideReloadSwitchGroup = (string)data[dataIndex + 8];
            dataIndex += 9;
        }

        /// <summary>
        /// Start() is called when the gun is created. It's also called when the player picks up or drops the gun.
        /// </summary>
        public virtual void Start()
        {
            gun = base.GetComponent<Gun>();
            gun.OnInitializedWithOwner += OnInitializedWithOwner;
            if (gun.CurrentOwner != null)
            {
                OnInitializedWithOwner(gun.CurrentOwner);
            }
            gun.PostProcessProjectile += PostProcessProjectile;
            gun.PostProcessVolley += PostProcessVolley;
            gun.OnDropped += OnDropped;
            gun.OnAutoReload += OnAutoReload;
            gun.OnReloadPressed += OnReloadPressed;
            gun.OnFinishAttack += OnFinishAttack;
            gun.OnPostFired += OnPostFired;
            gun.OnAmmoChanged += OnAmmoChanged;
            gun.OnBurstContinued += OnBurstContinued;
            gun.OnPreFireProjectileModifier += OnPreFireProjectileModifier;
            base.StartCoroutine(UpdateCR());
        }

        public virtual void BraveOnLevelWasLoaded()
        {
        }

        private IEnumerator UpdateCR()
        {
            while (true)
            {
                NonCurrentGunUpdate();
                yield return null;
            }
        }

        /// <summary>
        /// NonCurrentGunUpdate() is called every tick EVEN IF THE GUN ISN'T ENABLED. That means it's able to run even if the player's current gun isn't this behaviour's gun.
        /// </summary>
        protected virtual void NonCurrentGunUpdate()
        {
        }

        /// <summary>
        /// OnInitializedWithOwner() is called when a GunInventory creates a gun to add (for example when the player picks the gun up.) 
        /// </summary>
        /// <param name="actor">The gun's owner.</param>
        public virtual void OnInitializedWithOwner(GameActor actor)
        {
        }

        /// <summary>
        /// PostProcessProjectile() is called right after the gun shoots a projectile. If you want to change properties of a projectile in runtime, this is the place to do it.
        /// </summary>
        /// <param name="projectile">The target projectile.</param>
        public virtual void PostProcessProjectile(Projectile projectile)
        {
        }

        /// <summary>
        /// PostProcessVolley() is called when PlayerStats rebuilds a gun's volley. It's used by things like VolleyModificationSynergyProcessor to change the gun's volley if the player has a synergy.
        /// </summary>
        /// <param name="volley">The target volley.</param>
        public virtual void PostProcessVolley(ProjectileVolleyData volley)
        {
        }

        /// <summary>
        /// OnDropped() is called when an a player drops the gun. gun.CurrentOwner is set to null before this method is even called, so I wouldn't reccomend using it.
        /// </summary>
        public virtual void OnDropped()
        {
        }

        /// <summary>
        /// OnAutoReload() is called when a player reloads the gun with an empty clip.
        /// </summary>
        /// <param name="player">The player that reloaded the gun. Will be null if the gun's owner isn't a player.</param>
        /// <param name="gun">The gun.</param>
        public virtual void OnAutoReload(PlayerController player, Gun gun)
        {
            if (player != null)
            {
                OnAutoReloadSafe(player, gun);
            }
        }

        /// <summary>
        /// OnAutoReloadSafe() is called when a player reloads the gun with an empty clip and the gun's owner is a player.
        /// </summary>
        /// <param name="player">The player that reloaded the gun. Can't be null.</param>
        /// <param name="gun">The gun.</param>
        public virtual void OnAutoReloadSafe(PlayerController player, Gun gun)
        {
        }

        /// <summary>
        /// OnReloadPressed() is called when the owner reloads the gun or the player presses the reload key.
        /// </summary>
        /// <param name="player">The player that reloaded the gun/pressed the reload key. Will be null if the gun's owner isn't a player.</param>
        /// <param name="gun">The gun.</param>
        /// <param name="manualReload">True if the owner reloaded the gun by pressing the reload key. False if the owner reloaded the gun by firing with an empty clip.</param>
        public virtual void OnReloadPressed(PlayerController player, Gun gun, bool manualReload)
        {
            if (hasReloaded && gun.IsReloading)
            {
                OnReload(player, gun);
                hasReloaded = false;
            }
            if (player != null)
            {
                OnReloadPressedSafe(player, gun, manualReload);
            }
        }

        /// <summary>
        /// OnGunsChanged() is called when the player changes the current gun.
        /// </summary>
        /// <param name="previous">The previous current gun.</param>
        /// <param name="current">The new current gun.</param>
        /// <param name="newGun">True if the gun was changed because player picked up a new gun.</param>
        public virtual void OnGunsChanged(Gun previous, Gun current, bool newGun)
        {
            if (previous != gun && current == gun)
            {
                OnSwitchedToThisGun();
            }
            if (previous == gun && current != gun)
            {
                OnSwitchedAwayFromThisGun();
            }
        }

        /// <summary>
        /// OnSwitchedToThisGun() when the player switches to this behaviour's affected gun.
        /// </summary>
        public virtual void OnSwitchedToThisGun()
        {
        }

        /// <summary>
        /// OnSwitchedToThisGun() when the player switches away from this behaviour's affected gun.
        /// </summary>
        public virtual void OnSwitchedAwayFromThisGun()
        {
        }

        /// <summary>
        /// OnReloadPressedSafe() is called when the owner reloads the gun or the player presses the reload key and the gun's owner is a player.
        /// </summary>
        /// <param name="player">The player that reloaded the gun/pressed the reload key. Can't be null.</param>
        /// <param name="gun">The gun.</param>
        /// <param name="manualReload">True if the owner reloaded the gun by pressing the reload key. False if the owner reloaded the gun by firing with an empty clip.</param>
        public virtual void OnReloadPressedSafe(PlayerController player, Gun gun, bool manualReload)
        {
            if (hasReloaded && gun.IsReloading)
            {
                OnReloadSafe(player, gun);
                hasReloaded = false;
            }
        }

        /// <summary>
        /// OnReload() is called when the gun is reloaded.
        /// </summary>
        /// <param name="player">The player that reloaded the gun. Will be null if the gun's owner isn't a player.</param>
        /// <param name="gun">The gun.</param>
        public virtual void OnReload(PlayerController player, Gun gun)
        {
        }

        public static void Setup()
        {
            Hook hook = new Hook(
              typeof(Gun).GetMethod("Reload", BindingFlags.Public | BindingFlags.Instance),
              typeof(AdvancedGunBehaviour).GetMethod("ReloadHook")
            );
        }

        /// <summary>
        /// OnReloadEnded() is called at the end of reload.
        /// </summary>
        /// <param name="player">The player that reloaded the gun. Will be null if the gun's owner isn't a player.</param>
        /// <param name="gun">The gun.</param>
        public virtual void OnReloadEnded(PlayerController player, Gun gun)
        {
            if (player != null)
            {
                OnReloadEndedSafe(player, gun);
            }
        }

        /// <summary>
        /// OnReloadEndedSafe() is called at the end of reload and if the gun's owner is a player.
        /// </summary>
        /// <param name="player">The player that reloaded the gun. Can't be null.</param>
        /// <param name="gun">The gun.</param>
        public virtual void OnReloadEndedSafe(PlayerController player, Gun gun)
        {

        }

        /// <summary>
        /// OnReloadSafe() is called when the gun is reloaded and the gun's owner is a player.
        /// </summary>
        /// <param name="player">The player that reloaded the gun. Can't be null.</param>
        /// <param name="gun">The gun.</param>
        public virtual void OnReloadSafe(PlayerController player, Gun gun)
        {
        }

        /// <summary>
        /// OnFinishAttack() is called when the gun finishes firing, for example when the player releases the Shoot key or the gun's clip empties and if the owner is a player.
        /// </summary>
        /// <param name="player">The player. Can't be null.</param>
        /// <param name="gun">The gun.</param>
        public virtual void OnFinishAttack(PlayerController player, Gun gun)
        {
        }

        /// <summary>
        /// OnPostFired() is called after the gun fired and if the owner is a player.
        /// </summary>
        /// <param name="player">The player. Can't be null.</param>
        /// <param name="gun">The gun.</param>
        public virtual void OnPostFired(PlayerController player, Gun gun)
        {
            if (gun.IsHeroSword)
            {
                if (HeroSwordCooldown == 0.5f)
                {
                    OnHeroSwordCooldownStarted(player, gun);
                }
            }
        }

        /// <summary>
        /// OnHeroSwordCooldownStarted() when the gun's Sword Slash started and if the gun is a HeroSword (if gun.IsHeroSword = true).
        /// </summary>
        /// <param name="player"></param>
        /// <param name="gun"></param>
        public virtual void OnHeroSwordCooldownStarted(PlayerController player, Gun gun)
        {
            if (usesOverrideHeroSwordCooldown)
            {
                HeroSwordCooldown = overrideHeroSwordCooldown;
            }
        }

        /// <summary>
        /// OnAmmoChanged() is called when the gun's ammo amount increases/decreases.
        /// </summary>
        /// <param name="player">The player. Will be null if the gun's owner isn't a player.</param>
        /// <param name="gun">The gun.</param>
        public virtual void OnAmmoChanged(PlayerController player, Gun gun)
        {
            if (player != null)
            {
                OnAmmoChangedSafe(player, gun);
            }
        }

        /// <summary>
        /// OnAmmoChangedSafe() is called when the gun's ammo amount increases/decreases and if the gun's owner is a player.
        /// </summary>
        /// <param name="player">The player. Can't be null.</param>
        /// <param name="gun">The gun.</param>
        public virtual void OnAmmoChangedSafe(PlayerController player, Gun gun)
        {
        }

        /// <summary>
        /// OnBurstContinued() is called when the gun continues a burst (attacks while bursting).
        /// </summary>
        /// <param name="player">The player. Will be null if the gun's owner isn't a player.</param>
        /// <param name="gun">The gun.</param>
        public virtual void OnBurstContinued(PlayerController player, Gun gun)
        {
            if (player != null)
            {
                OnBurstContinuedSafe(player, gun);
            }
        }

        /// <summary>
        /// OnBurstContinuedSafe() is called when the gun continues a burst (attacks while bursting) and if the gun's owner is a player.
        /// </summary>
        /// <param name="player">The player. Can't be null.</param>
        /// <param name="gun">The gun.</param>
        public virtual void OnBurstContinuedSafe(PlayerController player, Gun gun)
        {
        }

        /// <summary>
        /// OnPreFireProjectileModifier() is called before the gun shoots a projectile. If the method returns something that's not the projectile argument, the projectile the gun will shoot will be replaced with the returned projectile.
        /// </summary>
        /// <param name="gun">The gun.</param>
        /// <param name="projectile">Original projectile.</param>
        /// <param name="mod">Target ProjectileModule.</param>
        /// <returns>The replacement projectile.</returns>
        public virtual Projectile OnPreFireProjectileModifier(Gun gun, Projectile projectile, ProjectileModule mod)
        {
            return projectile;
        }

        public AdvancedGunBehaviour()
        {
        }

        /// <summary>
        /// OnPickup() is called when an actor picks the gun up.
        /// </summary>
        /// <param name="owner">The actor that picked up the gun.</param>
        protected virtual void OnPickup(GameActor owner)
        {
            if (owner is PlayerController)
            {
                OnPickedUpByPlayer(owner as PlayerController);
            }
            if (owner is AIActor)
            {
                OnPickedUpByEnemy(owner as AIActor);
            }
        }

        /// <summary>
        /// OnPostDrop() is called AFTER the owner drops the gun.
        /// </summary>
        /// <param name="owner">The actor that dropped the gun.</param>
        protected virtual void OnPostDrop(GameActor owner)
        {
            if (owner is PlayerController)
            {
                OnPostDroppedByPlayer(owner as PlayerController);
            }
            if (owner is AIActor)
            {
                OnPostDroppedByEnemy(owner as AIActor);
            }
        }

        /// <summary>
        /// OnPickup() is called when a player picks the gun up.
        /// </summary>
        /// <param name="player">The player that picked up the gun.</param>
        protected virtual void OnPickedUpByPlayer(PlayerController player)
        {
            player.GunChanged += OnGunsChanged;
        }

        /// <summary>
        /// OnPostDrop() is called AFTER the player drops the gun. If you modify player's stats here, don't forget to call player.stats.RecalculateStats()!
        /// </summary>
        /// <param name="player">The player that dropped the gun.</param>
        protected virtual void OnPostDroppedByPlayer(PlayerController player)
        {
        }

        /// <summary>
        /// OnPickup() is called when an enemy picks the gun up.
        /// </summary>
        /// <param name="enemy">The enemy that picked up the gun.</param>
        protected virtual void OnPickedUpByEnemy(AIActor enemy)
        {
        }

        /// <summary>
        /// OnPostDrop() is called AFTER the enemy drops the gun.
        /// </summary>
        /// <param name="enemy">The enemy that dropped the gun.</param>
        protected virtual void OnPostDroppedByEnemy(AIActor enemy)
        {
        }

        /// <summary>
        /// Returns true if the gun's current owner isn't null.
        /// </summary>
        public bool PickedUp
        {
            get
            {
                return gun.CurrentOwner != null;
            }
        }

        /// <summary>
        /// If the gun's owner is a player, returns the gun's current owner as a player.
        /// </summary>
        public PlayerController Player
        {
            get
            {
                if (gun.CurrentOwner is PlayerController)
                {
                    return gun.CurrentOwner as PlayerController;
                }
                return null;
            }
        }

        /// <summary>
        /// Returns the HeroSwordCooldown of the gun if it isn't null. If it's null, returns -1.
        /// </summary>
        public float HeroSwordCooldown
        {
            get
            {
                if (gun != null)
                {
                    return (float)heroSwordCooldown.GetValue(gun);
                }
                return -1f;
            }
            set
            {
                if (gun != null)
                {
                    heroSwordCooldown.SetValue(gun, value);
                }
            }
        }

        /// <summary>
        /// Returns the gun's current owner.
        /// </summary>
        public GameActor Owner
        {
            get
            {
                return gun.CurrentOwner;
            }
        }

        /// <summary>
        /// Returns true if the gun's owner isn't null and is a player.
        /// </summary>
        public bool PickedUpByPlayer
        {
            get
            {
                return Player != null;
            }
        }

        private bool pickedUpLast = false;
        private GameActor lastOwner = null;
        /// <summary>
        /// Returns true if the gun was ever picked up by a player.
        /// </summary>
        public bool everPickedUpByPlayer = false;
        /// <summary>
        /// Returns true if the gun was ever picked up.
        /// </summary>
        public bool everPickedUp = false;
        /// <summary>
        /// Returns the gun this behaviour is applied to.
        /// </summary>
        protected Gun gun;
        private bool hasReloaded = true;
        /// <summary>
        /// If true, prevents the gun's normal fire audio.
        /// </summary>
        public bool preventNormalFireAudio;
        /// <summary>
        /// If true, prevents the gun's normal reload audio.
        /// </summary>
        public bool preventNormalReloadAudio;
        /// <summary>
        /// The gun's override fire audio. Only works if preventNormalFireAudio is true.
        /// </summary>
        public string overrideNormalFireAudio;
        public string overrideReloadSwitchGroup;
        /// <summary>
        /// The gun's override reload audio. Only works if preventNormalReloadAudio is true.
        /// </summary>
        public string overrideNormalReloadAudio;
        public bool usesOverrideHeroSwordCooldown;
        public float overrideHeroSwordCooldown;
        private static FieldInfo heroSwordCooldown = typeof(Gun).GetField("HeroSwordCooldown", BindingFlags.NonPublic | BindingFlags.Instance);
    }
}

using GadgetCore.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// Base class to extend when making custom entities as an alterative to the vanilla entity classes.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public abstract class CustomEntityScript<T> : CustomEntityScript where T : CustomEntityScript<T>
    {
        /// <summary>
        /// This entity's HP.
        /// </summary>
        public override int HP
        {
            get
            {
                return IsMaster ? m_HP : Master.HP;
            }

            protected set
            {
                if (IsMaster) m_HP = value;
                else Master.HP = value;
            }
        }
        private int m_HP;

        /// <summary>
        /// This entity's max HP.
        /// </summary>
        public override int MaxHP
        {
            get
            {
                return IsMaster ? m_MaxHP : Master.MaxHP;
            }

            protected set
            {
                if (IsMaster) m_MaxHP = value;
                else Master.MaxHP = value;
            }
        }
        private int m_MaxHP;

        /// <summary>
        /// Indicates whether this entity is currently considered to be dead.
        /// </summary>
        public override bool IsDead
        {
            get
            {
                return IsMaster ? m_IsDead : Master.IsDead;
            }

            protected set
            {
                if (IsMaster) m_IsDead = value;
                else Master.IsDead = value;
            }
        }
        private bool m_IsDead;

        /// <summary>
        /// The amount of EXP that this entity will drop.
        /// </summary>
        public override int EXP
        {
            get
            {
                return IsMaster ? m_EXP : Master.EXP;
            }

            protected set
            {
                if (IsMaster) m_EXP = value;
                else Master.EXP = value;
            }
        }
        private int m_EXP;

        /// <summary>
        /// This entity's "Enemy ID", as used by the base game's quest system.
        /// </summary>
        public override int EnemyID
        {
            get
            {
                return IsMaster ? m_EnemyID : Master.EnemyID;
            }

            protected set
            {
                if (IsMaster) m_EnemyID = value;
                else Master.EnemyID = value;
            }
        }
        private int m_EnemyID;

        /// <summary>
        /// Indicates whether this enemy is flying, and thereby able to ignore terrain collision.
        /// </summary>
        public override bool IsFlying
        {
            get
            {
                return IsMaster ? m_IsFlying : Master.IsFlying;
            }

            protected set
            {
                if (IsMaster) m_IsFlying = value;
                else Master.IsFlying = value;
            }
        }
        private bool m_IsFlying;

        /// <summary>
        /// Indicates whether this enemy is a boss.
        /// </summary>
        public override bool IsBoss
        {
            get
            {
                return IsMaster ? m_IsBoss : Master.IsBoss;
            }

            protected set
            {
                if (IsMaster) m_IsBoss = value;
                else Master.IsBoss = value;
            }
        }
        private bool m_IsBoss;

        /// <summary>
        /// This entity's contact damage. Will not cause contact damage at all if value is 0.
        /// </summary>
        public override int ContactDamage
        {
            get
            {
                return m_ContactDamage ?? (IsMaster ? 0 : Master.ContactDamage);
            }

            protected set
            {
                m_ContactDamage = value;
            }
        }
        private int? m_ContactDamage;

        /// <summary>
        /// How much poison effect this entity inflicts with its contact damage. Note that this is specific to this script - it is not inherited from the master script, if applicable.
        /// </summary>
        public override int PoisonEffect { get { return m_PoisonEffect + GameScript.challengeLevel * 5; } protected set { m_PoisonEffect = value; } }
        private int m_PoisonEffect;
        /// <summary>
        /// How much poison effect this entity inflicts with its contact damage. Note that this is specific to this script - it is not inherited from the master script, if applicable.
        /// </summary>
        public override int FrostEffect { get { return m_FrostEffect + GameScript.challengeLevel * 5; } protected set { m_FrostEffect = value; } }
        private int m_FrostEffect;
        /// <summary>
        /// How much poison effect this entity inflicts with its contact damage. Note that this is specific to this script - it is not inherited from the master script, if applicable.
        /// </summary>
        public override int BurnEffect { get { return m_BurnEffect + GameScript.challengeLevel * 5; } protected set { m_BurnEffect = value; } }
        private int m_BurnEffect;
        /// <summary>
        /// How much poison effect this entity inflicts with its contact damage. Note that this is specific to this script - it is not inherited from the master script, if applicable.
        /// </summary>
        public override int ConfuseEffect { get { return m_ConfuseEffect + GameScript.challengeLevel * 5; } protected set { m_ConfuseEffect = value; } }
        private int m_ConfuseEffect;
        /// <summary>
        /// How much poison effect this entity inflicts with its contact damage. Note that this is specific to this script - it is not inherited from the master script, if applicable.
        /// </summary>
        public override int SilenceEffect { get { return m_SilenceEffect + GameScript.challengeLevel * 5; } protected set { m_SilenceEffect = value; } }
        private int m_SilenceEffect;
        /// <summary>
        /// How much poison effect this entity inflicts with its contact damage. Note that this is specific to this script - it is not inherited from the master script, if applicable.
        /// </summary>
        public override int PetrifyEffect { get { return m_PetrifyEffect + GameScript.challengeLevel * 5; } protected set { m_PetrifyEffect = value; } }
        private int m_PetrifyEffect;

        /// <summary>
        /// Indicates whether this entity is immune to knockback.
        /// </summary>
        public override bool KnockbackImmune
        {
            get
            {
                return IsMaster ? m_KnockbackImmune : Master.KnockbackImmune;
            }

            protected set
            {
                if (IsMaster) m_KnockbackImmune = value;
                else Master.KnockbackImmune = value;
            }
        }
        private bool m_KnockbackImmune;

        /// <summary>
        /// The current target that this entity is trying to attack, or null if it is not currently trying to attack anything.
        /// </summary>
        public override GameObject AttackTarget
        {
            get
            {
                return IsMaster ? m_AttackTarget : Master.AttackTarget;
            }

            set
            {
                if (IsMaster) m_AttackTarget = value;
                else Master.AttackTarget = value;
            }
        }
        private GameObject m_AttackTarget;

        /// <summary>
        /// Represents whether this entity is stunned, and for how long, if it is a value greater than 0.
        /// </summary>
        public override float StunTime
        {
            get
            {
                return m_StunTimeUntil > Time.time ? m_StunTimeUntil - Time.time : 0;
            }

            set
            {
                m_StunTimeUntil = Time.time + value;
            }
        }
        private float m_StunTimeUntil;

        /// <summary>
        /// The master script for this entity, if applicable.
        /// </summary>
        public T Master
        {
            get
            {
                return !IsMaster ? m_Master.IsMaster ? m_Master : (m_Master = m_Master.Master) : null;
            }

            private set
            {
                m_Master = value;
            }
        }
        private T m_Master;

        /// <summary>
        /// The script that this entity is set to follow, usually assigned by <see cref="SetMaster(T, bool)"/>
        /// </summary>
        public T FollowTarget { get; protected set; }

        /// <summary>
        /// Indicates whether this script is the master of its own entity.
        /// </summary>
        public bool IsMaster { get; private set; } = true;

        /// <summary>
        /// The ID of this entity's loot table.
        /// </summary>
        public virtual string LootTableID
        {
            get
            {
                return "entity:" + transform.GetHighestParent().name.Split('(')[0];
            }
        }

        /// <summary>
        /// List of this entity's currency drops. Represented by ID, Quantity, and Quantity Variation in that order.
        /// </summary>
        protected List<Tuple<int, int, int>> currencyDrops = new List<Tuple<int, int, int>>();

        /// <summary>
        /// An array used for vanilla-style drops.
        /// </summary>
        protected int[] vanillaStyleDrops;

        /// <summary>
        /// The list of subordinate scripts that this script is the master of, if applicable.
        /// </summary>
        public ReadOnlyCollection<T> Subordinates
        {
            get
            {
                return m_Subordinates ?? (m_Subordinates = SubordinatesInternal.AsReadOnly());
            }
        }
        private ReadOnlyCollection<T> m_Subordinates;
        private readonly List<T> SubordinatesInternal = new List<T>();

        /// <summary>
        /// An array of two triggers that the vanilla game uses for many entities to assist in their targeting AI.
        /// </summary>
        public GameObject[] trig;

        /// <summary>
        /// Indicates whether this entity has been initialized with either <see cref="Initialize"/> or <see cref="SetMaster(T, bool)"/> yet.
        /// </summary>
        public virtual bool Initialized { get; protected set; }
        /// <summary>
        /// The object who's position is tracked and synchronized by the network.
        /// </summary>
        public virtual Transform TrackedPositionTransform { get; protected set; }
        /// <summary>
        /// The object who's position is tracked and synchronized by the network.
        /// </summary>
        public virtual Transform TrackedRotationTransform { get; protected set; }
        /// <summary>
        /// The Animation component that is controlled by this entity's animation mode.
        /// </summary>
        public virtual Animation AnimatedComponent { get; protected set; }

        /// <summary>
        /// Once true, indicates that this entity is ready for network position and rotation synchronization.
        /// </summary>
        protected bool networkReady;
        /// <summary>
        /// Field used for syncing the entity's position between clients.
        /// </summary>
        protected Vector3 networkPosition;
        /// <summary>
        /// Field used for syncing the entity's position between clients.
        /// </summary>
        protected Quaternion networkRotation;
        /// <summary>
        /// The current animation mode, which is synced between clients.
        /// </summary>
        protected int animMode;

        /// <summary>
        /// The list of possible animation modes for this entity.
        /// </summary>
        protected List<string> animModes = new List<string>();
        /// <summary>
        /// The set of <see cref="animModes"/> that reset back to 0 after playing once.
        /// </summary>
        protected HashSet<int> resettingModes = new HashSet<int>();

        /// <summary>
        /// Initializes this <see cref="CustomEntityScript{T}"/> with the given properties.
        /// </summary>
        protected virtual void Initialize(int hp, int contactDamage, int exp, bool isFlying = false, bool isBoss = false, bool knockbackImmune = false, int enemyID = -1)
        {
            MaxHP = hp;
            HP = hp;
            ContactDamage = contactDamage;
            EXP = exp;
            IsFlying = isFlying;
            IsBoss = isBoss;
            KnockbackImmune = knockbackImmune;
            EnemyID = enemyID;

            TrackedPositionTransform = transform.GetHighestParent();
            Transform eTransform = transform.Find("e");
#pragma warning disable IDE0029 // Use coalesce expression
            TrackedRotationTransform = eTransform != null ? eTransform : transform;
#pragma warning restore IDE0029 // Use coalesce expression
            AnimatedComponent = GetComponentInChildren<Animation>();

            InternalInit();
        }

        /// <summary>
        /// Sets the given GameObject as the master of this GameObject, so that most events and properties will be handled by it.
        /// If this method is called, then there is no need to call <see cref="Initialize"/>.
        /// </summary>
        protected void SetMaster(GameObject obj, bool follow)
        {
            T entity = obj.GetComponent<T>();
            if (entity == null) throw new InvalidOperationException($"Cannot set object without {typeof(T).Name} component on it as master of this script!");
            SetMaster(entity, follow);
        }

        /// <summary>
        /// Sets the given GameObject as the master of this GameObject, so that most events and properties will be handled by it.
        /// If this method is called, then there is no need to call <see cref="Initialize"/>.
        /// </summary>
        protected void SetMaster(T entity, bool follow)
        {
            entity.SubordinatesInternal.Add((T)this);
            Master = entity;
            IsMaster = false;

            TrackedPositionTransform = transform;
            TrackedRotationTransform = transform;
            AnimatedComponent = GetComponentInChildren<Animation>();

            InternalInit();

            if (follow)
            {
                StartCoroutine(UpdateFollowTarget(entity));
            }
        }

        /// <summary>
        /// Method for initialization called by both <see cref="Initialize"/> and <see cref="SetMaster(T, bool)"/>
        /// </summary>
        protected virtual void InternalInit()
        {
            if (!Initialized)
            {
                NetworkEnemyBasic networkEnemyBasic = GetComponent<NetworkEnemyBasic>();
                if (networkEnemyBasic != null) Destroy(networkEnemyBasic);
                NetworkR2 networkR2 = GetComponent<NetworkR2>();
                if (networkR2 != null) Destroy(networkR2);
                NetworkR4 networkR4 = GetComponent<NetworkR4>();
                if (networkR4 != null) Destroy(networkR4);
                NetworkRotation networkRotation = GetComponent<NetworkRotation>();
                if (networkRotation != null) Destroy(networkRotation);

                StartCoroutine(InternalFixedUpdate());

                if (Network.isServer && trig != null && trig.Length >= 2 && trig[0] != null && trig[1] != null) StartCoroutine(TriggerAlternate());

                StartCoroutine(Ready());
            }

            Initialized = true;
        }

        /// <summary>
        /// Sets <see cref="networkReady"/> after a delay.
        /// </summary>
        protected virtual IEnumerator Ready()
        {
            yield return new WaitForSeconds(0.1f);
            networkReady = true;
            yield break;
        }

        private IEnumerator TriggerAlternate()
        {
            while (true)
            {
                if (!IsDead)
                {
                    trig[0].SetActive(false);
                    trig[1].SetActive(true);
                    yield return new WaitForSeconds(0.5f);
                    trig[1].SetActive(false);
                    trig[0].SetActive(true);
                }
                yield return new WaitForSeconds(1f);
            }
        }

        /// <summary>
        /// Adds an item to this entity's loot table.
        /// </summary>
        public void AddLootTableDrop(Item item, float dropChance, int minDropQuantity, int maxDropQuantity = -1, Func<Vector3, bool> CheckValidToDrop = null, Func<Item, Vector3, bool> CustomDropBehavior = null)
        {
            LootTables.AddItemToLootTable(item, LootTableID, dropChance, minDropQuantity, maxDropQuantity, CheckValidToDrop, CustomDropBehavior);
        }

        /// <summary>
        /// Adds a quantity of currency to this entity's drops.
        /// </summary>
        public void AddCurrencyDrop(int currencyID, int quantity, int quantityVariation = 0)
        {
            currencyDrops.Add(Tuple.Create(currencyID, quantity, quantityVariation));
        }

        /// <summary>
        /// Sets a vanilla-style drops array to be used for this entity's drops.
        /// </summary>
        protected void SetVanillaStyleDrops(int percent30Drop, int percent25Drop, int percent20Drop, int failDrop = 0)
        {
            vanillaStyleDrops = new int[]
            {
                percent30Drop,
                percent25Drop,
                percent20Drop,
                failDrop
            };
        }

        /// <summary>
        /// Adds a new animation mode, and returns the index of the new mode. The mode with an index of 0 will be the default mode.
        /// </summary>
        protected virtual int AddAnimationMode(string name)
        {
            int index = animModes.Count;
            animModes.Add(name);
            return index;
        }

        /// <summary>
        /// Adds the vanilla animation modes supported by <see cref="NetworkR2"/>.
        /// If this method is to be used, it should be used before calling <see cref="AddAnimationMode(string)"/>.
        /// Be aware that any and all animations previously added by <see cref="AddAnimationMode(string)"/> will be offset by 6 after using this method.
        /// Additionally, the animation "i" will become the default mode with an index of 0.
        /// </summary>
        protected void AddVanillaR2Animations()
        {
            animModes.InsertRange(0, new string[]
            {
                "i",
                "a",
                "j",
                "a",
                "a2",
                "r"
            });
            resettingModes = new HashSet<int>(resettingModes.Select(x => x + 6).Union(new int[] { 1, 3, 4 }));
        }

        private IEnumerator InternalFixedUpdate()
        {
            while (true)
            {
                if (StunTime > 0 && rigidbody != null) rigidbody.velocity = Vector3.zero;

                Animation anim = AnimatedComponent;
                if (anim != null && animMode >= 0 && animMode < animModes.Count)
                {
                    anim.Play(animModes[animMode]);
                    if (resettingModes.Contains(animMode)) animMode = 0;
                }

                if (!GetComponent<NetworkView>().isMine)
                {
                    if (networkReady)
                    {
                        Transform trackedPosition = TrackedPositionTransform;
                        if (trackedPosition != null) TrackedPositionTransform.position = Vector3.Lerp(trackedPosition.position, networkPosition, 0.3f);
                        Transform trackedRotation = TrackedRotationTransform;
                        if (trackedRotation != null) TrackedRotationTransform.rotation = networkRotation;
                    }
                }

                yield return new WaitForFixedUpdate();
            }
        }

        /// <summary>
        /// Updates the entity's position and rotation to follow its <see cref="FollowTarget"/>
        /// </summary>
        protected internal IEnumerator UpdateFollowTarget(T followTarget)
        {
            if (followTarget != null) FollowTarget = followTarget;
            while (FollowTarget)
            {
                Vector3 fromTo = FollowTarget.transform.position - transform.position;
                float distance = fromTo.magnitude;
                if (distance > MaxFollowDistance)
                {
                    Vector3 newPos = FollowTarget.transform.position - fromTo * (MaxFollowDistance / distance);
                    transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(newPos.y - transform.position.y, newPos.x - transform.position.x) * Mathf.Rad2Deg, Vector3.forward);
                    if (rigidbody != null)
                    {
                        rigidbody.MovePosition(newPos);
                    }
                    else
                    {
                        transform.position = newPos;
                    }
                }
                else if (distance < MinFollowDistance)
                {
                    Vector3 newPos = FollowTarget.transform.position - fromTo * (MinFollowDistance / distance);
                    transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(newPos.y - transform.position.y, newPos.x - transform.position.x) * Mathf.Rad2Deg, Vector3.forward);
                    if (rigidbody != null)
                    {
                        rigidbody.MovePosition(newPos);
                    }
                    else
                    {
                        transform.position = newPos;
                    }
                }
                yield return new WaitForFixedUpdate();
            }
        }

        /// <summary>
        /// Breaks the master/subordinate relationship between this script and the specified subordinate.
        /// Optionally physically detaches the subordinate object to convert it into an independent object.
        /// </summary>
        protected virtual bool SeperateSubordinate(T subordinate, bool unfollow, bool detach)
        {
            if (SubordinatesInternal.Remove(subordinate))
            {
                if (subordinate != null)
                {
                    subordinate.m_MaxHP = MaxHP;
                    subordinate.m_HP = HP;
                    subordinate.m_IsDead = IsDead;
                    subordinate.m_EXP = EXP;
                    subordinate.m_EnemyID = EnemyID;
                    subordinate.m_IsFlying = IsFlying;
                    subordinate.m_IsBoss = IsBoss;
                    subordinate.m_ContactDamage = ContactDamage;
                    subordinate.m_PoisonEffect = PoisonEffect;
                    subordinate.m_FrostEffect = FrostEffect;
                    subordinate.m_BurnEffect = BurnEffect;
                    subordinate.m_ConfuseEffect = ConfuseEffect;
                    subordinate.m_SilenceEffect = SilenceEffect;
                    subordinate.m_PetrifyEffect = PetrifyEffect;
                    subordinate.m_KnockbackImmune = KnockbackImmune;
                    subordinate.m_AttackTarget = AttackTarget;

                    subordinate.Master = null;
                    subordinate.IsMaster = true;

                    if (unfollow) subordinate.FollowTarget = null;

                    if (detach) subordinate.transform.parent = null;

                    subordinate.Initialize(MaxHP, ContactDamage, EXP, IsFlying, IsBoss, KnockbackImmune, EnemyID);

                    subordinate.HP = HP;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Called to make this entity take damage from player attacks.
        /// </summary>
        [RPC]
        public virtual void TD(float[] msg)
        {
            int damage = (int)msg[0];
            float knockbackSourceX = msg[1];

            Damage(damage, knockbackSourceX);
        }

        /// <summary>
        /// Called to display damage numbers on all clients when the entity takes damage.
        /// </summary>
        [RPC]
        public virtual void TDTEXT(int a)
        {
            if (a > 0)
            {
                AudioClip damageSound = DamageSound;
                if (damageSound != null)
                {
                    AudioSource audioSource = GetComponent<AudioSource>();
                    if (audioSource != null)
                    {
                        audioSource.PlayOneShot(damageSound, Menuu.soundLevel / 10f);
                    }
                }
            }

            GameObject gameObject = (GameObject)Instantiate(Resources.Load("txt"), transform.position, Quaternion.identity);
            gameObject.SendMessage("Init", a);
        }

        /// <summary>
        /// Damages this entity by the given amount, and with knockback from the given X coordinate.
        /// A knockback value of exactly -100f is a special value that prevents any knockback from being applied.
        /// </summary>
        public virtual void Damage(int damage, float knockbackSourceX)
        {
            if (!IsMaster)
            {
                Master.Damage(damage, knockbackSourceX);
                return;
            }
            if (IsDead)
            {
                return;
            }

            float knockback = knockbackSourceX != -100f ? knockbackSourceX > transform.position.x ? -20f : 20f : 0f;

            HandleDamage(ref damage, ref knockback);

            GetComponent<NetworkView>().RPC("TDTEXT", RPCMode.All, damage);

            if (knockback != 0)
            {
                Knockback(new Vector3(knockback, rigidbody.velocity.y + 2f, 0));
            }

            if (damage != 0)
            {
                HP -= damage;
                if (HP <= 0)
                {
                    Kill(true);
                }
            }
        }

        /// <summary>
        /// Called when the planet this entity is in unloads.
        /// </summary>
        protected virtual void Wipe()
        {
            if (Network.isServer && !IsDead)
            {
                if (!IsMaster)
                {
                    Master.Wipe();
                    return;
                }

                HandleDeath(false);
                GetComponent<NetworkView>().RPC("Die", RPCMode.All, 0);
            }
        }

        /// <summary>
        /// Called whenever this entity takes damage - the damage and knockback amount can be modified by this method.
        /// </summary>
        protected virtual void HandleDamage(ref int damage, ref float knockback) { }

        /// <summary>
        /// Knocks the enemy back in the specified direction.
        /// </summary>
        public virtual void Knockback(Vector3 direction)
        {
            if (!KnockbackImmune) StartCoroutine(KnockbackRoutine(direction));
        }

        private IEnumerator KnockbackRoutine(Vector3 direction)
        {
            Stun(0.2f);
            float startTime = Time.time;
            while (Time.time - startTime < 0.2f)
            {
                rigidbody.MovePosition(transform.position + direction * Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }
        }

        /// <summary>
        /// Stuns the enemy for a specific period of time.
        /// </summary>
        public virtual void Stun(float time)
        {
            if (StunTime < time) StunTime = time;
        }

        /// <summary>
        /// Causes this entity to die.
        /// </summary>
        public virtual void Kill(bool isPlayerKill)
        {
            if (!IsMaster)
            {
                Master.Kill(isPlayerKill);
                return;
            }
            if (IsDead)
            {
                return;
            }

            if (HandleDeath(isPlayerKill))
            {
                m_HP = 0;
                IsDead = true;
                GetComponent<NetworkView>().RPC("Die", RPCMode.All, isPlayerKill ? 1 : 0);
            }
        }

        /// <summary>
        /// RPC to facilitate the entity's death. An 'a' value of 1 indicates that it was killed by the player, a value of 0 indicates that it was erased by the game.
        /// </summary>
        [RPC]
        public virtual void Die(int a)
        {
            DieInternal(a);
        }

        /// <summary>
        /// Recursive function to destroy this object, as well as all subordinate objects.
        /// </summary>
        protected virtual void DieInternal(int a)
        {
            m_HP = 0;
            IsDead = true;

            foreach (CustomEntityScript<T> subordinate in Subordinates)
            {
                subordinate.DieInternal(a);
            }

            if (IsMaster && a == 1) DropLocal();
            Hide();
        }

        /// <summary>
        /// Hides this object without actually destroying or disabling it, to ensure that RPCs can still be sent.
        /// </summary>
        protected virtual void Hide()
        {
            transform.position = new Vector3(-500f, -500f, -500f);
        }

        /// <summary>
        /// Truly destroys this object on the server, after a delay to allow RPCs to be sent first.
        /// </summary>
        protected virtual IEnumerator Exile()
        {
            yield return new WaitForSeconds(1.5f);
            Network.RemoveRPCs(GetComponent<NetworkView>().viewID);
            Network.Destroy(gameObject);
            yield break;
        }

        /// <summary>
        /// Spawns this entity's drops locally.
        /// </summary>
        protected virtual void DropLocal()
        {
            GadgetCoreAPI.SpawnExp(transform.position, EXP);

            foreach (Tuple<int, int, int> currencyDrop in currencyDrops)
            {
                int quantity = currencyDrop.Item2 + UnityEngine.Random.Range(0, currencyDrop.Item3 + 1);
                if (quantity > 0)
                {
                    GadgetCoreAPI.SpawnItemLocal(transform.position, new Item(currencyDrop.Item1, quantity, 0, 0, 0, new int[3], new int[3]));
                }
            }

            if (vanillaStyleDrops != null)
            {
                int num = UnityEngine.Random.Range(0, 100);
                int num2 = UnityEngine.Random.Range(0, 90);
                num2 += (int)(PlayerGearModsTracker.GetGearMods(FindObjectsOfType<PlayerScript>().MinBy(x => (x.transform.position - transform.position).sqrMagnitude))[21] * 1.5);
                int num3 = 1;
                if (num2 > 110)
                {
                    num3 = 5;
                }
                else if (num2 > 100)
                {
                    num3 = 4;
                }
                else if (num2 > 90)
                {
                    num3 = 3;
                }
                else if (num2 > 80)
                {
                    num3 = 2;
                }

                Item vanillaDrop = new Item(num < 20 ? vanillaStyleDrops[2] : num < 45 ? vanillaStyleDrops[1] : num < 75 ? vanillaStyleDrops[0] : vanillaStyleDrops[3], num3, 0, 0, 0, new int[3], new int[3]);

                if (vanillaDrop.id > 0)
                {
                    GadgetCoreAPI.SpawnItemLocal(transform.position, vanillaDrop);
                }
            }

            LootTables.DropLoot(LootTableID, transform.position);
        }

        /// <summary>
        /// Called when a collider for this entity collides with something. Generally used for dealing contact damage to players.
        /// </summary>
        public virtual void OnCollisionEnter(Collision c)
        {
            if (c.gameObject.layer == 8 && c.gameObject.GetComponent<NetworkView>().isMine)
            {
                int contactDamage = ContactDamage;
                if (contactDamage > 0)
                {
                    c.gameObject.SendMessage("TD", contactDamage);
                    c.gameObject.SendMessage("K", transform.position.x);
                    int poisonEffect = PoisonEffect;
                    if (poisonEffect > 0)
                    {
                        c.gameObject.SendMessage("POI", poisonEffect);
                    }
                    int frostEffect = FrostEffect;
                    if (frostEffect > 0)
                    {
                        c.gameObject.SendMessage("FRO", frostEffect);
                    }
                    int burnEffect = BurnEffect;
                    if (burnEffect > 0)
                    {
                        c.gameObject.SendMessage("BUR", burnEffect);
                    }
                    int confuseEffect = ConfuseEffect;
                    if (confuseEffect > 0)
                    {
                        c.gameObject.SendMessage("CON", confuseEffect);
                    }
                    int silenceEffect = SilenceEffect;
                    if (silenceEffect > 0)
                    {
                        c.gameObject.SendMessage("SIL", silenceEffect);
                    }
                    int petrifyEffect = PetrifyEffect;
                    if (petrifyEffect > 0)
                    {
                        c.gameObject.SendMessage("PET", petrifyEffect);
                    }
                }
            }
            if (IsFlying && c.gameObject.layer == 11)
            {
                Physics.IgnoreCollision(GetComponent<Collider>(), c.gameObject.GetComponent<Collider>());
            }
        }

        /// <summary>
        /// Called when a trigger for this entity is triggered by something. Generally used for detecting players to target.
        /// </summary>
        public virtual void OnTriggerEnter(Collider c)
        {
            if (Network.isServer && (c.gameObject.layer == 8 || c.gameObject.layer == 26))
            {
                AttackTarget = c.gameObject;
            }
        }

        /// <summary>
        /// Handles data serialization for this entity.
        /// </summary>
        public void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
        {
            Transform trackedPosition = TrackedPositionTransform;
            Transform trackedRotation = TrackedRotationTransform;
            Animation trackedAnimation = AnimatedComponent;
            if (stream.isWriting)
            {
                if (trackedPosition != null)
                {
                    networkPosition = trackedPosition != null ? trackedPosition.position : Vector3.zero;
                    stream.Serialize(ref networkPosition);
                }
                if (trackedRotation != null)
                {
                    networkRotation = trackedRotation != null ? trackedRotation.rotation : Quaternion.identity;
                    stream.Serialize(ref networkRotation);
                }
                if (trackedAnimation != null)
                {
                    stream.Serialize(ref animMode);
                }
            }
            else
            {
                if (trackedPosition != null)
                {
                    stream.Serialize(ref networkPosition);
                }
                if (trackedRotation != null)
                {
                    stream.Serialize(ref networkRotation);
                }
                if (trackedAnimation != null)
                {
                    stream.Serialize(ref animMode);
                }
            }
            OnSerializeExtra(stream, info);
        }

        /// <summary>
        /// Can be overriden to perform extra network serialization beyond the standard position, rotation, and animation mode.
        /// </summary>
        protected virtual void OnSerializeExtra(BitStream stream, NetworkMessageInfo info) { }

        /// <summary>
        /// Called when this entity is about to die - return false to prevent it.
        /// </summary>
        protected virtual bool HandleDeath(bool isPlayerKill) { return true; }

        /// <summary>
        /// Called when this entity dies, on all clients.
        /// </summary>
        protected virtual void HandleDeathClient(bool isPlayerKill)
        {
            if (isPlayerKill)
            {
                if (EnemyID > 0)
                {
                    Camera.main.SendMessage("EnemyID", EnemyID);
                }
                if (SpawnerScript.curBiome < GameScript.record.Length)
                {
                    GameScript.record[SpawnerScript.curBiome]++;
                }
                if (IsBoss)
                {
                    GameScript.cadetValue += 15;
                }
                else
                {
                    GameScript.cadetValue += 5;
                }
                Menuu.characterStat[0]++;
                if (IsBoss)
                {
                    Menuu.characterStat[9]++;
                }
            }
        }

        /// <summary>
        /// Instantiates a CustomEntityScript. Do not use this, let Unity handle instantiation of the script.
        /// </summary>
        public CustomEntityScript() : base() { }
    }

    /// <summary>
    /// Non-generic base class for custom entities using <see cref="CustomEntityScript{T}"/>. Do not extend this class directly.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public abstract class CustomEntityScript : MonoBehaviour
    {
        /// <summary>
        /// Provides a quick reference to this GameObject's rigidbody, if it exists.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity Field")]
        public Rigidbody rigidbody
        {
            get
            {
                if (m_rigidbody != null) return m_rigidbody;
                else return m_rigidbody = GetComponent<Rigidbody>();
            }
        }
        private Rigidbody m_rigidbody;

        /// <summary>
        /// This entity's HP.
        /// </summary>
        public abstract int HP { get; protected set; }

        /// <summary>
        /// This entity's max HP.
        /// </summary>
        public abstract int MaxHP { get; protected set; }

        /// <summary>
        /// Indicates whether this entity is currently considered to be dead.
        /// </summary>
        public abstract bool IsDead { get; protected set; }

        /// <summary>
        /// The amount of EXP that this entity will drop.
        /// </summary>
        public abstract int EXP { get; protected set; }

        /// <summary>
        /// This entity's "Enemy ID", as used by the vanilla quest system.
        /// </summary>
        public abstract int EnemyID { get; protected set; }

        /// <summary>
        /// Indicates whether this enemy is flying, and thereby able to ignore terrain collision.
        /// </summary>
        public abstract bool IsFlying { get; protected set; }

        /// <summary>
        /// Indicates whether this enemy is a boss.
        /// </summary>
        public abstract bool IsBoss { get; protected set; }

        /// <summary>
        /// This entity's contact damage. Will not cause contact damage at all if value is 0.
        /// </summary>
        public abstract int ContactDamage { get; protected set; }

        /// <summary>
        /// How much poison effect this entity inflicts with its contact damage. Note that this is specific to this script - it is not inherited from the master script, if applicable.
        /// </summary>
        public abstract int PoisonEffect { get; protected set; }
        /// <summary>
        /// How much poison effect this entity inflicts with its contact damage. Note that this is specific to this script - it is not inherited from the master script, if applicable.
        /// </summary>
        public abstract int FrostEffect { get; protected set; }
        /// <summary>
        /// How much poison effect this entity inflicts with its contact damage. Note that this is specific to this script - it is not inherited from the master script, if applicable.
        /// </summary>
        public abstract int BurnEffect { get; protected set; }
        /// <summary>
        /// How much poison effect this entity inflicts with its contact damage. Note that this is specific to this script - it is not inherited from the master script, if applicable.
        /// </summary>
        public abstract int ConfuseEffect { get; protected set; }
        /// <summary>
        /// How much poison effect this entity inflicts with its contact damage. Note that this is specific to this script - it is not inherited from the master script, if applicable.
        /// </summary>
        public abstract int SilenceEffect { get; protected set; }
        /// <summary>
        /// How much poison effect this entity inflicts with its contact damage. Note that this is specific to this script - it is not inherited from the master script, if applicable.
        /// </summary>
        public abstract int PetrifyEffect { get; protected set; }

        /// <summary>
        /// Indicates whether this entity is immune to knockback.
        /// </summary>
        public abstract bool KnockbackImmune { get; protected set; }

        /// <summary>
        /// The current target that this entity is trying to attack, or null if it is not currently trying to attack anything.
        /// </summary>
        public abstract GameObject AttackTarget { get; set; }

        /// <summary>
        /// Represents whether this entity is stunned, and for how long, if it is a value greater than 0.
        /// </summary>
        public abstract float StunTime { get; set; }

        /// <summary>
        /// The distance at which this component will follow its follow target component, if it is a value greater than 0.
        /// </summary>
        public virtual float MaxFollowDistance { get; protected set; }

        /// <summary>
        /// The distance at which this component will follow its follow target component, if it is a value greater than 0.
        /// </summary>
        public virtual float MinFollowDistance { get; protected set; }


        /// <summary>
        /// The sound that plays when this entity is damaged.
        /// </summary>
        public virtual AudioClip DamageSound { get; protected set; }

        internal CustomEntityScript() { }
    }
}

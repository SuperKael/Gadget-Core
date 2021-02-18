using GadgetCore.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// Represents a craft menu, such as the emblem forge or the alchemy station. Also includes custom craft menus.
    /// </summary>
    public class PlanetInfo : RegistryEntry<PlanetInfo, PlanetType>
    {
        /// <summary>
        /// The PlanetType of this Planet
        /// </summary>
        public readonly PlanetType Type;

        /// <summary>
        /// The name of this Planet
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The list of possible exit portal destinations.
        /// </summary>
        public readonly List<Tuple<int, int>> WeightedExitPortalIDs;


        /// <summary>
        /// The Texture associated with this planet's entrance. May be null.
        /// </summary>
        public virtual Texture EntranceTex { get; protected set; }
        /// <summary>
        /// The Texture associated with this planet's zones. May be null.
        /// </summary>
        public virtual Texture ZoneTex { get; protected set; }
        /// <summary>
        /// The Texture associated with this planet's solid mid-chunks. May be null.
        /// </summary>
        public virtual Texture MidChunk0Tex { get; protected set; }
        /// <summary>
        /// The Texture associated with this planet's hollow mid-chunks. May be null.
        /// </summary>
        public virtual Texture MidChunk1Tex { get; protected set; }
        /// <summary>
        /// The Texture associated with this planet's horizontal side walls. May be null.
        /// </summary>
        public virtual Texture SideHTex { get; protected set; }
        /// <summary>
        /// The Texture associated with this planet's vertical side walls. May be null.
        /// </summary>
        public virtual Texture SideVTex { get; protected set; }
        /// <summary>
        /// The Texture associated with this planet's portal sign. May be null.
        /// </summary>
        public virtual Texture PortalSignTex { get; protected set; }
        /// <summary>
        /// The Texture associated with this planet's button in the planet selector. May be null.
        /// </summary>
        public virtual Texture SelectorButtonTex { get; protected set; }
        /// <summary>
        /// The Texture associated with this planet's icon in the planet selector. May be null.
        /// </summary>
        public virtual Texture SelectorIconTex { get; protected set; }
        /// <summary>
        /// The Texture associated with this planet's background parallax. May be null.
        /// </summary>
        public virtual Texture BackgroundParallaxTex { get; protected set; }
        /// <summary>
        /// The Textures associated with this planet's background images. There should be 4. May be null.
        /// </summary>
        public virtual Texture[] BackgroundImageTexes { get; protected set; }
        /// <summary>
        /// The Material associated with this planet's entrance. May be null.
        /// </summary>
        public virtual Material EntranceMat { get; protected set; }
        /// <summary>
        /// The Material associated with this planet's zones. May be null.
        /// </summary>
        public virtual Material ZoneMat { get; protected set; }
        /// <summary>
        /// The Material associated with this planet's solid mid-chunks. May be null.
        /// </summary>
        public virtual Material MidChunk0Mat { get; protected set; }
        /// <summary>
        /// The Material associated with this planet's hollow mid-chunks. May be null.
        /// </summary>
        public virtual Material MidChunk1Mat { get; protected set; }
        /// <summary>
        /// The Material associated with this planet's horizontal side walls. May be null.
        /// </summary>
        public virtual Material SideHMat { get; protected set; }
        /// <summary>
        /// The Material associated with this planet's vertical side walls. May be null.
        /// </summary>
        public virtual Material SideVMat { get; protected set; }
        /// <summary>
        /// The Material associated with this planet's portal sign. May be null.
        /// </summary>
        public virtual Material PortalSignMat { get; protected set; }
        /// <summary>
        /// The Material associated with this planet's button in the planet selector. May be null.
        /// </summary>
        public virtual Material SelectorButtonMat { get; protected set; }
        /// <summary>
        /// The Material associated with this planet's icon in the planet selector. May be null.
        /// </summary>
        public virtual Material SelectorIconMat { get; protected set; }
        /// <summary>
        /// The Material associated with this planet's background parallax. May be null.
        /// </summary>
        public virtual Material BackgroundParallaxMat { get; protected set; }
        /// <summary>
        /// The Materials associated with this planet's background images. There should be 4. May be null.
        /// </summary>
        public virtual Material[] BackgroundImageMats { get; protected set; }
        /// <summary>
        /// The background music that plays when at this Planet.
        /// </summary>
        public virtual AudioClip BackgroundMusic { get; protected set; }

        /// <summary>
        /// The number of portal uses the player has for this planet.
        /// </summary>
        public virtual int PortalUses
        {
            get
            {
                return m_PortalUses;
            }

            set
            {
                m_PortalUses = value;
                PreviewLabs.PlayerPrefs.SetInt("portalUses" + ID, m_PortalUses);
            }
        }
        internal int m_PortalUses;

        /// <summary>
        /// The number of relics the player has collected for this planet.
        /// </summary>
        public virtual int Relics
        {
            get
            {
                return m_Relics;
            }

            set
            {
                m_Relics = value;
                PreviewLabs.PlayerPrefs.SetInt("planetRelics" + ID, m_Relics);
            }
        }
        internal int m_Relics;

        /// <summary>
        /// Weighted list of possible spawn delegates in world spawn slots.
        /// </summary>
        protected List<Tuple<Func<Vector3, GameObject[]>, int>> weightedWorldSlotSpawns;
        /// <summary>
        /// Weighted list of possible spawn delegates in town spawn slots.
        /// </summary>
        protected List<Tuple<Func<Vector3, GameObject[]>, int>> weightedTownSlotSpawns;

        private Func<Vector3, GameObject[]>[] weightedWorldSlotSpawnsCache;
        private Func<Vector3, GameObject[]>[] weightedTownSlotSpawnsCache;

        /// <summary>
        /// Use to create a new PlanetInfo. Make sure to call Register on it to register your Planet.
        /// </summary>
        public PlanetInfo(PlanetType Type, string Name, Tuple<int, int>[] WeightedExitPortalIDs)
        {
            this.Type = Type;
            this.Name = Name;
            this.WeightedExitPortalIDs = WeightedExitPortalIDs?.ToList();
            BackgroundMusic = null;
        }

        /// <summary>
        /// Use to create a new PlanetInfo. Make sure to call Register on it to register your Planet.
        /// </summary>
        public PlanetInfo(PlanetType Type, string Name, Tuple<int, int>[] WeightedExitPortalIDs, AudioClip BackgroundMusic)
        {
            this.Type = Type;
            this.Name = Name;
            this.WeightedExitPortalIDs = WeightedExitPortalIDs?.ToList();
            this.BackgroundMusic = BackgroundMusic;
        }

        /// <summary>
        /// Sets information related to this planet's portal. If <paramref name="SelectorButtonTex"/> is null, the planet will not appear in the planet selector. This must be called before Register.
        /// </summary>
        public void SetPortalInfo(Texture PortalSignTex, Texture SelectorButtonTex = null, Texture SelectorIconTex = null)
        {
            this.PortalSignTex = PortalSignTex;
            this.SelectorButtonTex = SelectorButtonTex;
            this.SelectorIconTex = SelectorIconTex;
        }

        /// <summary>
        /// Sets information related to this planet's portal. If <paramref name="SelectorButtonMat"/> is null, the planet will not appear in the planet selector. This must be called before Register.
        /// </summary>
        public void SetPortalInfo(Material PortalSignMat, Material SelectorButtonMat = null, Material SelectorIconMat = null)
        {
            this.PortalSignMat = PortalSignMat;
            this.SelectorButtonMat = SelectorButtonMat;
            this.SelectorIconMat = SelectorIconMat;
        }

        /// <summary>
        /// Sets information related to this planet's terrain. This must be called before Register.
        /// </summary>
        public void SetTerrainInfo(Texture EntranceTex, Texture ZoneTex, Texture MidChunk0Tex, Texture MidChunk1Tex, Texture SideHTex, Texture SideVTex)
        {
            this.EntranceTex = EntranceTex;
            this.ZoneTex = ZoneTex;
            this.MidChunk0Tex = MidChunk0Tex;
            this.MidChunk1Tex = MidChunk1Tex;
            this.SideHTex = SideHTex;
            this.SideVTex = SideVTex;
        }

        /// <summary>
        /// Sets information related to this planet's terrain. This must be called before Register.
        /// </summary>
        public void SetTerrainInfo(Material EntranceMat, Material ZoneMat, Material MidChunk0Mat, Material MidChunk1Mat, Material SideHMat, Material SideVMat)
        {
            this.EntranceMat = EntranceMat;
            this.ZoneMat = ZoneMat;
            this.MidChunk0Mat = MidChunk0Mat;
            this.MidChunk1Mat = MidChunk1Mat;
            this.SideHMat = SideHMat;
            this.SideVMat = SideVMat;
        }

        /// <summary>
        /// Sets information related to this planet's background. This must be called before Register.
        /// </summary>
        public void SetBackgroundInfo(Texture BackgroundParallaxTex, Texture BackgroundImageTex0, Texture BackgroundImageTex1, Texture BackgroundImageTex2, Texture BackgroundImageTex3)
        {
            this.BackgroundParallaxTex = BackgroundParallaxTex;
            BackgroundImageTexes = new Texture[] { BackgroundImageTex0, BackgroundImageTex1, BackgroundImageTex2, BackgroundImageTex3 };
        }
        /// <summary>
        /// Sets information related to this planet's background. This must be called before Register.
        /// </summary>
        public void SetBackgroundInfo(Material BackgroundParallaxMat, Material BackgroundImageMat0, Material BackgroundImageMat1, Material BackgroundImageMat2, Material BackgroundImageMat3)
        {
            this.BackgroundParallaxMat = BackgroundParallaxMat;
            BackgroundImageMats = new Material[] { BackgroundImageMat0, BackgroundImageMat1, BackgroundImageMat2, BackgroundImageMat3 };
        }

        /// <summary>
        /// Registers this PlanetInfo to the PlanetRegistry.
        /// </summary>
        /// <param name="name">The registry name to use.</param>
        /// <param name="preferredID">If specified, will use this registry ID.</param>
        /// <param name="overrideExisting">If false, will not register if the preferred ID is already used. Ignored if no preferred ID is specified.</param>
        public virtual PlanetInfo Register(string name, int preferredID = -1, bool overrideExisting = true)
        {
            return RegisterInternal(name, preferredID, overrideExisting) as PlanetInfo;
        }

        /// <summary>
        /// Called after this Registry Entry has been registered to its Registry. You should never call this yourself.
        /// </summary>
        protected internal override void PostRegister()
        {
            if (GetEntryType() != PlanetType.SPECIAL || EntranceMat != null || EntranceTex != null)
            {
                if (EntranceMat == null)
                {
                    EntranceMat = new Material(Shader.Find("Unlit/Transparent"))
                    {
                        mainTexture = EntranceTex
                    };
                }
                else
                {
                    EntranceTex = EntranceMat.mainTexture;
                }
                GadgetCoreAPI.AddCustomResource("z/entrance" + ID, EntranceMat);
            }
            if (GetEntryType() != PlanetType.SPECIAL || ZoneMat != null || ZoneTex != null)
            {
                if (ZoneMat == null)
                {
                    ZoneMat = new Material(Shader.Find("Unlit/Transparent"))
                    {
                        mainTexture = ZoneTex
                    };
                }
                else
                {
                    ZoneTex = ZoneMat.mainTexture;
                }
                GadgetCoreAPI.AddCustomResource("z/z" + ID, ZoneMat);
            }
            if (GetEntryType() != PlanetType.SPECIAL || MidChunk0Mat != null || MidChunk0Tex != null)
            {
                if (MidChunk0Mat == null)
                {
                    MidChunk0Mat = new Material(Shader.Find("Unlit/Transparent"))
                    {
                        mainTexture = MidChunk0Tex
                    };
                }
                else
                {
                    MidChunk0Tex = MidChunk0Mat.mainTexture;
                }
                GadgetCoreAPI.AddCustomResource("z/midChunk0b" + ID, MidChunk0Mat);
            }
            if (GetEntryType() != PlanetType.SPECIAL || MidChunk1Mat != null || MidChunk1Tex != null)
            {
                if (MidChunk1Mat == null)
                {
                    MidChunk1Mat = new Material(Shader.Find("Unlit/Transparent"))
                    {
                        mainTexture = MidChunk1Tex
                    };
                }
                else
                {
                    MidChunk1Tex = MidChunk1Mat.mainTexture;
                }
                GadgetCoreAPI.AddCustomResource("z/midChunk1b" + ID, MidChunk1Mat);
            }
            if (GetEntryType() != PlanetType.SPECIAL || SideHMat != null || SideHTex != null)
            {
                if (SideHMat == null)
                {
                    SideHMat = new Material(Shader.Find("Unlit/Transparent"))
                    {
                        mainTexture = SideHTex
                    };
                }
                else
                {
                    SideHTex = SideHMat.mainTexture;
                }
                GadgetCoreAPI.AddCustomResource("side/sideH" + ID, SideHMat);
            }
            if (GetEntryType() != PlanetType.SPECIAL || SideVMat != null || SideVTex != null)
            {
                if (SideVMat == null)
                {
                    SideVMat = new Material(Shader.Find("Unlit/Transparent"))
                    {
                        mainTexture = SideVTex
                    };
                }
                else
                {
                    SideVTex = SideVMat.mainTexture;
                }
                GadgetCoreAPI.AddCustomResource("side/sideV" + ID, SideVMat);
            }
            if (GetEntryType() != PlanetType.SPECIAL || PortalSignMat != null || PortalSignTex != null)
            {
                if (PortalSignMat == null)
                {
                    PortalSignMat = new Material(Shader.Find("Unlit/Transparent"))
                    {
                        mainTexture = PortalSignTex
                    };
                }
                else
                {
                    PortalSignTex = PortalSignMat.mainTexture;
                }
                GadgetCoreAPI.AddCustomResource("sign/sign" + ID, PortalSignMat);
            }
            if (GetEntryType() != PlanetType.SPECIAL || SelectorButtonMat != null || SelectorButtonTex != null)
            {
                if (SelectorButtonMat == null)
                {
                    SelectorButtonMat = new Material(Shader.Find("Unlit/Transparent"))
                    {
                        mainTexture = SelectorButtonTex
                    };
                }
                else
                {
                    SelectorButtonTex = SelectorButtonMat.mainTexture;
                }
                GadgetCoreAPI.AddCustomResource("mat/planetIcon" + ID, SelectorButtonMat);
            }
            if (GetEntryType() != PlanetType.SPECIAL || SelectorIconMat != null || SelectorIconTex != null)
            {
                if (SelectorIconMat == null)
                {
                    SelectorIconMat = new Material(Shader.Find("Unlit/Transparent"))
                    {
                        mainTexture = SelectorIconTex
                    };
                }
                else
                {
                    SelectorIconTex = SelectorIconMat.mainTexture;
                }
                GadgetCoreAPI.AddCustomResource("mat/planet" + ID, SelectorIconMat);
            }
            if (GetEntryType() != PlanetType.SPECIAL || BackgroundParallaxMat != null || BackgroundParallaxTex != null)
            {
                if (BackgroundParallaxMat == null)
                {
                    BackgroundParallaxMat = new Material(Shader.Find("Unlit/Transparent"))
                    {
                        mainTexture = BackgroundParallaxTex
                    };
                }
                else
                {
                    BackgroundParallaxTex = BackgroundParallaxMat.mainTexture;
                }
                GadgetCoreAPI.AddCustomResource("par/parallax" + ID, BackgroundParallaxMat);
            }
            if (GetEntryType() != PlanetType.SPECIAL || BackgroundImageMats != null || BackgroundImageTexes != null)
            {
                if (BackgroundImageMats == null)
                {
                    BackgroundImageMats = BackgroundImageTexes?.Select(x => new Material(Shader.Find("Unlit/Transparent"))
                    {
                        mainTexture = x
                    }).ToArray();
                }
                else
                {
                    BackgroundImageTexes = BackgroundImageMats.Select(x => x.mainTexture).ToArray();
                }
                for (int i = 0; i < BackgroundImageMats.Length; i++)
                {
                    GadgetCoreAPI.AddCustomResource("bg/b" + ID + "bg" + i, BackgroundImageMats[i]);
                }
            }

            if (BackgroundMusic != null)
            {
                GadgetCoreAPI.AddCustomResource("Au/biome" + ID, BackgroundMusic);
            }

            for (int i = 0; i < WeightedExitPortalIDs.Count; i++)
            {
                if (WeightedExitPortalIDs[i].Item1 == -1) WeightedExitPortalIDs[i] = Tuple.Create(ID, WeightedExitPortalIDs[i].Item2);
            }
        }

        /// <summary>
        /// Adds the weighted spawn event to <see cref="OnSpawnBiomeSlot"/>
        /// </summary>
        protected virtual void AddWeightedBiomeSpawnEvent()
        {
            weightedWorldSlotSpawns = new List<Tuple<Func<Vector3, GameObject[]>, int>>();
            OnSpawnBiomeSlot += (Chunk c, Vector3 pos) =>
            {
                if (weightedWorldSlotSpawnsCache == null)
                {
                    List<Func<Vector3, GameObject[]>> weightedWorldSlotSpawnsCacheList = new List<Func<Vector3, GameObject[]>>();
                    foreach (Tuple<Func<Vector3, GameObject[]>, int> spawn in weightedWorldSlotSpawns)
                    {
                        weightedWorldSlotSpawnsCacheList.AddRange(Enumerable.Repeat(spawn.Item1, spawn.Item2));
                    }
                    weightedWorldSlotSpawnsCache = weightedWorldSlotSpawnsCacheList.ToArray();
                }
                return weightedWorldSlotSpawnsCache[UnityEngine.Random.Range(0, weightedWorldSlotSpawnsCache.Length)](pos);
            };
        }

        /// <summary>
        /// Adds the weighted spawn event to <see cref="OnSpawnBiomeSlot"/>
        /// </summary>
        protected virtual void AddWeightedTownSpawnEvent()
        {
            weightedTownSlotSpawns = new List<Tuple<Func<Vector3, GameObject[]>, int>>();
            OnSpawnTownSlot += (Chunk c, Vector3 pos) =>
            {
                if (weightedTownSlotSpawnsCache == null)
                {
                    List<Func<Vector3, GameObject[]>> weightedTownSlotSpawnsCacheList = new List<Func<Vector3, GameObject[]>>();
                    foreach (Tuple<Func<Vector3, GameObject[]>, int> spawn in weightedTownSlotSpawns)
                    {
                        weightedTownSlotSpawnsCacheList.AddRange(Enumerable.Repeat(spawn.Item1, spawn.Item2));
                    }
                    weightedTownSlotSpawnsCache = weightedTownSlotSpawnsCacheList.ToArray();
                }
                return weightedTownSlotSpawnsCache[UnityEngine.Random.Range(0, weightedTownSlotSpawnsCache.Length)](pos);
            };
        }

        /// <summary>
        /// Adds a weighted spawn to the world spawn slots.
        /// </summary>
        public virtual void AddWeightedWorldSpawn(EntityInfo entityInfo, int weight)
        {
            if (weightedWorldSlotSpawns == null) AddWeightedBiomeSpawnEvent();
            weightedWorldSlotSpawns.Add(Tuple.Create<Func<Vector3, GameObject[]>, int>((pos) => new GameObject[] { (GameObject)Network.Instantiate((GameObject)Resources.Load(entityInfo.ResourcePath), pos, Quaternion.identity, 0) }, weight));
            weightedWorldSlotSpawnsCache = null;
        }

        /// <summary>
        /// Adds a weighted spawn to the world spawn slots.
        /// </summary>
        public virtual void AddWeightedWorldSpawn(ObjectInfo objectInfo, int weight)
        {
            if (weightedWorldSlotSpawns == null) AddWeightedBiomeSpawnEvent();
            weightedWorldSlotSpawns.Add(Tuple.Create<Func<Vector3, GameObject[]>, int>((pos) => new GameObject[] { (GameObject)Network.Instantiate((GameObject)Resources.Load(objectInfo.ResourcePath), pos, Quaternion.identity, 0) }, weight));
            weightedWorldSlotSpawnsCache = null;
        }

        /// <summary>
        /// Adds a weighted spawn to the world spawn slots.
        /// </summary>
        public virtual void AddWeightedWorldSpawn(string resourcePath, int weight)
        {
            if (weightedWorldSlotSpawns == null) AddWeightedBiomeSpawnEvent();
            weightedWorldSlotSpawns.Add(Tuple.Create<Func<Vector3, GameObject[]>, int>((pos) => new GameObject[] { (GameObject)Network.Instantiate((GameObject)Resources.Load(resourcePath), pos, Quaternion.identity, 0) }, weight));
            weightedWorldSlotSpawnsCache = null;
        }

        /// <summary>
        /// Adds a weighted spawn to the world spawn slots.
        /// </summary>
        public virtual void AddWeightedWorldSpawn(Func<Vector3, GameObject> spawnDelegate, int weight)
        {
            if (weightedWorldSlotSpawns == null) AddWeightedBiomeSpawnEvent();
            weightedWorldSlotSpawns.Add(Tuple.Create<Func<Vector3, GameObject[]>, int>((pos) => new GameObject[] { spawnDelegate(pos) }, weight));
            weightedWorldSlotSpawnsCache = null;
        }

        /// <summary>
        /// Adds a weighted spawn to the world spawn slots.
        /// </summary>
        public virtual void AddWeightedWorldSpawn(Func<Vector3, GameObject[]> spawnDelegate, int weight)
        {
            if (weightedWorldSlotSpawns == null) AddWeightedBiomeSpawnEvent();
            weightedWorldSlotSpawns.Add(Tuple.Create(spawnDelegate, weight));
            weightedWorldSlotSpawnsCache = null;
        }

        /// <summary>
        /// Adds a weighted spawn to the town spawn slots.
        /// </summary>
        public virtual void AddWeightedTownSpawn(string resourcePath, int weight)
        {
            if (weightedTownSlotSpawns == null) AddWeightedTownSpawnEvent();
            weightedTownSlotSpawns.Add(Tuple.Create<Func<Vector3, GameObject[]>, int>((pos) => new GameObject[] { (GameObject)Network.Instantiate((GameObject)Resources.Load(resourcePath), pos, Quaternion.identity, 0) }, weight));
            weightedTownSlotSpawnsCache = null;
        }

        /// <summary>
        /// Adds a weighted spawn to the town spawn slots.
        /// </summary>
        public virtual void AddWeightedTownSpawn(Func<Vector3, GameObject> spawnDelegate, int weight)
        {
            if (weightedTownSlotSpawns == null) AddWeightedTownSpawnEvent();
            weightedTownSlotSpawns.Add(Tuple.Create<Func<Vector3, GameObject[]>, int>((pos) => new GameObject[] { spawnDelegate(pos) }, weight));
            weightedTownSlotSpawnsCache = null;
        }

        /// <summary>
        /// Adds a weighted spawn to the town spawn slots.
        /// </summary>
        public virtual void AddWeightedTownSpawn(Func<Vector3, GameObject[]> spawnDelegate, int weight)
        {
            if (weightedTownSlotSpawns == null) AddWeightedTownSpawnEvent();
            weightedTownSlotSpawns.Add(Tuple.Create(spawnDelegate, weight));
            weightedTownSlotSpawnsCache = null;
        }

        /// <summary>
        /// Returns the Registry Entry's Type enum. Used in the registration process, although it is safe to check this yourself by directly accessing the <see cref="Type"/> property.
        /// </summary>
        public override PlanetType GetEntryType()
        {
            return Type;
        }

        /// <summary>
        /// Returns the singleton of the registry used for storing this type of Registry Entry.
        /// </summary>
        public override Registry<PlanetInfo, PlanetType> GetRegistry()
        {
            return PlanetRegistry.Singleton;
        }

        /// <summary>
        /// Returns whether the specified ID is valid for this Registry Entry's Type.
        /// </summary>
        public override bool IsValidIDForType(int id)
        {
            return id > 0;
        }

        /// <summary>
        /// Returns the next valid ID for this Registry Entry's Type, after the provided lastValidID. Should skip the vanilla ID range.
        /// </summary>
        public override int GetNextIDForType(int lastValidID)
        {
            if (lastValidID < GetRegistry().GetIDStart() - 1) lastValidID = GetRegistry().GetIDStart() - 1;
            return ++lastValidID;
        }

        /// <summary>
        /// This event is invoked when it is time for this planet to generate a world.
        /// </summary>
        public event Action<SpawnerScript, int[]> OnGenerateWorld;
        /// <summary>
        /// This event is invoked when it is time for this planet to generate a town.
        /// </summary>
        public event Action<SpawnerScript, int[]> OnGenerateTown;
        /// <summary>
        /// This event is invoked when it is time for this planet to generate a world chunk. Returns any and all networked objects that are spawned.
        /// </summary>
        public event Func<Chunk, IEnumerable<GameObject>> OnGenerateInside;
        /// <summary>
        /// This event is invoked when it is time for this planet to generate a town chunk. Returns any and all networked objects that are spawned.
        /// </summary>
        public event Func<Chunk, IEnumerable<GameObject>> OnGenerateInsideTown;
        /// <summary>
        /// This event is invoked when it is time for this planet to generate a world chunk spawn slot. Returns any and all networked objects that are spawned.
        /// </summary>
        public event Func<Chunk, Vector3, IEnumerable<GameObject>> OnSpawnBiomeSlot;
        /// <summary>
        /// This event is invoked when it is time for this planet to generate a town chunk spawn slot. Returns any and all networked objects that are spawned.
        /// </summary>
        public event Func<Chunk, Vector3, IEnumerable<GameObject>> OnSpawnTownSlot;

        internal void InvokeOnGenerateWorld(SpawnerScript spawner, int[] s) { OnGenerateWorld?.Invoke(spawner, s); }
        internal void InvokeOnGenerateTown(SpawnerScript spawner, int[] s) { OnGenerateTown?.Invoke(spawner, s); }
        internal IEnumerable<GameObject> InvokeOnGenerateInside(Chunk chunk) { return OnGenerateInside?.GetInvocationList().SelectMany(x => ((Func<Chunk, IEnumerable<GameObject>>)x)(chunk)); }
        internal IEnumerable<GameObject> InvokeOnGenerateInsideTown(Chunk chunk) { return OnGenerateInsideTown?.GetInvocationList().SelectMany(x => ((Func<Chunk, IEnumerable<GameObject>>)x)(chunk)); }
        internal IEnumerable<GameObject> InvokeOnSpawnBiomeSlot(Chunk chunk, Vector3 pos) { return OnSpawnBiomeSlot?.GetInvocationList().Cast<Func<Chunk, Vector3, IEnumerable<GameObject>>>().SelectMany(x => x(chunk, pos)); }
        internal IEnumerable<GameObject> InvokeOnSpawnTownSlot(Chunk chunk, Vector3 pos) { return OnSpawnTownSlot?.GetInvocationList().Cast<Func<Chunk, Vector3, IEnumerable<GameObject>>>().SelectMany(x => x(chunk, pos)); }
    }
}

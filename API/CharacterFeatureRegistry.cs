using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GadgetCore.API
{
    /// <summary>
    /// This registry is an abstract base class for registries that add custom character features, such as races, uniforms, and augments.
    /// </summary>
    public abstract class CharacterFeatureRegistry<R, E> : Registry<R, E, CharacterFeatureType>, ICharacterFeatureRegistry where R : CharacterFeatureRegistry<R, E>, new() where E : CharacterFeatureRegistryEntry<E>
    {
        /// <summary>
        /// Represents the highest-numbered page for this feature on the character creation screen - this is generally auto-calculated based on the IDs of the registered entries.
        /// </summary>
        public static int MaxPage { get; protected set; } = 1;

        /// <summary>
        /// Represents the number of rows on each page for this feature on the character creation screen. By the default implementation of <see cref="CharacterFeatureRegistry{R, E}"/>, this must be 4.
        /// </summary>
        public static int PageRows { get; protected set; } = 4;

        /// <summary>
        /// Represents the number of rows on each page for this feature on the character creation screen. By the default implementation of <see cref="CharacterFeatureRegistry{R, E}"/>, this must be 12.
        /// </summary>
        public static int PageColumns { get; protected set; } = 12;

        /// <summary>
        /// Represents the number of entries on each page for this feature on the character creation screen. By the default implementation of <see cref="CharacterFeatureRegistry{R, E}"/>, this must be 48.
        /// </summary>
        public static int PageSize => PageRows * PageColumns;

        /// <summary>
        /// The current page for this feature on the character creation screen. Setting this value will update the character feature selector.
        /// </summary>
        public static int CurrentPage
        {
            get
            {
                return m_CurrentPage;
            }
            set
            {
                m_CurrentPage = value;
                if (InstanceTracker.Menuu != null)
                {
                    CharacterFeatureRegistry<R, E> Singleton = (CharacterFeatureRegistry<R, E>)CharacterFeatureRegistry<R, E>.Singleton;
                    bool isVanilla = Singleton.GetFeatureType() < CharacterFeatureType.OTHER;
                    if (m_CurrentPage == 1 && isVanilla)
                    {
                        switch (Singleton.GetFeatureType())
                        {
                            case CharacterFeatureType.RACE:
                                InstanceTracker.Menuu.menuStuff.GetComponent<Renderer>().material = (Material)Resources.Load("mat/mRace");
                                break;
                            case CharacterFeatureType.UNIFORM:
                                InstanceTracker.Menuu.menuStuff.GetComponent<Renderer>().material = (Material)Resources.Load("mat/mUniform");
                                break;
                            case CharacterFeatureType.AUGMENT:
                                InstanceTracker.Menuu.menuStuff.GetComponent<Renderer>().material = (Material)Resources.Load("mat/mUpgrade");
                                break;
                        }
                    }
                    else
                    {
                        InstanceTracker.Menuu.menuStuff.GetComponent<Renderer>().material = (Material)Resources.Load("mat/m" + Singleton.GetRegistryName() + "Back");
                    }
                    Material backgroundMaterial = (Material)Resources.Load("mat/m" + Singleton.GetRegistryName() + "Slot");
                    GameObject[] box = InstanceTracker.Menuu.box;
                    for (int i = 0; i < box.Length; i++)
                    {
                        box[i].GetComponent<Renderer>().enabled = !Singleton.IsFeatureUnlocked((m_CurrentPage - 1) * PageSize + i);
                        SceneInjector.CharacterFeatureSelectIcons[i].transform.GetChild(0).GetComponent<Renderer>().material = backgroundMaterial;
                        if (Singleton.TryGetEntry((m_CurrentPage - 1) * PageSize + i, out E entry) && entry.SelectorIconMat != null)
                        {
                            box[i].SetActive(true);
                            SceneInjector.CharacterFeatureSelectIcons[i].GetComponent<Renderer>().material = entry.SelectorIconMat;
                            SceneInjector.CharacterFeatureSelectIcons[i].SetActive(true);
                        }
                        else
                        {
                            box[i].SetActive(isVanilla && m_CurrentPage == 1 && (Singleton.GetFeatureType() == CharacterFeatureType.RACE ? i < 36 : i < 24));
                            SceneInjector.CharacterFeatureSelectIcons[i].SetActive(false);
                        }
                    }
                    InstanceTracker.Menuu.stuffChosen.SetActive((Singleton.GetSelectedFeature() / PageSize) + 1 == m_CurrentPage);
                    SceneInjector.CharacterFeatureSelectPageBack.gameObject.SetActive(m_CurrentPage > 1);
                    SceneInjector.CharacterFeatureSelectPageForward.gameObject.SetActive(m_CurrentPage < MaxPage);
                }
            }
        }
        private static int m_CurrentPage = 1;

        private static int m_SelectedFeature = 0;

        /// <summary>
        /// Represents currently unlocked features. May be null.
        /// </summary>
        protected static Dictionary<int, int> unlockedFeatures;

        /// <summary>
        /// Gets the type of this registry. Must be constant.
        /// </summary>
        public abstract CharacterFeatureType GetFeatureType();

        private readonly int SelectorID;

        /// <summary>
        /// Constructs a CharacterFeatureRegistry. Registers the <see cref="OnSceneLoaded"/> handler.
        /// </summary>
        public CharacterFeatureRegistry()
        {
            SelectorID = PatchMethods.characterFeatureRegistries.Count;
            PatchMethods.characterFeatureRegistries[SelectorID] = this;
            PatchMethods.OnLevelUp += OnLevelUp;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        /// <summary>
        /// Finalizes this CharacterFeatureRegistry. Unregisters the <see cref="OnSceneLoaded"/> handler.
        /// </summary>
        ~CharacterFeatureRegistry()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            PatchMethods.OnLevelUp -= OnLevelUp;
        }

        /// <summary>
        /// Returns the ID of this feature for use in Menuu.stuffSelecting
        /// </summary>
        public int GetSelectorID()
        {
            return SelectorID;
        }

        /// <summary>
        /// Gets the page size for this feature on the character creation screen.
        /// </summary>
        public int GetPageSize()
        {
            return PageSize;
        }

        /// <summary>
        /// Gets the current page for this feature on the character creation screen.
        /// </summary>
        public int GetCurrentPage()
        {
            return CurrentPage;
        }

        /// <summary>
        /// Gets the highest-numbered page for this feature on the character creation screen.
        /// </summary>
        public int GetMaxPage()
        {
            return MaxPage;
        }

        /// <summary>
        /// Sets the current page for this feature on the character creation screen. Calling this will update the character feature selector.
        /// </summary>
        public void SetCurrentPage(int page)
        {
            if (page < 1) page = 1;
            else if (page > MaxPage) page = MaxPage;
            CurrentPage = page;
        }

        /// <summary>
        /// Triggers this registry's entries' OnLevelUp events. You should never call this yourself.
        /// </summary>
        protected virtual void OnLevelUp(int level)
        {
            foreach (CharacterFeatureRegistryEntry<E> entry in GetAllEntries())
            {
                entry.InvokeOnLevelUp(level);
            }
        }

        /// <summary>
        /// Sets up the unlocked features tracking. Do not call this yourself.
        /// </summary>
        protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.buildIndex == 0) unlockedFeatures = null;
        }

        /// <summary>
        /// Called when a feature within this character feature registry is selected during character creation. Generally, you should not call this yourself.
        /// </summary>
        public virtual void SelectFeature(int feature)
        {
            m_SelectedFeature = feature;
        }

        /// <summary>
        /// Gets the feature currently selected for this feature during character creation.
        /// </summary>
        public virtual int GetSelectedFeature()
        {
            return m_SelectedFeature;
        }


        /// <summary>
        /// Gets an entry as an <see cref="ICharacterFeatureRegistryEntry"/>
        /// </summary>
        public ICharacterFeatureRegistryEntry GetEntryInterface(int ID)
        {
            return (ICharacterFeatureRegistryEntry)GetEntry(ID);
        }

        /// <summary>
        /// Tries to get an entry as an <see cref="ICharacterFeatureRegistryEntry"/>
        /// </summary>
        public bool TryGetEntryInterface(int ID, out ICharacterFeatureRegistryEntry entry)
        {
            bool success = TryGetEntry(ID, out E genericEntry);
            entry = genericEntry;
            return success;
        }

        /// <summary>
        /// Returns the unlock level of the specified feature.
        /// </summary>
        public virtual int GetFeatureUnlockLevel(int ID)
        {
            if (unlockedFeatures == null)
            {
                unlockedFeatures = new Dictionary<int, int>();
                string keyName = "u" + (GetFeatureType() < CharacterFeatureType.OTHER ? char.ToString(GetRegistryName()[0]) : GetRegistryName());
                if (GetFeatureType() < CharacterFeatureType.OTHER)
                {
                    for (int i = 0; i < GetIDStart(); i++)
                    {
                        unlockedFeatures[i] = PreviewLabs.PlayerPrefs.GetInt(keyName + i, i == 0 ? 1 : 0);
                    }
                }
                foreach (CharacterFeatureRegistryEntry<E> entry in GetAllEntries())
                {
                    int entryID = entry.GetID();
                    unlockedFeatures[entryID] = PreviewLabs.PlayerPrefs.GetInt(keyName + entryID);
                }
            }
            return unlockedFeatures.TryGetValue(ID, out int unlockLevel) ? unlockLevel : 0;
        }

        /// <summary>
        /// Returns whether the character feature with the specified ID is unlocked.
        /// </summary>
        public virtual bool IsFeatureUnlocked(int ID)
        {
            return GetFeatureUnlockLevel(ID) > 0;
        }

        /// <summary>
        /// Sets the unlock level of the specified feature. Setting it to 0 locks the feature.
        /// </summary>
        public virtual void SetFeatureUnlocked(int ID, int unlockLevel = 1)
        {
            string keyName = "u" + (GetFeatureType() < CharacterFeatureType.OTHER ? char.ToString(GetRegistryName()[0]) : GetRegistryName());
            PreviewLabs.PlayerPrefs.SetInt(keyName + ID, unlockLevel);
            if (unlockedFeatures != null) unlockedFeatures[ID] = unlockLevel;
        }

        /// <summary>
        /// Called after the specified Registry Entry has been registered. You should never call this yourself. Note that this is called before <see cref="RegistryEntry{E, T}.PostRegister"/>
        /// </summary>
        protected override void PostRegistration(E entry)
        {
            if (entry.SelectorIconMat != null || entry.SelectorIconTex != null)
            {
                int entryPage = entry.GetID() / PageSize + 1;
                if (entryPage > MaxPage) MaxPage = entryPage;
            }
        }

        /// <summary>
        /// Gets the ID that modded IDs should start at for this registry. Always returns 48 for default <see cref="CharacterFeatureType"/>s, and 0 for <see cref="CharacterFeatureType.OTHER"/>.
        /// </summary>
        public override int GetIDStart()
        {
            return GetFeatureType() < CharacterFeatureType.OTHER ? 48 : 0;
        }
    }

    /// <summary>
    /// CharacterFeatureRegistry interface without the self-referencing supertype.
    /// </summary>
    public interface ICharacterFeatureRegistry
    {
        /// <summary>
        /// Gets the SelectorID of this registry. Must be constant.
        /// </summary>
        int GetSelectorID();
        /// <summary>
        /// Gets the type of this registry. Must be constant.
        /// </summary>
        CharacterFeatureType GetFeatureType();
        /// <summary>
        /// Gets the page size for this feature on the character creation screen.
        /// </summary>
        int GetPageSize();
        /// <summary>
        /// Gets the current page for this feature on the character creation screen.
        /// </summary>
        int GetCurrentPage();
        /// <summary>
        /// Gets the highest-numbered page for this feature on the character creation screen.
        /// </summary>
        int GetMaxPage();
        /// <summary>
        /// Sets the current page for this feature on the character creation screen. Calling this will update the character feature selector.
        /// </summary>
        void SetCurrentPage(int page);
        /// <summary>
        /// Returns the unlock level of the specified feature.
        /// </summary>
        int GetFeatureUnlockLevel(int ID);
        /// <summary>
        /// Returns whether the character feature with the specified ID is unlocked.
        /// </summary>
        bool IsFeatureUnlocked(int ID);
        /// <summary>
        /// Sets the unlock level of the specified feature. Setting it to 0 locks the feature.
        /// </summary>
        void SetFeatureUnlocked(int ID, int unlockLevel = 1);
        /// <summary>
        /// Called when a feature within this character feature registry is selected during character creation. Generally, you should not call this yourself.
        /// </summary>
        void SelectFeature(int feature);
        /// <summary>
        /// Gets the feature currently selected for this feature during character creation.
        /// </summary>
        int GetSelectedFeature();
        /// <summary>
        /// Gets an entry as an <see cref="ICharacterFeatureRegistryEntry"/>
        /// </summary>
        ICharacterFeatureRegistryEntry GetEntryInterface(int ID);
        /// <summary>
        /// Tries to get an entry as an <see cref="ICharacterFeatureRegistryEntry"/>
        /// </summary>
        bool TryGetEntryInterface(int ID, out ICharacterFeatureRegistryEntry entry);
    }

    /// <summary>
    /// Specifies what type of character feature this registry is for.
    /// </summary>
    public enum CharacterFeatureType
    {
        /// <summary>
        /// This registry is for the character's Race.
        /// </summary>
        RACE,
        /// <summary>
        /// This registry is for the character's Uniform.
        /// </summary>
        UNIFORM,
        /// <summary>
        /// This registry is for the character's Augment.
        /// </summary>
        AUGMENT,
        /// <summary>
        /// This registry is for some form of custom character feature other than the default ones.
        /// </summary>
        OTHER
    }
}

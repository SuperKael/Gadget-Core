using GadgetCore.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// This registry is filled with MenuInfos, and is used for registering custom menus to the game.
    /// </summary>
    public class PlanetRegistry : Registry<PlanetRegistry, PlanetInfo, PlanetType>
    {
        private static readonly FieldInfo portalUses = typeof(GameScript).GetField("portalUses", BindingFlags.NonPublic | BindingFlags.Instance);

        private static Dictionary<string, int> planetIDsByName;
        private static Dictionary<string, int> planetIDsByRegistryName;

        /// <summary>
        /// The number of pages that the planet selector has thanks to added planets.
        /// </summary>
        public static int PlanetSelectorPages
        {
            get
            {
                return ((Singleton.AsEnumerable().Count(x => x.SelectorButtonMat != null) + 13) / 14) + 1;
            }
        }

        /// <summary>
        /// The current page of the planet selector.
        /// </summary>
        public static int PlanetSelectorPage { get; internal set; }

        internal static int oldPage;

        internal static PlanetInfo[] selectorPlanets;
        internal static GameObject[][] planetButtonIcons;
        internal static TextMesh planetPageText;

        internal static List<Tuple<int, int, int>>[] VanillaWeightedExitPortalIDs = new List<Tuple<int, int, int>>[]
        {
            new List<Tuple<int, int, int>>(new Tuple<int, int, int>[] { Tuple.Create(0, 50, -1), Tuple.Create(1, 25, -1), Tuple.Create(2, 25, -1) }),
            new List<Tuple<int, int, int>>(new Tuple<int, int, int>[] { Tuple.Create(1, 50, -1), Tuple.Create(2, 25, -1), Tuple.Create(0, 25, -1) }),
            new List<Tuple<int, int, int>>(new Tuple<int, int, int>[] { Tuple.Create(2, 50, -1), Tuple.Create(3, 25, -1), Tuple.Create(4, 25, -1) }),
            new List<Tuple<int, int, int>>(new Tuple<int, int, int>[] { Tuple.Create(3, 30, -1), Tuple.Create(1, 10, -1), Tuple.Create(2, 20, -1), Tuple.Create(4, 40, -1) }),
            new List<Tuple<int, int, int>>(new Tuple<int, int, int>[] { Tuple.Create(4, 50, -1), Tuple.Create(5, 25, -1), Tuple.Create(6, 25, -1) }),
            new List<Tuple<int, int, int>>(new Tuple<int, int, int>[] { Tuple.Create(5, 50, -1), Tuple.Create(6, 25, -1), Tuple.Create(7, 25, -1) }),
            new List<Tuple<int, int, int>>(new Tuple<int, int, int>[] { Tuple.Create(6, 50, -1), Tuple.Create(7, 25, -1), Tuple.Create(5, 25, -1) }),
            new List<Tuple<int, int, int>>(new Tuple<int, int, int>[] { Tuple.Create(10, 45, -1), Tuple.Create(6, 30, -1), Tuple.Create(9, 25, -1) }),
            new List<Tuple<int, int, int>>(new Tuple<int, int, int>[] { Tuple.Create(8, 100, -1) }),
            new List<Tuple<int, int, int>>(new Tuple<int, int, int>[] { Tuple.Create(9, 50, -1), Tuple.Create(7, 25, -1), Tuple.Create(10, 25, -1) }),
            new List<Tuple<int, int, int>>(new Tuple<int, int, int>[] { Tuple.Create(10, 50, -1), Tuple.Create(9, 25, -1), Tuple.Create(7, 25, -1) }),
            new List<Tuple<int, int, int>>(new Tuple<int, int, int>[] { Tuple.Create(11, 100, -1) }),
            new List<Tuple<int, int, int>>(new Tuple<int, int, int>[] { Tuple.Create(12, 100, -1) }),
            new List<Tuple<int, int, int>>(new Tuple<int, int, int>[] { Tuple.Create(13, 100, -1) })
        };

        internal static void UnregisterGadget(int gadgetID)
        {
            for (int i = 0; i < VanillaWeightedExitPortalIDs.Length; i++)
            {
                VanillaWeightedExitPortalIDs[i].RemoveAll(x => x.Item3 == gadgetID);
            }
        }

        /// <summary>
        /// Sets the weight for a planet of ID <paramref name="portalID"/> to appear in an exit portal on the vanilla planet of ID <paramref name="planetID"/>
        /// </summary>
        public static void SetVanillaExitPortalWeight(int planetID, int portalID, int weight)
        {
            if (!registeringVanilla && gadgetRegistering < 0) throw new InvalidOperationException("Data registration may only be performed by the Initialize method of a Gadget!");
            if (planetID < 0 || planetID > VanillaWeightedExitPortalIDs.Length) throw new ArgumentOutOfRangeException("planetID");
            int index = VanillaWeightedExitPortalIDs[planetID].IndexOf(x => x.Item3 == Registry.gadgetRegistering);
            if (index != -1)
            {
                VanillaWeightedExitPortalIDs[planetID][index] = Tuple.Create(portalID, weight, gadgetRegistering);
            }
            else
            {
                VanillaWeightedExitPortalIDs[planetID].Add(Tuple.Create(portalID, weight, gadgetRegistering));
            }
        }

        /// <summary>
        /// Gets the weights of the possible exit portal plenets for the given vanilla planet ID
        /// </summary>
        public static Tuple<int, int, int>[] GetVanillaExitPortalWeights(int planetID)
        {
            HashSet<int> IDs = new HashSet<int>();
            List<Tuple<int, int, int>> portals = new List<Tuple<int, int, int>>();
            for (int i = VanillaWeightedExitPortalIDs[planetID].Count - 1; i >= 0; i--)
            {
                int portalID = VanillaWeightedExitPortalIDs[planetID][i].Item1;
                if (!IDs.Contains(portalID))
                {
                    IDs.Add(portalID);
                    portals.Add(VanillaWeightedExitPortalIDs[planetID][i]);
                }
            }
            return portals.ToArray();
        }

        /// <summary>
        /// The name of this registry.
        /// </summary>
        public const string REGISTRY_NAME = "Planet";

        static PlanetRegistry()
        {
            InitializeVanillaPlanetIDNames();
        }

        /// <summary>
        /// Gets the name of this registry. Must be constant. Returns <see cref="REGISTRY_NAME"/>.
        /// </summary>
        public override string GetRegistryName()
        {
            return REGISTRY_NAME;
        }

        /// <summary>
        /// Gets the planet ID for the given name. Case-insensitive. Returns -1 if there is no planet with the given name.
        /// </summary>
        public static int GetPlanetIDByName(string name)
        {
            name = name.ToLowerInvariant();
            if (planetIDsByName.ContainsKey(name))
            {
                return planetIDsByName[name];
            }
            return -1;
        }

        /// <summary>
        /// Gets the planet ID for the given registry name. Case-insensitive. Returns -1 if there is no planet with the given name.
        /// </summary>
        public static int GetPlanetIDByRegistryName(string name)
        {
            name = name.ToLowerInvariant();
            if (planetIDsByRegistryName.ContainsKey(name))
            {
                return planetIDsByRegistryName[name];
            }
            return -1;
        }

        /// <summary>
        /// Called after the specified Registry Entry has been registered. You should never call this yourself. Note that this is called before <see cref="RegistryEntry{E, T}.PostRegister"/>
        /// </summary>
        protected override void PostRegistration(PlanetInfo entry)
        {
            planetIDsByName[entry.Name] = entry.ID;
            planetIDsByRegistryName[entry.RegistryName] = entry.ID;
        }

        /// <summary>
        /// Called just before an entry is removed from the registry by <see cref="Registry.UnregisterGadget(GadgetInfo)"/>
        /// </summary>
        protected override void OnUnregister(PlanetInfo entry)
        {
            planetIDsByName.Remove(entry.Name);
            planetIDsByRegistryName.Remove(entry.RegistryName);
        }

        /// <summary>
        /// Gets the ID that modded IDs should start at for this registry. <see cref="MenuRegistry"/> always returns 10000.
        /// </summary>
        public override int GetIDStart()
        {
            return 100;
        }

        internal static void UpdatePlanetSelector()
        {
            int totalPages = (planetButtonIcons?.Length ?? 0) + 1;
            if (planetPageText != null)
            {
                foreach (TextMesh text in planetPageText.GetComponentsInChildren<TextMesh>())
                {
                    text.text = "Page " + PlanetSelectorPage + "/" + totalPages;
                }
            }
            if (totalPages > 1)
            {
                if (oldPage < 1) oldPage = PlanetSelectorPage;
                if (oldPage > 1)
                {
                    if (oldPage > totalPages) oldPage = totalPages;
                    foreach (GameObject buttonIcon in planetButtonIcons[oldPage - 2])
                    {
                        buttonIcon.SetActive(false);
                    }
                }
                if (PlanetSelectorPage > 1)
                {
                    if (PlanetSelectorPage > totalPages) PlanetSelectorPage = totalPages;
                    int planetIndex = (PlanetSelectorPage - 2) * 14 + GameScript.curPlanet;
                    PlanetInfo planet = planetIndex >= 0 && planetIndex < selectorPlanets.Length ? selectorPlanets[planetIndex] : null;
                    if (planet == null || planet.PortalUses == 0)
                    {
                        GameScript.curPlanet = selectorPlanets.Select((x, i) => Tuple.Create(x, i)).FirstOrDefault(x => x.Item2 >= (PlanetSelectorPage - 2) * 14 && x.Item2 < (PlanetSelectorPage - 1) * 14 && x.Item1 != null && x.Item1.PortalUses != 0)?.Item2 ?? 0;
                        planetIndex = (PlanetSelectorPage - 2) * 14 + GameScript.curPlanet;
                        planet = planetIndex >= 0 && planetIndex < selectorPlanets.Length ? selectorPlanets[planetIndex] : null;
                    }
                    if (planet != null && planet.PortalUses != 0)
                    {
                        InstanceTracker.GameScript.planetObj.GetComponent<Renderer>().material = (Material)Resources.Load("mat/planet" + planet.ID);
                        if (oldPage != PlanetSelectorPage) InstanceTracker.GameScript.planetObj.GetComponent<Animation>().Play();
                        InstanceTracker.GameScript.txtPlanetName[0].text = planet.Name;
                        InstanceTracker.GameScript.txtPlanetName[1].text = InstanceTracker.GameScript.txtPlanetName[0].text;
                        InstanceTracker.GameScript.txtHostile[0].text = "Hostile Lv." + InstanceTracker.GameScript.InvokeMethod("GetPlanetHostile", planet.ID);
                        InstanceTracker.GameScript.txtHostile[1].text = InstanceTracker.GameScript.txtHostile[0].text;
                        InstanceTracker.GameScript.planetSelector.transform.position = new Vector3(InstanceTracker.GameScript.planetGuard[GameScript.curPlanet].transform.position.x, InstanceTracker.GameScript.planetGuard[GameScript.curPlanet].transform.position.y, InstanceTracker.GameScript.planetSelector.transform.position.z);
                    }
                    else
                    {
                        InstanceTracker.GameScript.planetObj.GetComponent<Renderer>().material = (Material)Resources.Load("mat/trans");
                        InstanceTracker.GameScript.txtPlanetName[0].text = string.Empty;
                        InstanceTracker.GameScript.txtPlanetName[1].text = InstanceTracker.GameScript.txtPlanetName[0].text;
                        InstanceTracker.GameScript.txtHostile[0].text = string.Empty;
                        InstanceTracker.GameScript.txtHostile[1].text = InstanceTracker.GameScript.txtHostile[0].text;
                        InstanceTracker.GameScript.planetSelector.transform.position = new Vector3(999f, 999f, -999f);
                    }
                    if (planet != null && planet.PortalUses > 0)
                    {
                        InstanceTracker.GameScript.txtPortalUses[0].text = "Portal Uses: " + planet.PortalUses;
                        InstanceTracker.GameScript.txtPortalUses[1].text = InstanceTracker.GameScript.txtPortalUses[0].text;
                    }
                    else if (planet != null && planet.PortalUses == -1)
                    {
                        InstanceTracker.GameScript.txtPortalUses[0].text = "Portal Uses: Infinite";
                        InstanceTracker.GameScript.txtPortalUses[1].text = InstanceTracker.GameScript.txtPortalUses[0].text;
                    }
                    else
                    {
                        InstanceTracker.GameScript.txtPortalUses[0].text = string.Empty;
                        InstanceTracker.GameScript.txtPortalUses[1].text = InstanceTracker.GameScript.txtPortalUses[0].text;
                    }
                    for (int i = 0; i < planetButtonIcons[PlanetSelectorPage - 2].Length; i++)
                    {
                        GameObject buttonIcon = planetButtonIcons[PlanetSelectorPage - 2][i];
                        buttonIcon.SetActive(true);
                        planetIndex = (PlanetSelectorPage - 2) * 14 + i;
                        planet = planetIndex >= 0 && planetIndex < selectorPlanets.Length ? selectorPlanets[planetIndex] : null;
                        if (planet != null)
                        {
                            if (planet.Relics > 99)
                            {
                                planet.Relics = 99;
                                planet.PortalUses = -1;
                            }
                            InstanceTracker.GameScript.planetGuard[i].GetComponent<MeshRenderer>().enabled = true;
                            if (planet.PortalUses > 0)
                            {
                                InstanceTracker.GameScript.planetGuard[i].GetComponent<BoxCollider>().enabled = true;
                                InstanceTracker.GameScript.planetGuard[i].SendMessage("PlanetYes");
                                InstanceTracker.GameScript.planetGuard[i].GetComponent<Renderer>().material = InstanceTracker.GameScript.planetMat;
                            }
                            else if (planet.PortalUses == -1)
                            {
                                InstanceTracker.GameScript.planetGuard[i].GetComponent<BoxCollider>().enabled = true;
                                InstanceTracker.GameScript.planetGuard[i].SendMessage("PlanetYes");
                                InstanceTracker.GameScript.planetGuard[i].GetComponent<Renderer>().material = InstanceTracker.GameScript.planetMat;
                            }
                            else
                            {
                                InstanceTracker.GameScript.planetGuard[i].GetComponent<BoxCollider>().enabled = false;
                                InstanceTracker.GameScript.planetGuard[i].SendMessage("PlanetNo");
                                InstanceTracker.GameScript.planetGuard[i].GetComponent<Renderer>().material = InstanceTracker.GameScript.planetGuardMat;
                            }
                        }
                        else
                        {
                            InstanceTracker.GameScript.planetGuard[i].GetComponent<MeshRenderer>().enabled = false;
                            InstanceTracker.GameScript.planetGuard[i].GetComponent<BoxCollider>().enabled = false;
                        }
                    }
                }
                else if (PlanetSelectorPage == 1)
                {
                    int[] portalUses = PlanetRegistry.portalUses.GetValue<int[]>(InstanceTracker.GameScript);
                    if (portalUses[GameScript.curPlanet] == 0)
                    {
                        GameScript.curPlanet = 0;
                    }
                    if (portalUses[GameScript.curPlanet] != 0)
                    {
                        InstanceTracker.GameScript.planetObj.GetComponent<Renderer>().material = (Material)Resources.Load("mat/planet" + GameScript.curPlanet);
                        if (oldPage != PlanetSelectorPage) InstanceTracker.GameScript.planetObj.GetComponent<Animation>().Play();
                        InstanceTracker.GameScript.txtPlanetName[0].text = string.Empty + InstanceTracker.GameScript.InvokeMethod("GetPlanetName", GameScript.curPlanet);
                        InstanceTracker.GameScript.txtPlanetName[1].text = InstanceTracker.GameScript.txtPlanetName[0].text;
                        InstanceTracker.GameScript.txtHostile[0].text = "Hostile Lv." + InstanceTracker.GameScript.InvokeMethod("GetPlanetHostile", GameScript.curPlanet);
                        InstanceTracker.GameScript.txtHostile[1].text = InstanceTracker.GameScript.txtHostile[0].text;
                        InstanceTracker.GameScript.planetSelector.transform.position = new Vector3(InstanceTracker.GameScript.planetGuard[GameScript.curPlanet].transform.position.x, InstanceTracker.GameScript.planetGuard[GameScript.curPlanet].transform.position.y, InstanceTracker.GameScript.planetSelector.transform.position.z);
                    }
                    else
                    {
                        InstanceTracker.GameScript.planetObj.GetComponent<Renderer>().material = (Material)Resources.Load("mat/trans");
                        InstanceTracker.GameScript.txtPlanetName[0].text = string.Empty;
                        InstanceTracker.GameScript.txtPlanetName[1].text = InstanceTracker.GameScript.txtPlanetName[0].text;
                        InstanceTracker.GameScript.txtHostile[0].text = string.Empty;
                        InstanceTracker.GameScript.txtHostile[1].text = InstanceTracker.GameScript.txtHostile[0].text;
                        InstanceTracker.GameScript.planetSelector.transform.position = new Vector3(999f, 999f, -999f);
                    }
                    for (int i = 0; i < 20; i++)
                    {
                        InstanceTracker.GameScript.planetGuard[i].GetComponent<MeshRenderer>().enabled = i < 14;
                        InstanceTracker.GameScript.planetGuard[i].GetComponent<BoxCollider>().enabled = true;
                        if (GameScript.planetRelics[i] > 99)
                        {
                            GameScript.planetRelics[i] = 99;
                            portalUses[i] = -1;
                        }
                        if (portalUses[i] > 0)
                        {
                            InstanceTracker.GameScript.planetGuard[i].SendMessage("PlanetYes");
                            InstanceTracker.GameScript.planetGuard[i].GetComponent<Renderer>().material = InstanceTracker.GameScript.planetMat;
                        }
                        else if (portalUses[i] == -1)
                        {
                            InstanceTracker.GameScript.planetGuard[i].SendMessage("PlanetYes");
                            InstanceTracker.GameScript.planetGuard[i].GetComponent<Renderer>().material = InstanceTracker.GameScript.planetMat;
                        }
                        else
                        {
                            InstanceTracker.GameScript.planetGuard[i].SendMessage("PlanetNo");
                            InstanceTracker.GameScript.planetGuard[i].GetComponent<Renderer>().material = InstanceTracker.GameScript.planetGuardMat;
                        }
                    }
                    if (portalUses[GameScript.curPlanet] > 0)
                    {
                        InstanceTracker.GameScript.txtPortalUses[0].text = "Portal Uses: " + portalUses[GameScript.curPlanet];
                        InstanceTracker.GameScript.txtPortalUses[1].text = InstanceTracker.GameScript.txtPortalUses[0].text;
                    }
                    else if (portalUses[GameScript.curPlanet] == -1)
                    {
                        InstanceTracker.GameScript.txtPortalUses[0].text = "Portal Uses: Infinite";
                        InstanceTracker.GameScript.txtPortalUses[1].text = InstanceTracker.GameScript.txtPortalUses[0].text;
                    }
                    else
                    {
                        InstanceTracker.GameScript.txtPortalUses[0].text = string.Empty;
                        InstanceTracker.GameScript.txtPortalUses[1].text = InstanceTracker.GameScript.txtPortalUses[0].text;
                    }
                }
                oldPage = PlanetSelectorPage;
            }
        }

        private static void InitializeVanillaPlanetIDNames()
        {
            planetIDsByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["Desolate Canyon"] = 0,
                ["Deep Jungle"] = 1,
                ["Hollow Caverns"] = 2,
                ["Shroomtown"] = 3,
                ["Ancient Ruins"] = 4,
                ["Plaguelands"] = 5,
                ["The Byfrost"] = 6,
                ["Molten Crag"] = 7,
                ["Mech City"] = 8,
                ["Demon's Rift"] = 9,
                ["The Whisperwood"] = 10,
                ["Old Earth"] = 11,
                ["Forbidden Arena"] = 12,
                ["The Cathedral"] = 13
            };
            planetIDsByRegistryName = new Dictionary<string, int>(planetIDsByName.Comparer);
            foreach (KeyValuePair<string, int> item in planetIDsByName)
            {
                planetIDsByRegistryName["Roguelands:" + item.Key] = item.Value;
            }
        }
    }

    /// <summary>
    /// Specifies what type of planet this is.
    /// </summary>
    public enum PlanetType
    {
        /// <summary>
        /// A standard planet with alternating worlds and towns.
        /// </summary>
        NORMAL,
        /// <summary>
        /// A planet consisting only of towns, like Old Earth.
        /// </summary>
        TOWNS,
        /// <summary>
        /// A planet with only a single town, like Mech City.
        /// </summary>
        SINGLE,
        /// <summary>
        /// Some special sort of planet with unique world generation.
        /// </summary>
        SPECIAL
    }
}

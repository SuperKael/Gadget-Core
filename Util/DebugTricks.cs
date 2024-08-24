using GadgetCore.API;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GadgetCore.Util
{
    /// <summary>
    /// Contains utility methods intended for debugging, perhaps through the /reflect command
    /// </summary>
    public static class DebugTricks
    {
        /// <summary>
        /// Lists all NetworkViews, their IDs, and their names in the console.
        /// </summary>
        public static void ListNetworkViews()
        {
            foreach (NetworkView view in UnityEngine.Object.FindObjectsOfType<NetworkView>().OrderBy(x => x.viewID.ToString()))
            {
                GadgetConsole.Print($"NetworkView ID: ({view.viewID}), Name: ({view.name})", "ListNetworkViews");
            }
        }

        /// <summary>
        /// Spawns a random modded item into the world using <see cref="GadgetCoreAPI.SpawnItem"/>
        /// </summary>
        public static void SpawnRandomItem()
        {
            List<ItemInfo> allItems = ItemRegistry.Singleton.ToList();
            GadgetCoreAPI.SpawnItem(InstanceTracker.PlayerScript.transform.position, new Item(allItems[UnityEngine.Random.Range(0, allItems.Count)].GetID(), 1, 0, GadgetCoreAPI.GetRandomCraftTier(), 0, new int[3], new int[3]));
        }
    }
}

using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(PlayerScript))]
    [HarmonyPatch("SetName")]
    static class Patch_PlayerScript_SetName
    {
        [HarmonyPrefix]
        public static void Prefix(PlayerScript __instance, ref string n)
        {
            PlayerScript existingPlayer = null;
            if (n == Menuu.curName)
            {
                existingPlayer = GadgetCoreAPI.GetPlayerByName(n);
                if (existingPlayer != null && existingPlayer != __instance)
                {
                    existingPlayer.GetComponent<NetworkView>().RPC("SetName", RPCMode.AllBuffered, new object[]
                    {
                    n + "-1"
                    });
                    foreach (KeyValuePair<string, PlayerScript> entry in GadgetCoreAPI.playersByName.Where(x => x.Value == existingPlayer).ToList())
                    {
                        GadgetCoreAPI.playersByName.Remove(entry.Key);
                        GadgetNetwork.NetworkPlayersByName.Remove(entry.Key);
                        GadgetNetwork.NamesByNetworkPlayer.Remove(entry.Value.GetComponent<NetworkView>().owner);
                    }
                    if (InstanceTracker.PlayerScript == __instance) GadgetCoreAPI.playerName = n + "-1";
                    GadgetCoreAPI.playersByName[n + "-1"] = existingPlayer;
                    GadgetNetwork.NetworkPlayersByName[n + "-1"] = existingPlayer.GetComponent<NetworkView>().owner;
                    GadgetNetwork.NamesByNetworkPlayer[existingPlayer.GetComponent<NetworkView>().owner] = n + "-1";
                }
            }
            else
            {
                bool wasOp = false;
                foreach (KeyValuePair<string, PlayerScript> entry in GadgetCoreAPI.playersByName.Where(x => x.Value == __instance).ToList())
                {
                    if (GadgetConsole.operators.Contains(entry.Key))
                    {
                        GadgetConsole.operators.Remove(entry.Key);
                        wasOp = true;
                    }
                }
                if (wasOp) GadgetConsole.operators.Add(n);
            }
            if (existingPlayer == null) existingPlayer = GadgetCoreAPI.GetPlayerByName(n + "-1");
            if (existingPlayer != null && existingPlayer != __instance)
            {
                int num = 2;
                while (GadgetCoreAPI.GetPlayerByName(n + "-" + num) != null) num++;
                n = n + "-" + num;
            }
            else
            {
                existingPlayer = __instance;
            }
            foreach (KeyValuePair<string, PlayerScript> entry in GadgetCoreAPI.playersByName.Where(x => x.Value == __instance).ToList())
            {
                GadgetCoreAPI.playersByName.Remove(entry.Key);
                GadgetNetwork.NetworkPlayersByName.Remove(entry.Key);
                GadgetNetwork.NamesByNetworkPlayer.Remove(entry.Value.GetComponent<NetworkView>().owner);
            }
            if (InstanceTracker.PlayerScript == __instance) GadgetCoreAPI.playerName = n;
            GadgetCoreAPI.playersByName[n] = __instance;
            GadgetNetwork.NetworkPlayersByName[n] = existingPlayer.GetComponent<NetworkView>().owner;
            GadgetNetwork.NamesByNetworkPlayer[existingPlayer.GetComponent<NetworkView>().owner] = n;
            if (__instance.GetComponent<NetworkView>().owner == RPCHooks.Singleton.GetComponent<NetworkView>().owner) GadgetNetwork.ServerPlayerName = n;
        }
    }
}
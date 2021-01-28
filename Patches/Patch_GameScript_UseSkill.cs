using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Collections;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("UseSkill")]
    static class Patch_GameScript_UseSkill
    {
        public static readonly MethodInfo Die = typeof(GameScript).GetMethod("Die", BindingFlags.Public | BindingFlags.Instance);
        public static readonly FieldInfo skilling = typeof(GameScript).GetField("skilling", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, ref int[] ___combatChips, ref bool ___skilling, ref bool[] ___skillUsin)
        {
            if (GadgetCoreAPI.IsInputFrozen()) return false;
            int chipID = ___combatChips[slot];
            if (ChipRegistry.Singleton.HasEntry(chipID))
            {
                ChipInfo chip = ChipRegistry.Singleton.GetEntry(chipID);
                if (!___skilling && !___skillUsin[slot])
                {

                    ___skillUsin[slot] = true;
                    __instance.StartCoroutine(SkillUsin(slot, ___skillUsin));
                    __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/error"), Menuu.soundLevel / 10f);
                    __instance.combatChipObj[slot].GetComponent<Animation>().Play();
                    if (chip.IsChipActive(slot))
                    {
                        return false;
                    }
                    int cost = chip.Cost; // TODO: Make chip cost cancelable.
                    if (Menuu.curAugment == 18)
                    {
                        cost /= 2;
                    }
                    if (cost < 0) cost = 0;
                    if ((chip.Type & ChipType.ACTIVE) > 0)
                    {
                        if ((chip.CostType == ChipInfo.ChipCostType.MANA && GameScript.mana >= cost) ||
                            (chip.CostType == ChipInfo.ChipCostType.ENERGY && GameScript.energy >= cost) ||
                            (chip.CostType == ChipInfo.ChipCostType.HEALTH_SAFE && GameScript.hp > cost) ||
                            chip.CostType == ChipInfo.ChipCostType.HEALTH_LETHAL || chip.CostType == ChipInfo.ChipCostType.HEALTH_LETHAL_POSTMORTEM)
                        {
                            GameObject gameObject = (GameObject)Object.Instantiate(Resources.Load("txtSkill"), new Vector3(0f, 0f, 0f), Quaternion.identity);
                            gameObject.transform.parent = Camera.main.transform;
                            gameObject.transform.localPosition = new Vector3(-14f, 7f, 0.35f);
                            gameObject.SendMessage("InitSkill", chip.Name);
                            if (chip.CostType == ChipInfo.ChipCostType.MANA)
                            {
                                GameScript.mana -= cost;
                                __instance.BARMANA.GetComponent<Animation>().Play();
                                chip.InvokeOnUse(slot);
                                __instance.StartCoroutine(WaitChip(__instance));
                                __instance.UpdateMana();
                            }
                            else if (chip.CostType == ChipInfo.ChipCostType.ENERGY)
                            {
                                GameScript.energy -= cost;
                                chip.InvokeOnUse(slot);
                                __instance.StartCoroutine(WaitChip(__instance));
                                __instance.UpdateEnergy();
                            }
                            else
                            {
                                GameScript.hp -= cost;
                                if (GameScript.hp <= 0)
                                {
                                    GameScript.dead = true;
                                }
                                if (GameScript.hp > 0 || chip.CostType == ChipInfo.ChipCostType.HEALTH_LETHAL_POSTMORTEM)
                                {
                                    chip.InvokeOnUse(slot);
                                    __instance.StartCoroutine(WaitChip(__instance));
                                }
                                if (GameScript.hp <= 0)
                                {
                                    GameScript.dead = true;
                                    GameScript.hp = 0;
                                    Die.Invoke(__instance, new object[] { });
                                }
                                else
                                {
                                    GameScript.dead = false;
                                }
                                __instance.UpdateHP();
                            }
                        }
                    }
                }
                return false;
            }
            return true;
        }

        private static IEnumerator SkillUsin(int slot, bool[] skillUsin)
        {
            yield return new WaitForSeconds(0.2f);
            skillUsin[slot] = false;
            yield break;
        }

        private static IEnumerator WaitChip(GameScript instance)
        {
            yield return new WaitForSeconds(0.1f);
            skilling.SetValue(instance, false);
            yield break;
        }
    }
}
using HarmonyLib;
using GadgetCore.API;
using GadgetCore;
using UnityEngine;
using System.Collections;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("UseSkill")]
    static class Patch_GameScript_UseSkill
    {
        public static readonly FieldInfo skilling = typeof(GameScript).GetField("skilling", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, ref int[] ___combatChips, ref bool ___skilling, ref bool[] ___skillUsin)
        {
            int chipID = ___combatChips[slot];
            if (ChipRegistry.GetSingleton().HasEntry(chipID))
            {
                ChipInfo chip = ChipRegistry.GetSingleton().GetEntry(chipID);
                if (!___skilling && !___skillUsin[slot])
                {

                    ___skillUsin[slot] = true;
                    __instance.StartCoroutine(SkillUsin(slot, ___skillUsin));
                    __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/error"), Menuu.soundLevel / 10f);
                    __instance.combatChipObj[slot].GetComponent<Animation>().Play();
                    if (chip.IsChipActive())
                    {
                        return false;
                    }
                    int cost = chip.Cost;
                    if (Menuu.curAugment == 18)
                    {
                        cost /= 2;
                    }
                    if (cost < 0) cost = 0;
                    if ((chip.Type & ChipType.ACTIVE) > 0)
                    {
                        if ((chip.CostType == ChipInfo.ChipCostType.MANA && GameScript.mana >= cost) ||
                            (chip.CostType == ChipInfo.ChipCostType.ENERGY && GameScript.energy >= cost) ||
                            (chip.CostType == ChipInfo.ChipCostType.HEALTH && GameScript.hp > cost) ||
                            (chip.CostType == ChipInfo.ChipCostType.HEALTH_LETHAL && GameScript.hp >= cost))
                        {
                            GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("txtSkill"), new Vector3(0f, 0f, 0f), Quaternion.identity);
                            gameObject.transform.parent = Camera.main.transform;
                            gameObject.transform.localPosition = new Vector3(-14f, 7f, 0.35f);
                            gameObject.SendMessage("InitSkill", chip.Name);
                            if (chip.CostType == ChipInfo.ChipCostType.MANA)
                            {
                                GameScript.mana -= cost;
                                __instance.BARMANA.GetComponent<Animation>().Play();
                                __instance.UpdateMana();
                            }
                            else if (chip.CostType == ChipInfo.ChipCostType.ENERGY)
                            {
                                GameScript.energy -= cost;
                                __instance.UpdateEnergy();
                            }
                            else
                            {
                                GameScript.hp -= cost;
                                __instance.UpdateHP();
                            }
                            chip.InvokeOnUse(slot);
                            __instance.StartCoroutine(WaitChip(__instance));
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
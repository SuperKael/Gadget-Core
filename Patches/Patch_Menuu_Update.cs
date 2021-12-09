using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Collections;
using GadgetCore.API.ConfigMenu;
using System.Collections.Generic;
using System.Reflection.Emit;
using GadgetCore.Util;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Menuu))]
    [HarmonyPatch("Update")]
    static class Patch_Menuu_Update
    {
        [HarmonyPrefix]
        [HarmonyOverrides]
        public static void Prefix(Menuu __instance, ref Ray ___ray, ref RaycastHit ___hit)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                ___ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(___ray, out ___hit, 10f))
                {
                    switch (___hit.transform.gameObject.name)
                    {
                        case "bModMenu":
                            __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/confirm"), Menuu.soundLevel / 10f);
                            __instance.StartCoroutine(ModMenu(__instance));
                            break;
                        case "bQuit":
                            foreach (System.Diagnostics.Process process in ModMenuController.ConfigHandles)
                            {
                                if (process != null && !process.HasExited) process.Kill();
                            }
                            break;
                        case "bSelectorPageBack":
                            __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/confirm"), Menuu.soundLevel / 10f);
                            {
                                if (PatchMethods.characterFeatureRegistries.TryGetValue(__instance.GetFieldValue<int>("stuffSelecting"), out ICharacterFeatureRegistry characterFeatureRegistry))
                                {
                                    int currentPage = characterFeatureRegistry.GetCurrentPage();
                                    characterFeatureRegistry.SetCurrentPage(--currentPage);
                                }
                            }
                            break;
                        case "bSelectorPageForward":
                            __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/confirm"), Menuu.soundLevel / 10f);
                            {
                                if (PatchMethods.characterFeatureRegistries.TryGetValue(__instance.GetFieldValue<int>("stuffSelecting"), out ICharacterFeatureRegistry characterFeatureRegistry))
                                {
                                    int currentPage = characterFeatureRegistry.GetCurrentPage();
                                    characterFeatureRegistry.SetCurrentPage(++currentPage);
                                }
                            }
                            break;
                    }
                }
            }
        }

        private static IEnumerator ModMenu(Menuu instance)
        {
            ModBrowser.CloseModBrowser();
            SceneInjector.ModMenuPanel.Rebuild();
            SceneInjector.ModMenuBackButtonBeam.transform.localScale = new Vector3(30f, 0f, 1f);
            SceneInjector.ModMenuBackButtonHolder.transform.position = new Vector3(-40f, 0f, 0f);
            instance.menuMain.SetActive(false);
            SceneInjector.ModMenu.SetActive(true);
            SceneInjector.ModMenuCanvas.GetComponent<CanvasGroup>().alpha = 1;
            SceneInjector.ModMenuCanvas.GetComponent<CanvasGroup>().interactable = true;
            SceneInjector.ModMenuCanvas.GetComponent<CanvasGroup>().blocksRaycasts = true;
            instance.StartCoroutine(DelayRebuildConfigMenus());
            SceneInjector.ModMenuBackButtonBeam.GetComponent<Animation>().Play();
            yield return new WaitForSeconds(0.1f);
            SceneInjector.ModMenuBackButtonHolder.GetComponent<Animation>().Play();
            yield return null;
            yield break;
        }

        private static IEnumerator DelayRebuildConfigMenus()
        {
            yield return new WaitForEndOfFrame();
            GadgetModConfigs.ResetAllConfigMenus();
            yield break;
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var p = TranspilerHelper.CreateProcessor(instructions, generator);

            var allegianceUpRef = p.FindRefByInsns(new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldsfld, "System.Int32 curAllegiance"),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Add),
                new CodeInstruction(OpCodes.Stsfld, "System.Int32 curAllegiance"),
                new CodeInstruction(OpCodes.Ldsfld, "System.Int32 curAllegiance"),
                new CodeInstruction(OpCodes.Ldc_I4_3),
                new CodeInstruction(OpCodes.Ble),
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Stsfld, "System.Int32 curAllegiance"),
            });
            p.InjectInsn(allegianceUpRef, new CodeInstruction(OpCodes.Ldc_I4_1));
            p.InjectHook(allegianceUpRef, typeof(AllegianceRegistry).GetMethod("CycleAllegianceSelection"));
            p.RemoveInsns(allegianceUpRef, 9);

            var allegianceDownRef = p.FindRefByInsns(new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldsfld, "System.Int32 curAllegiance"),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Sub),
                new CodeInstruction(OpCodes.Stsfld, "System.Int32 curAllegiance"),
                new CodeInstruction(OpCodes.Ldsfld, "System.Int32 curAllegiance"),
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Bge),
                new CodeInstruction(OpCodes.Ldc_I4_3),
                new CodeInstruction(OpCodes.Stsfld, "System.Int32 curAllegiance"),
            });
            p.InjectInsn(allegianceDownRef, new CodeInstruction(OpCodes.Ldc_I4_0));
            p.InjectHook(allegianceDownRef, typeof(AllegianceRegistry).GetMethod("CycleAllegianceSelection"));
            p.RemoveInsns(allegianceDownRef, 9);

            return p.GetInstructions();
        }
    }
}
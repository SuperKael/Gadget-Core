using HarmonyLib;
using GadgetCore.API;
using GadgetCore;
using UnityEngine;
using System.Collections;
using Ionic.Zip;
using System.IO;
using UModFramework.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Menuu))]
    [HarmonyPatch("Update")]
    static class Patch_Menuu_Update
    {
        [HarmonyPrefix]
        public static bool Prefix(Menuu __instance, ref Ray ___ray, ref RaycastHit ___hit)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                ___ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(___ray, out ___hit, 10f))
                {
                    if (___hit.transform.gameObject.name.Equals("bModMenu"))
                    {
                        __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/confirm"), Menuu.soundLevel / 10f);
                        if (GadgetCore.IsUnpacked)
                        {
                            __instance.StartCoroutine(ModMenu(__instance));
                        }
                        else
                        {
                            __instance.StartCoroutine(UnpackGadgetCore());
                        }
                        return false;
                    }
                    else if (___hit.transform.gameObject.name.Equals("bQuit"))
                    {
                        foreach (System.Diagnostics.Process process in ModDescPanelController.ConfigHandles)
                        {
                            if (process != null && !process.HasExited) process.Kill();
                        }
                    }
                    else if (___hit.transform.gameObject.name.Equals("bPlay") && !GadgetCore.IsUnpacked)
                    {
                        GadgetCore.Log(GadgetCoreAPI.GetCursorPos().ToString());
                        GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("txtError"), GadgetCoreAPI.GetCursorPos() + new Vector3(0, 0, 9), Quaternion.identity);
                        gameObject.SendMessage("InitError", "You must unpack Gadget Core before playing!");
                        return false;
                    }
                }
            }
            return true;
        }
        private static IEnumerator ModMenu(Menuu instance)
        {
            SceneInjector.ModMenuBackButtonBeam.transform.localScale = new Vector3(30f, 0f, 1f);
            SceneInjector.ModMenuBackButtonHolder.transform.position = new Vector3(-40f, 0f, 0f);
            instance.menuMain.SetActive(false);
            SceneInjector.ModMenu.SetActive(true);
            SceneInjector.ModMenuBackButtonBeam.GetComponent<Animation>().Play();
            yield return new WaitForSeconds(0.1f);
            SceneInjector.ModMenuBackButtonHolder.GetComponent<Animation>().Play();
            yield return null;
            yield break;
        }
        private static IEnumerator UnpackGadgetCore()
        {
            if (ZipUtils.UnpackMod("GadgetCore"))
            {
                yield return new WaitForSeconds(0.25f);
                Application.Quit();
            }
            yield break;
        }
    }
}
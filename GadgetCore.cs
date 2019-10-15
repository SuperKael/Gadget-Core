using UnityEngine;
using UModFramework.API;
using UnityEngine.SceneManagement;

namespace URP
{
    class URP
    {
        public static Texture2D recipesStaminaMiscFixed;
        public static Material r1t1Fixed;

        internal static void Log(string text, bool clean = false)
        {
            using (UMFLog log = new UMFLog()) log.Log(text, clean);
        }

        [UMFConfig]
        public static void LoadConfig()
        {
            URPConfig.Load();
        }

		[UMFHarmony(20)] //Set this to the number of harmony patches in your mod.
        public static void Start()
		{
			Log("Unofficial Roguelands Patch v" + UMFMod.GetModVersion().ToString(), true);
            recipesStaminaMiscFixed = UModFramework.API.UMFAsset.LoadTexture2D("recipesSTAMINAMISC_Fixed.png");
            recipesStaminaMiscFixed.filterMode = FilterMode.Point;
            r1t1Fixed = Resources.Load<Material>("mat/r1t1");
            r1t1Fixed.mainTexture = recipesStaminaMiscFixed;
		}
	}
}
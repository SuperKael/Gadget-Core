﻿using GadgetCore.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GadgetCore
{

    [RequireComponent(typeof(Toggle))]
    internal class KeybindToggle : MonoBehaviour
    {
        /// <summary>
        /// Whether a binding operation is currently in progress.
        /// </summary>
        public static bool Binding { get; private set; }

        private Toggle toggle;
        private List<string> keyList;
        private Action<string> keybindSetter;
        private bool allowMultiBind;
        private bool clickSkip;
        public void Init(Toggle toggle, Action<string> keybindSetter, bool allowMultiBind)
        {
            this.toggle = toggle;
            this.keybindSetter = keybindSetter;
            this.allowMultiBind = allowMultiBind;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Unity Event")]
        private void Update()
        {
            if (toggle != null && toggle.isOn)
            {
                if (keyList == null)
                {
                    keyList = new List<string>();
                    keybindSetter("");
                    clickSkip = false;
                    GadgetCoreAPI.FreezeInput("Keybinding");
                }
                toggle.graphic.color = new Color(toggle.graphic.color.r, toggle.graphic.color.g, toggle.graphic.color.b, Mathf.RoundToInt(Time.time * 8) % 2);
                foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
                {
                    if (key != KeyCode.Escape && Input.GetKeyDown(key))
                    {
                        if (!allowMultiBind)
                        {
                            keyList = new List<string>();
                        }
                        keyList.Add(key.ToString());
                        keybindSetter(keyList.Aggregate(new StringBuilder(), (a, b) => { if (a.Length > 0) a.Append('+'); a.Append(b); return a; }).ToString());
                    }
                    else if (Input.GetKeyUp(key))
                    {
                        if (!clickSkip && key == KeyCode.Mouse0)
                        {
                            clickSkip = true;
                            continue;
                        }
                        keyList = null;
                        toggle.isOn = false;
                        GadgetCoreAPI.UnfreezeInput("Keybinding");
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Unity Event")]
        private void OnDisable()
        {
            GadgetCoreAPI.UnfreezeInput("Keybinding");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Unity Event")]
        private void OnDestroy()
        {
            GadgetCoreAPI.UnfreezeInput("Keybinding");
        }
    }
}

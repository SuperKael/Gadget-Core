using GadgetCore.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GadgetCore
{
    public class GadgetModHookScript : MonoBehaviour
    {
        public GadgetModInfo Mod { get; internal set; }
        public void Awake()
        {
            if (Mod.Enabled) Mod.Mod.ScriptAwake();
        }
        public void Start()
        {
            if (Mod.Enabled) Mod.Mod.ScriptStart();
        }
        public void Update()
        {
            if (Mod.Enabled) Mod.Mod.ScriptUpdate();
        }
        public void FixedUpdate()
        {
            if (Mod.Enabled) Mod.Mod.ScriptFixedUpdate();
        }
    }
}

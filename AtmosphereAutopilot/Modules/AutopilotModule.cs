﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AtmosphereAutopilot
{

    abstract class AutopilotModule : GUIWindow, IAutoSerializable
    {
        protected Vessel vessel = null;
        protected bool enabled = false;
        protected string module_name;

        public AutopilotModule(Vessel v, int wnd_id, string module_name):
            base(module_name, wnd_id, new Rect(50.0f, 80.0f, 220.0f, 150.0f))
        {
            vessel = v;
            this.module_name = module_name;
        }

        public void Activate()
        {
            if (!enabled)
                OnActivate();
            enabled = true;
        }

        public string ModuleName { get { return module_name; } }

        protected abstract void OnActivate();

        public void Deactivate()
        {
            if (enabled)
                OnDeactivate();
            enabled = false;
        }

		protected abstract void OnDeactivate();

        public bool Active
        {
            get { return enabled; }
            set
            {
                if (value)
                    if (!enabled)
                        Activate();
                    else {}
                else
                    if (enabled)
                        Deactivate();
            }
        }

        #region Serialization

        public bool DeserializeVesselSpecific()
        {
            return AutoSerialization.Deserialize(this, module_name.Replace(' ', '_'),
                KSPUtil.ApplicationRootPath + "GameData/AtmosphereAutopilot/" + vessel.vesselName + ".cfg",
                typeof(VesselSerializable), OnDeserialize);
        }

        public bool DeserializeGlobalSpecific()
        {
            return AutoSerialization.Deserialize(this, module_name.Replace(' ', '_'), 
                KSPUtil.ApplicationRootPath + "GameData/AtmosphereAutopilot/Global_settings.cfg",
                typeof(GlobalSerializable), OnDeserialize);
        }

        public void Serialize()
        {
            BeforeSerialize();
            AutoSerialization.Serialize(this, module_name.Replace(' ', '_'),
                KSPUtil.ApplicationRootPath + "GameData/AtmosphereAutopilot/" + vessel.vesselName + ".cfg",
                typeof(VesselSerializable), OnSerialize);
            AutoSerialization.Serialize(this, module_name.Replace(' ', '_'),
                KSPUtil.ApplicationRootPath + "GameData/AtmosphereAutopilot/Global_settings.cfg",
                typeof(GlobalSerializable), OnSerialize);
        }

        protected virtual void BeforeSerialize() { }

        protected virtual void BeforeDeserialize() { }

        public bool Deserialize()
        {
            BeforeDeserialize();
            return (DeserializeVesselSpecific() & DeserializeGlobalSpecific());
        }

        protected virtual void OnDeserialize(ConfigNode node, Type attribute_type) { }

        protected virtual void OnSerialize(ConfigNode node, Type attribute_type) { }

        #endregion


     
        #region GUI

        [GlobalSerializable("window_x")]
        public float WindowLeft { get { return window.xMin; } set { window.xMin = value; } }

        [GlobalSerializable("window_y")]
        public float WindowTop { get { return window.yMin; } set { window.yMin = value; } }

        [GlobalSerializable("window_width")]
        public float WindowWidth { get { return window.width; } set { window.width = value; } }

        protected override void _drawGUI(int id)
        {
            GUILayout.BeginVertical();
            AutoGUI.AutoDrawObject(this);
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        #endregion
    }
}

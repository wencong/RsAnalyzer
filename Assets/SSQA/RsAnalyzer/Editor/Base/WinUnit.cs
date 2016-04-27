using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SSQA {
    public static class WinUnitConfig {
        public static GUILayoutOption sButtonWidth = GUILayout.Width(100);
        public static GUILayoutOption sLabelWidth = GUILayout.Width(100);
        public static GUILayoutOption sExpandWidth = GUILayout.Width(25);
        public static GUILayoutOption sNameWidth = GUILayout.Width(200);
    }

    public interface IWinUnit {
        void Start();
        void OnGUI();
        void OnDisable();
    }
}

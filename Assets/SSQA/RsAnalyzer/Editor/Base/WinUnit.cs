using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SSQA {
    public static class WinUnitConfig {
        public static GUILayoutOption sButtonWidth = GUILayout.Width(100);
        public static GUILayoutOption sLabelWidth = GUILayout.Width(100);
        public static GUILayoutOption sExpandWidth = GUILayout.Width(25);
        public static GUILayoutOption sNameWidth = GUILayout.Width(300);
        public static GUILayoutOption sHalfNameWidth = GUILayout.Width(150);
    }

    public interface IWinUnit {
        void Start();
        void OnGUI();
        void Update();
        void OnDisable();
    }
}

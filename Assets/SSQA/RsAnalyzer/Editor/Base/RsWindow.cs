using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace SSQA {
    public class RsWindow : EditorWindow {

        //private IWinUnit prefabAnalyzer = new PrefabInstanceAnalyzer();
        private Dictionary<string, IWinUnit> m_analyzerWindows = new Dictionary<string, IWinUnit>();
        private List<string> m_inspectName = new List<string>();

        private int m_nActiveAnalyzer = 0;
        private IWinUnit m_activeAnalyzer = null;

        [MenuItem("SSQA/RsAnalyzer")]
        public static void SceneAnalyzer() {
            var win = GetWindow<RsWindow>();

            win.RegisterAnalyzerWindow("PrefabAnalyzer", new PrefabInstanceAnalyzer());
            win.RegisterAnalyzerWindow("ShaderAnalyzer", new ShaderAnalyzer());
            win.RegisterAnalyzerWindow("PixelAnalyzer", new PixelAnalyzer());

            win.Show();
        }

        private void RegisterAnalyzerWindow(string szWindowName, IWinUnit window) {
            if (m_analyzerWindows.ContainsKey(szWindowName)) {
                return;
            }

            m_analyzerWindows.Add(szWindowName, window);
            m_inspectName.Add(szWindowName);
        }

        private void UnRegisterAllWindow() {
            m_analyzerWindows.Clear();
            m_inspectName.Clear();
        }

        public void Update() {
            if (m_activeAnalyzer != null) {
                m_activeAnalyzer.Update();
            }
        }

        public void OnGUI() {
            GUILayout.BeginHorizontal();
            {
                m_nActiveAnalyzer = GUILayout.Toolbar(m_nActiveAnalyzer, m_inspectName.ToArray());
            }
            GUILayout.EndHorizontal();

            if (m_inspectName.Count == 0) {
                return;
            }

            string szAnalyzer = m_inspectName[m_nActiveAnalyzer];
            m_activeAnalyzer = m_analyzerWindows[szAnalyzer];

            if (m_activeAnalyzer == null) {
                return;
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Start")) {
                //prefabAnalyzer.Start(Selection.activeGameObject.transform);
                //if (Selection.activeGameObject != null) {
                m_activeAnalyzer.Start();
                //}
                
            }
            GUILayout.EndHorizontal();

            m_activeAnalyzer.OnGUI();
        }

        public void OnDisable() {
            if (m_activeAnalyzer != null) {
                m_activeAnalyzer.OnDisable();
            }

            UnRegisterAllWindow();

            m_nActiveAnalyzer = 0;
            m_activeAnalyzer = null;
        }
    }
}

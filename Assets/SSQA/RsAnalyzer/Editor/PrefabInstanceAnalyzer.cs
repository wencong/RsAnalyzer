using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SSQA {
    public class WinPrefabItem {
        public string szName;
        //public int nInsts;
        public int nVertex;
        public int nTriangle;

        public List<PrefabInstanceInfo> insts = new List<PrefabInstanceInfo>();

        public WinPrefabItem(string name, int nVertex, int nTriangle) {
            this.szName = name;
            this.nVertex = nVertex;
            this.nTriangle = nTriangle;
        }

        public void OnGUI() {
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(szName, WinUnitConfig.sNameWidth)) {
                    RsUtil.SelectRss(insts.ToArray());

                    PrefabInstanceInfo info = insts[0];
                    Debug.LogFormat("{0} {1}",info.szName, info.szPath);
                }

                int nInstance = insts.Count;
                GUILayout.Label(nInstance.ToString(), WinUnitConfig.sButtonWidth);
                GUILayout.Label(nVertex.ToString(), WinUnitConfig.sButtonWidth);
                GUILayout.Label(nTriangle.ToString(), WinUnitConfig.sButtonWidth);
                GUILayout.Label((nInstance * nVertex).ToString(), WinUnitConfig.sButtonWidth);
                GUILayout.Label((nInstance * nTriangle).ToString(), WinUnitConfig.sButtonWidth);
            }
            GUILayout.EndHorizontal();
        }
    }

    public enum SortType {
        eSortByDefault,
        eSortByInst,
        eSortByVert,
        eSortByTriangle,
        eSortByTotalVerts,
        eSortByTotalTriangles
    }

    public class PrefabInstanceAnalyzer : IWinUnit {
        private List<WinPrefabItem> m_prefabItems = new List<WinPrefabItem>();
        private Dictionary<string, WinPrefabItem> m_nameMap = new Dictionary<string, WinPrefabItem>();

        private string m_szSearchItem = string.Empty;
        private bool m_bAnalyzed = false;

        private SortType m_sortType = SortType.eSortByDefault;

        private Vector2 m_scrollPos = Vector2.zero;
        public void Start() {
            OnDisable();  //有点搓~.~

            GameObject select = Selection.activeGameObject;
            if (select != null) {
                _Analyzer(select.transform);
                m_bAnalyzed = true;
            }
        }

        private void _Analyzer(Transform trans) {
            for (int i = 0; i < trans.childCount; ++i) {
                GameObject gameobject = trans.GetChild(i).gameObject;

                if (RsUtil.IsPrefabInstance(gameobject)) {
                    string szAssetName = RsUtil.GetPrefaInstancebAssetName(gameobject);
                    if (szAssetName == string.Empty) {
                        Debug.LogErrorFormat("Can't find the asset of {0}", gameobject.name);
                        continue;
                    }

                    PrefabInstanceInfo prefabInstanceInfo = new PrefabInstanceInfo(gameobject);
                    WinPrefabItem prefabItem = null;

                    if (!m_nameMap.TryGetValue(szAssetName, out prefabItem)) {
                        prefabItem = new WinPrefabItem(prefabInstanceInfo.szName, prefabInstanceInfo.nTotalVertex, prefabInstanceInfo.nTotalTriangle);

                        m_nameMap.Add(szAssetName, prefabItem);
                        
                        m_prefabItems.Add(prefabItem);
                    }
                    prefabItem.insts.Add(prefabInstanceInfo);
                }
                else {
                    _Analyzer(gameobject.transform);
                }
            }
        }

        public void OnGUI() {
            if (m_bAnalyzed) {
                _DrawUIHead();

                _DrawSummaryInfo();

                _DrawSearchLabel();

                _DrawDataBody();
            }
        }

        public void Update() {

        }

        private void _DrawUIHead() {
            GUILayout.BeginHorizontal();
            {
                SortType eSortType = SortType.eSortByDefault;
                GUILayout.Label("PrefabName", WinUnitConfig.sNameWidth);

                if (GUILayout.Button("Instances", WinUnitConfig.sButtonWidth)) {
                    eSortType = SortType.eSortByInst;
                }

                if (GUILayout.Button("Vertex", WinUnitConfig.sButtonWidth)) {
                    eSortType = SortType.eSortByVert;
                }

                if (GUILayout.Button("Triangle", WinUnitConfig.sButtonWidth)) {
                    eSortType = SortType.eSortByTriangle;
                }

                if (GUILayout.Button("TotalVertex", WinUnitConfig.sButtonWidth)) {
                    eSortType = SortType.eSortByTotalVerts;
                }

                if (GUILayout.Button("TotalTriangle", WinUnitConfig.sButtonWidth)) {
                    eSortType = SortType.eSortByTotalTriangles;
                }
                _SortWinPrefabItem(eSortType);
            }
            GUILayout.EndHorizontal();
        }

        private void _SortWinPrefabItem(SortType eType) {
            if (eType == SortType.eSortByDefault) {
                return;
            }

            if (m_sortType == eType) {
                m_prefabItems.Reverse();
                return;
            }

            switch (eType) {
                case SortType.eSortByInst: {
                    m_prefabItems.Sort(_SortByInstanceCount);
                    break;
                }
                case SortType.eSortByVert: {
                    m_prefabItems.Sort(_SortByVert);
                    break;
                }
                case SortType.eSortByTriangle: {
                    m_prefabItems.Sort(_SortByTriangle);
                    break;
               }
               case SortType.eSortByTotalVerts: {
                   m_prefabItems.Sort(_SortByTotalVerts);
                   break;
               }
               case SortType.eSortByTotalTriangles: {
                   m_prefabItems.Sort(_SortByTotalTriangle);
                   break;
                }
            }

            m_sortType = eType;
        }

        private void _DrawSummaryInfo() {
            GUILayout.BeginHorizontal();
            {
                int nPrefabCount = 0;
                int nInstCount = 0;
                int nVert = 0;
                int nTotalVert = 0;
                int nTriangle = 0;
                int nTotalTriangle = 0;

                nPrefabCount = m_nameMap.Count;

                for (int i = 0; i < m_prefabItems.Count; ++i) {
                    WinPrefabItem prefabItem = m_prefabItems[i];

                    nInstCount += prefabItem.insts.Count;
                    nVert += prefabItem.nVertex;
                    nTriangle += prefabItem.nTriangle;
                    nTotalVert += prefabItem.insts.Count * prefabItem.nVertex;
                    nTotalTriangle += prefabItem.insts.Count * prefabItem.nTriangle;
                }

                GUILayout.Label("Total", WinUnitConfig.sNameWidth);
                GUILayout.Label(string.Format("{0}/{1}", nPrefabCount, nInstCount), WinUnitConfig.sButtonWidth);
                GUILayout.Label(string.Format("{0}", nVert), WinUnitConfig.sButtonWidth);
                GUILayout.Label(string.Format("{0}", nTriangle), WinUnitConfig.sButtonWidth);
                GUILayout.Label(string.Format("{0}", nTotalVert), WinUnitConfig.sButtonWidth);
                GUILayout.Label(string.Format("{0}", nTotalTriangle), WinUnitConfig.sButtonWidth);
            }
            GUILayout.EndHorizontal();
        }

        private void _DrawSearchLabel() {
            GUILayout.BeginHorizontal();
            {
                m_szSearchItem = GUILayout.TextField(m_szSearchItem, WinUnitConfig.sNameWidth);
            }
            GUILayout.EndHorizontal();
        }

        private void _DrawDataBody() {
            m_scrollPos = GUILayout.BeginScrollView(m_scrollPos);
            for (int nIndex = 0; nIndex < m_prefabItems.Count; ++nIndex) {
                WinPrefabItem prefabItem = m_prefabItems[nIndex];
                if (prefabItem.szName.ToLower().Contains(m_szSearchItem.ToLower())) {
                    m_prefabItems[nIndex].OnGUI();
                }
            }
            GUILayout.EndScrollView();
        }

        public void OnDisable() {
            m_prefabItems.Clear();
            m_nameMap.Clear();
        }

        #region SortMethod
        private int _SortByInstanceCount(WinPrefabItem e1, WinPrefabItem e2) {
            if (e1.insts.Count > e2.insts.Count) {
                return -1;
            }
            else if (e1.insts.Count < e2.insts.Count) {
                return 1;
            }
            return 0;
        }
        private int _SortByVert(WinPrefabItem e1, WinPrefabItem e2) {
            if (e1.nVertex > e2.nVertex) {
                return -1;
            }
            else if (e1.nVertex < e2.nVertex) {
                return 1;
            }
            return 0;
        }
        private int _SortByTriangle(WinPrefabItem e1, WinPrefabItem e2) {
            if (e1.nTriangle > e2.nTriangle) {
                return -1;
            }
            else if (e1.nTriangle < e2.nTriangle) {
                return 1;
            }
            return 0;
        }
        private int _SortByTotalVerts(WinPrefabItem e1, WinPrefabItem e2) {
            int nInst1 = e1.insts.Count;
            int nInst2 = e2.insts.Count;

            if (e1.nVertex * nInst1 > e2.nVertex * nInst2) {
                return -1;
            }
            else if (e1.nVertex * nInst1 < e2.nVertex * nInst2) {
                return 1;
            }
            return 0;
        }
        private int _SortByTotalTriangle(WinPrefabItem e1, WinPrefabItem e2) {
            int nInst1 = e1.insts.Count;
            int nInst2 = e2.insts.Count;

            if (e1.nTriangle * nInst1 > e2.nTriangle * nInst2) {
                return -1;
            }
            else if (e1.nTriangle * nInst1 < e2.nTriangle * nInst2) {
                return 1;
            }
            return 0;
        }

        #endregion
    }
}

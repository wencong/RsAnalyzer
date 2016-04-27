using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;
using System.IO;

namespace SSQA {
    public class ShaderItem {
        public ShaderInfo shaderInfo;
        public List<MaterialInfo> lstMaterialInfo = new List<MaterialInfo>();
        public Dictionary<MaterialInfo, List<ModelInfo>> matMap = new Dictionary<MaterialInfo, List<ModelInfo>>();

        public int nTotalVerts = 0;
        public int nTotalTriangle = 0;

        //public Dictionary<MaterialInfo, ModelInfo> mat2ObjMap = new Dictionary<MaterialInfo, ModelInfo>();
        public bool bShowMatRef = false;

        public ShaderItem(ShaderInfo shaderInfo) {
            this.shaderInfo = shaderInfo;
        }

        public void AddMaterialInfo(MaterialInfo matInfo, ModelInfo modelInfo) {
            if (!lstMaterialInfo.Contains(matInfo)) {
                lstMaterialInfo.Add(matInfo);
                matMap.Add(matInfo, new List<ModelInfo>() { modelInfo });
            }
            else {
                matMap[matInfo].Add(modelInfo);
            }

            MeshInfo meshInfo = modelInfo.GetMeshInfo();
            if (meshInfo != null) {
                nTotalVerts += meshInfo.nVertex;
                nTotalTriangle += meshInfo.nTriangle;
            }
        }

        public void OnGUI() {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(shaderInfo.szName, WinUnitConfig.sNameWidth);

                if (GUILayout.Button(string.Format("{0} ref", lstMaterialInfo.Count), WinUnitConfig.sButtonWidth)) {
                    bShowMatRef = !bShowMatRef;
                }

                GUILayout.Label(string.Format("Verts: {0}", nTotalVerts), WinUnitConfig.sButtonWidth);
                GUILayout.Label(string.Format("Triangles: {0}", nTotalTriangle), WinUnitConfig.sButtonWidth);
            }
            GUILayout.EndHorizontal();

            if (bShowMatRef) {
                for (int i = 0; i < lstMaterialInfo.Count; ++i) {
                    MaterialInfo matInfo = lstMaterialInfo[i];
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button(matInfo.szName, WinUnitConfig.sNameWidth)) {
                            RsUtil.SelectRs(matInfo);
                        }
                        List<ModelInfo> lstModels = matMap[matInfo];
                        if (GUILayout.Button(string.Format("{0} objs", lstModels.Count), WinUnitConfig.sButtonWidth)) {
                            RsUtil.SelectRss(lstModels.ToArray());
                        }

                        int nVerts = 0;
                        int nTriangle = 0;
                        for (int j = 0; j < lstModels.Count; ++j) {
                            MeshInfo meshInfo = lstModels[j].meshInfo;
                            if (meshInfo != null) {
                                nVerts += meshInfo.nVertex;
                                nTriangle += meshInfo.nTriangle;
                            }
                        }
                        GUILayout.Label(string.Format("Verts: {0}", nVerts), WinUnitConfig.sButtonWidth);
                        GUILayout.Label(string.Format("Triangles: {0}", nTriangle), WinUnitConfig.sButtonWidth);

                        TextureInfo[] texsInfo = matInfo.GetTextureInfo();
                        for (int j = 0; j < texsInfo.Length; ++j) {
                            TextureInfo tInfo = texsInfo[j];
                            try {
                                if (tInfo != null) {
                                    if (GUILayout.Button(string.Format("{0} * {1}", tInfo.nWidth, tInfo.nHeight), WinUnitConfig.sButtonWidth)) {
                                        RsUtil.SelectRs(tInfo);
                                    }
                                }
                                else {
                                    GUILayout.Button(string.Format("null"), WinUnitConfig.sButtonWidth);
                                }
                            }
                            catch (Exception ex) {
                                Debug.LogException(ex);
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("", WinUnitConfig.sNameWidth);
                }
                GUILayout.EndHorizontal();
            }
        }
    }

    public class ShaderAnalyzer : IWinUnit {
        private List<ShaderItem> m_lstShaderItem = new List<ShaderItem>();
        private Dictionary<string, ShaderItem> m_nameMap = new Dictionary<string,ShaderItem>();

        // Use this for initialization
        public void Start () {
            OnDisable();

            GameObject select = Selection.activeGameObject;
            if (select != null) {
                _Analyze(select.transform);
            }
            else {
                UnityEngine.Object asset = Selection.activeObject;
                if (asset != null) {
                    //Debug.Log(AssetDatabase.GetAssetPath(asset));
                    string path = AssetDatabase.GetAssetPath(asset);
                    if (Directory.Exists(path)) {
                        string[] arrayFiles = Directory.GetFiles(path, "*.prefab", SearchOption.AllDirectories);
                        Debug.LogFormat(arrayFiles.Length.ToString());
                    }
                }
            }
        }

        // Update is called once per frame
        public void OnGUI() {
            for (int i = 0; i < m_lstShaderItem.Count; ++i) {
                m_lstShaderItem[i].OnGUI();
            }
        }

        private void _Analyze(Transform trans) {
            for (int i = 0; i < trans.childCount; ++i) {
                GameObject gameobject = trans.GetChild(i).gameObject;

                if (gameobject.transform.childCount > 0) {
                    _Analyze(gameobject.transform);
                }

                ModelInfo modelInfo = new ModelInfo(gameobject);

                _AnalyzeModel(modelInfo);
            }
        }

        private void _AnalyzeModel(ModelInfo modelInfo) {
            MeshInfo meshInfo = modelInfo.GetMeshInfo();
            MaterialInfo[] matsInfo = modelInfo.GetMaterialInfo();

            if (meshInfo == null || matsInfo == null) {
                return;
            }

            for (int j = 0; j < matsInfo.Length; ++j) {
                MaterialInfo matInfo = matsInfo[j];
                ShaderInfo shaderInfo = matInfo.GetShaderInfo();

                if (shaderInfo == null) {
                    continue;
                }

                ShaderItem shaderItem = null;
                if (!m_nameMap.TryGetValue(shaderInfo.szName, out shaderItem)) {
                    shaderItem = new ShaderItem(shaderInfo);

                    m_nameMap.Add(shaderInfo.szName, shaderItem);
                    m_lstShaderItem.Add(shaderItem);
                }

                shaderItem.AddMaterialInfo(matInfo, modelInfo);
            }
        }

        public void OnDisable() {
            m_lstShaderItem.Clear();
            m_nameMap.Clear();
        }
    }

}

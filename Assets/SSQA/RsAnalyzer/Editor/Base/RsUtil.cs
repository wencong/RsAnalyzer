using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SSQA {
    public static class RsUtil {
        public static int GetUniqueID(UnityEngine.Object obj) {
            if (obj == null) {
                Debug.LogErrorFormat("obj is null");
                return -1;
            }
            int nUniqueID = obj.GetHashCode();
            return nUniqueID;
        }

        public static string GetRsPath(UnityEngine.Object obj) {
            if (obj == null) {
                Debug.LogErrorFormat("obj is null");
                return string.Empty;
            }
            string szAssetPath = AssetDatabase.GetAssetPath(obj);
            return szAssetPath;
        }

        public static Dictionary<string, Texture> GetTextureProperty(Material mat) {
            Shader shader = mat.shader;
            Dictionary<string, Texture> texProperty = new Dictionary<string, Texture>();
            
            int nPropertyCount = ShaderUtil.GetPropertyCount(shader);
            for (int i = 0; i < nPropertyCount; ++i) {
                string szPropertyName = ShaderUtil.GetPropertyName(shader, i);
                ShaderUtil.ShaderPropertyType eType = ShaderUtil.GetPropertyType(shader, i);

                switch (eType) {
                    case ShaderUtil.ShaderPropertyType.Color: {
                        break;
                    }

                    case ShaderUtil.ShaderPropertyType.Vector: {
                        break;
                    }

                    case ShaderUtil.ShaderPropertyType.Float: {
                        break;
                    }

                    case ShaderUtil.ShaderPropertyType.Range: {
                        break;
                    }

                    case ShaderUtil.ShaderPropertyType.TexEnv: {
                        Texture tex = mat.GetTexture(szPropertyName);
                        texProperty.Add(szPropertyName, tex);
                        break;
                    }
                }
            }
            return texProperty;
        }

        public static bool IsPrefabInstance(GameObject obj) {
            PrefabType type = PrefabUtility.GetPrefabType(obj);
            return type == PrefabType.PrefabInstance ||
                   type == PrefabType.ModelPrefabInstance ||
                   type == PrefabType.MissingPrefabInstance ||
                   type == PrefabType.DisconnectedPrefabInstance ||
                   type == PrefabType.DisconnectedModelPrefabInstance;
        }

        public static string GetPrefaInstancebAssetName(GameObject prefabInstance) {
            UnityEngine.Object asset = PrefabUtility.GetPrefabParent(prefabInstance);
            if (asset == null) {
                return string.Empty;
            }
            return asset.name;
        }

        public static void SelectRss(RsInfo[] arrayRs) {
            UnityEngine.Object[] selectObjs = new UnityEngine.Object[arrayRs.Length];
            for (int i = 0; i < arrayRs.Length; ++i) {
                selectObjs[i] = arrayRs[i].obj;
            }
            Selection.objects = selectObjs;
        }

        public static void SelectRs(RsInfo rsInfo) {
            RsInfo[] arrayRs = new RsInfo[1];
            arrayRs[0] = rsInfo;
            SelectRss(arrayRs);
        }
    }
}

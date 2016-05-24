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

        public static Material CreateSolidColorMaterial(Color color) {
            Material matSolidColor = new Material(Shader.Find("SolidColor"));
            matSolidColor.SetColor("_Color", color);
            return matSolidColor;
        }

        public static Material CreateBlendColorMaterial(Color color) {
            Material matBlend = new Material(Shader.Find("BlendColor"));
            matBlend.SetColor("_Color", color);
            return matBlend;
        }

        public static Color CreateSolidColor() {
            /*
            s_r += s_deltColor;
            if (s_r > 1.0f) {
                s_r = 0.0f;
                s_g += s_deltColor;
                if (s_g > 1.0f) {
                    s_g = 0.0f;
                    s_b += s_deltColor;
                    if (s_b > 1.0f) {
                        s_r = s_g = s_b = 0.0f;
                    }
                }
            }*/

            float r = UnityEngine.Random.Range(0.0f, 1.0f);
            float g = UnityEngine.Random.Range(0.0f, 1.0f);
            float b = UnityEngine.Random.Range(0.0f, 1.0f);
            Color color = new Color(r, g, b, 0.4f);

            //Color color = new Color(s_r, s_g, s_b);
            return color;
        }

        public static void SetObjectLayer(GameObject go, int layer) {
            go.layer = layer;
            for (int i = 0; i < go.transform.childCount; ++i) {
                SetObjectLayer(go.transform.GetChild(i).gameObject, layer);
            }
        }

        public static void SetCameraCullingMask(Camera camera, string layerName) {
            camera.cullingMask = (1 << LayerMask.NameToLayer(layerName));
            Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer(layerName));
        }

        public static bool IsEqual(Color32 c1, Color32 c2) {
            if (Mathf.Abs(c1.r - c2.r) <= 2 &&
                Mathf.Abs(c1.b - c2.b) <= 2 &&
                Mathf.Abs(c1.g - c2.g) <= 2) {
                return true;
            }
            return false;
        }

        public static PixelObject GetPixelObject(Color32 color, List<PixelObject> PixelObjects) {
            for (int i = 0; i < PixelObjects.Count; ++i) {
                if (IsEqual(PixelObjects[i].renderColor, color)) {
                    return PixelObjects[i];
                }
            }
            return null;
        }

        public static bool IsObjectInFrustumEx(Plane[] planes, GameObject go) {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();

            for (int nIndex = 0; nIndex < renderers.Length; ++nIndex) {
                Renderer renderer = renderers[nIndex];
                if (GeometryUtility.TestPlanesAABB(planes, renderer.bounds)) {
                    return true;
                }
            }

            return false;
        }

        public static bool AddLayer(string layerName) {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layerProp = tagManager.FindProperty("layers");

            if (layerProp != null) {
                int nFirstEmpty = 0;
                bool bExist = false;

                for (int i = 8; i < 32; ++i) {
                    SerializedProperty p = layerProp.GetArrayElementAtIndex(i);
                    if (p.stringValue.Equals("") && nFirstEmpty == 0) {
                        nFirstEmpty = i;
                    }

                    if (p.stringValue.Equals(layerName)) {
                        bExist = true;
                        break;
                    }
                }

                if (!bExist) {
                    SerializedProperty p = layerProp.GetArrayElementAtIndex(nFirstEmpty);
                    p.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                }
            }
            return true;
        }
    }
}

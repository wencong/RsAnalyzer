using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace SSQA {
    public abstract class RsInfo {
        public RsInfo(UnityEngine.Object obj) {
            this.obj = obj;
            this.szName = obj.name;
            this.szPath = RsUtil.GetRsPath(obj);
            this.ID     = RsUtil.GetUniqueID(obj);
        }

        public int ID;
        public string szName;
        public string szAssetName;
        public string szPath;
        public UnityEngine.Object obj;
    }

    public class ShaderInfo : RsInfo{
        public ShaderInfo(Shader shader) 
            : base(shader) {
            this.shader = shader;
        }
        //public override void OnGUI() { }

        public Shader shader;
    }

    public class TextureInfo : RsInfo{
        public TextureInfo(Texture tex)
            : base(tex) {
            this.tex = tex;
            this.nWidth = tex.width;
            this.nHeight = tex.height;
        }
        //public override void OnGUI() { }

        public int nWidth;
        public int nHeight;
        public Texture tex;
    }

    public class MeshInfo : RsInfo {
        public MeshInfo(Mesh mesh)
            : base(mesh) {
            this.nVertex = mesh.vertexCount;
            this.nTriangle = mesh.triangles.Length / 3;
        }
        //public override void OnGUI() { }

        public int nVertex;
        public int nTriangle;
    }

    public class MaterialInfo : RsInfo {
        public MaterialInfo(Material mat)
            : base(mat) {
            this.mat = mat;

            if (mat.shader != null) {
                this.shaderInfo = ShaderPool.Instance.Get(mat.shader);
                if (this.shaderInfo == null) {
                    this.shaderInfo = ShaderPool.Instance.Add(mat.shader);
                }
            }
            else {
                UnityEngine.Debug.LogWarningFormat("{0} lost shader", mat.name);
            }

            Dictionary<string, Texture> texProperty = RsUtil.GetTextureProperty(mat);

            arrayTexInfo = new TextureInfo[texProperty.Count];

            int nIdx = 0;
            foreach (var pair in texProperty) {
                string szPropertyName = pair.Key;
                Texture tex = pair.Value;
                if (tex == null) {
                    Debug.LogWarningFormat("{0}' {1} texture is null", szName, szPropertyName);
                    continue;
                }

                arrayTexInfo[nIdx] = TexturePool.Instance.Get(tex);
                if (arrayTexInfo[nIdx] == null) {
                    arrayTexInfo[nIdx] = TexturePool.Instance.Add(tex);
                }
                nIdx++;
            }
        }
        //public override void OnGUI() { }
        public ShaderInfo GetShaderInfo() {
            return shaderInfo;
        }

        public TextureInfo[] GetTextureInfo() {
            return arrayTexInfo;
        }

        public ShaderInfo shaderInfo;
        public TextureInfo[] arrayTexInfo;
        public Material mat;
    }


    public class ModelInfo : RsInfo {
        public ModelInfo(GameObject gameobject)
            : base(gameobject) {
            this.gameobject = gameobject;

            PickUpMats();
            PickUpMesh();
            PickUpSkinMesh();
        }
        //public override void OnGUI() { }

        private void PickUpSkinMesh() {
            SkinnedMeshRenderer skinMeshR = gameobject.GetComponent<SkinnedMeshRenderer>();
            if (skinMeshR == null) {
                return;
            }

            Mesh mesh = skinMeshR.sharedMesh;
            if (mesh != null) {
                if (meshInfo != null) {
                    UnityEngine.Debug.LogErrorFormat("{0} contains more than 1 mesh", gameobject.name);
                    return;
                }
                meshInfo = MeshPool.Instance.Get(mesh);
                if (meshInfo == null) {
                    meshInfo = MeshPool.Instance.Add(mesh);
                }
            }
            
            if (skinMeshR.sharedMaterials == null) {
                return;
            }

            if (arrayMatInfo != null) {
                UnityEngine.Debug.LogErrorFormat("{0} contains more than 1 renderer", gameobject.name);
                return;
            }

            arrayMatInfo = new MaterialInfo[skinMeshR.sharedMaterials.Length];

            for (int i = 0; i < arrayMatInfo.Length; ++i) {
                Material mat = skinMeshR.sharedMaterials[i];
                if (mat == null) {
                    continue;
                }
                arrayMatInfo[i] = MaterialPool.Instance.Get(mat);
                if (arrayMatInfo[i] == null) {
                    arrayMatInfo[i] = MaterialPool.Instance.Add(mat);
                }
            }

        }
        private void PickUpMesh() {
            MeshFilter mf = gameobject.GetComponent<MeshFilter>();
            if (mf == null) {
                return;
            }

            Mesh mesh = mf.sharedMesh;
            if (mesh == null) {
                return;
            }

            meshInfo = MeshPool.Instance.Get(mesh);
            if (meshInfo == null) {
                meshInfo = MeshPool.Instance.Add(mesh);
            }
        }

        private void PickUpMats() {
            Renderer render = gameobject.GetComponent<Renderer>();
            if (render == null) {
                return;
            }

            if (render.sharedMaterials == null || render.sharedMaterials.Length == 0) {
                Debug.LogWarningFormat("{0} mats is null", szName);
                return;
            }

            arrayMatInfo = new MaterialInfo[render.sharedMaterials.Length];

            for (int i = 0; i < arrayMatInfo.Length; ++i) {
                if (render.sharedMaterials[i] == null) {
                    continue;
                }

                arrayMatInfo[i] = MaterialPool.Instance.Get(render.sharedMaterials[i]);
                if (arrayMatInfo[i] == null) {
                    arrayMatInfo[i] = MaterialPool.Instance.Add(render.sharedMaterials[i]);
                }
            }
        }

        public MeshInfo GetMeshInfo() {
            return meshInfo;
        }

        public MaterialInfo[] GetMaterialInfo() {
            return arrayMatInfo;
        }

        public GameObject gameobject = null;
        public MeshInfo meshInfo = null;
        public MaterialInfo[] arrayMatInfo = null;
    }

    public class PrefabInstanceInfo : RsInfo {
        public PrefabInstanceInfo(GameObject gameobject)
            : base(gameobject) {
            this.gameobject = gameobject;

            arrayModelInfo = new ModelInfo[gameobject.transform.childCount + 1];

            arrayModelInfo[0] = new ModelInfo(gameobject);
            MeshInfo meshInfo = arrayModelInfo[0].GetMeshInfo();
            if (meshInfo != null) {
                nTotalVertex += meshInfo.nVertex;
                nTotalTriangle += meshInfo.nTriangle;
            }
            

            for (int i = 0; i < gameobject.transform.childCount; ++i) {
                GameObject child = gameobject.transform.GetChild(i).gameObject;
                
                ModelInfo modelInfo = new ModelInfo(child);
                arrayModelInfo[i + 1] = modelInfo;

                if (modelInfo.meshInfo != null) {
                    nTotalVertex += modelInfo.meshInfo.nVertex;
                    nTotalTriangle += modelInfo.meshInfo.nTriangle;
                }
            }
        }

        public ModelInfo[] GetModelInfo() {
            return arrayModelInfo;
        }

        public GameObject gameobject;
        public ModelInfo[] arrayModelInfo;
        //public bool bExpand = false;

        public int nTotalVertex = 0;
        public int nTotalTriangle = 0;
    }
}

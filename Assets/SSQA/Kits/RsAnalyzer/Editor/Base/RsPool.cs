using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SSQA {
    public class RsPool<TYPE> where TYPE : new() {
        private Dictionary<int, RsInfo> m_pool = new Dictionary<int, RsInfo>();
        private static TYPE ms_inst;
        public static TYPE Instance {
            get {
                if (ms_inst == null) {
                    ms_inst = new TYPE();
                }
                return ms_inst;
            }
        }

        protected RsPool() { }

        public bool Add(RsInfo rs) {
            int ID = rs.ID;
            if (!m_pool.ContainsKey(ID)) {
                m_pool.Add(ID, rs);
                return true;
            }
            return false;
        }

        public bool Del(RsInfo rs) {
            int ID = rs.ID;
            if (m_pool.ContainsKey(ID)) {
                m_pool.Remove(ID);
                return true;
            }
            return false;
        }

        public void Release() {
            m_pool.Clear();
        }

        public T GetRsByID<T>(int ID) where T : RsInfo{
            try {
                RsInfo rs = null;
                m_pool.TryGetValue(ID, out rs);
                return rs as T;
            }
            catch (Exception ex) {
                UnityEngine.Debug.LogException(ex);
            }
            return null;
        }

        public T GetRsByName<T>(string szName) where T : RsInfo {
            RsInfo rs = null;
            return rs as T;
        }
    }

    public class TexturePool : RsPool<TexturePool> {
        public TextureInfo Add(Texture tex) {
            TextureInfo texInfo = new TextureInfo(tex);
            base.Add(texInfo);
            return texInfo;
        }

        public TextureInfo Get(Texture tex) {
            int ID = RsUtil.GetUniqueID(tex);
            return GetRsByID<TextureInfo>(ID);
        }
    }

    public class ShaderPool : RsPool<ShaderPool> {
        public ShaderInfo Add(Shader shader) {
            ShaderInfo shaderInfo = new ShaderInfo(shader);
            base.Add(shaderInfo);
            return shaderInfo;
        }
        public ShaderInfo Get(Shader shader) {
            int ID = RsUtil.GetUniqueID(shader);
            return GetRsByID<ShaderInfo>(ID);
        }
    }

    public class MaterialPool : RsPool<MaterialPool> {
        public MaterialInfo Add(Material mat) {
            MaterialInfo matInfo = new MaterialInfo(mat);
            base.Add(matInfo);
            return matInfo;
        }

        public MaterialInfo Get(Material mat) {
            int ID = RsUtil.GetUniqueID(mat);
            return GetRsByID<MaterialInfo>(ID);
        }
    }

    public class MeshPool : RsPool<MeshPool> {
        public MeshInfo Add(Mesh mesh) {
            MeshInfo meshInfo = new MeshInfo(mesh);
            base.Add(meshInfo);
            return meshInfo;
        }

        public MeshInfo Get(Mesh mesh) {
            int ID = RsUtil.GetUniqueID(mesh);
            return GetRsByID<MeshInfo>(ID);
        }
    }
}

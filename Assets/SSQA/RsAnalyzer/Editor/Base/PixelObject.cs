using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SSQA {
    public class PixelObject {
        // 保存物体的原材质
        public Dictionary<Renderer, Material[]> matInfo = new Dictionary<Renderer, Material[]>();

        // 计算像素时的材质
        private Dictionary<Renderer, Material[]> matsPixel = new Dictionary<Renderer, Material[]>();

        // 渲染像素时的材质
        private Material matPixel = null;
        public Color32 renderColor;

        private int nOldLayer;
        public GameObject gameObject = null;
        public string name;

        // pixel info
        public int nVisiblePixel = 0;
        public int nRenderPixels = 0;
        public int nScreenPixels = 0;

        public int nVertex = 0;

        #region property
        #endregion

        public PixelObject(GameObject gameObject) {
            this.gameObject = gameObject;
            this.nOldLayer = gameObject.layer;
            this.name = gameObject.name;

            //matPixel   = RuntimeUtils.CreateBlendColorMaterial();
            matPixel = RsUtil.CreateSolidColorMaterial(this);

            _AnalyzeMat();
            _AnalyzeVert();
        }

        private void _AnalyzeVert() {
            ModelInfo modelInfo = new ModelInfo(gameObject);
            nVertex = modelInfo.meshInfo.nVertex;
        }

        private void _AnalyzeMat() {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer render in renderers) {
                matInfo[render] = render.sharedMaterials;
            }
        }

        public void ResetMat() {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();

            foreach (Renderer render in renderers) {
                render.sharedMaterials = matInfo[render];
            }
        }

        public void ResetLayer() {
            SetLayer(nOldLayer);
        }

        public void SetPixelMat() {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();

            foreach (Renderer render in renderers) {
                Material[] mats = null;
                matsPixel.TryGetValue(render, out mats);
                if (mats == null) {
                    if (render.sharedMaterials != null) {
                        mats = new Material[render.sharedMaterials.Length];
                        for (int i = 0; i < render.sharedMaterials.Length; ++i) {
                            mats[i] = matPixel;
                        }
                    }
                    if (mats != null) {
                        matsPixel.Add(render, mats);
                    }
                }
                else {
                    render.sharedMaterials = mats;
                }
            }

            foreach (Renderer render in renderers) {
                render.sharedMaterials = matsPixel[render];
            }
        }

        public void SetActive(bool bActive) {
            gameObject.SetActive(bActive);
        }

        public bool IsActive() {
            return gameObject.activeSelf;
        }

        public void SetLayer(int layer) {
            RsUtil.SetObjectLayer(gameObject, layer);
        }

        public int GetLayer() {
            return gameObject.layer;
        }
    }
}
    


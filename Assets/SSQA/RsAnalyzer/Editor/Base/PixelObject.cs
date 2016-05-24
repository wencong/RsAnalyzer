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
        private Material matPixel = null;

        // 计算遮挡时的透明材质
        private Dictionary<Renderer, Material[]> matsBlend = new Dictionary<Renderer, Material[]>();
        private Material matBlend = null;
        // 渲染像素时的材质
        
        public Color32 renderColor;

        private int nOldLayer;
        public GameObject gameObject = null;
        public string name;

        // pixel info
        public int nVisiblePixel = 0;
        public int nRenderPixels = 0;

        public int nVertex = 0;

        #region property
        public float pixelContribution {
            get {
                if (PixelCamera.sTotalScreenPixels == 0.0f) {
                    return 0.0f;
                }
                else {
                    return (float)nVisiblePixel / PixelCamera.sTotalScreenPixels;
                }
            }
        }

        public float modelComplex {
            get {
                if (nVisiblePixel == 0) {
                    return 1.0f;
                }
                else {
                    return (float)nVertex / nVisiblePixel;
                }
            }
        }

        #endregion

        public PixelObject(GameObject gameObject) {
            this.gameObject = gameObject;
            this.nOldLayer = gameObject.layer;
            this.name = gameObject.name;

            //matPixel   = RuntimeUtils.CreateBlendColorMaterial();
            renderColor = RsUtil.CreateSolidColor();
            matPixel = RsUtil.CreateSolidColorMaterial(renderColor);
            matBlend = RsUtil.CreateBlendColorMaterial(renderColor);

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
                if (mats!= null) {
                    render.sharedMaterials = mats;
                }
            }
        }

        public void SetBlendMat() {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer render in renderers) {
                Material[] mats = null;
                matsBlend.TryGetValue(render, out mats);
                if (mats == null) {
                    if (render.sharedMaterials != null) {
                        mats = new Material[render.sharedMaterials.Length];
                        for (int i = 0; i < render.sharedMaterials.Length; ++i) {
                            mats[i] = matBlend;
                        }
                    }
                    if (mats != null) {
                        matsBlend.Add(render, mats);
                    }
                }

                if (mats != null) {
                    render.sharedMaterials = mats;
                }
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
    


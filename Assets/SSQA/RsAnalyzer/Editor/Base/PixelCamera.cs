using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace SSQA {
    public class PixelCamera {
        private RenderTexture renderTexture = null;
        public Texture2D texRender = null;
        public Texture2D texPixels = null;
        private GameObject renderCamera = null;

        public static int sTotalScreenPixels = 0;

        #region private method
        private bool _CreateCamera(string layerName) {
            renderCamera = new GameObject("Camera");
            Camera cam = renderCamera.AddComponent<Camera>();
            cam.CopyFrom(Camera.main);
            cam.backgroundColor = new Color(1f, 1f, 1f, 0.5f);
            cam.clearFlags = CameraClearFlags.Color;
            cam.targetTexture = renderTexture;

            RsUtil.SetCameraCullingMask(cam, layerName);
            renderCamera.transform.parent = Camera.main.transform;
            //renderCamera.tag = layerName;
            renderCamera.layer = LayerMask.NameToLayer(layerName);

            return true;
        }

        private bool _DestoryCamera() {
            Debug.Log("destory camera");
            if (renderCamera != null) {
                Debug.Log("destory camera");
                GameObject.DestroyImmediate(renderCamera);
            }
            return true;
        }

        private void _ReadPixelsFromRenderTexture(Texture2D tex) {
            RenderTexture rtOld = RenderTexture.active;
            RenderTexture.active = renderCamera.GetComponent<Camera>().targetTexture;

            tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            tex.Apply();

            RenderTexture.active = rtOld;
        }

        /*
        private void _CalculatePixels(PixelObject pixelObject, Texture2D texRender, Texture2D texPixels) {
            Color32[] pixels = texRender.GetPixels32();

            pixelObject.nVisiblePixel = 0;

            pixels = texPixels.GetPixels32();

            Color32 objectColor = new Color32();

            for (int i = 0; i < pixels.Length; ++i) {
                if (pixels[i].a != 128 || pixels[i].r != 128 || pixels[i].g != 128 || pixels[i].b != 128) {
                    Color32 tmpColor = pixels[i];

                    if (!objectColor.Equals(tmpColor)) {
                        objectColor = tmpColor;
                        //Debug.Log(string.Format("{0} Color:{1}", pixelObject.gameObject.name, objectColor.ToString()));
                    }

                    pixelObject.nVisiblePixel++;
                }
            }
        }
         */ 
        #endregion

        #region public method
        public bool Init(string layer, int nWidth, int nHeight) {
            nHeight = Camera.main.pixelHeight;
            nWidth = Camera.main.pixelWidth;
            sTotalScreenPixels = nHeight * nWidth;

            renderTexture = new RenderTexture(nWidth, nHeight, 24);
            texRender = new Texture2D(nWidth, nHeight);
            texPixels = new Texture2D(nWidth, nHeight);
            _CreateCamera(layer);

            return true;
        }

        public bool UnInit() {
            //colorHashMap.Clear();
            _DestoryCamera();
            texRender = null;
            texPixels = null;
            renderTexture = null;

            return true;
        }

        public bool ResetClippingPlanes() {
            if (renderCamera == null || Camera.main == null) {
                return false;
            }

            Camera cam = renderCamera.GetComponent<Camera>();
            cam.nearClipPlane = Camera.main.nearClipPlane;
            cam.farClipPlane = Camera.main.farClipPlane;

            return true;
        }

        /*
        public bool Render(PixelObject pixelObject, bool bCalculate) {
            ResetClippingPlanes();
            RenderSettings.fog = false;

            int oldLayer = pixelObject.GetLayer();
            pixelObject.SetLayer(renderCamera.layer);

            renderTexture.DiscardContents();
            renderCamera.GetComponent<Camera>().Render();
            _ReadPixelsFromRenderTexture(texRender);

            renderTexture.DiscardContents();

            pixelObject.SetPixelMat();
            renderCamera.GetComponent<Camera>().Render();
            _ReadPixelsFromRenderTexture(texPixels);
            pixelObject.ResetMat();

            if (bCalculate) {
                _CalculatePixels(pixelObject, texRender, texPixels);
            }

            pixelObject.SetLayer(oldLayer);

            RenderSettings.fog = true;

            return true;
        }
        */

        public void CalculatePixelContribution(List<PixelObject> pixelObjects) {
            if (texPixels == null) {
                return;
            }

            Color32[] pixels = texPixels.GetPixels32();
            for (int nIndex = 0; nIndex < pixels.Length; ++nIndex) {
                Color32 color = pixels[nIndex];
                PixelObject po = null;
                po = RsUtil.GetPixelObject(color, pixelObjects);
                if (po != null) {
                    po.nVisiblePixel++;
                }
            }
        }

        public bool Render(List<PixelObject> pixelObjects) {
            ResetClippingPlanes();
            RenderSettings.fog = false;

            foreach (PixelObject pixelObject in pixelObjects) {
                pixelObject.SetLayer(renderCamera.layer);
                pixelObject.SetPixelMat();
            }

            renderTexture.DiscardContents();
            renderCamera.GetComponent<Camera>().Render();
            _ReadPixelsFromRenderTexture(texPixels);

            // blend mode
            foreach (PixelObject pixelObject in pixelObjects) {
                //pixelObject.SetLayer(renderCamera.layer);
                pixelObject.SetBlendMat();
            }

            renderTexture.DiscardContents();
            renderCamera.GetComponent<Camera>().Render();
            _ReadPixelsFromRenderTexture(texRender);


            foreach (PixelObject pixelObject in pixelObjects) {
                pixelObject.ResetMat();
                pixelObject.ResetLayer();
            }

            RenderSettings.fog = true;
            return true;
        }

        /*
        public bool Render(List<PixelObject> pixelObjects) {
            RenderSettings.fog = false;

            foreach (PixelObject pixelObject in pixelObjects) {
                pixelObject.SetLayer(renderCamera.layer);
            }

            renderTexture.DiscardContents();
            renderCamera.GetComponent<Camera>().Render();
            _ReadPixelsFromRenderTexture(texRender);

            foreach (PixelObject pixelObject in pixelObjects) {
                pixelObject.SetPixelMat();
            }

            renderTexture.DiscardContents();
            renderCamera.GetComponent<Camera>().Render();
            _ReadPixelsFromRenderTexture(texPixels);

            foreach (PixelObject pixelObject in pixelObjects) {
                pixelObject.ResetMat();
                pixelObject.ResetLayer();
            }

            RenderSettings.fog = true;
            return true;
        }
        */

        #endregion

        #region property

        #endregion
    }
}

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

        private void _CalculatePixels(PixelsObject pixelObject, Texture2D texRender, Texture2D texPixels) {
            Color32[] pixels = texRender.GetPixels32();

            pixelObject.nVisiblePixel = 0;
            pixelObject.nScreenPixels = pixels.Length;

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
        #endregion

        #region public method
        public bool Init(string layer, int nWidth, int nHeight) {
            nWidth = Screen.width;
            nHeight = Screen.height;

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

        public bool Render(PixelsObject pixelObject, bool bCalculate) {
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

        public bool RenderAndCalculateEveryPixel(List<PixelsObject> pixelObjects) {
            //ResetClippingPlanes();

            RenderSettings.fog = false;

            foreach (PixelsObject pixelObject in pixelObjects) {
                pixelObject.SetLayer(renderCamera.layer);
                pixelObject.SetPixelMat();
            }

            renderTexture.DiscardContents();
            renderCamera.GetComponent<Camera>().Render();
            _ReadPixelsFromRenderTexture(texPixels);

            Color32[] pixels = texPixels.GetPixels32();
            for (int nIndex = 0; nIndex < pixels.Length; ++nIndex) {
                Color32 color = pixels[nIndex];
                //uint hashCode = RuntimeUtils.ConvertColor32ToInt(color);

                PixelsObject po = null;
                //RuntimeUtils.colorHashMap.TryGetValue(hashCode, out po);
                po = RsUtil.GetPixelObject(color, pixelObjects);
                if (po != null) {
                    po.nVisiblePixel++;
                }
            }

            foreach (PixelsObject pixelObject in pixelObjects) {
                pixelObject.nScreenPixels = pixels.Length;
                /*
                if (pixelObject.pixelsInfo.nVisiblePixels > pixelObject.pixelsInfo.nMaxVisiblePIxels) {
                    pixelObject.pixelsInfo.nMaxVisiblePIxels = pixelObject.pixelsInfo.nVisiblePixels;
                }

                pixelObject.pixelsInfo.nVisiblePixels = 0;
                */

                pixelObject.ResetMat();
                pixelObject.ResetLayer();
            }

            RenderSettings.fog = true;
            return true;
        }

        public bool Render(List<PixelsObject> pixelObjects) {
            RenderSettings.fog = false;

            foreach (PixelsObject pixelObject in pixelObjects) {
                pixelObject.SetLayer(renderCamera.layer);
            }

            renderTexture.DiscardContents();
            renderCamera.GetComponent<Camera>().Render();
            _ReadPixelsFromRenderTexture(texRender);

            foreach (PixelsObject pixelObject in pixelObjects) {
                pixelObject.SetPixelMat();
            }

            renderTexture.DiscardContents();
            renderCamera.GetComponent<Camera>().Render();
            _ReadPixelsFromRenderTexture(texPixels);

            foreach (PixelsObject pixelObject in pixelObjects) {
                pixelObject.ResetMat();
                pixelObject.ResetLayer();
            }

            RenderSettings.fog = true;
            return true;
        }

        #endregion

        #region property

        #endregion
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SSQA {
    public class PixelController {
        public enum RenderStatus
        {
            eSnapShotAll,
            eSnapShotSingle,
            eSnapShotNone
        }

        public enum RenderMode
        {
            eRenderInEditor,
            eRenderInDevice,
            eRenderNone,
        }

        private List<PixelsObject> pixelsObjects = new List<PixelsObject>();
        //private IComparer<PixelsObject>[] SortMethod = new IComparer<PixelsObject>[5] { new SortByP1(), new SortByP2(), new SortByP3(), new SortByP4(), new SortByP5() };

        private string layerName = "";
        private PixelCamera renderCamera = null;
        private RenderStatus renderStatus = RenderStatus.eSnapShotNone;
        private RenderMode renderMode = RenderMode.eRenderNone;

        private float fDeltTime = 0.0f;
        //private float fUpdateRate = 1.0f;  // 1秒刷新一次
        public int nSnapIndex = 0;
        private bool bInited = false;
        
#region property
        public List<PixelsObject> objects
        {
            get
            {
                return pixelsObjects;
            }
        }

        public Texture2D texPixels
        {
            get
            {
                return renderCamera.texPixels;
            }
        }
        public Texture2D texRender
        {
            get
            {
                return renderCamera.texRender;
            }
        }

#endregion

#region private method
        private void _AddPixelObject(GameObject go, bool bIsEffect = false)
        {
            /*
            Transform transform = go.transform;
            GameObject clone = GameObject.Instantiate(go, transform.position, transform.rotation) as GameObject;
            clone.transform.parent = go.transform;
            ShaderAnalyzerBase.SetObjectLayer(clone, layerName);
            */

            pixelsObjects.Add(new PixelsObject(go));
        }

        private bool _Contain(GameObject gameObject)
        {
            foreach (PixelsObject pixelObject in pixelsObjects)
            {
                if (pixelObject.gameObject == gameObject)
                    return true;
            }
            return false;
        }
#endregion 

        public PixelController(string layer)
        {
            layerName = layer;
            renderCamera = new PixelCamera();
        }

        public bool Init()
        {
            if (bInited)
            {
                Debug.Log("RenderController is alread Init");
                return false;
            }
            renderCamera.Init(layerName, 600, 400);
            bInited = true;
            return true;
        }

        public bool UnInit()
        {
            pixelsObjects.Clear();
            renderCamera.UnInit();
            bInited = false;

            return true;
        }

        public bool ClearObjecs()
        {
            pixelsObjects.Clear();
            return true;
        }

        public bool SetRenderMode(RenderMode mode)
        {
            renderMode = mode;
            return true;
        }

        public bool SetRenderStatus(RenderStatus status)
        {
            renderStatus = status;
            return true;
        }

        public void FindSceneModelSelect(UnityEngine.GameObject root, bool bClear = false) {
            if (bClear == true) {
                pixelsObjects.Clear();
                nSnapIndex = 0;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; ++i) {
                _AddPixelObject(renderers[i].gameObject);
            }

        }

        public List<PixelsObject> GetObjectInFrustum()
        {
            List<PixelsObject> retList = new List<PixelsObject>();

            Plane[] planesFrustum = GeometryUtility.CalculateFrustumPlanes(Camera.main);

            //for (Renderer renderer in pixelsObjects)
            for (int i = 0; i < pixelsObjects.Count; ++i)
            {
                PixelsObject po = pixelsObjects[i];

                if (RsUtil.IsObjectInFrustumEx(planesFrustum, po.gameObject))
                {
                    retList.Add(po);
                }
            }

            return retList;
        }

        public void Update(float dt)
        {
            if (renderCamera == null)
                return;

            if (renderMode == RenderMode.eRenderNone)
            {
                //Debug.LogError("Please set rendermode");
                return;
            }

            //Debug.Log("SA update");
            //Debug.Log(renderStatus.ToString());

            switch (renderStatus)
            {
                case RenderStatus.eSnapShotAll:
                    {
                        /*
                        if (EditorApplication.timeSinceStartup - fDeltTime < fUpdateRate) {
                            return;
                        }
                        */
                        fDeltTime = (float)EditorApplication.timeSinceStartup;

                        //Debug.Log("SA update all");

                        if (renderMode == RenderMode.eRenderInEditor)
                        {
                            //renderCamera.Render(pixelsObjects);
                            List<PixelsObject> renderlist = GetObjectInFrustum();
                            Debug.Log("RenderAndCalculateEveryPixel total : " + renderlist.Count);
                            renderCamera.RenderAndCalculateEveryPixel(renderlist);
                        }
                        else if (renderMode == RenderMode.eRenderInDevice)
                        {
                            // todo
                        }
                        renderStatus = RenderStatus.eSnapShotNone;
                        break;
                    }

                case RenderStatus.eSnapShotSingle:
                    {
                        //Debug.Log("SA update single");
                        if (nSnapIndex == pixelsObjects.Count)
                        {
                            //renderCamera.RenderAndCalculateEveryPixel(pixelsObjects);

                            nSnapIndex = 0;
                            renderStatus = RenderStatus.eSnapShotNone;

                            if (RenderMode.eRenderInEditor == renderMode)
                            {
                                renderStatus = RenderStatus.eSnapShotAll;
                            }
                            return;
                        }
                        PixelsObject pixelObject = pixelsObjects[nSnapIndex];
                        renderCamera.Render(pixelObject, true);
                        nSnapIndex++;

                        break;
                    }
            }
        }
    }
}

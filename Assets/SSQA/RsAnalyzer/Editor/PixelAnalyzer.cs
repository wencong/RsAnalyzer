using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SSQA {
    public class PixelAnalyzer : IWinUnit {
        private string szLayerName = "pixel";

        private PixelController controller = null;

        private string m_szSearchItem = string.Empty;

        private Vector2 m_scrollPos = Vector2.zero;


        private long _currentTimeInMilliseconds = 0;
        private long _tickNetLast = 0;
        private long _tickNetInterval = 2000;


        public void Start() {
            RsUtil.AddLayer(szLayerName);

            if (controller == null) {
                controller = new PixelController(szLayerName);
            }

            controller.Init();
            controller.SetRenderMode(PixelController.RenderMode.eRenderInEditor);
            controller.SetRenderStatus(PixelController.RenderStatus.eSnapShotAll);

            if (Selection.activeGameObject != null) {
                controller.FindSceneModelSelect(Selection.activeGameObject, true);
            }
        }

        public void Update() {
            _currentTimeInMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            if (_currentTimeInMilliseconds - _tickNetLast > _tickNetInterval) {
                if (controller != null) {
                    controller.SetRenderMode(PixelController.RenderMode.eRenderInEditor);
                    controller.SetRenderStatus(PixelController.RenderStatus.eSnapShotAll);
                    controller.Update(_tickNetInterval);
                }
                _tickNetLast = _currentTimeInMilliseconds;
            }
        }

        public void Refresh() {
            if (controller != null && Selection.activeGameObject != null) {
                controller.FindSceneModelSelect(Selection.activeGameObject, true);
            }
        }

        public void OnGUI() {
            if (controller == null) {
                return;
            }

            _DrawTexture();

            _DrawDataHead();

            _DrawSearchLabel();

            _DrawPixelData();

        }

        private void _DrawTexture() {
            /*
            if (cameraRenderPixels == null)
                return;
            */
            //cameraRenderPixels.Update(0.03f);

            Texture2D texRender = controller.texRender;
            Texture2D texPixels = controller.texPixels;

            int nTextWidth = 800;
            int nTextHeight = (int)(nTextWidth * texRender.height / texRender.width);

            GUILayout.BeginHorizontal();
            GUILayout.Box(texPixels, GUILayout.Width(nTextWidth), GUILayout.Height(nTextHeight));
            GUILayout.Box(texRender, GUILayout.Width(nTextWidth), GUILayout.Height(nTextHeight));
            GUILayout.EndHorizontal();
        }

        private void _DrawDataHead() {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Name", WinUnitConfig.sNameWidth);
                if (GUILayout.Button("像素数", WinUnitConfig.sButtonWidth)) {
                    controller.objects.Sort((e1, e2) => {
                        if (e1.nVisiblePixel > e2.nVisiblePixel) {
                            return -1;
                        }
                        else if (e1.nVisiblePixel < e2.nVisiblePixel) {
                            return 1;
                        }
                        return 0;
                    }
                    );
                }

                if (GUILayout.Button("顶点数", WinUnitConfig.sButtonWidth)) {
                    controller.objects.Sort((e1, e2) => {
                        if (e1.nVertex > e2.nVertex) {
                            return -1;
                        }
                        else if (e1.nVertex < e2.nVertex) {
                            return 1;
                        }
                        return 0;
                    }
                    );
                }

                if (GUILayout.Button("贡献率", WinUnitConfig.sButtonWidth)) {
                    controller.objects.Sort((e1, e2) => {
                        if (e1.pixelContribution > e2.pixelContribution) {
                            return -1;
                        }
                        else if (e1.pixelContribution < e2.pixelContribution) {
                            return 1;
                        }
                        return 0;
                    }
                    );
                }

                if (GUILayout.Button("复杂率", WinUnitConfig.sButtonWidth)) {
                    controller.objects.Sort((e1, e2) => {
                        if (e1.modelComplex > e2.modelComplex) {
                            return -1;
                        }
                        else if (e1.modelComplex < e2.modelComplex) {
                            return 1;
                        }
                        return 0;
                    }
                    );
                }
            }
            GUILayout.EndHorizontal();
        }

        private void _DrawSearchLabel() {
            GUILayout.BeginHorizontal();
            {
                m_szSearchItem = GUILayout.TextField(m_szSearchItem, WinUnitConfig.sNameWidth);
            }
            GUILayout.EndHorizontal();
        }

        private void _DrawPixelData() {
            List<PixelObject> objs = controller.objects;

            m_scrollPos = GUILayout.BeginScrollView(m_scrollPos);
            for (int i = 0; i < objs.Count; ++i) {
                PixelObject obj = objs[i];
                if (!obj.name.ToLower().Contains(m_szSearchItem.ToLower())) {
                    continue;
                }

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(obj.name, WinUnitConfig.sNameWidth)) {
                        Selection.activeObject = obj.gameObject;
                    }
                    GUILayout.Label(obj.nVisiblePixel.ToString(), WinUnitConfig.sButtonWidth);
                    GUILayout.Label(obj.nVertex.ToString(), WinUnitConfig.sButtonWidth);

                    GUILayout.Label(string.Format("{0:0.00}", obj.pixelContribution), WinUnitConfig.sButtonWidth);
                    GUILayout.Label(string.Format("{0:.00}", obj.modelComplex), WinUnitConfig.sButtonWidth);

                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }

        public void OnDisable() {
            if (controller != null) {
                controller.UnInit();
            }
        }

    }
}
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

        public void Start() {
            RsUtil.AddLayer(szLayerName);

            if (controller == null) {
                controller = new PixelController(szLayerName);
                controller.Init();
            }
            
            controller.SetRenderMode(PixelController.RenderMode.eRenderInEditor);
            controller.SetRenderStatus(PixelController.RenderStatus.eSnapShotAll);

            if (Selection.activeGameObject != null) {
                controller.FindSceneModelSelect(Selection.activeGameObject, true);
            }
        }

        public void Update() {
            if (controller == null) {
                return;
            }

            controller.Update(0.03f);
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
            int nWidthc = Screen.width;
            int nHeightc = Screen.height;
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
            //GUILayout.Box(texRender, GUILayout.Width(nTextWidth), GUILayout.Height(nTextHeight));
            GUILayout.Box(texPixels, GUILayout.Width(nTextWidth), GUILayout.Height(nTextHeight));
            GUILayout.EndHorizontal();
        }

        private void _DrawDataHead() {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Name", WinUnitConfig.sNameWidth);
                if (GUILayout.Button("像素数", WinUnitConfig.sButtonWidth)) {
                    controller.objects.Sort((e1, e2) => {
                        return 1;
                    }
                    );
                }

                if (GUILayout.Button("顶点数", WinUnitConfig.sButtonWidth)) {
                    controller.objects.Sort((e1, e2) => {
                        return 1;
                    }
                    );
                }

                if (GUILayout.Button("贡献率", WinUnitConfig.sButtonWidth)) {
                    controller.objects.Sort((e1, e2) => {
                        return 1;
                    }
                    );
                }

                if (GUILayout.Button("复杂率", WinUnitConfig.sButtonWidth)) {

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
                }
                GUILayout.EndHorizontal();
            }
        }

        public void OnDisable() {
            if (controller != null) {
                controller.UnInit();
            }
        }

    }
}
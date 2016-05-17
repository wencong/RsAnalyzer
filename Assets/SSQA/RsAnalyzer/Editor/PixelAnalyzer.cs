using UnityEngine;
using UnityEditor;
using System.Collections;

namespace SSQA {
    public class PixelAnalyzer : IWinUnit {

        private string szLayerName = "pixel";

        private PixelController controller = null;

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
            GUILayout.Box(texRender, GUILayout.Width(nTextWidth), GUILayout.Height(nTextHeight));
            GUILayout.Box(texPixels, GUILayout.Width(nTextWidth), GUILayout.Height(nTextHeight));
            GUILayout.EndHorizontal();

        }
        public void OnDisable() {
            if (controller != null) {
                controller.UnInit();
            }
        }

    }
}
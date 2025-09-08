using MGAlienLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using System.Windows.Forms;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace MGAEditor
{
    public class editorEntryPoint : ComponentBase
    {
        private Camera mainCam;

        public override void Awake()
        {
            base.Awake();
            Logger.Log("editorEntry Awake");

            //uiman.cam.clearFlags = Camera.eCameraClearFlag.SolidColor;
            //uiman.cam.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);

            var mainCamObj = hierarchyManager.CreateGameObject("mainCam");
            mainCam = mainCamObj.AddComponent<Camera>();
            //mainCam.useAsUI = true;
            //mainCam.uiRoot = this.transform;
            //mainCam.viewport = new Rectangle(0, 0, Screen.width, Screen.height);
            mainCam.nearClipPlane = 1;
            mainCam.farClipPlane = 10000;
            mainCam.cullingMask = LayerMask.GetMask("Default");
            mainCam.clearFlags = Camera.eCameraClearFlag.SolidColor;
            mainCam.backgroundColor = Color.CornflowerBlue;

            //bool quaterView = false;
            //if (quaterView)
            //{
            //    mainCam.transform.position = new Vector3(-1000, -1000, 1000);
            //    mainCam.transform.LookAt(Vector3.Zero, Vector3.Backward);
            //}
            //else
            //{
            //    mainCam.transform.position = new Vector3(0, 0, 1000);
            //    mainCam.transform.LookAt(Vector3.Zero, Vector3.Up);
            //    mainCam.transform.position = new Vector3(500, 500, 1000);
            //}
            mainCam.orthographic = false;
            //mainCam.orthographicSize = 500;//  Screen.height / 2;
            mainCam.fieldOfView = 45;
            mainCam.aspectRatio = (float)Screen.width / (float)Screen.height;
            mainCam.renderPriority = 1;

            //mainCam.transform.position = new Vector3(0, 0, 100);
            //mainCam.transform.LookAt(Vector3.Zero, Vector3.Up);
            mainCam.transform.position = new Vector3(0, 0, 1000);
            mainCam.transform.LookAt(Vector3.Zero, Vector3.Up);
            //mainCam.transform.position = new Vector3(500, 500, 1000);

            mainCam.orthographic = true;

            AddComponent<EditorFunctionality>();

            RefreshScreenSize();

        }

        public override void Update()
        {
            base.Update();
            if (inputManager.WasReleasedThisFrame(Keys.Space))
            {
                FormsUtility formsUtility = new FormsUtility();
                formsUtility.OpenFileExplorerSTA(game.config.rawAssetsRootPath);
            }

            if (Screen.screenSizeWasChangedThisFrame)
            {
                RefreshScreenSize();
            }

        }
        private void RefreshScreenSize()
        {
            mainCam.viewport = new Rectangle(0, 0, Screen.width, Screen.height);
            mainCam.aspectRatio = (float)Screen.width / (float)Screen.height;
        }

    }
}

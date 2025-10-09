
using Microsoft.Xna.Framework;
using MGAlienLib;

namespace Project1
{
    public class _2DDemoBase : DemoBase
    {
        protected Vector2 lastMousePos = Vector2.Zero;
        protected bool scrolling;


        public override void Awake()
        {
            base.Awake();

            // setup world camera
            {
                var camObj = hierarchyManager.CreateGameObject("cam", transform);
                cam = camObj.AddComponent<Camera>();
                cam.useAsUI = true;
                cam.uiRoot = this.transform;
                //cam.viewport = new Rectangle(0, 0, Screen.width, Screen.height);
                cam.nearClipPlane = 100;
                cam.farClipPlane = 10000;
                cam.cullingMask = LayerMask.GetMask("Default");
                cam.clearFlags = Camera.eCameraClearFlag.SolidColor;
                cam.backgroundColor = Color.CornflowerBlue;

                bool quaterView = false;
                if (quaterView)
                {
                    cam.transform.position = new Vector3(-1000, -1000, 1000);
                    cam.transform.LookAt(Vector3.Zero, Vector3.Backward);
                }
                else
                {
                    cam.transform.position = new Vector3(0, 0, 1000);
                    cam.transform.LookAt(Vector3.Zero, Vector3.Up);
                    cam.transform.position = new Vector3(500, 500, 1000);
                }
                cam.orthographic = true;
                cam.orthographicSize = 500;//  Screen.height / 2;
                cam.fieldOfView = 45;
                cam.aspectRatio = (float)Screen.width / (float)Screen.height;
                cam.renderPriority = 1;
            }


        }

        public override void Update()
        {
            base.Update();


            if (inputManager.WasPressedThisFrame(eMouseButton.Left))
            {
                var pickedUI = uiman.cam.UIRaycast(inputManager.GetMousePos());
                if (pickedUI != null)
                {
                    //logPanel.Log("picked " + pickedUI.gameObject.name);
                }
                else
                {
                    Vector2 mousePos = inputManager.GetMousePos();
                    lastMousePos = mousePos;
                    // Logger.Log(mousePos.ToString());
                    var worldPos = cam.UnprojectAtZ(mousePos, 0);
                    // Logger.Log(worldPos.ToString());
                    scrolling = true;
                }
            }

            if (inputManager.WasReleasedThisFrame(eMouseButton.Left))
            {
                scrolling = false;
            }

            if (scrolling)
            {
                float mul = 1;
                if (!cam.orthographic) mul = 1;

                var mousePos = inputManager.GetMousePos();
                var lastWorldPos = cam.UnprojectAtZ(lastMousePos, 0);
                var currentWorldPos = cam.UnprojectAtZ(mousePos, 0);
                var worldDelta = currentWorldPos - lastWorldPos;
                cam.transform.position -= worldDelta * mul;
                lastMousePos = mousePos;
            }

            if (inputManager.WasPressedThisFrame(eMouseButton.Middle))
            {
                cam.orthographic = !cam.orthographic;
            }


            float wheel = inputManager.GetMouseWheelDelta();
            if (Mathf.Abs(wheel) > 0)
            {
                if (cam.orthographic)
                {
                    double newSize = (cam.orthographicSize) * 1 + wheel * -0.2;
                    if (newSize < 10)
                    {
                        newSize = 10;
                    }
                    cam.orthographicSize = (float)newSize;
                }
                else
                {
                    cam.transform.position -= new Vector3(0, 0, wheel * 1f);
                }
            }

        }

    }
}

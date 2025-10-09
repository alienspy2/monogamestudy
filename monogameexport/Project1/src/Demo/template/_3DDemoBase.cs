using MGAlienLib;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1
{
    public class _3DDemoBase : DemoBase
    {
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
                cam.nearClipPlane = 0.1f;
                cam.farClipPlane = 100;
                cam.cullingMask = LayerMask.GetMask("Default");
                cam.clearFlags = Camera.eCameraClearFlag.SolidColor;
                cam.backgroundColor = Color.CornflowerBlue;

                cam.transform.position = new Vector3(-10, 10, 10);
                cam.transform.LookAt(Vector3.Zero, Vector3.Up);

                cam.orthographic = false;
                cam.fieldOfView = 45;
                cam.aspectRatio = (float)Screen.width / (float)Screen.height;
                cam.renderPriority = 1;
            }

            testEntry.Instance.editorFunction.sceneViewControl.SetTargetCamera(cam);
        }


        public override void Update()
        {
            base.Update();



        }

    }
}

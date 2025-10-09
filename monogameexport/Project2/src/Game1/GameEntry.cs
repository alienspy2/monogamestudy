using MGAlienLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Project2
{
    internal class GameEntry : ComponentBase
    {
        private const bool test_debugDraw = false;
        private const bool test_debugMesh = false;
        private const bool test_softwareCursor = false;
        private const bool test_loadDDS = false;
        private const bool test_mesh = true;

        private Camera cam;
        private SpriteRenderer cursor;

        private Random random = new Random();

        private List<GravityBall> balls;

        private EditorFunctionality editorFunction;

        private List<Vector3> test_oldMousePos = new List<Vector3>();

        DebugDraw.DebugMesh test_debugMeshData;
        GameObject test_meshObj;
        GameObject test_ddsImage;

        public override void Awake()
        {
            // setup world camera
            {
                var camObj = hierarchyManager.CreateGameObject("cam", transform);
                cam = camObj.AddComponent<Camera>();
                cam.useAsUI = false;
                cam.uiRoot = this.transform;
                //cam.viewport = new Rectangle(0, 0, Screen.width, Screen.height);
                cam.nearClipPlane = 0.1f;
                cam.farClipPlane = 1000;
                cam.cullingMask = LayerMask.GetMask("Default");
                cam.clearFlags = Camera.eCameraClearFlag.SolidColor;
                cam.backgroundColor = Color.CornflowerBlue;

                cam.transform.position = new Vector3(-5, 5, 10);
                //cam.transform.localRotation = Quaternion.CreateFromYawPitchRoll(0,-10,0);
                cam.transform.LookAt(new Vector3(0,.5f,0), Vector3.Up);
                //var ypr = cam.transform.rotation.ToEulerAngles();
                //cam.transform.rotation = new Vector3(-20,0,0).FromEulerAnglesToQuaternion();

                //var r = ypr.FromEulerAnglesToQuaternion();
                //var ypr2 = r.ToEulerAngles();

                cam.orthographic = false;
                //cam.orthographicSize = 4;

                cam.fieldOfView = 45;
                cam.aspectRatio = (float)Screen.width / (float)Screen.height;
                cam.renderPriority = 1;
            }

            // mouse cursor
            if (test_softwareCursor)
            {
                var cursorObj = CreateGameObject("cursor", transform);
                cursorObj.layer = LayerMask.NameToLayer("UI");
                cursor = cursorObj.AddComponent<SpriteRenderer>();
                var filename = assetManager.SearchRawFiles("ui cursor")[0];
                cursor.Load(filename);
                cursor.transform.position = new Vector3(500, 500, 2000);
                cursor.transform.localScale = Vector3.One * 50f;
                cursor.pivot = new Vector2(0.35f, 0.65f);
                game.IsMouseVisible = false;
            }

            balls = new List<GravityBall>();

            editorFunction = AddComponent<EditorFunctionality>();
            editorFunction.Hide();

            uiman.console.RegisterCommand("stat", (args) =>
            {
                uiman.console.Log($"FPS : {GameBase.Instance.performanceManager.fps} Drawcalls : {GameBase.Instance.performanceManager.drawcallCount}");
            });

            Test();
        }

        public override void Start()
        {
            base.Start();
            Logger.Log("GameEntry Start");

            editorFunction.sceneViewControl.SetTargetCamera(cam);
        }

        public override void Update()
        {
            if (Screen.screenSizeWasChangedThisFrame)
            {
                RefreshScreenSize();
            }

            if (test_softwareCursor)
            {
                var mousePos = inputManager.GetMousePos();
                cursor.transform.position = new Vector3(mousePos.X, Screen.height - mousePos.Y, cursor.transform.position.Z);
            }

            if (test_debugDraw)
            {
                var mousePos = inputManager.GetMousePos();
                var worldPos = cam.UnprojectAtZ(mousePos, 0);
                test_oldMousePos.Add(worldPos);

                if (test_oldMousePos.Count >= 2) DebugDraw.DrawLineStrip(test_oldMousePos, Color.Red);
                if (test_oldMousePos.Count > 20) test_oldMousePos.RemoveAt(0);
            }

            // 망가졌음
            //if (inputManager.WasPressedThisFrame(Keys.Space))
            //{
            //    var newBallObj = CreateGameObject("ball" + balls.Count, transform);
            //    newBallObj.layer = LayerMask.NameToLayer("Default");
            //    var newBall = newBallObj.AddComponent<GravityBall>();

            //    newBall.velocityX = (float)(random.NextDouble() * 2 - 1) * 50f;
            //    newBall.ball.color = new Color(
            //        (float)random.NextDouble(),
            //        (float)random.NextDouble(),
            //        (float)random.NextDouble()
            //        );

            //    balls.Add(newBall);
            //}

            // test serializing
            if (inputManager.WasPressedThisFrame(Keys.D1))
            {
                Logger.Log(Serializer.SerializePrefab(gameObject));
            }

            if (test_debugMesh)
            {
                DebugDraw.DrawMesh(test_debugMeshData);
            }

            if (editorFunction.sceneViewControl.Activated)
                return;

            if (test_mesh)
            {
                float yaw = Time.time;
                float pitch = 0;// Time.time* 1.1f;
                float roll = 0;// Time.time * 1.2f;

                test_meshObj.transform.localRotation = Quaternion.CreateFromYawPitchRoll(yaw,pitch,roll);
            }

        }

        private void RefreshScreenSize()
        {
            cam.viewport = new Rectangle(0, 0, Screen.width, Screen.height);
            cam.aspectRatio = (float)Screen.width / (float)Screen.height;
        }


        private void Test()
        {

            if (test_loadDDS)
            {
                test_ddsImage = CreateGameObject("image", transform);
                test_ddsImage.layer = LayerMask.NameToLayer("Default");
                test_ddsImage.transform.position = new Vector3(2, 0, 0);
                test_ddsImage.transform.localScale = Vector3.One * 1f;
                var imgRenderer = test_ddsImage.AddComponent<SpriteRenderer>();
                imgRenderer.Load("raw://test/ball.dds");
                imgRenderer.UseAsWorldSpace(billboardMode: true);
            }

            if (test_debugMesh)
            {
                test_debugMeshData = MGAGLTFUtil.Load("test/box.glb");
            }

            if (test_mesh)
            {
                test_meshObj = CreateAndLoadMesh("raw://test/tank.glb", Vector3.Right * -2);
                CreateAndLoadMesh("raw://test/buggycar.glb", Vector3.Right * 2);
                CreateAndLoadMesh("raw://test/bomber.glb", Vector3.Right * 4);
                CreateAndLoadMesh("raw://test/cannonTurret.glb", Vector3.Right * 6);
                var island = CreateAndLoadMesh("raw://test/island.glb", Vector3.Right * 8);
                island.transform.position = new Vector3(8, 13, -72);
                island.transform.scale = new Vector3(200, 200, 200);
                island.transform.IsSelectableInSceneView = false;
            }
        }

        private GameObject CreateAndLoadMesh(string address, Vector3 position)
        {
            var obj = CreateGameObject("mesh", transform);
            var start = address.LastIndexOf("/");
            var end = address.IndexOf(".");
            obj.name = address.Substring(start + 1, end - start - 1);
            obj.layer = LayerMask.NameToLayer("Default");
            obj.transform.position = position;
            obj.transform.localScale = Vector3.One * 1;
            var meshRenderer = obj.AddComponent<MeshRenderer>();
            meshRenderer.Load(address);
            meshRenderer.LoadMaterial("MG/3D/Lit");

            return obj;
        }
    }


}

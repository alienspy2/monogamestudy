using MGAlienLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project2
{
    internal class GameEntry : ComponentBase
    {
        private Camera cam;
        private SpriteRenderer cursor;

        private Random random = new Random();

        private List<GravityBall> balls;

        private EditorFunctionality editorFunction;

        private List<Vector3> test_oldMousePos = new List<Vector3>();

        public override void Awake()
        {
            // setup world camera
            {
                var camObj = hierarchyManager.CreateGameObject("cam", transform);
                cam = camObj.AddComponent<Camera>();
                cam.useAsUI = false;
                cam.uiRoot = this.transform;
                //cam.viewport = new Rectangle(0, 0, Screen.width, Screen.height);
                cam.nearClipPlane = 100;
                cam.farClipPlane = 10000;
                cam.cullingMask = LayerMask.GetMask("Default");
                cam.clearFlags = Camera.eCameraClearFlag.SolidColor;
                cam.backgroundColor = Color.CornflowerBlue;

                cam.transform.position = new Vector3(0, 0, 1000);
                cam.transform.LookAt(Vector3.Zero, Vector3.Up);
                cam.transform.position = new Vector3(0, 0, 1000);

                cam.orthographic = true;
                cam.orthographicSize = 4;//  Screen.height / 2;
                cam.fieldOfView = 45;
                cam.aspectRatio = (float)Screen.width / (float)Screen.height;
                cam.renderPriority = 1;
            }

            // mouse cursor
            bool cursorTest = true;
            if (cursorTest)
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
        }

        public override void Update()
        {
            if (Screen.screenSizeWasChangedThisFrame)
            {
                RefreshScreenSize();
            }

            if (cursor != null)
            {
                var mousePos = inputManager.GetMousePos();
                cursor.transform.position = new Vector3(mousePos.X, Screen.height - mousePos.Y, cursor.transform.position.Z);

                var worldPos = cam.Unproject(mousePos, 0);
                test_oldMousePos.Add(worldPos);

                if (test_oldMousePos.Count>=2) DebugDraw.DrawLineStrip(test_oldMousePos, Color.Red);
                if (test_oldMousePos.Count > 20) test_oldMousePos.RemoveAt(0);
            }

            if (inputManager.WasPressedThisFrame(Keys.Space))
            {
                var newBallObj = CreateGameObject("ball" + balls.Count, transform);
                newBallObj.layer = LayerMask.NameToLayer("Default");
                var newBall = newBallObj.AddComponent<GravityBall>();

                newBall.velocityX = (float)(random.NextDouble() * 2 - 1) * 50f;
                newBall.ball.color = new Color(
                    (float)random.NextDouble(),
                    (float)random.NextDouble(),
                    (float)random.NextDouble()
                    );

                balls.Add(newBall);
            }

            // test serializing
            if (inputManager.WasPressedThisFrame(Keys.D1))
            {
                Logger.Log(Serializer.SerializePrefab(gameObject));
            }

            DebugDraw.DrawMesh(mesh);
        }

        private void RefreshScreenSize()
        {
            cam.viewport = new Rectangle(0, 0, Screen.width, Screen.height);
            cam.aspectRatio = (float)Screen.width / (float)Screen.height;
        }

        DebugDraw.DebugMesh mesh;

        private void Test()
        {
            //var image = CreateGameObject("image", transform);
            //image.layer = LayerMask.NameToLayer("Default");
            //image.transform.position = new Vector3(0, 0, 0);
            //image.transform.localScale = Vector3.One * 1f;
            //var imgRenderer = image.AddComponent<SpriteRenderer>();
            //imgRenderer.Load("raw://art/etc/hello.png");

            mesh = MGAGLTFUtil.Load("test/box.glb");
        }
    }
}

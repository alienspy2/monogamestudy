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

        public override void Awake()
        {
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

                cam.transform.position = new Vector3(0, 0, 1000);
                cam.transform.LookAt(Vector3.Zero, Vector3.Up);
                cam.transform.position = new Vector3(0, 0, 1000);

                cam.orthographic = true;
                cam.orthographicSize = 500;//  Screen.height / 2;
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
                var filename = assetManager.SearchFiles("ui cursor")[0];
                cursor.Load(filename);
                cursor.transform.position = new Vector3(500, 500, 1000-1);
                cursor.transform.localScale = Vector3.One * 50f;
                cursor.pivot = new Vector2(0.35f, 0.65f);
                game.IsMouseVisible = false;
                //var cursorSpr = cursorObj.AddComponent<SpriteRenderer>();
                //cursorSpr.sprite = new Sprite("art/UI/cursor.png");
                //cursorSpr.transform.position = new Vector3(0, 0, 0);
                //cursorSpr.transform.localScale = Vector3.One * 1f;
            }

            balls = new List<GravityBall>();
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
            }

            if (inputManager.WasPressedThisFrame(Keys.OemTilde))
            {
                uiman.ShowConsole(!uiman.IsConsoleVisible());
                uiman.console.RegisterCommand("stat", (args) =>
                {
                    uiman.console.Log($"FPS : {GameBase.Instance.performanceManager.fps} Drawcalls : {GameBase.Instance.performanceManager.drawcallCount}");
                });
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
        }

        private void RefreshScreenSize()
        {
            cam.viewport = new Rectangle(0, 0, Screen.width, Screen.height);
            cam.aspectRatio = (float)Screen.width / (float)Screen.height;
        }

    }
}

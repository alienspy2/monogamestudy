
using MGAlienLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Project1
{
    public class testEntry : ComponentBase
    {
        public static testEntry Instance;

        private const string defaultImage = "raw://art/UI/white.png";

        //public static readonly bool IsAddableFromInspector = true;

        private AutoAtlasSpriteRenderer cursor;
        private Coroutine corTest;

        //private AutoAtlasSpriteRendererDemo aaDemo;
        //private TweenerDemo tweenerDemo;
        //private _3DUIDemo _3DUIDemo;
        //private bepuPhysicsDemo physicsDemo;
        private TextRenderer statusText_1;
        private TextRenderer statusText_2;
        private TextRenderer statusText_3;
        [SerializeField] private List<UIButton> test_List = new();

        private DemoBase lastDemo;

        public EditorFunctionality editorFunction;


        public override void Awake()
        {
            Instance = this;

            bool deleteAllPng = false;
            if (deleteAllPng)
            {
                // 현재 폴더의 디버깅용 png파일을 모두 지운다
                var files = System.IO.Directory.GetFiles(".", "*.png");
                foreach (var file in files)
                {
                    System.IO.File.Delete(file);
                }
            }

            Logger.Log($"***** Hello ***** : {DateTime.Now.ToString("yyMMdd_HH:mm:ss")}");

            bool useDemoPanel = true;
            if (useDemoPanel)
            {
                var demoPanel = UIPanel.Build<UIPanel>(uiman.uiRoot,
                    "Demo",
                    new RectangleF(0, 0, 10, 10), 100,
                    contentBGTexAddress: defaultImage,
                    useTitleBar: true,
                    useResizer: false,
                    useVStacker: true,
                    useCloseButton: false,
                    useContentBgSlice: false,
                    useContextSizeFitter: true,
                    titleBarBGTexAddress: defaultImage,
                    titleTextColor: Color.White,
                    titleBarColor: Color.Black,
                    contentColor: Color.White);

                var vStacker = demoPanel.contentRoot.AddComponent<UIVStacker>();
                vStacker.SetMargins(10, 10, 10, 10, 10);

                statusText_1 = TextRenderer.BuildAsUI(demoPanel.contentRoot.transform,
                    "statusText", "notoKR", 16, "Hello", Color.Black,
                    new RectangleF(0, 0, 120, 20), 0.1f,
                    eHAlign.Center, eVAlign.Middle);

                statusText_2 = TextRenderer.BuildAsUI(demoPanel.contentRoot.transform,
                    "statusText", "notoKR", 16, "Hello", Color.Black,
                    new RectangleF(0, 0, 120, 20), 0.1f,
                    eHAlign.Center, eVAlign.Middle);

                statusText_3 = TextRenderer.BuildAsUI(demoPanel.contentRoot.transform,
                    "statusText", "notoKR", 16, "Hello", Color.Black,
                    new RectangleF(0, 0, 120, 20), 0.1f,
                    eHAlign.Center, eVAlign.Middle);

                var buttonColor = new Color(107, 199, 204, 255).Dimming(0.6f);

                var bt1 = UIButton.Build(demoPanel.contentRoot.transform,
                    "aademo",
                    defaultImage, true, false,
                    new RectangleF(0, 0, 120, 30), 0.1f,
                    (_) =>
                    {
                        if (lastDemo != null) Destroy(lastDemo.gameObject);

                        var obj = hierarchyManager.CreateGameObject("aademo", transform);
                        lastDemo = obj.AddComponent<AutoAtlasSpriteRendererDemo>();
                    },
                    text: "AA Sprite", textColor: Color.White,
                    color: buttonColor);

                var bt2 = UIButton.Build(demoPanel.contentRoot.transform,
                    "tweener",
                    defaultImage, true, false,
                    new RectangleF(0, 0, 120, 30), 0.1f,
                    (_) =>
                    {
                        if (lastDemo != null) Destroy(lastDemo.gameObject);

                        var obj = hierarchyManager.CreateGameObject("tweener", transform);
                        lastDemo = obj.AddComponent<TweenerDemo>();
                    },
                    text: "tweener", textColor: Color.White,
                    color: buttonColor);

                var bt3 = UIButton.Build(demoPanel.contentRoot.transform,
                    "3D UI",
                    defaultImage, true, false,
                    new RectangleF(0, 0, 120, 30), 0.1f,
                    (_) =>
                    {
                        if (lastDemo != null) Destroy(lastDemo.gameObject);

                        var obj = hierarchyManager.CreateGameObject("3D UI", transform);
                        lastDemo = obj.AddComponent<_3DUIDemo>();
                    },
                    text: "3D UI", textColor: Color.White,
                    color: buttonColor);

                var bt4 = UIButton.Build(demoPanel.contentRoot.transform,
                    "bepu physics",
                    defaultImage, true, false,
                    new RectangleF(0, 0, 120, 30), 0.1f,
                    (_) =>
                    {
                        if (lastDemo != null) Destroy(lastDemo.gameObject);

                        var obj = hierarchyManager.CreateGameObject("physics", transform);
                        lastDemo = obj.AddComponent<bepuPhysicsDemo>();
                    },
                    text: "bepu physcis", textColor: Color.White,
                    color: buttonColor);
            }

            // mouse cursor
            bool cursorTest = true;
            if (cursorTest)
            {
                var cursorObj = CreateGameObject("cursor", transform);
                cursorObj.layer = LayerMask.NameToLayer("UI");
                cursor = cursorObj.AddComponent<AutoAtlasSpriteRenderer>();
                var filename = assetManager.SearchRawFiles("ui cursor")[0];
                cursor.Load(filename, false);
                cursor.transform.position = new Vector3(500, 500, 1000);
                cursor.transform.localScale = Vector3.One * 50f;
                cursor.pivot = new Vector2(0.35f, 0.65f);
                game.IsMouseVisible = false;
                //var cursorSpr = cursorObj.AddComponent<SpriteRenderer>();
                //cursorSpr.sprite = new Sprite("art/UI/cursor.png");
                //cursorSpr.transform.position = new Vector3(0, 0, 0);
                //cursorSpr.transform.localScale = Vector3.One * 1f;
            }

            console.RegisterCommand("startcor", (_) => corTest = StartCoroutine(CorTest()));
            console.RegisterCommand("stopcor", (_) => StopCoroutine(corTest));
            console.RegisterCommand("perf", (_) => Logger.Log(GameBase.Instance.performanceManager.ToString()));

            console.RegisterCommand("startmicro", (_) =>
            {
                float lifespan = float.Parse(_);
                StartMicroRoutine((dt, data) =>
                {
                    float remainTime = (float)data;
                    remainTime -= dt;
                    Logger.Log($"micro routine: {remainTime:F2}");
                    return (remainTime > 0f, remainTime);
                }, lifespan);
            });

            //console.RegisterCommand("avalonia", (_) =>
            //{
            //    AvaloniaBootstrap.Init();

            //    // Show file dialog from a window
            //    //var mainWindow = new Window(); // Your actual main window
            //    string? selectedFile = FileDialogService.ShowOpenFileDialog(null).Result;
            //    Console.WriteLine(selectedFile ?? "No file selected");

            //});

            console.RegisterCommand("gtk", (_) =>
            {
                //var path = GameBase.Instance.config.rawAssetsRootPath;
                //Logger.Log(FileDialogHelper.ShowSaveFileDialog("hello", path));

                var color = GtkUtility.ShowColorPickerDialog("hello", Color.Red);
                Logger.Log($"color: {color}");
            });

            editorFunction = AddComponent<EditorFunctionality>();
            editorFunction.Hide();

        }


        public override void Update()
        {
            base.Update();

            if (cursor != null)
            {
                var mousePos = inputManager.GetMousePos();
                cursor.transform.position = new Vector3(mousePos.X, Screen.height - mousePos.Y, cursor.transform.position.Z);
            }

            statusText_1.text = $"FPS : {GameBase.Instance.performanceManager.fps}";
            statusText_2.text = $"Drawcalls : {GameBase.Instance.performanceManager.drawcallCount}";
            statusText_3.text = $"Verts : {GameBase.Instance.performanceManager.verticesCount}";
        }


        private IEnumerator CorTest()
        {
            var waiter = new WaitForSeconds(1f);
            for (int i = 0; i < 10000; i++)
            {
                Logger.Log($"cor_test {i}");

                yield return waiter;
            }
            yield break;
        }

    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Tomlyn;

namespace MGAlienLib
{
    /// <summary>
    /// Base class for game
    /// </summary>
    public class GameBase : Game
    {
        public static GameBase Instance { get; private set; }

        #region graphics device
        protected GraphicsDeviceManager _graphics;
        protected Vector2 baseScreenSize = new Vector2(1280, 720);
        public int backbufferWidth { get; private set; }
        public int backbufferHeight { get; private set; }
        public bool screenSizeChangedThisFrame { get; private set; } = true;
        #endregion

        #region time
        private DateTime lastUpdateTick = DateTime.Now;
        public float deltaTime;
        public float time = 0;
        public int frameCount = 0;
        #endregion

        public bool exiting { get; private set; } = false;

        // managers
        private List<ManagerBase> managers = new List<ManagerBase>();

        public InputManager inputManager;
        public FontManager fontManager;
        public HierarchyManager hierarchyManager;
        public PerformanceManager performanceManager;
        public RenderQueue renderQueue;
        public AssetManager assetManager;
        public ShaderManager shaderManager;
        public LayerManager layerManager;
        public InternalRenderManager internalRenderManager;
        public DynamicTextureAtlasManager dynamicTextureAtlasManager;
        public BuiltinUIManager builtinUIManager;
        public SelectionManager selectionManager;

        #region config
        public class Config
        {
            public string rawAssetsRootPath { get; set; } = "C:\\playground\\study\\monogame\\Project1\\Project1\\Assets";
            public string packedAssetsRootPath { get; set; } = "C:\\playground\\study\\monogame\\Project1\\Project1\\PackedAssets";
            public int tagetFPS { get; set; } = 60;
            public int vSync { get; set; } = 1;
            public bool logToFile { get; set; } = true;
        }

        public Config config = new Config();
        #endregion

        #region default assets
        public class DefaultAssets
        {
            private SharedTexture.Reference _whiteTexture;
            private SharedTexture.Reference _blackTexture;
            private SharedTexture.Reference _redTexture;
            private SharedTexture.Reference _magentaTexture;
            private Shader _unlitShader;
            private Shader _ttsfontShader;
            public SpriteFont _debugFont;

            public SharedTexture.Reference whiteTexture => _whiteTexture.Clone();
            public SharedTexture.Reference blackTexture => _blackTexture.Clone();
            public SharedTexture.Reference redTexture => _redTexture.Clone();
            public SharedTexture.Reference magentaTexture => _magentaTexture.Clone();

            public Shader unlitShader => _unlitShader;
            public Shader ttsfontShader => _ttsfontShader;
            public SpriteFont debugFont => _debugFont;

            public DefaultAssets(GameBase owner)
            {
                // 텍스처 로드. 절대 해제하지 않는다
                _whiteTexture = SharedTexture.Get("mgcb://DefaultTex/white");
                _blackTexture = SharedTexture.Get("mgcb://DefaultTex/black");
                _redTexture = SharedTexture.Get("mgcb://DefaultTex/red");
                _magentaTexture = SharedTexture.Get("mgcb://DefaultTex/magenta");

                _unlitShader = owner.shaderManager.LoadShader("MG/unlit", "Effects/Unlit");
                _ttsfontShader = owner.shaderManager.LoadShader("MG/TTSFont", "Effects/TTSFont");
                _debugFont = owner.Content.Load<SpriteFont>("Fonts/debugFont");
            }
        }

        public DefaultAssets defaultAssets;
        #endregion

        public GameBase()
        {
            Instance = this;

            TryReadConfig();

            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferWidth = (int)baseScreenSize.X;
            _graphics.PreferredBackBufferHeight = (int)baseScreenSize.Y;
            _graphics.PreferredDepthStencilFormat = DepthFormat.Depth24; // 24비트 깊이 버퍼
            _graphics.SynchronizeWithVerticalRetrace = true;
            _graphics.ApplyChanges();

            Window.AllowUserResizing = true;

            managers.Add(layerManager = new LayerManager(this));
            managers.Add(inputManager = new InputManager(this));
            managers.Add(assetManager = new AssetManager(this));
            managers.Add(fontManager = new FontManager(this));
            managers.Add(hierarchyManager = new HierarchyManager(this));
            managers.Add(performanceManager = new PerformanceManager(this));
            managers.Add(renderQueue = new RenderQueue(this));
            managers.Add(shaderManager = new ShaderManager(this));
            managers.Add(internalRenderManager = new InternalRenderManager(this));
            managers.Add(dynamicTextureAtlasManager = new DynamicTextureAtlasManager(this));
            managers.Add(builtinUIManager = new BuiltinUIManager(this));
            managers.Add(selectionManager = new SelectionManager(this));

            hierarchyManager.Intenal_CreateRoot();

        }

        public new void Exit()
        {
            exiting = true;
            base.Exit();
        }

        protected sealed override void LoadContent()
        {
            defaultAssets = new DefaultAssets(this);

            foreach (var manager in managers)
            {
                manager.OnPreLoadContent();
            }

            OnLoadContent();

            foreach (var manager in managers)
            {
                manager.OnPostLoadContent();
            }
        }

        protected virtual void OnLoadContent()
        {
        }

        protected sealed override void Initialize()
        {
            base.Initialize();
            foreach (var manager in managers)
            {
                manager.OnPreInitialize();
            }

            OnInitialize();

            foreach (var manager in managers)
            {
                manager.OnPostInitialize();
            }
        }

        protected virtual void OnInitialize()
        {
        }

        protected sealed override void Update(GameTime gameTime)
        {
            deltaTime = (float)DateTime.Now.Subtract(lastUpdateTick).TotalSeconds;
            time += deltaTime;
            lastUpdateTick = DateTime.Now;
            frameCount++;

            //Confirm the screen has not been resized by the user
            if (backbufferHeight != GraphicsDevice.PresentationParameters.BackBufferHeight ||
                backbufferWidth != GraphicsDevice.PresentationParameters.BackBufferWidth)
            {
                ScalePresentationArea();
            }

            foreach (var manager in managers)
            {
                if (exiting) return;
                manager.OnPreUpdate();
            }

            if (exiting) return;
            OnUpdate(deltaTime);

            foreach (var manager in managers)
            {
                if (exiting) return;
                manager.OnPostUpdate();
            }

            if (exiting) return;
            _DrawFromUpdate();

            screenSizeChangedThisFrame = false;
        }

        protected virtual void OnUpdate(float dt)
        {

        }

        private void _DrawFromUpdate()
        {
            foreach (var manager in managers)
            {
                if (exiting) return;
                manager.OnPreDraw();
            }

            if (exiting) return;
            OnDraw(deltaTime);

            foreach (var manager in managers)
            {
                if (exiting) return;
                manager.OnPostDraw();
            }
        }

        protected virtual void OnDraw(float dt)
        {
        }

        public void ScalePresentationArea()
        {
            // Work out how much we need to scale our graphics to fill the screen
            backbufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            backbufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
            float horScaling = backbufferWidth / baseScreenSize.X;
            float verScaling = backbufferHeight / baseScreenSize.Y;
            Vector3 screenScalingFactor = new Vector3(horScaling, verScaling, 1);

            Logger.Log("Screen Size - Width[" + GraphicsDevice.PresentationParameters.BackBufferWidth + "] Height [" + GraphicsDevice.PresentationParameters.BackBufferHeight + "]");
            screenSizeChangedThisFrame = true;
        }

        protected override void OnExiting(object sender, ExitingEventArgs args)
        {
            base.OnExiting(sender, args);
            WriteConfig();
        }

        private void TryReadConfig()
        {
            try
            {
                // config.toml 파일 있으면 읽기
                var toml = System.IO.File.ReadAllText("config.toml");
                var _config = Toml.ToModel<Config>(toml);
                if (config != null) config = _config;
            }
            catch (Exception _)
            {
                WriteConfig();
            }

        }

        private void WriteConfig()
        {
            try
            {
                // config.toml 파일 저장
                var toml = Toml.FromModel(config);
                System.IO.File.WriteAllText("config.toml", toml);
            }
            catch (Exception _)
            {
                // nothing to do
            }
        }
    }
}
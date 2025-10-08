using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace MGAlienLib
{
    public class UIPanel : UISpacer
    {
        protected static readonly string TitleFontName = "notoKR";
        protected static readonly Vector2 MinSize = new Vector2(100, 100);
        protected static readonly int TitleHeight = 30;
        protected static readonly int TitleFontSize = 13;
        protected static readonly string BgTexAddress = "raw://art/UI/white.png";
        protected static readonly string ResizerTexAddress = "raw://art/UI/resize3.png";

        [SerializeField] protected bool _useTitleBar = false;
        [SerializeField] protected Color _titleBarColor = Color.DarkGray;
        [SerializeField] protected Color _titleTextColor = Color.Wheat;
        [SerializeField] protected bool _useResizer = false;
        [SerializeField] protected bool _useCloseButton = false;
        [SerializeField] protected bool _useContextSizeFitter = false;

        [SerializeField] protected UIButton _closeButton;
        [SerializeField] protected AutoAtlasSpriteRenderer _titleBG;
        [SerializeField] protected TextRenderer _titleText;
        [SerializeField] protected AutoAtlasSpriteRenderer _resizer;
        [SerializeField] protected UITransform _contentRoot;
        [SerializeField] protected AutoAtlasSpriteRenderer _contentBG;

        protected bool _moveHold { get; private set; } = false;
        protected bool _resizeHold { get; private set; } = false;
        private Vector2 _oldSize = Vector2.Zero;

        public TextRenderer titleText => _titleText;

        public UITransform contentRoot => _contentRoot;

        private RectangleF CalculatContentRectFromPanel()
        {
            var rect = UITransform.anchoredRect;
            rect.X = 0;
            rect.Y = 0;
            if (_useTitleBar) rect.Y += -TitleHeight;
            rect.Height -= TitleHeight;
            contentRoot.anchoredRect = rect;
            return rect;
        }

        private Vector2 CalculatePanelSizeFromContent()
        {
            var size = _contentRoot.size;

            if (_useTitleBar) 
            {
                size.Y += TitleHeight;
            }

            return size;
        }

        private void Repoistion()
        {
            float contentY = _useTitleBar ? -UIPanel.TitleHeight : 0;
            float contentHeight = _useTitleBar ? 
                UITransform.anchoredRect.Height - UIPanel.TitleHeight : 
                UITransform.anchoredRect.Height;

            _contentBG.UITransform.anchoredRect = new RectangleF(0, contentY, UITransform.anchoredRect.Width, contentHeight);
        }

        public bool useTitleBar
        {
            get => _useTitleBar;
            set 
            {
                if (_useTitleBar != value)
                {
                    _useTitleBar = value;
                    if (_useTitleBar && _titleBG == null)
                    {
                        var anchorAndPivot = Vector2.UnitY;

                        // title bar
                        var titleRect = new RectangleF(0, 0, UITransform.anchoredRect.Width, UIPanel.TitleHeight);
                        _titleBG = AutoAtlasSpriteRenderer.BuildAsUI<AutoAtlasSpriteRenderer>(transform,
                            name + "_titleBG",
                            UIPanel.BgTexAddress, true, false,
                            titleRect, 0.1f,
                            anchorAndPivot, anchorAndPivot,
                            _titleBarColor);
                        _titleBG.gameObject.layer = gameObject.layer;
                        _titleBG.sliceLeftMargin = 24;
                        _titleBG.sliceTopMargin = 0;
                        _titleBG.sliceRightMargin = 24;
                        _titleBG.sliceBottomMargin = 0;

                        // title text
                        titleRect.Y -= 2;
                        var titleText = "";
                        _titleText = TextRenderer.BuildAsUI(_titleBG.transform,
                            name + "_titleText", TitleFontName, TitleFontSize,
                            titleText, _titleTextColor,
                            titleRect, 0.1f,
                            eHAlign.Center, eVAlign.Middle);
                        _titleText.SetOutline(true);
                        _titleText.gameObject.layer = gameObject.layer;
                    }
                    else
                    {
                        Destroy(_titleBG.gameObject);
                        _titleBG.gameObject = null;
                    }

                    contentRoot.anchoredRect = CalculatContentRectFromPanel();
                    Repoistion();
                }
            }
        }

        public bool useResizer
        {
            get => _useResizer;
            set
            {
                if (_useResizer != value)
                {
                    _useResizer = value;
                    if (_useResizer)
                    {
                        var anchorAndPivot = Vector2.UnitY;
                        _resizer = AutoAtlasSpriteRenderer.BuildAsUI<AutoAtlasSpriteRenderer>(transform,
                            name + "_resizer",
                            UIPanel.ResizerTexAddress, false, false,
                            new Rectangle(0, 0, 32, 32), 1f,
                            new Vector2(1, 0), new Vector2(1, 0),
                            Color.White);
                        _resizer.gameObject.layer = gameObject.layer;
                    }
                    else
                    {
                        Destroy(_resizer.gameObject);
                        _resizer.gameObject = null;
                    }
                }
            }
        }

        public bool useCloseButton
        {
            get => _useCloseButton;
            set
            {
                if (_useCloseButton != value)
                {
                    _useCloseButton = value;
                    if (_useCloseButton)
                    {
                        // close button
                        _closeButton = UIButton.Build(_titleBG.transform,
                            name + "_closeBtn",
                            "raw://art/UI/close.png", false, false,
                            new RectangleF(0, 0, 30, 30), 0.1f,
                            anchor: Vector2.One, pivot: Vector2.One,
                            color: Color.White);
                        _closeButton.gameObject.layer = gameObject.layer;
                        _closeButton.UITransform.anchor = new Vector2(1, 1);
                        _closeButton.UITransform.pivot = new Vector2(1, 1);
                        _closeButton.OnUICommand = (e) => OnCloseButtonClicked();
                    }
                    else
                    {
                        _closeButton.OnUICommand = null;
                        Destroy(_closeButton.gameObject);
                        _closeButton.gameObject = null;
                    }
                }
            }
        }

        public AutoAtlasSpriteRenderer contentBG => _contentBG;

        public override void Start()
        {
            base.Start();
            InitPanel();
        }

        private void InitPanel()
        {

           
            if (_titleBG != null)
            {
                _titleBG.OnUIPointerUp -= OnTitleBGUIPointerUp;
                _titleBG.OnUIPointerDown -= OnTitleBGUIPointerDown;
            }

            if (_resizer != null)
            {
                _resizer.OnUIPointerUp -= OnResizerUIPointerUp;
                _resizer.OnUIPointerDown -= OnResizerUIPointerDown;
            }

            if (_useTitleBar)
            {
                _titleBG.OnUIPointerDown += OnTitleBGUIPointerDown;
                _titleBG.OnUIPointerUp += OnTitleBGUIPointerUp;
            }

            if (_useResizer)
            {
                _resizer.OnUIPointerDown += OnResizerUIPointerDown;
                _resizer.OnUIPointerUp += OnResizerUIPointerUp;
            }
        }

        private void CreateContentRoot()
        {
            // content root
            if (_contentBG == null)
            {
                string contentBGTexAddress = BgTexAddress;
                var anchorAndPivot = Vector2.UnitY;
                var contentColor = new Color(0, 0, 0, 1f);

                float contentY = _useTitleBar ? -UIPanel.TitleHeight : 0;
                float contentHeight = _useTitleBar ? UITransform.anchoredRect.Height - UIPanel.TitleHeight : UITransform.anchoredRect.Height;

                _contentBG = AutoAtlasSpriteRenderer.BuildAsUI<AutoAtlasSpriteRenderer>(transform,
                    name + "_contentBG",
                    contentBGTexAddress, true, false,
                    new RectangleF(0, contentY, UITransform.anchoredRect.Width, contentHeight), 0,
                    anchorAndPivot, anchorAndPivot,
                    color: contentColor);
                _contentBG.gameObject.layer = gameObject.layer;
                _contentRoot = _contentBG.UITransform;
            }

        }

        private void OnTitleBGUIPointerDown(UIRenderable _) => _moveHold = true;
        private void OnTitleBGUIPointerUp(UIRenderable _) => _moveHold = false;

        private void OnResizerUIPointerDown(UIRenderable _) => _resizeHold = true;
        private void OnResizerUIPointerUp(UIRenderable _) => _resizeHold = false;

        public override void OnEnable()
        {
            base.OnEnable();
            if (_contentRoot!= null)
            {
                OnContentSizeChanged((Vector2)_contentRoot.anchoredRect.Size);
            }
        }

        public override void PreUpdate()
        {
            base.PreUpdate();
            if (useAsUI)
            {
                if (_oldSize != UITransform.size)
                {
                    _oldSize = UITransform.size;
                    var contentRect = CalculatContentRectFromPanel();
                    OnContentSizeChanged(contentRect.Size);
                }
            }

        }

        public override void Update()
        {
            // titleBG 를 잡고 이동
            if (_useTitleBar && _moveHold)
            {
                var delta = inputManager.GetMousePosDelta();
                delta.Y *= -1;
                var newRect = UITransform.anchoredRect;
                newRect.X += (int)delta.X;
                newRect.Y += (int)delta.Y;
                UITransform.anchoredRect = newRect;
            }

            // 로그 패널 크기 조절
            // resizer 를 잡고 이동
            if (_useResizer && _resizeHold)
            {
                var delta = inputManager.GetMousePosDelta();
                var newRect = UITransform.anchoredRect;
                newRect.Width += (int)delta.X;
                newRect.Height += (int)delta.Y;

                if (newRect.Width < MinSize.X) newRect.Width = MinSize.X;
                if (newRect.Height < MinSize.Y) newRect.Height = MinSize.Y;

                UITransform.anchoredRect = newRect;
                if (_titleBG != null)
                {
                    _titleBG.UITransform.anchoredRect = new RectangleF(0, 0, newRect.Width, TitleHeight);
                }
                _contentRoot.anchoredRect = new RectangleF(0, -TitleHeight, newRect.Width, newRect.Height - TitleHeight);

                OnContentSizeChanged((Vector2)_contentRoot.anchoredRect.Size);
            }

            if (_useTitleBar || _useResizer)
            {
                UITransform.ClampWindowToBounds(30);
            }

            if (_useContextSizeFitter)
            {
                OnContentSizeChanged(_contentRoot.size);
            }

        }

        protected virtual void OnContentSizeChanged(Vector2 newSize)
        {
            if (_useContextSizeFitter)
            {
                UITransform.size = CalculatePanelSizeFromContent();
            }
            else if (_useResizer)
            {
                contentRoot.anchoredRect = CalculatContentRectFromPanel();
            }

            if (_useTitleBar)
            {
                var titleRect = new RectangleF(0, 
                    0, 
                    UITransform.anchoredRect.Width, 
                    UIPanel.TitleHeight
                    );
                _titleBG.UITransform.anchoredRect = titleRect;
                _titleText.UITransform.anchoredRect = titleRect;
                _titleText.UITransform.anchor = new Vector2(.5f, .5f);
                _titleText.UITransform.pivot = new Vector2(.5f, .5f);
            }
        }

        protected virtual void OnCloseButtonClicked()
        {
            // Logger.Log($"{name} : Close button clicked");
        }

        public override void FinalizeDeserialize(DeserializeContext context)
        {
            base.FinalizeDeserialize(context);
            InitPanel();
        }

        public static T Build<T>(Transform parent,
            string name,
            RectangleF anchoredRect, float elevation,
            bool useTitleBar = true,
            bool useResizer = true,
            bool useVStacker = true,
            bool useCloseButton = true,
            bool useContextSizeFitter = false,
            string? bgTexAddress = null, bool dialate = false,
            string? titleBarBGTexAddress = null,
            Color? titleBarColor = null, string? titleText = null, Color? titleTextColor = null,
            string? contentBGTexAddress = null,
            bool useContentBgSlice = false,
            Color? contentColor = null,
            string layer = "UI") where T : UIPanel
        {
            titleBarBGTexAddress ??= UIPanel.BgTexAddress;
            bgTexAddress ??= UIPanel.BgTexAddress;
            contentBGTexAddress ??= UIPanel.BgTexAddress;
            titleBarColor ??= Color.White;
            contentColor ??= new Color(0, 0, 0, 1f);
            titleTextColor ??= new Color(titleBarColor.Value.R / 4,
                                    titleBarColor.Value.G / 4,
                                    titleBarColor.Value.B / 4, 255);
            titleText ??= name;

            var anchorAndPivot = Vector2.UnitY;
            var panel = UISpacer.Build<T>(parent, anchoredRect, elevation, layer: layer);
            panel.name = name;

            panel.useAsUI = true;
            panel.CreateContentRoot();

            panel.useTitleBar = useTitleBar;
            panel.useResizer = useResizer;
            panel.useCloseButton = useCloseButton;
            panel._useContextSizeFitter = useContextSizeFitter;
            panel.UITransform.size = new Vector2(anchoredRect.Width, anchoredRect.Height);

            if (panel._titleBG != null)
            {
                panel._titleBG.color = titleBarColor.Value;
            }

            if (panel.titleText != null)
            {
                panel.titleText.color = titleTextColor.Value;
                panel.titleText.text = titleText;
            }

            panel._contentBG.sliceLeftMargin = 24;
            panel._contentBG.sliceTopMargin = 1;
            panel._contentBG.sliceRightMargin = 24;
            panel._contentBG.sliceBottomMargin = 24;
            panel.contentBG.color = contentColor.Value;

            return panel;
        }

        public override void internal_Invalidate()
        {
            base.internal_Invalidate();
            OnContentSizeChanged(size);
        }

    }
}

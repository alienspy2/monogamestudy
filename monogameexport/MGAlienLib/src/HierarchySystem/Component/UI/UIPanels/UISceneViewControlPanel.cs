
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace MGAlienLib
{
    public class UISceneViewControlPanel : UIPanel
    {
        public enum eTooolMode
        {
            None,
            Translate,
            Rotate,
            Scale,
            External
        }

        private const string defaultImage = "raw://art/UI/white.png";
        private TextRenderer infoText;
        private eTooolMode toolMode = eTooolMode.Translate;
        private UIButton moveBtn;
        private UIButton rotateBtn;
        private UIButton scaleBtn;

        public void AfterBuild()
        {
            UITransform.size = new Vector2(300, 100);

            infoText = TextRenderer.BuildAsUI(contentRoot.transform, "Info Text",
                "notoKR", 10, "info",
                Color.White,
                new RectangleF(0, 0, 300, 10),
                hAlign: eHAlign.Center,
                vAlign: eVAlign.Top
                );
            infoText.UITransform.pivot = new Vector2(0, 1);
            infoText.UITransform.anchor = new Vector2(0, 1);
            infoText.UITransform.offset = new Vector2(5, -5);


            moveBtn = UIButton.Build(contentRoot.transform,
                "Move Tool Button",
                "raw://EditorResources/icons/moveTool.png", false, false,
                new RectangleF(0, -24, 32, 32), 0.1f,
                onCommand: (_) => ChangeToolMode(eTooolMode.Translate),
                pivot: Vector2.UnitY, anchor: Vector2.UnitY);

            rotateBtn = UIButton.Build(contentRoot.transform,
                "Rotate Tool Button",
                "raw://EditorResources/icons/rotateTool.png", false, false,
                new RectangleF(32, -24, 32, 32), 0.1f,
                onCommand: (_) => ChangeToolMode(eTooolMode.Rotate),
                pivot: Vector2.UnitY, anchor: Vector2.UnitY);

            scaleBtn = UIButton.Build(contentRoot.transform,
                "Scale Tool Button",
                "raw://EditorResources/icons/scaleTool.png", false, false,
                new RectangleF(64, -24, 32, 32), 0.1f,
                onCommand: (_) => ChangeToolMode(eTooolMode.Scale),
                pivot: Vector2.UnitY, anchor: Vector2.UnitY);

            transform.hideInHierarchy = true;

            ChangeToolMode(eTooolMode.Translate);
        }

        private void ChangeToolMode(eTooolMode newMode)
        {
            toolMode = newMode;
            var selectedColor = Color.DarkOrange;
            var normalColor = Color.White;

            moveBtn.color = (toolMode == eTooolMode.Translate) ? selectedColor : normalColor;
            rotateBtn.color = (toolMode == eTooolMode.Rotate) ? selectedColor : normalColor;
            scaleBtn.color = (toolMode == eTooolMode.Scale) ? selectedColor : normalColor;
        }

        public override void Update()
        {
            base.Update();
            if (infoText != null)
            {
                string humanReadableVerticesCount = StringUtility.ToReadableSizeString(GameBase.Instance.performanceManager.verticesCount);
                infoText.text = $"FPS : {GameBase.Instance.performanceManager.fps}  "+
                    $"batch : {GameBase.Instance.performanceManager.drawcallCount}  "+
                    $"verts : {humanReadableVerticesCount}";
            }
        }

        public static UISceneViewControlPanel Build(Transform parent)
        {
            Color contentBGColor = Color.White.Dimming(0.1f);

            var sceneControlPanel = UIPanel.Build<UISceneViewControlPanel>(parent, "Scene view control panel",
                new RectangleF(1000, 0, 400, 100), BuiltinUIManager.SceneControlViewElevation,
                true, false, false, false, false,
                titleBarColor: Color.Black,
                titleTextColor: Color.White,
                contentColor: contentBGColor,
                layer: "UI");

            sceneControlPanel.AfterBuild();

            return sceneControlPanel;
        }
    }
    }

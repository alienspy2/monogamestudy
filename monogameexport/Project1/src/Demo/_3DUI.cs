
using Microsoft.Xna.Framework;
using System;
using MGAlienLib;

namespace Project1
{
    public class _3DUIDemo : _2DDemoBase
    {
        private UICanvas anchorTestCanvas;

        public override void Awake()
        {
            base.Awake();

            string layer = "Default";

            float alpha = 1f;

            var anchorTestCanvasObj = hierarchyManager.CreateGameObject("anchorTestCanvas", transform);
            anchorTestCanvas = anchorTestCanvasObj.AddComponent<UICanvas>();
            
            anchorTestCanvas.mode = eCanvasType.World;
            //anchorTestCanvas.mode = eCanvasType.Screen;
            var anchorTestFrame = AutoAtlasSpriteRenderer.BuildAsUI<AutoAtlasSpriteRenderer>(anchorTestCanvas.transform, "uiimage",
                "raw://art/UI/SimpleFrame.png", false, true,
                new Rectangle(100, 100, 500, 500), 0,
                layer: layer);
            anchorTestFrame.color = Color.Wheat * alpha;

            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                {
                    var uiimage2 = SpriteRenderer.BuildAsUI<UIImage>(anchorTestFrame.transform, 
                        $"uiimage2 {x},{y}", "raw://art/UI/SimpleFrame.png",
                        new Rectangle(0, 0, 100, 100), -0.1f,
                        new Vector2(x * .5f, y * .5f),
                        new Vector2(x * .5f, y * .5f),
                        layer: layer);
                    uiimage2.color = Color.Red * alpha;
                    uiimage2.OnUIPointerEnter += (_) =>
                    {
                        uiimage2.color = uiimage2.color * .5f;
                    };
                    uiimage2.OnUIPointerExit += (_) =>
                    {
                        uiimage2.color = uiimage2.color * 2f;
                    };
                    uiimage2.OnUICommand += (_) =>
                    {
                        if (uiimage2.color.R == 0)
                            uiimage2.color = Color.Red;
                        else
                            uiimage2.color = Color.Blue;
                        Logger.Log($"OnCommand: {uiimage2.name}");
                    };

                    var uitext2 = TextRenderer.BuildAsUI(uiimage2.transform,
                        $"uitext2 {x},{y}",
                        "notoKR", 16,
                        "Hello",
                        Color.White,// * alpha,
                        new Rectangle(0, 0, 100, 100), -0.1f,
                        x switch
                        {
                            0 => eHAlign.Left,
                            1 => eHAlign.Center,
                            2 => eHAlign.Right,
                            _ => throw new NotImplementedException(),
                        },
                        y switch
                        {
                            0 => eVAlign.Bottom,
                            2 => eVAlign.Top,
                            1 => eVAlign.Middle,
                            _ => throw new NotImplementedException(),
                        },
                        layer: layer);
                }
        }

        public override void Update()
        {
            base.Update();

            var r1 = Mathf.Sin(Time.time * 1f) * 30f.ToRadians();
            var r2 = Mathf.Cos(Time.time * 1f) * 30f.ToRadians();
            var r3 = Mathf.Sin(Time.time * 0.5f) * 30f.ToRadians();
            anchorTestCanvas.transform.localRotation = Quaternion.CreateFromYawPitchRoll(r1, r2, r3);

        }
    }
}

//using Microsoft.Xna.Framework;
//using MonoGame.Extended;
//using System;
//using System.Collections.Generic;

//namespace MGAlienLib.Utility
//{
//    /// <summary>
//    /// UI 를 생성하는 유틸리티 클래스
//    /// </summary>
//    public static class UIBuilder
//    {
//        private static HierarchyManager hierarchyManager => GameBase.Instance.hierarchyManager;
//        private static AssetManager assetManager => GameBase.Instance.assetManager;

//        //public static UITransform BuildUISpacer(Transform parent,
//        //    RectangleF anchoredRect,
//        //    Vector2? pivot = null, Vector2? anchor = null,
//        //    string layer = "UI")
//        //{
//        //    var uiimageObj = hierarchyManager.CreateGameObject("*spacer*", parent);
//        //    uiimageObj.layer = LayerMask.NameToLayer(layer);
//        //    var uitransform = uiimageObj.AddComponent<UITransform>();
//        //    uitransform.elevation = 0;
//        //    uitransform.anchoredRect = anchoredRect;
//        //    uitransform.pivot = pivot.HasValue ? pivot.Value : Vector2.Zero;
//        //    uitransform.anchor = anchor.HasValue ? anchor.Value : Vector2.Zero;

//        //    return uitransform;
//        //}

//        /// <summary>
//        /// SpriteRenderer 를 생성합니다.
//        /// Button, Image 용도로 사용합니다.
//        /// </summary>
//        /// <param name="parent"></param>
//        /// <param name="name"></param>
//        /// <param name="textureAddress"></param>
//        /// <param name="anchoredRect"></param>
//        /// <param name="elevation"></param>
//        /// <param name="pivot"></param>
//        /// <param name="anchor"></param>
//        /// <param name="layer"></param>
//        /// <returns></returns>
//        //public static SpriteRenderer BuildUISprite(Transform parent,
//        //    string name,  
//        //    string textureAddress, 
//        //    RectangleF anchoredRect, float elevation, 
//        //    Vector2? pivot = null, Vector2? anchor = null, 
//        //    string layer = "UI")
//        //{
//        //    var uiimageObj = hierarchyManager.CreateGameObject(name, parent);
//        //    uiimageObj.layer = LayerMask.NameToLayer(layer);
//        //    var spr = uiimageObj.AddComponent<SpriteRenderer>();
//        //    spr.useAsUI = true;
//        //    spr.enableUIRaycast = true;
//        //    spr.sprite = new Sprite(textureAddress);
//        //    int w = (int)spr.sprite.width / 5;
//        //    int h = (int)spr.sprite.height / 5;
//        //    spr.sprite.size = new Vector2(1, 1);
//        //    spr.sprite.pivot = new Vector2(0, 0);

//        //    var uitransform = uiimageObj.GetComponent<UITransform>();
//        //    //uitransform.transform.localPosition = new Vector3(0, 0, z);
//        //    uitransform.elevation = elevation;
//        //    uitransform.anchoredRect = anchoredRect;
//        //    uitransform.pivot = pivot.HasValue ? pivot.Value : Vector2.Zero;
//        //    uitransform.anchor = anchor.HasValue ? anchor.Value : Vector2.Zero;

//        //    return spr;
//        //}

//        //public static T BuildUIAASprite<T>(Transform parent,
//        //    string name,
//        //    string textureAddress,
//        //    bool dialate,
//        //    RectangleF anchoredRect, float elevation,
//        //    Vector2? pivot = null, Vector2? anchor = null,
//        //    Color? color = null,
//        //    string layer = "UI") where T : AutoAtlasSpriteRenderer
//        //{
//        //    color ??= Color.White;
//        //    anchor ??= Vector2.UnitY;
//        //    pivot ??= Vector2.UnitY;

//        //    var obj = hierarchyManager.CreateGameObject(name, parent);
//        //    obj.layer = LayerMask.NameToLayer(layer);
//        //    var aaspr = obj.AddComponent<T>();
//        //    aaspr.useAsUI = true;
//        //    aaspr.enableUIRaycast = true;
//        //    aaspr.Load(textureAddress, dialate);
//        //    aaspr.size = new Vector2(1, 1);
//        //    aaspr.color = color.Value;

//        //    var uitransform = aaspr.GetComponent<UITransform>();
//        //    uitransform.elevation = elevation;
//        //    uitransform.anchoredRect = anchoredRect;
//        //    uitransform.pivot = pivot.Value;
//        //    uitransform.anchor = anchor.Value;

//        //    return aaspr;
//        //}

//        //public static AutoAtlasSpriteRenderer BuildButton(Transform parent,
//        //    string name,
//        //    string textureAddress,
//        //    bool dialate,
//        //    RectangleF anchoredRect, float z,
//        //    Action<Renderable> onCommand = null,
//        //    string? text = null,
//        //    Vector2? pivot = null, Vector2? anchor = null,
//        //    Color? color = null,
//        //    string layer = "UI")
//        //{
//        //    anchor = pivot = Vector2.UnitY;
//        //    var btn = AutoAtlasSpriteRenderer.BuildAsUI<AutoAtlasSpriteRenderer>(parent, 
//        //        name, 
//        //        textureAddress, dialate, 
//        //        anchoredRect, z, 
//        //        pivot, anchor, 
//        //        color, 
//        //        layer);
//        //    //var btn = BuildUISprite(parent, name, textureAddress, anchoredRect, z, pivot, anchor, layer);
//        //    btn.enableUIRaycast = true;
//        //    btn.OnUICommand += (R) => onCommand?.Invoke(R);

//        //    if (text != null) 
//        //    {
//        //        var textRdr = TextRenderer.BuildAsUI(btn.transform,
//        //            name + "_text", 
//        //            "todo", 16, 
//        //            text, Color.Black, 
//        //            anchoredRect, 0.1f,
//        //            eHAlign.Center, eVAlign.Middle,
//        //            useOutLine: false,
//        //            layer: layer);
//        //        textRdr.UITransform.expandWidthToParent = true;
//        //    }
           
//        //    return btn;
//        //}

//        ///// <summary>
//        ///// TextRenderer 를 생성합니다.
//        ///// </summary>
//        ///// <param name="parent"></param>
//        ///// <param name="name"></param>
//        ///// <param name="fontAddress"></param>
//        ///// <param name="fontSize"></param>
//        ///// <param name="text"></param>
//        ///// <param name="color"></param>
//        ///// <param name="anchoredRect"></param>
//        ///// <param name="elevation"></param>
//        ///// <param name="layer"></param>
//        ///// <returns></returns>
//        //public static TextRenderer BuildUIText(Transform parent, 
//        //    string name, 
//        //    string fontAddress,
//        //    int fontSize,
//        //    string text, 
//        //    Color color, 
//        //    RectangleF anchoredRect, float elevation = 0.1f,
//        //    eHAlign hAlign = eHAlign.Left,
//        //    eVAlign vAlign = eVAlign.Bottom,
//        //    //Vector2? pivot = null, 
//        //    //Vector2? anchor = null, 
//        //    string layer = "UI")
//        //{
//        //    var textObj = hierarchyManager.CreateGameObject(name, parent);
//        //    textObj.layer = LayerMask.NameToLayer(layer);
//        //    var textRenderer = textObj.AddComponent<TextRenderer>();
//        //    textRenderer.useAsUI = true;
//        //    textRenderer.text = text;
//        //    textRenderer.fontSize = fontSize;
//        //    textRenderer.color = color;

//        //    textRenderer.HAlign = hAlign;
//        //    textRenderer.VAlign = vAlign;
//        //    textRenderer.SetOutline(true);

//        //    var uitransform = textObj.GetComponent<UITransform>();
//        //    uitransform.elevation = elevation;
//        //    uitransform.anchoredRect = anchoredRect;
//        //    uitransform.pivot = uitransform.anchor = Vector2.Zero;
//        //    return textRenderer;
//        //}

//        /// <summary>
//        /// (WIP) UILogPanel 을 생성합니다.
//        /// </summary>
//        /// <param name="parent"></param>
//        /// <param name="name"></param>
//        /// <param name="anchoredRect"></param>
//        /// <param name="z"></param>
//        /// <param name="layer"></param>
//        /// <returns></returns>
//        //public static oldUILogPanel temp_BuildLogPanel(Transform parent,
//        //    string name,
//        //    RectangleF anchoredRect,
//        //    float z,
//        //    string layer = "UI")
//        //{
//        //    int fontSize = 16;
//        //    int lineHeight = fontSize * 160 / 100;
//        //    int titleHeight = 30;

//        //    var anchorAndPivot = Vector2.UnitY;

//        //    // frame panel
//        //    var frame = BuildUIAASprite<AutoAtlasSpriteRenderer>(parent, 
//        //        name, 
//        //        "art/UI/white.png",  true,
//        //        anchoredRect, z,
//        //        anchorAndPivot, anchorAndPivot, 
//        //        layer: layer);
//        //    frame.color = new Color(0,0,0,0.8f);

//        //    var uiLogPanel = frame.AddComponent<oldUILogPanel>();
//        //    uiLogPanel.frame = frame;
//        //    uiLogPanel.fontSize = fontSize;

//        //    // title bar
//        //    var titleRect = new RectangleF(0, 0, anchoredRect.Width, titleHeight);
//        //    var titleBG = BuildUIAASprite<AutoAtlasSpriteRenderer>(frame.transform,
//        //        "titleBG",
//        //        "art/UI/white.png", true,
//        //        titleRect, 0.1f,
//        //        anchorAndPivot, anchorAndPivot,
//        //        layer: layer);
//        //    titleBG.color = Color.Gray;
//        //    uiLogPanel.titleBG = titleBG;

//        //    uiLogPanel.titleText = BuildUIText(titleBG.transform,
//        //        name + "_title", "todo", 16,
//        //        "Log panel", Color.White,
//        //        titleRect, 0.2f,
//        //        eHAlign.Center, eVAlign.Middle,
//        //        layer: layer);
//        //    //titleText.color = new Color(titleBG.color.R / 4,
//        //    //                        titleBG.color.G / 4,
//        //    //                        titleBG.color.B / 4, 255);

//        //    // resizer
//        //    var resizer = BuildUIAASprite<AutoAtlasSpriteRenderer>(frame.transform,
//        //        "resizer",
//        //        "art/UI/resize.png", false,
//        //        new Rectangle(0, 0, 32, 32), 0.1f,
//        //        new Vector2(1,0), new Vector2(1,0),
//        //        layer: layer);
//        //    resizer.color = Color.White;
//        //    uiLogPanel.resizer = resizer;

//        //    // contents
//        //    var linesRoot = hierarchyManager.CreateGameObject("linesRoot", frame.transform);
//        //    var linesRootUIT = linesRoot.AddComponent<UITransform>();
//        //    linesRoot.layer = LayerMask.NameToLayer(layer);
//        //    linesRootUIT.pivot = linesRootUIT.anchor = Vector2.UnitY;
//        //    linesRootUIT.anchoredRect = new RectangleF(0, -titleHeight, anchoredRect.Width, anchoredRect.Height - titleHeight);
//        //    uiLogPanel.contentRoot = linesRootUIT;

//        //    int maxLines = (int)(linesRootUIT.anchoredRect.Height / lineHeight);

//        //    var vstacker = linesRoot.AddComponent<UIVStacker>();
//        //    vstacker.spacing = 0;
//        //    vstacker.autoSize = false;
//        //    vstacker.expandWidth = true;

//        //    // lines
//        //    //uiLogPanel.logLines = new List<TextRenderer>();

//        //    //for (int i = 0; i < maxLines; i++)
//        //    //{
//        //    //    var text = BuildUIText(linesRoot.transform,
//        //    //        name + "_text", "todo", fontSize,
//        //    //        "", Color.Wheat,
//        //    //        new RectangleF(0, 0, anchoredRect.Width, lineHeight),
//        //    //        z + 0.1f,
//        //    //        eHAlign.Left, eVAlign.Bottom,
//        //    //        layer);
//        //    //    uiLogPanel.logLines.Add(text);
//        //    //}

//        //    return uiLogPanel;
//        //}
//    }
//}

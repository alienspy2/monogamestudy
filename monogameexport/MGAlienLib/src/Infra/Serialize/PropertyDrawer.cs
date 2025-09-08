
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace MGAlienLib
{
    public static class PropertyDrawer
    {
        public static string PrettyFloatToString(float value, int precision = 4)
        {
            string format = "F" + precision;
            string result = value.ToString(format);
            if (result.EndsWith(".00"))
            {
                result = result.Substring(0, result.Length - 3);
            }
            else if (result.EndsWith(".0"))
            {
                result = result.Substring(0, result.Length - 2);
            }

            while (result.Contains('.') && result.EndsWith("0"))
            {
                result = result.TrimEnd('0');
            }

            if (result.EndsWith("."))
            {
                result = result.Substring(0, result.Length - 1);
            }

            if (result == "-0")
            {
                return "0";
            }

            return result;
        }

        public static UISpacer MakeVector2(Transform parent, Vector2 oldValue, Action<Vector2, bool> callback)
        {
            float parentW = parent.GetComponent<UITransform>().anchoredRect.Width;
            float parentH = parent.GetComponent<UITransform>().anchoredRect.Height;
            int dummyW = 10, dummyH = 10;

            var spacer = UISpacer.Build<UISpacer>(parent,
                new RectangleF(0, 0, dummyW, dummyH), 0.01f, true, true);
            spacer.UITransform.expandWidthToParent = true;
            spacer.UITransform.expandHeightToParent = true;

            spacer.AddComponent<UIHStacker>().SetMargins(0, 0, 0, 0, 20);

            var xInput = UIInputField.BuildAsUI<UIInputField>(spacer.transform, "xPropertyDrawer",
                "art/UI/white.png", true, false,
                new RectangleF(0, 0, parentW / 2f - 10, parentH), 0.01f);
            //xInput.UITransform.expandWidthToParent = true;
            //xInput.UITransform.expandHeightToParent = true;
            xInput.textOffset = new Vector2(8, 0);
            xInput.color = Color.White.Dimming(0.2f);
            xInput.activatedBGColor = Color.White.Dimming(0.4f);
            xInput.deactivatedBGColor = Color.White.Dimming(0.2f);
            xInput.text = PrettyFloatToString(oldValue.X);
            xInput.textRenderer.fontSize = UIInspectorPanel.DrawerFontSize;
            xInput.onEndEdit = (_if) =>
            {
                if (float.TryParse(_if.text, out var newValue))
                {
                    callback(new Vector2(newValue, oldValue.Y), true);
                    return;
                }
                callback(oldValue, false);
            };

            var yInput = UIInputField.BuildAsUI<UIInputField>(spacer.transform, "yPropertyDrawer",
                "art/UI/white.png", true, false,
                new RectangleF(0, 0, parentW / 2f - 10, parentH), 0.01f);
            //yInput.UITransform.expandWidthToParent = true;
            //yInput.UITransform.expandHeightToParent = true;
            yInput.textOffset = new Vector2(8, 0);
            yInput.color = Color.White.Dimming(0.2f);
            yInput.activatedBGColor = Color.White.Dimming(0.4f);
            yInput.deactivatedBGColor = Color.White.Dimming(0.2f);
            yInput.text = PrettyFloatToString(oldValue.Y);
            yInput.textRenderer.fontSize = UIInspectorPanel.DrawerFontSize;
            yInput.onEndEdit = (_if) =>
            {
                if (float.TryParse(_if.text, out var newValue))
                {
                    callback(new Vector2(oldValue.X, newValue), true);
                    return;
                }
                callback(oldValue, false);
            };

            return spacer;
        }

        public static UISpacer MakeVector3(Transform parent, Vector3 oldValue, Action<Vector3, bool> callback)
        {
            float parentW = parent.GetComponent<UITransform>().anchoredRect.Width;
            float parentH = parent.GetComponent<UITransform>().anchoredRect.Height;
            int dummyW = 10, dummyH = 10;

            var spacer = UISpacer.Build<UISpacer>(parent,
                new RectangleF(0, 0, dummyW, dummyH), 0.01f, true, true);
            spacer.UITransform.expandWidthToParent = true;
            spacer.UITransform.expandHeightToParent = true;

            spacer.AddComponent<UIHStacker>().SetMargins(0, 0, 0, 0, 10);

            float fieldWidth = (parentW - 10 * (3 - 1)) / 3f;

            // X Input
            var xInput = UIInputField.BuildAsUI<UIInputField>(spacer.transform, "xPropertyDrawer",
                "art/UI/white.png", true, false,
                new RectangleF(0, 0, fieldWidth, parentH), 0.01f);
            xInput.textOffset = new Vector2(8, 0);
            xInput.color = Color.White.Dimming(0.2f);
            xInput.activatedBGColor = Color.White.Dimming(0.4f);
            xInput.deactivatedBGColor = Color.White.Dimming(0.2f);
            xInput.text = PrettyFloatToString(oldValue.X);
            xInput.textRenderer.fontSize = UIInspectorPanel.DrawerFontSize;
            xInput.onEndEdit = (_if) =>
            {
                if (float.TryParse(_if.text, out var newValue))
                {
                    callback(new Vector3(newValue, oldValue.Y, oldValue.Z), true);
                    return;
                }
                callback(oldValue, false);
            };

            // Y Input
            var yInput = UIInputField.BuildAsUI<UIInputField>(spacer.transform, "yPropertyDrawer",
                "art/UI/white.png", true, false,
                new RectangleF(0, 0, fieldWidth, parentH), 0.01f);
            yInput.textOffset = new Vector2(8, 0);
            yInput.color = Color.White.Dimming(0.2f);
            yInput.activatedBGColor = Color.White.Dimming(0.4f);
            yInput.deactivatedBGColor = Color.White.Dimming(0.2f);
            yInput.text = PrettyFloatToString(oldValue.Y);
            yInput.textRenderer.fontSize = UIInspectorPanel.DrawerFontSize;
            yInput.onEndEdit = (_if) =>
            {
                if (float.TryParse(_if.text, out var newValue))
                {
                    callback(new Vector3(oldValue.X, newValue, oldValue.Z), true);
                    return;
                }
                callback(oldValue, false);
            };

            // Z Input
            var zInput = UIInputField.BuildAsUI<UIInputField>(spacer.transform, "zPropertyDrawer",
                "art/UI/white.png", true, false,
                new RectangleF(0, 0, fieldWidth, parentH), 0.01f);
            zInput.textOffset = new Vector2(8, 0);
            zInput.color = Color.White.Dimming(0.2f);
            zInput.activatedBGColor = Color.White.Dimming(0.4f);
            zInput.deactivatedBGColor = Color.White.Dimming(0.2f);
            zInput.text = PrettyFloatToString(oldValue.Z);
            zInput.textRenderer.fontSize = UIInspectorPanel.DrawerFontSize;
            zInput.onEndEdit = (_if) =>
            {
                if (float.TryParse(_if.text, out var newValue))
                {
                    callback(new Vector3(oldValue.X, oldValue.Y, newValue), true);
                    return;
                }
                callback(oldValue, false);
            };

            return spacer;
        }

        public static UISpacer MakeVector4(Transform parent, Vector4 oldValue, Action<Vector4, bool> callback)
        {
            float parentW = parent.GetComponent<UITransform>().anchoredRect.Width;
            float parentH = parent.GetComponent<UITransform>().anchoredRect.Height;
            int dummyW = 10, dummyH = 10;

            var spacer = UISpacer.Build<UISpacer>(parent,
                new RectangleF(0, 0, dummyW, dummyH), 0.01f, true, true);
            spacer.UITransform.expandWidthToParent = true;
            spacer.UITransform.expandHeightToParent = true;

            spacer.AddComponent<UIHStacker>().SetMargins(0, 0, 0, 0, 10);

            float fieldWidth = (parentW - 10 * (4 - 1)) / 4f;

            // X Input
            var xInput = UIInputField.BuildAsUI<UIInputField>(spacer.transform, "xPropertyDrawer",
                "art/UI/white.png", true, false,
                new RectangleF(0, 0, fieldWidth, parentH), 0.01f);
            xInput.textOffset = new Vector2(8, 0);
            xInput.color = Color.White.Dimming(0.2f);
            xInput.activatedBGColor = Color.White.Dimming(0.4f);
            xInput.deactivatedBGColor = Color.White.Dimming(0.2f);
            xInput.text = PrettyFloatToString(oldValue.X);
            xInput.textRenderer.fontSize = UIInspectorPanel.DrawerFontSize;
            xInput.onEndEdit = (_if) =>
            {
                if (float.TryParse(_if.text, out var newValue))
                {
                    callback(new Vector4(newValue, oldValue.Y, oldValue.Z, oldValue.W), true);
                    return;
                }
                callback(oldValue, false);
            };

            // Y Input
            var yInput = UIInputField.BuildAsUI<UIInputField>(spacer.transform, "yPropertyDrawer",
                "art/UI/white.png", true, false,
                new RectangleF(0, 0, fieldWidth, parentH), 0.01f);
            yInput.textOffset = new Vector2(8, 0);
            yInput.color = Color.White.Dimming(0.2f);
            yInput.activatedBGColor = Color.White.Dimming(0.4f);
            yInput.deactivatedBGColor = Color.White.Dimming(0.2f);
            yInput.text = PrettyFloatToString(oldValue.Y);
            yInput.textRenderer.fontSize = UIInspectorPanel.DrawerFontSize;
            yInput.onEndEdit = (_if) =>
            {
                if (float.TryParse(_if.text, out var newValue))
                {
                    callback(new Vector4(oldValue.X, newValue, oldValue.Z, oldValue.W), true);
                    return;
                }
                callback(oldValue, false);
            };

            // Z Input
            var zInput = UIInputField.BuildAsUI<UIInputField>(spacer.transform, "zPropertyDrawer",
                "art/UI/white.png", true, false,
                new RectangleF(0, 0, fieldWidth, parentH), 0.01f);
            zInput.textOffset = new Vector2(8, 0);
            zInput.color = Color.White.Dimming(0.2f);
            zInput.activatedBGColor = Color.White.Dimming(0.4f);
            zInput.deactivatedBGColor = Color.White.Dimming(0.2f);
            zInput.text = PrettyFloatToString(oldValue.Z);
            zInput.textRenderer.fontSize = UIInspectorPanel.DrawerFontSize;
            zInput.onEndEdit = (_if) =>
            {
                if (float.TryParse(_if.text, out var newValue))
                {
                    callback(new Vector4(oldValue.X, oldValue.Y, newValue, oldValue.W), true);
                    return;
                }
                callback(oldValue, false);
            };

            // W Input
            var wInput = UIInputField.BuildAsUI<UIInputField>(spacer.transform, "wPropertyDrawer",
                "art/UI/white.png", true, false,
                new RectangleF(0, 0, fieldWidth, parentH), 0.01f);
            wInput.textOffset = new Vector2(8, 0);
            wInput.color = Color.White.Dimming(0.2f);
            wInput.activatedBGColor = Color.White.Dimming(0.4f);
            wInput.deactivatedBGColor = Color.White.Dimming(0.2f);
            wInput.text = PrettyFloatToString(oldValue.W);
            wInput.textRenderer.fontSize = UIInspectorPanel.DrawerFontSize;
            wInput.onEndEdit = (_if) =>
            {
                if (float.TryParse(_if.text, out var newValue))
                {
                    callback(new Vector4(oldValue.X, oldValue.Y, oldValue.Z, newValue), true);
                    return;
                }
                callback(oldValue, false);
            };

            return spacer;
        }

        public static UISpacer MakeRectangle(Transform parent, Rectangle oldValue, Action<Rectangle, bool> callback)
        {
            float parentW = parent.GetComponent<UITransform>().anchoredRect.Width;
            float parentH = parent.GetComponent<UITransform>().anchoredRect.Height;
            int dummyW = 10, dummyH = 10;

            var spacer = UISpacer.Build<UISpacer>(parent,
                new RectangleF(0, 0, dummyW, dummyH), 0.01f, true, true);
            spacer.UITransform.expandWidthToParent = true;
            spacer.UITransform.expandHeightToParent = true;

            spacer.AddComponent<UIHStacker>().SetMargins(0, 0, 0, 0, 10);

            float fieldWidth = (parentW - 10 * (4 - 1)) / 4f;

            // x
            var xInput = UIInputField.BuildAsUI<UIInputField>(spacer.transform, "xPropertyDrawer",
                "art/UI/white.png", true, false,
                new RectangleF(0, 0, fieldWidth, parentH), 0.01f);
            xInput.textOffset = new Vector2(8, 0);
            xInput.color = Color.White.Dimming(0.2f);
            xInput.activatedBGColor = Color.White.Dimming(0.4f);
            xInput.deactivatedBGColor = Color.White.Dimming(0.2f);
            xInput.text = oldValue.X.ToString();
            xInput.textRenderer.fontSize = UIInspectorPanel.DrawerFontSize;
            xInput.onEndEdit = (_if) =>
            {
                if (int.TryParse(_if.text, out var newValue))
                {
                    callback(new Rectangle(newValue, oldValue.Y, oldValue.Width, oldValue.Height), true);
                    return;
                }
                callback(oldValue, false);
            };

            // y
            var yInput = UIInputField.BuildAsUI<UIInputField>(spacer.transform, "yPropertyDrawer",
                "art/UI/white.png", true, false,
                new RectangleF(0, 0, fieldWidth, parentH), 0.01f);
            yInput.textOffset = new Vector2(8, 0);
            yInput.color = Color.White.Dimming(0.2f);
            yInput.activatedBGColor = Color.White.Dimming(0.4f);
            yInput.deactivatedBGColor = Color.White.Dimming(0.2f);
            yInput.text = oldValue.Y.ToString();
            yInput.textRenderer.fontSize = UIInspectorPanel.DrawerFontSize;
            yInput.onEndEdit = (_if) =>
            {
                if (int.TryParse(_if.text, out var newValue))
                {
                    callback(new Rectangle(oldValue.X, newValue, oldValue.Width, oldValue.Height), true);
                    return;
                }
                callback(oldValue, false);
            };

            // width
            var widthInput = UIInputField.BuildAsUI<UIInputField>(spacer.transform, "widthPropertyDrawer",
                "art/UI/white.png", true, false,
                new RectangleF(0, 0, fieldWidth, parentH), 0.01f);
            widthInput.textOffset = new Vector2(8, 0);
            widthInput.color = Color.White.Dimming(0.2f);
            widthInput.activatedBGColor = Color.White.Dimming(0.4f);
            widthInput.deactivatedBGColor = Color.White.Dimming(0.2f);
            widthInput.text = oldValue.Width.ToString();
            widthInput.textRenderer.fontSize = UIInspectorPanel.DrawerFontSize;
            widthInput.onEndEdit = (_if) =>
            {
                if (int.TryParse(_if.text, out var newValue))
                {
                    callback(new Rectangle(oldValue.X, oldValue.Y, newValue, oldValue.Height), true);
                    return;
                }
                callback(oldValue, false);
            };

            // height
            var heightInput = UIInputField.BuildAsUI<UIInputField>(spacer.transform, "heightPropertyDrawer",
                "art/UI/white.png", true, false,
                new RectangleF(0, 0, fieldWidth, parentH), 0.01f);
            heightInput.textOffset = new Vector2(8, 0);
            heightInput.color = Color.White.Dimming(0.2f);
            heightInput.activatedBGColor = Color.White.Dimming(0.4f);
            heightInput.deactivatedBGColor = Color.White.Dimming(0.2f);
            heightInput.text = oldValue.Height.ToString();
            heightInput.textRenderer.fontSize = UIInspectorPanel.DrawerFontSize;
            heightInput.onEndEdit = (_if) =>
            {
                if (int.TryParse(_if.text, out var newValue))
                {
                    callback(new Rectangle(oldValue.X, oldValue.Y, oldValue.Width, newValue), true);
                    return;
                }
                callback(oldValue, false);
            };

            return spacer;
        }

        public static UISpacer MakeRectangleF(Transform parent, RectangleF oldValue, Action<RectangleF, bool> callback)
        {
            float parentW = parent.GetComponent<UITransform>().anchoredRect.Width;
            float parentH = parent.GetComponent<UITransform>().anchoredRect.Height;
            int dummyW = 10, dummyH = 10;

            var spacer = UISpacer.Build<UISpacer>(parent,
                new RectangleF(0, 0, dummyW, dummyH), 0.01f, true, true);
            spacer.UITransform.expandWidthToParent = true;
            spacer.UITransform.expandHeightToParent = true;

            spacer.AddComponent<UIHStacker>().SetMargins(0, 0, 0, 0, 10);

            float fieldWidth = (parentW - 10 * (4 - 1)) / 4f;

            // X
            var xInput = UIInputField.BuildAsUI<UIInputField>(spacer.transform, "xPropertyDrawer",
                "art/UI/white.png", true, false,
                new RectangleF(0, 0, fieldWidth, parentH), 0.01f);
            xInput.textOffset = new Vector2(8, 0);
            xInput.color = Color.White.Dimming(0.2f);
            xInput.activatedBGColor = Color.White.Dimming(0.4f);
            xInput.deactivatedBGColor = Color.White.Dimming(0.2f);
            xInput.text = PrettyFloatToString(oldValue.X);
            xInput.textRenderer.fontSize = UIInspectorPanel.DrawerFontSize;
            xInput.onEndEdit = (_if) =>
            {
                if (float.TryParse(_if.text, out var newValue))
                    callback(new RectangleF(newValue, oldValue.Y, oldValue.Width, oldValue.Height), true);
                else
                    callback(oldValue, false);
            };

            // Y
            var yInput = UIInputField.BuildAsUI<UIInputField>(spacer.transform, "yPropertyDrawer",
                "art/UI/white.png", true, false,
                new RectangleF(0, 0, fieldWidth, parentH), 0.01f);
            yInput.textOffset = new Vector2(8, 0);
            yInput.color = Color.White.Dimming(0.2f);
            yInput.activatedBGColor = Color.White.Dimming(0.4f);
            yInput.deactivatedBGColor = Color.White.Dimming(0.2f);
            yInput.text = PrettyFloatToString(oldValue.Y);
            yInput.textRenderer.fontSize = UIInspectorPanel.DrawerFontSize;
            yInput.onEndEdit = (_if) =>
            {
                if (float.TryParse(_if.text, out var newValue))
                    callback(new RectangleF(oldValue.X, newValue, oldValue.Width, oldValue.Height), true);
                else
                    callback(oldValue, false);
            };

            // Width
            var widthInput = UIInputField.BuildAsUI<UIInputField>(spacer.transform, "widthPropertyDrawer",
                "art/UI/white.png", true, false,
                new RectangleF(0, 0, fieldWidth, parentH), 0.01f);
            widthInput.textOffset = new Vector2(8, 0);
            widthInput.color = Color.White.Dimming(0.2f);
            widthInput.activatedBGColor = Color.White.Dimming(0.4f);
            widthInput.deactivatedBGColor = Color.White.Dimming(0.2f);
            widthInput.text = PrettyFloatToString(oldValue.Width);
            widthInput.textRenderer.fontSize = UIInspectorPanel.DrawerFontSize;
            widthInput.onEndEdit = (_if) =>
            {
                if (float.TryParse(_if.text, out var newValue))
                    callback(new RectangleF(oldValue.X, oldValue.Y, newValue, oldValue.Height), true);
                else
                    callback(oldValue, false);
            };

            // Height
            var heightInput = UIInputField.BuildAsUI<UIInputField>(spacer.transform, "heightPropertyDrawer",
                "art/UI/white.png", true, false,
                new RectangleF(0, 0, fieldWidth, parentH), 0.01f);
            heightInput.textOffset = new Vector2(8, 0);
            heightInput.color = Color.White.Dimming(0.2f);
            heightInput.activatedBGColor = Color.White.Dimming(0.4f);
            heightInput.deactivatedBGColor = Color.White.Dimming(0.2f);
            heightInput.text = PrettyFloatToString(oldValue.Height);
            heightInput.textRenderer.fontSize = UIInspectorPanel.DrawerFontSize;
            heightInput.onEndEdit = (_if) =>
            {
                if (float.TryParse(_if.text, out var newValue))
                    callback(new RectangleF(oldValue.X, oldValue.Y, oldValue.Width, newValue), true);
                else
                    callback(oldValue, false);
            };

            return spacer;
        }


        public static UIButton MakeColorButton(Transform parent, Color oldValue, Action<Color, bool> callback)
        {
            int dummyW = 10, dummyH = 10;

            var button = UIButton.Build(parent,
                "valueButton", "art/UI/white.png", true, false,
                new RectangleF(0, 0, dummyW, dummyH), 0.1f,
                (_) =>
                {
                    var newColor = GtkUtility.ShowColorPickerDialog(parent.name, oldValue, true);
                    if (newColor != null)
                    {
                        callback(newColor.Value, true);
                        return;
                    }
                    callback(oldValue, false);
                },
                null, oldValue, "notoKR", UIInspectorPanel.DrawerFontSize,
                color: oldValue);
            button.UITransform.expandWidthToParent = true;
            button.UITransform.expandHeightToParent = true;
            button.useDimminAnimation = false;

            return button;
        }



        public static UIButton MakeToggleButton(Transform parent, bool oldValue, Action<bool, bool> callback)
        {
            var baseColor = Color.LightPink.Dimming(0.4f);

            int dummyW = 10, dummyH = 10;

            var button = UIButton.Build(parent,
                "valueButton", "art/UI/white.png", true, false,
                new RectangleF(0, 0, dummyW, dummyH), 0.1f,
                (_) => callback(!oldValue, true),
                oldValue.ToString(), Color.White, "notoKR", UIInspectorPanel.DrawerFontSize,
                color: baseColor);
            button.UITransform.expandWidthToParent = true;
            button.UITransform.expandHeightToParent = true;

            return button;
        }

        public static UISpacer MakeStringWithBrowseButton(Transform parent, string oldValue, Action<string, bool> callback)
        {
            float parentW = parent.GetComponent<UITransform>().anchoredRect.Width;
            float parentH = parent.GetComponent<UITransform>().anchoredRect.Height;

            //int dummyW = 10, dummyH = 10;
            var spacer = UISpacer.Build<UISpacer>(parent,
                new RectangleF(0, 0, parentW, parentH), 0.01f, true, true);
            //spacer.UITransform.expandWidthToParent = true;
            //spacer.UITransform.expandHeightToParent = true;
            var getter = UIInputField.BuildAsUI<UIInputField>(spacer.transform, "stringPropertyDrawer",
                "art/UI/white.png", true, false,
                new RectangleF(parentH + 5, 0, parentW - parentH - 5, parentH), 0.01f);
            getter.textOffset = new Vector2(8, 3);
            getter.color = Color.White.Dimming(0.2f);
            getter.activatedBGColor = Color.White.Dimming(0.4f);
            getter.deactivatedBGColor = Color.White.Dimming(0.2f);
            getter.text = oldValue != null ? oldValue : "";
            getter.textRenderer.fontSize = UIInspectorPanel.DrawerFontSize;
            getter.onEndEdit = (_if) =>
            {
                callback(_if.text, true);
            };

            var button = UIButton.Build(spacer.transform, "browseButton",
                "art/UI/white.png", true, false,
                new RectangleF(0, 0, parentH, parentH), 0.01f,
                (_) =>
                {
                    var assetsDirectory = GameBase.Instance.config.rawAssetsRootPath;
                    var newValue = GtkUtility.ShowOpenFileDialog("Select File", assetsDirectory);
                    if (newValue != null)
                    {
                        var assetFolder = GameBase.Instance.config.rawAssetsRootPath;
                        if (newValue.StartsWith(assetFolder))
                        {
                            // 상대경로로 변환
                            var relativePath = System.IO.Path.GetRelativePath(assetFolder, newValue);
                            newValue = relativePath;
                            callback(newValue, true);
                        }
                        else
                        {
                            callback(oldValue, false);
                        }

                        return;
                    }
                    callback(oldValue, false);
                },
                "B", Color.White, "notoKR", UIInspectorPanel.DrawerFontSize,
                color: Color.CornflowerBlue.Dimming(0.4f));
            //button.UITransform.expandWidthToParent = true;
            //button.UITransform.expandHeightToParent = true;
            return spacer;
        }

        public static UISpacer MakeSingle<T>(Transform parent, T oldValue, Action<T, bool> callback)
        {
            int dummyW = 10, dummyH = 10;

            var spacer = UISpacer.Build<UISpacer>(parent,
                new RectangleF(0, 0, dummyW, dummyH), 0.01f, true, true);
            spacer.UITransform.expandWidthToParent = true;
            spacer.UITransform.expandHeightToParent = true;

            var getter = UIInputField.BuildAsUI<UIInputField>(spacer.transform, "intPropertyDrawer",
                "art/UI/white.png", true, false,
                new RectangleF(0, 0, dummyW, dummyH), 0.01f);
            getter.UITransform.expandWidthToParent = true;
            getter.UITransform.expandHeightToParent = true;
            getter.textOffset = new Vector2(8, 0);
            getter.color = Color.White.Dimming(0.2f);
            getter.activatedBGColor = Color.White.Dimming(0.4f);
            getter.deactivatedBGColor = Color.White.Dimming(0.2f);
            getter.text = oldValue != null ? oldValue.ToString() : "";
            getter.textRenderer.fontSize = UIInspectorPanel.DrawerFontSize;

            getter.onEndEdit = (_if) =>
            {
                if (typeof(T) == typeof(int))
                {
                    if (int.TryParse(_if.text, out var newValue))
                    {
                        callback((T)(object)newValue, true);
                        return;
                    }
                }
                else if (typeof(T) == typeof(float))
                {
                    if (float.TryParse(_if.text, out var newValue))
                    {
                        callback((T)(object)newValue, true);
                        return;
                    }
                }
                else if (typeof(T) == typeof(string))
                {
                    callback((T)(object)_if.text, true);
                    return;
                }

                callback(oldValue, false);
            };

            return spacer;
        }

        public static UISpacer MakeEnum<T>(Transform parent, T oldValue, Action<T, bool> callback) where T : Enum
        {
            var baseColor = Color.CornflowerBlue;

            int dummyW = 10, dummyH = 10;
            var spacer = UISpacer.Build<UISpacer>(parent,
                new RectangleF(0, 0, dummyW, dummyH), 0.01f, true, true);
            spacer.UITransform.expandWidthToParent = true;
            spacer.UITransform.expandHeightToParent = true;
            var getter = UIButton.Build(spacer.transform, "enumPropertyDrawer",
                "art/UI/white.png", true, false,
                new RectangleF(0, 0, dummyW, dummyH), 0.01f,
                (_) =>
                {
                    List<string> items = new List<string>();
                    foreach (var name in Enum.GetNames(typeof(T)))
                    {
                        items.Add(name);
                    }

                    var pos = GameBase.Instance.inputManager.GetMousePos();
                    pos.Y = -pos.Y;

                    GameBase.Instance.builtinUIManager.ShowPopupMenu(items, pos, (index) =>
                    {
                        if (index < 0 || index >= items.Count)
                        {
                            callback(oldValue, false);
                            return;
                        }
                        var selectedName = items[index];
                        if (Enum.TryParse(typeof(T), selectedName, out var newValue))
                        {
                            callback((T)newValue, true);
                            return;
                        }
                    });
                },
                color: baseColor.Dimming(0.4f),
                text: oldValue.ToString(), textColor: Color.White,
                fontSize: UIInspectorPanel.DrawerFontSize
                //activatedColor: baseColor.Dimming(0.6f),
                //deactivatedColor: baseColor.Dimming(0.4f));
                );

            getter.UITransform.expandWidthToParent = true;
            getter.UITransform.expandHeightToParent = true;
            //getter.textOffset = new Vector2(8, 3);

            return spacer;
        }
    }
}

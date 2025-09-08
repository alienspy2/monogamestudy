
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace MGAlienLib
{
    public enum eCanvasType
    {
        World,
        Screen
    }

    public class UICanvas : ComponentBase
    {
        public static readonly bool IsAddableFromInspector = true;

        [SerializeField] protected eCanvasType _mode = eCanvasType.Screen;
        [SerializeField] protected RectangleF _rect = new RectangleF(0, 0, Screen.width, Screen.height);

        public eCanvasType mode
        {
            get => _mode;
            set
            {
                _mode = value;
            }
        }

        public RectangleF rect
        {
            get => _rect;
            set
            {
                _rect = value;
            }
        }

        public override void Update()
        {
            if (mode == eCanvasType.Screen && Screen.screenSizeWasChangedThisFrame)
            {
                rect = new RectangleF(0, 0, Screen.width, Screen.height);
            }
        }

        public override void FinalizeDeserialize(DeserializeContext context)
        {
            base.FinalizeDeserialize(context);
            rect = new RectangleF(0, 0, Screen.width, Screen.height);
        }

    }
}

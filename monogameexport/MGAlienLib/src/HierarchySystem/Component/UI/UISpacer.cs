
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace MGAlienLib
{
    public class UISpacer : UIRenderable
    {
        public static readonly bool IsAddableFromInspector = true;

        public static T Build<T>(Transform parent,
            RectangleF anchoredRect, float elevation,
            bool expandWidth = false, bool expandHeight = false,
            Vector2? pivot = null, Vector2? anchor = null,
            string layer = "UI") where T : UISpacer
        {
            pivot ??= Vector2.UnitY;
            anchor ??= Vector2.UnitY;
            var obj = GameBase.Instance.hierarchyManager.CreateGameObject("*spacer*", parent);
            obj.layer = LayerMask.NameToLayer(layer);
            var spacer = obj.AddComponent<T>();
            spacer.useAsUI = true;
            spacer.UITransform.anchoredRect = anchoredRect;
            spacer.UITransform.elevation = elevation;
            spacer.UITransform.pivot = pivot.Value;
            spacer.UITransform.anchor = anchor.Value;
            spacer.UITransform.expandWidthToParent = expandWidth;
            spacer.UITransform.expandHeightToParent = expandHeight;

            return spacer;
        }
    }
}

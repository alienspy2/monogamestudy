
using MGAlienLib;
using Microsoft.Xna.Framework;

namespace Project1
{
    public class DemoBase : ComponentBase
    {
        protected Camera cam;

        public override void Awake()
        {
            base.Awake();
        }

        public override void Update()
        {
            base.Update();

            if (Screen.screenSizeWasChangedThisFrame)
            {
                RefreshScreenSize();
            }

        }


        private void RefreshScreenSize()
        {
            if (cam == null) return;

            cam.viewport = new Rectangle(0, 0, Screen.width, Screen.height);
            cam.aspectRatio = (float)Screen.width / (float)Screen.height;
        }

    }
}


using TrueTypeSharp;

namespace MGAlienLib
{
    /// <summary>
    /// (WIP) 폰트를 관리하는 매니저입니다.
    /// </summary>
    public sealed class FontManager : ManagerBase
    {
        public TrueTypeSharpUtility notosansKR;
        public SkiaFontUtility skNotosansKR;
        public SkiaFontUtility skArial;

        public FontManager(GameBase owner) : base(owner)
        {
        }

        public override void OnPreLoadContent()
        {
            base.OnPostLoadContent();
            notosansKR = new TrueTypeSharpUtility("Content/Fonts/NotoSansKR-Regular.ttf");
            skNotosansKR = new SkiaFontUtility("NotoSansKR-Regular");
            skArial = new SkiaFontUtility("arial");
        }

        public SkiaFontUtility GetFont(string fontName)
        {
            return fontName switch
            {
                "notoKR" => skNotosansKR,
                "arial" => skArial,
                _ => null
            };
        }
    }
}

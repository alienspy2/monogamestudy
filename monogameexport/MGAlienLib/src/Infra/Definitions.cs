
namespace MGAlienLib
{
    /// <summary>
    /// Enum for horizontal alignment
    /// </summary>
    public enum eHAlign
    {
        Left,
        Center,
        Right
    }

    /// <summary>
    /// Enum for vertical alignment
    /// </summary>
    public enum eVAlign
    {
        Top,
        Middle,
        Bottom
    }

    /// <summary>
    /// Asset의 원본을 나타냅니다.
    /// </summary>
    public enum eAssetSource
    {
        /// <summary>
        /// 더미 소스입니다.
        /// 실제로 asset 에 연결되지 않습니다.
        /// </summary>
        Dummy,
        /// <summary>
        /// rawAssetsRootPath/Assets 아래에 있는 raw file 로부터 로드합니다.
        /// </summary>
        RawAssets,
        /// <summary>
        /// rawAssetsRootPath/PackedAssets 폴더에 있는 7zip 압축 파일로부터 로드합니다.
        /// </summary>
        PackedAssets,
        /// <summary>
        /// MGCB로 빌드된 asset으로부터 로드합니다.
        /// effect 등 특정 파일은 반드시 MGCB로 빌드해야 합니다.
        /// </summary>
        MGCB,
        /// <summary>
        /// HTTP 프로토콜로부터 로드합니다.
        /// todo : async 로 바꿔야 함
        /// </summary>
        HTTP,
        /// <summary>
        /// HTTPS 프로토콜로부터 로드합니다.
        /// todo : async 로 바꿔야 함
        /// </summary>
        HTTPS,
        /// <summary>
        /// 알 수 없는 소스입니다.
        /// </summary>
        Unknown,
    }

    /// <summary>
    /// Asset의 타입을 나타냅니다.
    /// </summary>
    public enum eAssetType
    {
        None,
        Texture,
        AudioClip,
        TrueTypeFont,
    }

    /// <summary>
    /// DynamicTextureAtlasManager 에서 사용하는 카테고리를 나타냅니다.
    /// </summary>
    public enum eDynamicAtlasCategory
    {
        Font,
        AASprite,
    }

    /// <summary>
    /// 마우스 버튼을 나타냅니다.
    /// </summary>
    public enum eMouseButton
    {
        Left,
        Right,
        Middle
    }



}

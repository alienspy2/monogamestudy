
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MGAlienLib
{
    public class DynamicTextureAtlasManager : ManagerBase
    {
        private class Category
        {
            public eDynamicAtlasCategory category;
            public DynamicTexturePacker packer;
            public Dictionary<string, int> addressToId = new();
            public class PageData
            {
                public bool wasRemapped = true;
            }
            public List<PageData> pages = new();
        }

        private List<Category> data;

        public DynamicTextureAtlasManager(GameBase owner) : base(owner)
        {
        }

        public override void OnPostInitialize()
        {
            data = new List<Category>();

            // 0: font
            data.Add(new Category()
            {
                category = eDynamicAtlasCategory.Font,
                packer = new DynamicTexturePacker("Font", SurfaceFormat.Alpha8),
            });

            // 1: unlit (sprite)
            data.Add(new Category()
            {
                category = eDynamicAtlasCategory.AASprite,
                packer = new DynamicTexturePacker("unlit", SurfaceFormat.Color),
            });
        }

        public override void OnPostDraw()
        {
            foreach (var item in data)
            {
                foreach(var pageData in item.pages)
                {
                    pageData.wasRemapped = false;
                }
            }
        }

        /// <summary>
        /// 주어진 texture 를 atlas 에 넣고, 해당 texture 의 rect 를 반환한다.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="source"></param>
        /// <param name="rect"></param>
        /// <returns>atlasID , -1 이면 실패</returns>
        public int Insert(eDynamicAtlasCategory category, Texture2D source, bool dialate, out Rectangle rect)
        {
            int atlasId = data[(int)category].packer.Insert(source, dialate, out rect, out bool repacked);
            int pageIndex = data[(int)category].packer.GetPage(atlasId);

            while (data[(int)category].pages.Count <= pageIndex)
                data[(int)category].pages.Add(new Category.PageData());
            return atlasId;
        }

        /// <summary>
        /// assetmanager 를 이용하여 주어진 source 와 주소로부터 로드한 후 atlas 에 넣고, 해당 texture 의 rect 를 반환한다.
        /// 이후 같은 texAddress 로 다시 호출될 경우, texture 를 다시 로드하지 않고, 기존에 pack 한 atlasId 를 반환한다.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="source"></param>
        /// <param name="texAddress"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public int Insert(eDynamicAtlasCategory category, eAssetSource source, string texAddress, bool dialate, out Rectangle rect)
        {
            if (data[(int)category].addressToId.TryGetValue(texAddress, out int atlasId))
            {
                rect = data[(int)category].packer.GetRect(atlasId).Value;
                return atlasId;
            }
            else
            {
                var tex = owner.assetManager.GetTexture2D(source, texAddress);
                int newAtlasId = data[(int)category].packer.Insert(tex,dialate, out rect, out bool repacked);
                int pageIndex = data[(int)category].packer.GetPage(newAtlasId);
                while (data[(int)category].pages.Count <= pageIndex)
                    data[(int)category].pages.Add(new Category.PageData());
                return newAtlasId;
            }
        }

        /// <summary>
        /// atlasId 에 해당하는 texture 의 rect 를 반환한다.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="atlasId"></param>
        /// <returns></returns>
        public Rectangle? GetRect(eDynamicAtlasCategory category, int atlasId)
        {
            return data[(int)category].packer.GetRect(atlasId);
        }

        /// <summary>
        /// atlasId 에 해당하는 texture 의 크기를 반환한다.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="atlasId"></param>
        /// <returns></returns>
        public Vector2 GetTexSize(eDynamicAtlasCategory category, int atlasId)
        {
            return new Vector2(data[(int)category].packer.GetTexture(atlasId).Width, 
                data[(int)category].packer.GetTexture(atlasId).Height);
        }

        /// <summary>
        /// category 와 atlasId 에 해당하는 texture 가 remapped 되어서 변경된 상태인지 반환한다.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="atlasId"></param>
        /// <returns></returns>
        public bool IsDirty(eDynamicAtlasCategory category, int atlasId)
        {
            var pageIndex = data[(int)category].packer.GetPage(atlasId);
            return data[(int)category].pages[pageIndex].wasRemapped;
        }

        /// <summary>
        /// category 와 atlasId 에 해당하는 texture 를 atlas 에서 제거한다.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="atlasId"></param>
        public void Remove(eDynamicAtlasCategory category, int atlasId)
        {
            data[(int)category].packer.Remove(atlasId);
        }

        /// <summary>
        /// category 와 pageID 에 해당하는 atlas 의 texture 를 반환한다.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="pageId"></param>
        /// <returns></returns>
        public Texture2D GetTextureByPage(eDynamicAtlasCategory category, int pageId)
        {
            return data[(int)category].packer.GetTexureByPageIndex(pageId);
        }

        /// <summary>
        /// category 와 atlasId 에 해당하는 atlas 의 texture 를 반환한다.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="atlasId"></param>
        /// <returns></returns>
        public Texture2D GetTextureByAtlasId(eDynamicAtlasCategory category, int atlasId)
        {
            return data[(int)category].packer.GetTexture(atlasId);
        }
    }
}

using Microsoft.Xna.Framework.Graphics;
using SharpCompress.Archives;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace MGAlienLib
{

    // 디렉토리 노드 클래스
    public class DirectoryNode
    {
        public string Name { get; set; }
        public Dictionary<string, DirectoryNode> SubDirectories { get; set; } = new Dictionary<string, DirectoryNode>();
        public Dictionary<string, eAssetType> Files { get; set; } = new Dictionary<string, eAssetType>();
    }

    /// <summary>
    /// Asset 관리자입니다.
    /// 주로 내부에서 사용합니다.
    /// 최적화등의 이유로 복잡한 구현이 많기 때문에,
    /// AssetManager를 외부에서 직접 사용하는 경우는 드뭅니다.
    /// </summary>
    public sealed class AssetManager : ManagerBase
    {
        /// <summary>
        /// 파일 확장자로부터 AssetType을 결정합니다.
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        public eAssetType TypeByExtension(string ext)
        {
            return ext.ToLower() switch
            {
                ".png" => eAssetType.Texture,
                ".jpg" => eAssetType.Texture,
                ".jpeg" => eAssetType.Texture,
                ".bmp" => eAssetType.Texture,
                ".tga" => eAssetType.Texture,
                ".gif" => eAssetType.Texture,
                ".mp3" => eAssetType.AudioClip,
                ".wav" => eAssetType.AudioClip,
                ".ogg" => eAssetType.AudioClip,
                ".ttf" => eAssetType.TrueTypeFont,
                ".otf" => eAssetType.TrueTypeFont,
                _ => eAssetType.None,
            };
        }

        /// <summary>
        /// 대상 폴더를 지정합니다.
        /// 기본값은 cwd 입니다.
        /// </summary>
        public string rawAssetsRootPath { get; set; } = "";
        public string packedAssetsRootPath { get; set; } = "";

        /// <summary>
        /// 소스를 지정하지 않을 때 사용되는 기본 소스입니다.
        /// </summary>
        public eAssetSource defaultSource { get; set; } = eAssetSource.RawAssets;
        private DirectoryNode _rootNode;

        /// <summary>
        /// AssetManager를 생성합니다.
        /// </summary>
        /// <param name="owner"></param>
        public AssetManager(GameBase owner) : base(owner)
        {
            _rootNode = new DirectoryNode { Name = "" };
        }


        public override void OnPreLoadContent()
        {
            base.OnPostLoadContent();
            rawAssetsRootPath = owner.config.rawAssetsRootPath;
            RefreshDB();
        }

        /// <summary>
        /// DB를 갱신합니다.
        /// </summary>
        public void RefreshDB()
        {
            _rootNode.SubDirectories.Clear();
            _rootNode.Files.Clear();

            try
            {
                string assetsPath = rawAssetsRootPath;
                if (Directory.Exists(assetsPath))
                {
                    string[] files = Directory.GetFiles(assetsPath, "*.*", SearchOption.AllDirectories);

                    foreach (string file in files)
                    {
                        string normalizedPath = file.Replace('\\', '/');
                        string relativePath = normalizedPath.Substring(assetsPath.Length + 1);
                        string[] parts = relativePath.Split('/');
                        string fileName = parts[^1];
                        eAssetType type = TypeByExtension(Path.GetExtension(fileName));

                        if (type != eAssetType.None)
                        {
                            DirectoryNode currentNode = _rootNode;
                            for (int i = 0; i < parts.Length - 1; i++)
                            {
                                string dirName = parts[i];
                                if (!currentNode.SubDirectories.ContainsKey(dirName))
                                {
                                    currentNode.SubDirectories[dirName] = new DirectoryNode { Name = dirName };
                                }
                                currentNode = currentNode.SubDirectories[dirName];
                            }
                            currentNode.Files[fileName] = type;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error refreshing database: {ex.Message}");
            }
        }

        /// <summary>
        /// 파일을 검색합니다.
        /// 중간어 검색을 지원합니다.
        /// 예를 들어 art/UI/frame.png 라는 파일이 있을 때, "UI frame" 만으로도 검색이 가능합니다.
        /// </summary>
        /// <param name="searchTerms"></param>
        /// <param name="assetType"></param>
        /// <returns></returns>
        public List<string> SearchFiles(string searchTerms, eAssetType assetType = eAssetType.None)
        {
            List<string> results = new List<string>();
            string[] terms = searchTerms.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (terms.Length == 0) return results;

            SearchNode(_rootNode, "", terms, assetType, results);
            return results;
        }

        private void SearchNode(DirectoryNode node, string currentPath, string[] terms, eAssetType assetType, List<string> results)
        {
            foreach (var file in node.Files)
            {
                string fileName = file.Key;
                eAssetType type = file.Value;
                string fullPath = string.IsNullOrEmpty(currentPath) ? fileName : $"{currentPath}/{fileName}";
                string lowerFullPath = fullPath.ToLowerInvariant(); // 대소문자 구분 없애기 위해 소문자로 변환

                bool matchesInOrder = true;
                int lastIndex = -1;

                foreach (string term in terms)
                {
                    if (string.IsNullOrEmpty(term)) continue;

                    string lowerTerm = term.ToLowerInvariant();
                    int currentIndex = lowerFullPath.IndexOf(lowerTerm, lastIndex + 1);

                    if (currentIndex == -1 || (lastIndex >= 0 && currentIndex <= lastIndex))
                    {
                        matchesInOrder = false;
                        break;
                    }
                    lastIndex = currentIndex;
                }

                if (matchesInOrder && (assetType == eAssetType.None || type == assetType))
                {
                    results.Add(fullPath);
                }
            }

            foreach (var subDir in node.SubDirectories)
            {
                string newPath = string.IsNullOrEmpty(currentPath) ? subDir.Key : $"{currentPath}/{subDir.Key}";
                SearchNode(subDir.Value, newPath, terms, assetType, results);
            }
        }


        private void CollectFilesInNode(DirectoryNode node, string currentPath, eAssetType assetType, List<string> results)
        {
            foreach (var file in node.Files)
            {
                if (assetType == eAssetType.None || file.Value == assetType)
                {
                    string fullPath = string.IsNullOrEmpty(currentPath) ? file.Key : $"{currentPath}/{file.Key}";
                    results.Add(fullPath);
                }
            }

            foreach (var subDir in node.SubDirectories)
            {
                string newPath = string.IsNullOrEmpty(currentPath) ? subDir.Key : $"{currentPath}/{subDir.Key}";
                CollectFilesInNode(subDir.Value, newPath, assetType, results);
            }
        }

        //public int CacheCount => CountFilesInNode(_rootNode);

        private int CountFilesInNode(DirectoryNode node)
        {
            int count = node.Files.Count;
            foreach (var subDir in node.SubDirectories)
            {
                count += CountFilesInNode(subDir.Value);
            }
            return count;
        }

        /// <summary>
        /// 주어진 경로에 있는 파일의 타입을 반환합니다.
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        //public eAssetType GetFileType(string relativePath)
        //{
        //    string[] parts = relativePath.Split('/');
        //    DirectoryNode currentNode = _rootNode;

        //    for (int i = 0; i < parts.Length - 1; i++)
        //    {
        //        if (!currentNode.SubDirectories.TryGetValue(parts[i], out currentNode))
        //        {
        //            return eAssetType.None;
        //        }
        //    }

        //    return currentNode.Files.TryGetValue(parts[^1], out eAssetType type) ? type : eAssetType.None;
        //}

        private T ViaStream<T>(eAssetSource source, string address, Func<Stream, T> func) where T : class
        {
            if (source == eAssetSource.RawAssets)
            {
                // Assets 폴더 아래에서 파일 검색
                var filePath = Path.Combine(rawAssetsRootPath, $"{address}");
                if (!File.Exists(filePath))
                {
                    return null;
                }

                try
                {
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                    {
                        return func(fileStream);
                    }
                }
                catch (Exception ex)
                {
                    // 오류 처리 (예: 콘솔 출력)
                    Logger.Log("텍스처 로드 실패: " + ex.Message);
                }
            }
            else if (source == eAssetSource.PackedAssets)
            {
                // headless 의 첫번째 / 까지는 7zip file 이름
                int separatorIndex = address.IndexOf('/');

                // 첫 번째 '/'까지가 7zip 파일 이름
                var archivePath = separatorIndex >= 0 ? address.Substring(0, separatorIndex) : address;
                archivePath = Path.Combine(packedAssetsRootPath, $"{archivePath}.7z");

                // 나머지가 경로 (separatorIndex + 1부터 끝까지)
                var assetPath = separatorIndex >= 0 ? address.Substring(separatorIndex + 1) : "";

                using (var archive = ArchiveFactory.Open(archivePath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        // 파일이 디렉토리가 아니고, 원하는 파일 이름과 일치하는 경우
                        if (!entry.IsDirectory && entry.Key.Equals(assetPath, StringComparison.OrdinalIgnoreCase))
                        {
                            // 해당 파일의 내용을 스트림으로 리턴
                            return func(entry.OpenEntryStream());
                        }
                    }
                }
            }
            else if (source == eAssetSource.HTTP || source == eAssetSource.HTTPS)
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", "AlienMonoGame/1.0");

                        string url;
                        if (address.StartsWith("http://") || address.StartsWith("https://"))
                        {
                            url = address;
                        }
                        else
                        {
                            url = source switch
                            {
                                eAssetSource.HTTP => "http://{address}",
                                eAssetSource.HTTPS => "https://{address}",
                                _ => ""
                            };
                        }

                        var imageBytes = client.GetByteArrayAsync(url).Result;
                        using (var stream = new MemoryStream(imageBytes))
                        {
                            return func(stream);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error loading PNG: {ex.Message}");
                }
            }

            return null;
        }


        /// <summary>
        /// 지정된 소스와 주소로부터 Texture2D를 로드합니다.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="address"></param>
        /// <param name="importWidth"></param>
        /// <param name="importHeight"></param>
        /// <returns></returns>
        public Texture2D GetTexture2D(eAssetSource source, string address, int importWidth = 0, int importHeight = 0) => source switch
        {
            eAssetSource.MGCB => owner.Content.Load<Texture2D>(address),
            _ => ViaStream(source, address, (fileStream) =>
            {
                var info = Image.Identify(fileStream);
                fileStream.Position = 0;

                Texture2D texture = null;
                SurfaceFormat textureFormat = SurfaceFormat.Color;

                if (info.PixelType.BitsPerPixel == 32 || info.PixelType.BitsPerPixel == 24)
                {
                    Image<Rgba32> image = Image.Load<Rgba32>(fileStream);
                    textureFormat = SurfaceFormat.Color;
                    if (importWidth > 0 && importHeight > 0)
                    {
                        image.Mutate(x => x.Resize(importWidth, importHeight, KnownResamplers.Lanczos3)); // 리사이징 적용
                    }
                    byte[] pixels = new byte[image.Width * image.Height * (32 / 8)];
                    image.CopyPixelDataTo(pixels);
                    texture = new Texture2D(owner.GraphicsDevice, image.Width, image.Height, false, textureFormat);
                    texture.SetData(pixels);
                }
                else if (info.PixelType.BitsPerPixel == 8)
                {
                    Image<L8> image = Image.Load<L8>(fileStream);
                    textureFormat = SurfaceFormat.Alpha8;
                    if (importWidth > 0 && importHeight > 0)
                    {
                        image.Mutate(x => x.Resize(importWidth, importHeight, KnownResamplers.Lanczos3)); // 리사이징 적용
                    }
                    byte[] pixels = new byte[image.Width * image.Height * (info.PixelType.BitsPerPixel / 8)];
                    image.CopyPixelDataTo(pixels);
                    texture = new Texture2D(owner.GraphicsDevice, image.Width, image.Height, false, textureFormat);
                    texture.SetData(pixels);
                }

                texture.Name = address;

                return texture;
            }),
        };

        /// <summary>
        /// 주소로부터 Effect를 로드합니다.
        /// monogame 의 Effect는 shader 를 의미합니다.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Effect GetEffect(string path)
        {
            return owner.Content.Load<Effect>(path);
        }


    }
}

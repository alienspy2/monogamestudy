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
                ".dds" => eAssetType.Texture,
                ".mp3" => eAssetType.AudioClip,
                ".wav" => eAssetType.AudioClip,
                ".ogg" => eAssetType.AudioClip,
                ".ttf" => eAssetType.TrueTypeFont,
                ".otf" => eAssetType.TrueTypeFont,
                _ => eAssetType.None,
            };
        }
        
        public bool IsPathDDS(string address)
        {
            if (address.Length < 4) return false;
            var extstring = address.Substring(address.Length - 4);
            return (extstring.ToLower() == ".dds");
        }

        public bool IsAddressMGCB(string address)
        {
            return address.StartsWith("mgcb://");
        }

        public eAssetSource ParseAssetSourceFromAddress(string address, out string path)
        {
            eAssetSource source = eAssetSource.Unknown;
            path = null;

            if (address.StartsWith("mgcb://"))
            {
                source = eAssetSource.MGCB;
                path = address.Substring("mgcb://".Length);
            }
            else if (address.StartsWith("raw://"))
            {
                source = eAssetSource.RawAssets;
                path = address.Substring("raw://".Length);
            }
            else if (address.StartsWith("packed://"))
            {
                source = eAssetSource.PackedAssets;
                path = address.Substring("packed://".Length);
            }
            else if (address.StartsWith("http://"))
            {
                source = eAssetSource.HTTP;
                path = address; // 여기서는 전체 주소를 그대로 사용
            }
            else if (address.StartsWith("https://"))
            {
                source = eAssetSource.HTTPS;
                path = address; // 여기서는 전체 주소를 그대로 사용
            }
            else
            {
                source = eAssetSource.Unknown;
                path = address; // fallback
            }

            return source;
        }

        /// <summary>
        /// 대상 폴더를 지정합니다.
        /// 기본값은 cwd 입니다.
        /// </summary>
        public string rawAssetsRootPath { get; set; } = "";
        public string packedAssetsRootPath { get; set; } = "";

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
        public List<string> SearchRawFiles(string searchTerms, eAssetType assetType = eAssetType.None)
        {
            List<string> results = new List<string>();
            string[] terms = searchTerms.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (terms.Length == 0) return results;

            SearchNode(_rootNode, "", terms, assetType, results);
            return results;
        }

        /// <summary>
        /// 재귀적으로 디렉터리 트리를 탐색하면서 검색어(terms)가 순서대로 포함된 파일 경로를 찾아 results 리스트에 추가합니다.
        /// 중간어 검색을 지원합니다.
        /// </summary>
        /// <param name="node">현재 탐색 중인 DirectoryNode</param>
        /// <param name="currentPath">
        /// 현재 노드까지의 상대 경로 (루트에서부터 이어진 폴더 이름들, 예: "Assets/Textures")
        /// </param>
        /// <param name="terms">검색어 배열(순서대로 찾음). 대소문자 구분 없이 검색합니다.</param>
        /// <param name="assetType">
        /// 특정 자산 타입(eAssetType)을 필터링하려면 해당 enum 값을 지정. eAssetType.None이면 타입 무시.
        /// </param>
        /// <param name="results">검색 결과(경로)를 저장할 리스트</param>
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
                    results.Add("raw://" + fullPath);
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

        private int CountFilesInNode(DirectoryNode node)
        {
            int count = node.Files.Count;
            foreach (var subDir in node.SubDirectories)
            {
                count += CountFilesInNode(subDir.Value);
            }
            return count;
        }

        private T ViaStream<T>(eAssetSource source, string path, Func<Stream, T> func) where T : class
        {
            if (source == eAssetSource.Unknown)
                throw new Exception("지원하지 않는 주소 형식입니다. mgcb://, raw://, packed://, http://, https:// 만 지원합니다.");

            if (source == eAssetSource.RawAssets)
            {
                // Assets 폴더 아래에서 파일 검색
                var filePath = Path.Combine(rawAssetsRootPath, $"{path}");
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
                    Logger.Log($"로드 실패: {source} {path} {ex.Message}");
                }
            }
            else if (source == eAssetSource.PackedAssets)
            {
                // headless 의 첫번째 / 까지는 7zip file 이름
                int separatorIndex = path.IndexOf('/');

                // 첫 번째 '/'까지가 7zip 파일 이름
                var archivePath = separatorIndex >= 0 ? path.Substring(0, separatorIndex) : path;
                archivePath = Path.Combine(packedAssetsRootPath, $"{archivePath}.7z");

                // 나머지가 경로 (separatorIndex + 1부터 끝까지)
                var assetPath = separatorIndex >= 0 ? path.Substring(separatorIndex + 1) : "";

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
                        if (path.StartsWith("http://") || path.StartsWith("https://"))
                        {
                            url = path;
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
        /// 소스는 mgcb://, raw://, packed://, http://, https:// 등을 지원합니다.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="importWidth"></param>
        /// <param name="importHeight"></param>
        /// <returns></returns>
        public Texture2D GetTexture2D(string address, int importWidth = 0, int importHeight = 0)
        {
            var source = ParseAssetSourceFromAddress(address, out string path);
            if (IsAddressMGCB(address))
            {
                try
                {
                    Texture2D result = owner.Content.Load<Texture2D>(path);
                    return result;
                }
                catch (Exception ex)
                {
                    Logger.Log("MGCB 로드 실패: " + address);
                    return owner.defaultAssets.magentaTexture.asset;
                }
            }

            return ViaStream(source, path, (fileStream) =>
            {
                if (IsPathDDS(path))
                {
                    return LoadTextureDDS(owner.GraphicsDevice, fileStream);
                }
                else
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
                }

            });
        }

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

        private static Texture2D LoadTextureDDS(GraphicsDevice device, Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                // DDS 헤더 체크
                uint magic = reader.ReadUInt32();
                if (magic != 0x20534444) // "DDS "
                    throw new Exception("Not a DDS file.");

                // DDS_HEADER 읽기 (124 bytes)
                byte[] header = reader.ReadBytes(124);
                int height = BitConverter.ToInt32(header, 8);
                int width = BitConverter.ToInt32(header, 12);
                int linearSize = BitConverter.ToInt32(header, 16);
                int mipMapCount = Math.Max(1, BitConverter.ToInt32(header, 24));
                string fourCC = System.Text.Encoding.ASCII.GetString(header, 80, 4);

                SurfaceFormat format;
                if (fourCC == "DXT1") format = SurfaceFormat.Dxt1;
                else if (fourCC == "DXT3") format = SurfaceFormat.Dxt3;
                else if (fourCC == "DXT5") format = SurfaceFormat.Dxt5;
                else throw new NotSupportedException("Only DXT1/3/5 supported.");

                // 데이터 읽기
                int blockSize = (format == SurfaceFormat.Dxt1) ? 8 : 16;
                int dataSize = Math.Max(1, ((width + 3) / 4) * ((height + 3) / 4) * blockSize);
                byte[] ddsData = reader.ReadBytes(dataSize);

                // Texture2D 생성
                Texture2D texture = new Texture2D(device, width, height, false, format);
                texture.SetData(ddsData);
                return texture;
            }
        }

        public Mesh GetMesh(string address)
        {
            var source = ParseAssetSourceFromAddress(address, out string path);

            if (IsAddressMGCB(address))
            {
                throw new Exception("mgcb:// 는 지원하지 않습니다");
            }

            Mesh newMesh = ViaStream(source, path, (fileStream) =>
            {
                return MeshImporter.LoadSingleMesh(fileStream);
            });

            newMesh?.CalculateBounds();
            return newMesh;
        }
    }
}

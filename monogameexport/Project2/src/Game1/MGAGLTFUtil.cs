using SharpGLTF.Schema2;
using SharpGLTF.Geometry;
using SharpGLTF.Memory;
using Microsoft.Xna.Framework;
using MGAlienLib;

namespace Project2
{
    internal static class MGAGLTFUtil
    {

        public static DebugDraw.DebugMesh Load(string rawAssetPath)
        {
            DebugDraw.DebugMesh result = new DebugDraw.DebugMesh();
            var absPath = $"{GameBase.Instance.assetManager.rawAssetsRootPath}/{rawAssetPath}".Replace("/", "\\");
            var model = ModelRoot.Load(absPath);

            var mesh = model.LogicalMeshes[0];

            //foreach (var prim in mesh.Primitives)
            var prim = mesh.Primitives[0];
            {
                // 위치(Vertex Position) 읽기
                var positions = prim.GetVertexAccessor("POSITION")
                                    .AsVector3Array();

                // UV 읽기
                var texcoords = prim.GetVertexAccessor("TEXCOORD_0")
                                    ?.AsVector2Array();

                // 노멀 읽기
                var normals = prim.GetVertexAccessor("NORMAL")
                                  ?.AsVector3Array();

                result.v = new DebugDraw.VertexPositionNormalTextureColor[positions.Count];
                
                for(int i=0;i<result.v.Length;i++)
                {
                    result.v[i] = new DebugDraw.VertexPositionNormalTextureColor(positions[i], normals[i], texcoords[i], Color.White);
                }

                // 인덱스 읽기
                var indices = prim.GetIndexAccessor().AsIndicesArray();
                result.indices = new short[indices.Count];
                for (int i = 0; i < result.indices.Length; i++)
                {
                    result.indices[i] = (short)indices[i];
                }
            }

            return result;
        }
    }
}

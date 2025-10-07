using SharpGLTF.Schema2;
using System.IO;

namespace MGAlienLib
{
    public static class MeshImporter
    {
        public static Mesh LoadSingleMesh(Stream fstream)
        {
            Mesh result = new Mesh();
            var model = ModelRoot.ReadGLB(fstream);
            var mesh = model.LogicalMeshes[0];

            var prim = mesh.Primitives[0];
            {
                // 위치(Vertex Position) 읽기
                var positions = prim.GetVertexAccessor("POSITION")
                                    .AsVector3Array();

                // 노멀 읽기
                var normals = prim.GetVertexAccessor("NORMAL")
                                  ?.AsVector3Array();

                // 컬러 읽기
                var colors = prim.GetVertexAccessor("COLOR_0")?.AsColorArray();

                // UV 읽기
                var texcoords = prim.GetVertexAccessor("TEXCOORD_0")
                                    ?.AsVector2Array();

                result.v = new Mesh.VertexPositionNormalTextureColor[positions.Count];

                for (int i = 0; i < result.v.Length; i++)
                {
                    result.v[i] = new Mesh.VertexPositionNormalTextureColor(positions[i], normals[i], texcoords[i], colors[i]);
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

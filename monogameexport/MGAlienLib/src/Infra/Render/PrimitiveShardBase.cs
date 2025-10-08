
using Microsoft.Xna.Framework;
using System;

namespace MGAlienLib
{
    public class PrimitiveShardBase
    {
        protected virtual int TrianglesCount => 0;
        protected virtual int VerticesCount => 0;
        public int GetVerticesCount() => VerticesCount;

        protected Vector3[] pos;
        protected Vector3[] transformedPos;
        protected Vector2[] uvs;
        protected Vector4[] extData;
        protected VertexPositionColorTextureExt[] vstream;
        protected short[] istream;

        protected Color _color = Color.White;
        protected Vector2 _offset = Vector2.Zero;
        protected Vector2 _size = Vector2.One;
        protected Vector2 _pivot = Vector2.Zero;
        protected Transform _transform = new Transform();
        protected bool _FlipX = false;
        protected Rectangle? _sourceRect; // UV 일부를 위한 소스 영역 (null이면 전체 텍스처)
        protected Vector2 _textureSize = Vector2.One; // 텍스처 크기 (기본값 1x1)
        protected int _scissorsID = -1;

        protected bool vertexDirty = true;

        public int scissorsID
        {
            get => _scissorsID;
            set
            {
                if (_scissorsID != value)
                {
                    _scissorsID = value;
                    vertexDirty = true;
                }
            }
        }

        public Color color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    vertexDirty = true;
                }
            }
        }

        public Vector2 offset
        {
            get => _offset;
            set
            {
                if (_offset != value)
                {
                    _offset = value;
                    vertexDirty = true;
                }
            }
        }

        public Vector2 size
        {
            get => _size;
            set
            {
                if (_size != value)
                {
                    _size = value;
                    vertexDirty = true;
                }
            }
        }

        public Vector2 pivot
        {
            get => _pivot;
            set
            {
                if (_pivot != value)
                {
                    _pivot = value;
                    vertexDirty = true;
                }
            }
        }

        public Transform transform
        {
            get => _transform;
            set
            {
                if (_transform != value)
                {
                    if (_transform.position != value.position)
                    {
                        _transform.position = value.position;
                        vertexDirty = true;
                    }
                    if (_transform.rotation != value.rotation)
                    {
                        _transform.rotation = value.rotation;
                        vertexDirty = true;
                    }
                    if (_transform.scale != value.scale)
                    {
                        _transform.scale = value.scale;
                        vertexDirty = true;
                    }
                }
            }
        }

        public bool Flip
        {
            get => _FlipX;
            set
            {
                if (_FlipX != value)
                {
                    _FlipX = value;
                    vertexDirty = true;
                }
            }
        }

        public Rectangle? sourceRect
        {
            get => _sourceRect;
            set
            {
                if (_sourceRect != value)
                {
                    _sourceRect = value;
                    vertexDirty = true;
                }
            }
        }

        public Vector2 textureSize
        {
            get => _textureSize;
            set
            {
                if (_textureSize != value)
                {
                    _textureSize = value;
                    vertexDirty = true;
                }
            }
        }

        public PrimitiveShardBase()
        {
            pos = new Vector3[VerticesCount];
            transformedPos = new Vector3[VerticesCount];
            uvs = new Vector2[VerticesCount];
            extData = new Vector4[VerticesCount];
            vstream = new VertexPositionColorTextureExt[VerticesCount];
            istream = new short[TrianglesCount * 3];
        }

        protected virtual void UpdatePos()
        {

        }

        protected virtual void UpdateVertices()
        {

        }

        protected virtual void UpdateIndices(int baseIndex)
        {

        }

        public void Render(RenderChunk chunk)
        {
            if (vertexDirty)
            {
                UpdatePos();
                UpdateVertices();
                vertexDirty = false;
            }

            Array.Copy(pos, transformedPos, pos.Length);
            transform.TransformPoints(transformedPos);
            for (int i = 0; i < transformedPos.Length; i++)
            {
                vstream[i].Position = transformedPos[i];
            }


            UpdateIndices(chunk.vertexCount);

            chunk.AddVertex(vstream);
            chunk.AddIndices(istream);
        }

    }
}

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MGAlienLib
{
    /// <summary>
    /// draw 순서를 조정해서
    /// primitive 를 효율적으로 그리기 위해 사용하는 클래스
    /// 
    /// note : 현재 2D, dynamic batching 만 고려해서 만들었기 때문에, 기능을 확장해야 한다
    /// </summary>
    public class RenderChunk
    {
        public static readonly int MaxVertexCount = 60000;
        public static readonly int MaxIndexCount = 60000;

        public Material material;
        public VertexPositionColorTextureExt[] vertexStream = new VertexPositionColorTextureExt[MaxVertexCount];
        public int vertexCount = 0;
        public short[] indexStream = new short[MaxIndexCount];
        public int indexCount = 0;
        public PrimitiveBatch PrimitiveBatch = null;

        public void AddVertex(VertexPositionColorTextureExt[] source)
        {
            if (vertexCount + source.Length > MaxVertexCount)
            {
                throw new InvalidOperationException("Too many vertices in a single batch.");
            }

            Array.Copy(source, 0, vertexStream, vertexCount, source.Length);
            vertexCount += source.Length;
        }

        public void AddIndices(short[] source)
        {
            if (indexCount + source.Length > MaxIndexCount)
            {
                throw new InvalidOperationException("Too many indices in a single batch.");
            }
            Array.Copy(source, 0, indexStream, indexCount, source.Length);
            indexCount += source.Length;
        }
    }

    /// <summary>
    /// 렌더링 상태를 나타내는 클래스
    /// </summary>
    public class RenderState
    {
        public Camera camera;
        public Matrix viewProjectionMatrix;
        public int activeMaskingIndex;

        public Dictionary<string, RenderChunk> chunks = new();
        public List<Action> commands = new(); // note : 임시구현

        public void CheckAndAddPrimitiveBatch(Material material)
        {
            if (chunks.ContainsKey(material.id) == false)
            {
                chunks[material.id] = new RenderChunk
                {
                    material = material
                };

                chunks[material.id].PrimitiveBatch =
                    new PrimitiveBatch(material);
            }
        }

        public void CheckAndAddCommand(Action command)
        {
            commands.Add(command);
        }

        public void Reset()
        {
            activeMaskingIndex = 0;
            foreach (var kv in chunks)
            {
                kv.Value.vertexCount = 0;
                kv.Value.indexCount = 0;
            }

            commands.Clear(); // note : 임시구현
        }
    }

    /// <summary>
    /// 렌더 큐를 관리하는 클래스
    /// </summary>
    public class RenderQueue : ManagerBase
    {
        private List<Camera> cams = new();
        private List<Renderable> renderables = new();
        private List<RenderState> renderStates = new();

        public RenderQueue(GameBase owner) : base(owner)
        {
        }

        public override void OnPostInitialize()
        {
            base.OnPostInitialize();
        }

        public void AddCamera(Camera camera)
        {
            if (cams.Contains(camera)) return;
            cams.Add(camera);
        }

        public void RemoveCamera(Camera camera)
        {
            if (!cams.Contains(camera)) return;
            cams.Remove(camera);
        }


        public override void OnPreDraw()
        {
            var root = GameBase.Instance.hierarchyManager.GetRootTransform();
            renderables = root.GetComponentsInChildren<Renderable>();

            // making 처리
            UIMasking.GetReadyForRender();

            int totalBatchCount = 0;
            int totalVertexCount = 0;

            // note :
            //
            // state 0 에서 직접 그려도 되고
            // sprite 등은 dynamic batching을 위해 chunk 에 넣는다
            // Renderable.Render 는 여기에서 호출된다
            //
            // state 1 에서는 chunk 를 그린다

            for (int stage = 0; stage < 2; stage++)
            {
                // renderPriority 순서로 정렬
                var sortedCams = new List<Camera>(cams);
                sortedCams.Sort((a, b) =>
                {
                    return a.renderPriority.CompareTo(b.renderPriority);
                });

                for (int i_cam = 0; i_cam < sortedCams.Count; i_cam++)
                {
                    var cam = sortedCams[i_cam];
                    if (cam.gameObject.active == false) continue;
                    if (cam.enabled == false) continue;

                    if (renderStates.Count <= i_cam) renderStates.Add(new RenderState());

                    var renderState = renderStates[i_cam];


                    cam.internal_GetReadyToRender(stage);

                    if (stage == 0)
                    {
                        renderState.Reset();
                        renderState.camera = cam;
                        renderState.viewProjectionMatrix = cam.matViewProjection;

                        // stage 0 : add renderables to batch, draw to render target
                        // render target 을 사용할 때, backbuffer 가 black 으로 채워진다.
                        // 즉, 실제 draw 를 하기 전에 render texture 들을 먼저 그려야 한다.

                        // todo : alpha blend 될 애들은 정렬 필요
                        var sortedRenderables = renderables;

                        // renderables
                        foreach (var renderable in sortedRenderables)
                        {
                            if ((cam.cullingMask & (1 << renderable.gameObject.layer)) == 0) continue;
                            if (renderable.gameObject.active == false) continue;
                            if (renderable.enabled == false) continue;

                            renderable.internal_Render(renderState);
                        }
                    }
                    else if (stage == 1)
                    {
                        // stage 1 : draw to backbuffer

                        // draw primitives batches
                        {
                            var keys = renderState.chunks.Keys;
                            // keys 를 material 의 renderPriority 로 정렬
                            var sortedKeys = new List<string>(keys);
                            sortedKeys.Sort((a, b) =>
                            {
                                return renderState.chunks[a].material.renderPriority
                                    .CompareTo(renderState.chunks[b].material.renderPriority);
                            });

                            foreach (var key in sortedKeys)
                            {
                                var chunk = renderState.chunks[key];
                                if (chunk.vertexCount == 0) continue;
                                totalBatchCount++;
                                totalVertexCount += chunk.vertexCount;
                                chunk.PrimitiveBatch.Draw<VertexPositionColorTextureExt>(cam, chunk);
                            }
                        }

                        // 임시구현 : draw commands
                        {
                            foreach(var command in renderState.commands)
                            {
                                command?.Invoke();
                            }
                        }

                        // internal render : debug draw 나 tool 내부 용
                        // UI 레이어는 internalRenderManager 에서 제외
                        // note : UI layer 와 다른 layer 를 동시에 그릴 경우는 어쩌지? 그럴 경우 없나?
                        if (cam.cullingMask != (1 << LayerMask.NameToLayer("UI")))
                        {
                            owner.internalRenderManager.OnRenderQ(renderState);
                        }
                    }
                }
            }

            // internal render
            owner.internalRenderManager.OnEndRenderQ();
            owner.performanceManager.ReportRenderStatistics(totalBatchCount, totalVertexCount);
        }

        public void OnAddGameObject(GameObject gameObject)
        {
            //Logger.Log("Add: "+gameObject.name);
            //var cam = gameObject.GetComponent<Camera>();
            //if (cam != null) AddCamera(cam);
            //renderables.UnionWith(gameObject.GetComponentsInChildren<Renderable>());
        }

        public void OnRemoveGameObject(GameObject gameObject)
        {
            //Logger.Log("Remove: "+gameObject.name);
            //var cam = gameObject.GetComponent<Camera>();
            //if (cam != null) RemoveCamera(cam);
            //renderables.ExceptWith(gameObject.GetComponentsInChildren<Renderable>());
        }

    }
}

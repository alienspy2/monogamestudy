
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MGAlienLib
{
    public class UIMasking : UIRenderable
    {
        public static readonly bool IsAddableFromInspector = true;

        private static readonly int MaxScissorsCount = 16;
        private static Vector4[] scissorsDataTable = null;
        private static List<UIMasking> maskings = new();


        public static void GetReadyForRender()
        {
            if (scissorsDataTable == null)
            {
                scissorsDataTable = new Vector4[MaxScissorsCount];
            }

            for (int i = 0; i < MaxScissorsCount; i++)
            {
                scissorsDataTable[i] = new Vector4(0, 0, 0, 0);
            }

            int indexer = 0;
            foreach (var masking in maskings)
            {
                masking._scissorID = indexer++;
                if (indexer >= MaxScissorsCount)
                {
                    Logger.Log("Too many scissors! Max is " + MaxScissorsCount);
                    break;
                }

                var scissorsData = scissorsDataTable[masking.scissorsID];
                scissorsData.X = masking.UITransform.accumulatedRect.X;
                scissorsData.Y = masking.UITransform.accumulatedRect.Y;
                scissorsData.Z = masking.UITransform.accumulatedRect.Width;
                scissorsData.W = masking.UITransform.accumulatedRect.Height;
                scissorsDataTable[masking.scissorsID] = scissorsData;
            }

            var shader = GameBase.Instance.shaderManager.GetShaderByName("MG/UI/TTSFont");
            shader.SetVector4Array("_Scissors", scissorsDataTable);
            shader = GameBase.Instance.shaderManager.GetShaderByName("MG/UI/unlit");
            shader.SetVector4Array("_Scissors", scissorsDataTable);
        }


        private int _scissorID = -1;


        public int scissorsID
        {
            get { return _scissorID; }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            maskings.Add(this);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            maskings.Remove(this);
        }


    }
}

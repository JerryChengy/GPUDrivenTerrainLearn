using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PVS
{
    public struct SinglePatch
    {
        public Vector2 position;
        public Vector2 minMaxHeight;
        public uint lod ;
        public Vector3Int lodTrans;
        public int loadTransPad;

        public bool IsValid()
        {
            return !(position.x == 0 && position.y == 0);
        }
    }
    public class PatchInfo
    {
        public Mesh patchMesh;
        public static Dictionary<Vector3, List<SinglePatch>> allPosPatchList = new Dictionary<Vector3, List<SinglePatch>>();
        public static Camera sampleCamera;

        public static void Init(Camera camera)
        {
            PatchInfo.sampleCamera = camera;
            allPosPatchList.Clear();
        }
        public static void ReadFromPatchBuffer(ComputeBuffer buffer)
        {
            if (sampleCamera == null)
            {
                return;
            }

            Vector3 samplePos = sampleCamera.transform.position;
            if (allPosPatchList.ContainsKey(samplePos))
            {
                return;
            }
            var data = new SinglePatch[buffer.count];
            buffer.GetData(data);
            List<SinglePatch> patchList = new List<SinglePatch>();
            int index = 0;
            while (index < buffer.count)
            {
                if (!data[index].IsValid())
                {
                    break;
                }
                patchList.Add(data[index]);
                index++;
            }
            allPosPatchList.Add(sampleCamera.transform.position, patchList);
        }
    }
}


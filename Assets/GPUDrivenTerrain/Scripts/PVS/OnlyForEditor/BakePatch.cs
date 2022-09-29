using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;

#endif
using UnityEngine;
using UnityEngine.Serialization;

namespace PVS
{
    public class BakePatch
    {
        public PatchAsset patchAsset;
        private Camera m_bakeCamera;
        private Terrain m_bakeTerrain;

        public void Init(Camera camera, Terrain terrain)
        {
            m_bakeCamera = camera;
            m_bakeTerrain = terrain;
            patchAsset = new PatchAsset();
        }
        public void ReadFromPatchBuffer(ComputeBuffer buffer)
        {
            if (m_bakeCamera == null)
            {
                return;
            }

            Vector3 samplePos = m_bakeCamera.transform.position;
            Vector3Int logicPos = PatchUtility.WorldToLogicPos(m_bakeTerrain, samplePos, true, Vector2Int.zero);
            if (patchAsset.allPosPatchDict.ContainsKey(logicPos))
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
            patchAsset.allPosPatchDict.Add(logicPos, patchList);
        }
    }
}


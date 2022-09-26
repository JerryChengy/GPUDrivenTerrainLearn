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
    [System.Serializable]
    public struct SinglePatch
    {
        public Vector2 position;
        public Vector2 minMaxHeight;
        public byte lod ;
        public Vector3Int lodTrans;
        public int loadTransPad;

        public bool IsValid()
        {
            return !(position.x == 0 && position.y == 0);
        }

        public void Serialize(MemoryStream stream)
        {
            FileUtils.WriteVector2(stream, position);
            FileUtils.WriteByte(stream, lod);
            FileUtils.WriteVector3Byte(stream, lodTrans);
        }
    }
    [System.Serializable]
    public class SinglePatchWrap
    {
        public Vector3Int logicPos;
        public List<SinglePatch> PatchList;
    }
    
    public class PatchAsset : ScriptableObject
    {
        public List<SinglePatchWrap> allPosPatchList = new List<SinglePatchWrap>();
        [NonSerialized]
        public Dictionary<Vector3, List<SinglePatch>> allPosPatchDict = new Dictionary<Vector3, List<SinglePatch>>();

        public void Serialize(MemoryStream stream)
        {
#if UNITY_EDITOR
            for (int i = 0; i < allPosPatchList.Count; i++)
            {
                SinglePatchWrap singlePatchWrap = allPosPatchList[i];
                FileUtils.WriteVector3Int(stream, singlePatchWrap.logicPos);
                FileUtils.WriteInt(stream, singlePatchWrap.PatchList.Count);
                EditorUtility.DisplayProgressBar("将patch信息写入磁盘", string.Format("writing {0}/{1}", i, allPosPatchList.Count), (float)i/allPosPatchList.Count);
                for (int j = 0; j < singlePatchWrap.PatchList.Count; j++)
                {
                    singlePatchWrap.PatchList[j].Serialize(stream);
                }
            }
            EditorUtility.ClearProgressBar();
#endif
        }
    }
    public class PatchInfo
    {
        public Mesh patchMesh;
        public PatchAsset patchAsset;
        public Camera sampleCamera;
        private int m_mapSize;
        private Terrain m_terrain;

        public void Init(Camera camera, int mapSize, Terrain terrain)
        {
            sampleCamera = camera;
            m_mapSize = mapSize;
            m_terrain = terrain;
            patchAsset = new PatchAsset();
            patchAsset.allPosPatchDict.Clear();
        }

        /// <summary>
        /// logicPos: {[0,0,0],[63,63,2]}
        /// </summary>
        /// <param name="worldPos"></param>
        /// <returns></returns>
        private Vector3Int WorldToLogicPos(Vector3 worldPos)
        {
            Vector3Int logicPos = new Vector3Int();
            int sampleTileSize = CameraSample.SampleTileSize;
            logicPos.x = ((int)worldPos.x) / sampleTileSize;
            logicPos.z = ((int)worldPos.z) / sampleTileSize;
            Vector3 pos = new Vector3(logicPos.x * sampleTileSize + sampleTileSize / 2, 0,
                logicPos.z * sampleTileSize + sampleTileSize / 2);
            float terrainHeight = m_terrain.SampleHeight(pos);
            logicPos.y = Mathf.FloorToInt((worldPos.y - terrainHeight) / CameraSample.SampleHeightStep);
            return logicPos;
        }
        
        public void GenPatchList()
        {
            patchAsset.allPosPatchList.Clear();
            foreach (var pairPatch in patchAsset.allPosPatchDict)
            {
                SinglePatchWrap patch = new SinglePatchWrap();
                patch.logicPos = WorldToLogicPos(pairPatch.Key);
                patch.PatchList = pairPatch.Value;
                patchAsset.allPosPatchList.Add(patch);
                /*//test
                if (patchAsset.allPosPatchList.Count >= 10)
                {
                    break;
                    
                }*/
            }
        }

        public void ReadFromPatchBuffer(ComputeBuffer buffer)
        {
            if (sampleCamera == null)
            {
                return;
            }

            Vector3 samplePos = sampleCamera.transform.position;
            if (patchAsset.allPosPatchDict.ContainsKey(samplePos))
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
            patchAsset.allPosPatchDict.Add(sampleCamera.transform.position, patchList);
        }
    }
}


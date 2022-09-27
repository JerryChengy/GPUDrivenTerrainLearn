using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;

#endif
using UnityEngine;
using UnityEngine.Serialization;
//本文件存放最终渲染用的Patch Info数据结构
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

        public void Serialize(MemoryStream stream, bool bWrite)
        {
            if (bWrite)
            {
                FileUtils.WriteShort2(stream, position);
                FileUtils.WriteByte(stream, lod);
                FileUtils.WriteVector3Byte(stream, lodTrans);
            }
            else
            {
                position = FileUtils.ReadShort2(stream);
                lod = FileUtils.ReadByte(stream);
                lodTrans = FileUtils.ReadVector3Byte(stream);
            }
        }
    }
    [System.Serializable]
    public class SinglePatchWrap
    {
        public Vector3Int logicPos;
        public List<SinglePatch> PatchList;
    }
    
    //暂时继承于ScriptableObject，保留编辑器查看Patch信息的接口
    public class PatchAsset : ScriptableObject
    {
        public List<SinglePatchWrap> allPosPatchList = new List<SinglePatchWrap>();
        [NonSerialized]
        public Dictionary<Vector3Int, List<SinglePatch>> allPosPatchDict = new Dictionary<Vector3Int, List<SinglePatch>>();

        public void GenPatchDict()
        {
            allPosPatchDict.Clear();
            foreach (var singlePatchWrap in allPosPatchList)
            {
                if (!allPosPatchDict.ContainsKey(singlePatchWrap.logicPos))
                {
                    allPosPatchDict[singlePatchWrap.logicPos] = singlePatchWrap.PatchList;
                }
            }
        }
        public void GenPatchList()
        {
            allPosPatchList.Clear();
            foreach (var pairPatch in allPosPatchDict)
            {
                SinglePatchWrap patch = new SinglePatchWrap();
                patch.logicPos = pairPatch.Key;
                patch.PatchList = pairPatch.Value;
                allPosPatchList.Add(patch);
            }
        }
        public void Serialize(MemoryStream stream, bool bWrite)
        {
            if (bWrite)
            {
#if UNITY_EDITOR
                FileUtils.WriteInt(stream, allPosPatchList.Count);
                for (int i = 0; i < allPosPatchList.Count; i++)
                {
                    SinglePatchWrap singlePatchWrap = allPosPatchList[i];
                    FileUtils.WriteVector3Int(stream, singlePatchWrap.logicPos);
                    FileUtils.WriteInt(stream, singlePatchWrap.PatchList.Count);
                    EditorUtility.DisplayProgressBar("将patch信息写入磁盘", string.Format("writing {0}/{1}", i, allPosPatchList.Count), (float)i/allPosPatchList.Count);
                    for (int j = 0; j < singlePatchWrap.PatchList.Count; j++)
                    {
                        singlePatchWrap.PatchList[j].Serialize(stream, bWrite);
                    }
                }
                EditorUtility.ClearProgressBar();
#endif
            }
            else
            {
                allPosPatchList.Clear();
                int patchCount = FileUtils.ReadInt(stream);
                for (int i = 0; i < patchCount; i++)
                {
                    Vector3Int logicPos = FileUtils.ReadVector3Int(stream);
                    int singlePosPatchCount = FileUtils.ReadInt(stream);
                    List<SinglePatch> singlePosPatchList = new List<SinglePatch>();
                    for (int j = 0; j < singlePosPatchCount; j++)
                    {
                        SinglePatch singlePatch = new SinglePatch();
                        singlePatch.Serialize(stream, bWrite);
                        singlePosPatchList.Add(singlePatch);
                    }

                    SinglePatchWrap singlePatchWrap = new SinglePatchWrap();
                    singlePatchWrap.logicPos = logicPos;
                    singlePatchWrap.PatchList = singlePosPatchList;
                    allPosPatchList.Add(singlePatchWrap);
                }
            }

        }
    }

}


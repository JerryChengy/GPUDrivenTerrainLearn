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
    //优化后存储结构
    class PatchBlockSameZ
    {
        public int minX;
        public int maxX;
        public int z;

        public void Serialize(MemoryStream stream, bool bWrite)
        {
            if (bWrite)
            {
                FileUtils.WriteShort3(stream, new Vector3Int(minX, maxX, z));
            }
            else
            {
                Vector3Int val = FileUtils.ReadShort3(stream);
                minX = val.x;
                maxX = val.y;
                z = val.z;
            }
        }
    }
    class PerLodPatch
    {
        public byte lod;

        public Dictionary<Vector3Int, List<PatchBlockSameZ>> LodLodTransPatchList =
            new Dictionary<Vector3Int, List<PatchBlockSameZ>>();

        public void Serialize(MemoryStream stream, bool bWrite)
        {
            if (bWrite)
            {
                int lodTransCount = LodLodTransPatchList.Count;
                FileUtils.WriteByte(stream, (byte)lodTransCount);
                foreach (var pair in LodLodTransPatchList)
                {
                    FileUtils.WriteVector3Byte(stream, pair.Key);
                    FileUtils.WriteUShort(stream, (ushort)pair.Value.Count);
                    for (int i = 0; i < pair.Value.Count; i++)
                    {
                        PatchBlockSameZ patchBlockSameZ = pair.Value[i];
                        patchBlockSameZ.Serialize(stream, bWrite);
                    }
                }
            }
            else
            {
                int lodTransCount = FileUtils.ReadByte(stream);
                for (int i = 0; i < lodTransCount; i++)
                {
                    Vector3Int lodTrans = FileUtils.ReadVector3Byte(stream);
                    ushort patchBlockSameZCount = FileUtils.ReadUShort(stream);
                    for (int j = 0; j < patchBlockSameZCount; j++)
                    {
                        PatchBlockSameZ patchBlockSameZ = new PatchBlockSameZ();
                        patchBlockSameZ.Serialize(stream, bWrite);
                        if (!LodLodTransPatchList.ContainsKey(lodTrans))
                        {
                            List<PatchBlockSameZ> patchBlockSameZList = new List<PatchBlockSameZ>();
                            patchBlockSameZList.Add(patchBlockSameZ);
                            LodLodTransPatchList[lodTrans] = patchBlockSameZList;
                        }
                        else
                        {
                            LodLodTransPatchList[lodTrans].Add(patchBlockSameZ);
                        }
                    }

                    
                }
            }
            
        }
    }
    class PerLogicPosPatchs
    {
        private static int MAX_LOD_COUNT = 8;
        public Vector3Int logicPos;
        public PerLodPatch[] allLodPatchs = new PerLodPatch[MAX_LOD_COUNT];

        public PerLogicPosPatchs()
        {
            for (int i = 0; i < allLodPatchs.Length; i++)
            {
                allLodPatchs[i] = new PerLodPatch();
            }
        }

        public void Serialize(MemoryStream stream, bool bWrite)
        {
            if (bWrite)
            {
                FileUtils.WriteVector3Int(stream, logicPos);
                int lodCount = 0;
                for (int i = 0; i < allLodPatchs.Length; i++)
                {
                    if (allLodPatchs[i].LodLodTransPatchList.Count == 0)
                    {
                        lodCount = i + 1;
                        break;
                    }
                }

                if (lodCount > 0)
                {
                    FileUtils.WriteByte(stream, (byte)lodCount);
                    for (int i = 0; i < lodCount; i++)
                    {
                        allLodPatchs[i].Serialize(stream, bWrite);
                    }
                }
                
            }
            else
            {
                logicPos = FileUtils.ReadVector3Int(stream);
                int lodCount = FileUtils.ReadByte(stream);
                for (int i = 0; i < lodCount; i++)
                {
                    allLodPatchs[i].Serialize(stream, bWrite);
                }
            }
        }
    }
    
    //优化前存储结构
    //内存结构
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
    public class SinglePatchWrap
    {
        public Vector3Int logicPos;
        public List<SinglePatch> PatchList;
    }
    
    public class PatchAsset
    {
        public int LOD0_PATCH_SIZE = 8;
        //Runtime data
        public Dictionary<Vector3Int, List<SinglePatch>> allPosPatchDict = new Dictionary<Vector3Int, List<SinglePatch>>();
        
        public Vector2Int mapMinMaxPos;
        
        //优化后序列化结构
        private List<PerLogicPosPatchs> allPosPatchListByLod = new List<PerLogicPosPatchs>();
        
        //优化前序列化结构
        private List<SinglePatchWrap> allPosPatchList = new List<SinglePatchWrap>();

        private void GenLodPatchDict()
        {
            allPosPatchDict.Clear();
            foreach (var perLogicPosPatchs in allPosPatchListByLod)
            {
                if (!allPosPatchDict.ContainsKey(perLogicPosPatchs.logicPos))
                {
                    List<SinglePatch> patchList = new List<SinglePatch>();
                    for (int i = 0; i < perLogicPosPatchs.allLodPatchs.Length; i++)
                    {
                        PerLodPatch perLodPatch = perLogicPosPatchs.allLodPatchs[i];
                        int lod = i;
                        foreach (var pair in perLodPatch.LodLodTransPatchList)
                        {
                            Vector3Int lodTrans = pair.Key;
                            foreach (var patchBlockSameZ in pair.Value)
                            {
                                int lodPatchSize = LOD0_PATCH_SIZE * (int)Math.Pow(2, lod);
                                int patchCount = (patchBlockSameZ.maxX - patchBlockSameZ.minX) / lodPatchSize + 1;
                                for (int j = 0; j < patchCount; j++)
                                {
                                    SinglePatch singlePatch = new SinglePatch();
                                    singlePatch.lod = (byte)lod;
                                    singlePatch.lodTrans = lodTrans;
                                    singlePatch.position = new Vector2(patchBlockSameZ.minX + lodPatchSize * j, patchBlockSameZ.z);
                                    patchList.Add(singlePatch);
                                }
                            }
                        }
                    }
                    allPosPatchDict[perLogicPosPatchs.logicPos] = patchList;
                }
            }
        }
        private void GenLodPatchList()
        {
            allPosPatchListByLod.Clear();
            foreach (var pair in allPosPatchDict)
            {
                PerLogicPosPatchs perLogicPosPatchs = new PerLogicPosPatchs();
                allPosPatchListByLod.Add(perLogicPosPatchs);
                perLogicPosPatchs.logicPos = pair.Key;
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    SinglePatch singlePatch = pair.Value[i];
                    PerLodPatch perLodPatch = perLogicPosPatchs.allLodPatchs[singlePatch.lod];
                    perLodPatch.lod = singlePatch.lod;
                    PatchBlockSameZ patchBlockSameZ = new PatchBlockSameZ();
                    patchBlockSameZ.z = (int)singlePatch.position.y;
                    patchBlockSameZ.minX = (int)singlePatch.position.x;
                    patchBlockSameZ.maxX = patchBlockSameZ.minX;
                    if (!perLodPatch.LodLodTransPatchList.ContainsKey(singlePatch.lodTrans))
                    {
                        List<PatchBlockSameZ> patchBlockSameZList = new List<PatchBlockSameZ>();
                        patchBlockSameZList.Add(patchBlockSameZ);
                        perLodPatch.LodLodTransPatchList[singlePatch.lodTrans] = patchBlockSameZList;
                    }
                    else
                    {
                        List<PatchBlockSameZ> patchBlockSameZList =
                            perLodPatch.LodLodTransPatchList[singlePatch.lodTrans];
                        PatchBlockSameZ lastpatchBlockSameZ = patchBlockSameZList[patchBlockSameZList.Count - 1];
                        if (patchBlockSameZ.minX - lastpatchBlockSameZ.maxX == LOD0_PATCH_SIZE*Math.Pow(2, perLodPatch.lod))//合并
                        {
                            lastpatchBlockSameZ.maxX = patchBlockSameZ.minX;
                        }
                        else
                        {
                            patchBlockSameZList.Add(patchBlockSameZ);
                        }
                    }
                }
            }   
        }

        #region old patch
        private void GenPatchDict()
        {
            mapMinMaxPos = Vector2Int.zero;
            int iIndex = 0;
            allPosPatchDict.Clear();
            foreach (var singlePatchWrap in allPosPatchList)
            {
                if (!allPosPatchDict.ContainsKey(singlePatchWrap.logicPos))
                {
                    //地图固定为POT
                    if (iIndex == 0)
                    {
                        mapMinMaxPos.x = singlePatchWrap.logicPos.x;
                        mapMinMaxPos.y = singlePatchWrap.logicPos.x;
                    }
                    else
                    {
                        mapMinMaxPos.x = Math.Min(mapMinMaxPos.x, singlePatchWrap.logicPos.x);
                        mapMinMaxPos.y = Math.Max(mapMinMaxPos.y, singlePatchWrap.logicPos.x);
                    }
                    
                    allPosPatchDict[singlePatchWrap.logicPos] = singlePatchWrap.PatchList;
                }

                iIndex++;
            }
        }
        private void GenPatchList()
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
        

        #endregion


        public void GenRuntimeData()
        {
            GenLodPatchDict();
        }
        public void GenSerializeData()
        {
            GenLodPatchList();
        }

        public void SerializeNew(MemoryStream stream, bool bWrite)
        {
            if (bWrite)
            {
#if UNITY_EDITOR
                FileUtils.WriteInt(stream, allPosPatchListByLod.Count);
                for (int i = 0; i < allPosPatchListByLod.Count; i++)
                {
                    PerLogicPosPatchs perLogicPosPatchs = allPosPatchListByLod[i];
                    perLogicPosPatchs.Serialize(stream, bWrite);
                }
                EditorUtility.ClearProgressBar();
#endif
            }
            else
            {
                allPosPatchListByLod.Clear();
                int count = FileUtils.ReadInt(stream);
                for (int i = 0; i < count; i++)
                {
                    PerLogicPosPatchs perLogicPosPatchs = new PerLogicPosPatchs();
                    perLogicPosPatchs.Serialize(stream, bWrite);
                    allPosPatchListByLod.Add(perLogicPosPatchs);
                }
            }
        }
        public void Serialize(MemoryStream stream, bool bWrite)
        {
            if (bWrite)
            {
#if UNITY_EDITOR
                //计算lodtrans的最大值3
                /*int maxLodTrans = 0;
                for (int i = 0; i < allPosPatchList.Count; i++)
                {
                    SinglePatchWrap singlePatchWrap = allPosPatchList[i];
                    for (int j = 0; j < singlePatchWrap.PatchList.Count; j++)
                    {
                        maxLodTrans = Math.Max(maxLodTrans, singlePatchWrap.PatchList[j].lodTrans.x);
                        maxLodTrans = Math.Max(maxLodTrans, singlePatchWrap.PatchList[j].lodTrans.y);
                        maxLodTrans = Math.Max(maxLodTrans, singlePatchWrap.PatchList[j].lodTrans.z);
                    }
                }*/
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


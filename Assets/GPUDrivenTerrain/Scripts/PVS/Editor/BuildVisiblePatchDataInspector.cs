using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GPUDrivenTerrainLearn;
using UnityEngine;
using UnityEditor;

namespace PVS
{
    [CustomEditor(typeof(BuildVisiblePatchData))]
    public class BuildVisiblePatchDataInspector : Editor
    {
        private BuildVisiblePatchData m_buildVisiblePatchData;

        private int m_debugPosX = 0;
        private int m_debugPosZ = 0;
        public void OnEnable()
        {
            m_buildVisiblePatchData = target as BuildVisiblePatchData;
        }

        private PatchAsset DeserializePatchInfo(string filePath)
        {
            Debug.Log("DeserializePatchInfo begin");
            byte[] data = File.ReadAllBytes(filePath);
            MemoryStream stream = new MemoryStream(data);
            PatchAsset patchAsset = new PatchAsset();
            patchAsset.Serialize(stream, false);
            patchAsset.GenRuntimeData();
            Debug.Log("DeserializePatchInfo end");
            return patchAsset;
        }
        private PatchAsset DeserializePatchInfoNew(string filePath)
        {
            Debug.Log("DeserializePatchInfoNew begin");
            byte[] data = File.ReadAllBytes(filePath);
            MemoryStream stream = new MemoryStream(data);
            PatchAsset patchAsset = new PatchAsset();
            patchAsset.SerializeNew(stream, false);
            patchAsset.GenRuntimeData();
            Debug.Log("DeserializePatchInfoNew end");
            return patchAsset;
        }
        private void SerializePatchInfoNew(PatchAsset patchAsset, string savePath)
        {
            MemoryStream stream = new MemoryStream();
            patchAsset.SerializeNew(stream, true);
            if (stream.Length > 0)
            {
                File.WriteAllBytes(savePath, stream.ToArray());
            }
            AssetDatabase.Refresh();
        }
        private void SerializePatchInfo(PatchAsset patchAsset, string savePath)
        {
            MemoryStream stream = new MemoryStream();
            patchAsset.Serialize(stream, true);
            if (stream.Length > 0)
            {
                File.WriteAllBytes(savePath, stream.ToArray());
            }
            AssetDatabase.Refresh();
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("生成VisiblePatchInfo")) {
                m_buildVisiblePatchData.Build();
                var parentDir = "Assets/GPUDrivenTerrain/";
                var patchDir = "Patch/";
                var wholeDir = Path.Combine(parentDir, patchDir);
                if (!AssetDatabase.IsValidFolder(wholeDir))
                {
                    AssetDatabase.CreateFolder(parentDir, patchDir);
                }
                m_buildVisiblePatchData.bakePatch.patchAsset.GenSerializeData();
              //  SerializePatchInfo(m_buildVisiblePatchData.bakePatch.patchAsset, Path.Combine(wholeDir, "Patch.bytes"));
                SerializePatchInfoNew(m_buildVisiblePatchData.bakePatch.patchAsset, Path.Combine(wholeDir, "PatchNew.bytes"));
                
                //Test Deserialize
                
            }
            if (GUILayout.Button("重置")) {
                m_buildVisiblePatchData.Clear();
            }
           
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("生成QuadTreeMap"))
            {
                TerrainAsset terrainAsset = m_buildVisiblePatchData.terrainAsset;
                new GPUDrivenTerrainLearn.QuadTreeMapEditorBuilder(terrainAsset.MaxLodNodeCount,terrainAsset.MaxLod + 1).
                    BuildAsync();
                   
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("下一个采样点"))
            {
               CameraSample.SampleOneByOne();
                   
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            m_debugPosX = EditorGUILayout.IntField("posx",m_debugPosX);
            m_debugPosZ = EditorGUILayout.IntField("posz",m_debugPosZ);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("采样高度bytes"))
            {
                float h = 0;
                TerrainHeight.GetHeightInterpolated(new Vector3(m_debugPosX, 0, m_debugPosZ), ref h);
                Debug.LogFormat("采样高度bytes height: {0},{1}: {2}",m_debugPosX, m_debugPosZ, h);
            }
            if (GUILayout.Button("采样高度terraindata"))
            {
                float h = m_buildVisiblePatchData.staticTerrain.SampleHeight(new Vector3(m_debugPosX, 0, m_debugPosZ));
                Debug.LogFormat("采样高度terraindata height: {0},{1}: {2}",m_debugPosX, m_debugPosZ, h);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("生成高度文件"))
            {
                Terrain terrain = m_buildVisiblePatchData.staticTerrain;
                int resolution = terrain.terrainData.heightmapResolution;
                Vector3 heightMapScale = terrain.terrainData.heightmapScale;
                float[,] heightData = terrain.terrainData.GetHeights(0, 0, resolution, resolution);
                int headBytesLen = sizeof(float) * 3;
                byte[] heightBytes = new byte[headBytesLen + resolution * resolution * 2];
                float xMax = (resolution - 1) * heightMapScale.x;
                float zMax = (resolution - 1) * heightMapScale.z;
                float yMax = heightMapScale.y;
                byte[] buff = BitConverter.GetBytes(xMax);
                System.Buffer.BlockCopy(buff, 0, heightBytes, 0, sizeof(float));
                buff = BitConverter.GetBytes(zMax);
                System.Buffer.BlockCopy(buff, 0, heightBytes, 0 + sizeof(float), sizeof(float));
                buff = BitConverter.GetBytes(yMax);
                System.Buffer.BlockCopy(buff, 0, heightBytes, 0 + sizeof(float) + sizeof(float), sizeof(float));
                for (int hy = 0; hy < resolution; ++hy)
                {
                    for (int hx = 0; hx < resolution; ++hx)
                    {
                        float val = heightData[hy, hx] * yMax;
                        byte h = (byte)Mathf.FloorToInt(val);
                        byte l = (byte)Mathf.FloorToInt((val - h) * 255f);
                        if (hx == 50 && hy == 50)
                        {
                            float h1 = terrain.SampleHeight(new Vector3(200, 0, 200));
                            int iii = 0;
                            ++iii;
                        }
                        if (hx == 50 && hy == 51)
                        {
                            int iii = 0;
                            ++iii;
                        }
                        if (hx == 51 && hy == 51)
                        {
                            int iii = 0;
                            ++iii;
                        }
                        heightBytes[headBytesLen + hy * resolution * 2 + hx * 2] = h;
                        heightBytes[headBytesLen + hy * resolution * 2 + hx * 2 + 1] = l;
                    }
                }
              
                File.WriteAllBytes("Assets/GPUDrivenTerrain/Textures/terrrain_height.bytes", heightBytes);
                AssetDatabase.Refresh();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("使用PVS渲染"))
            {
                PatchAsset patchAsset = DeserializePatchInfoNew("Assets/GPUDrivenTerrain/Patch/PatchNew.bytes");
                PatchSystem.Instance.SetPatchData(patchAsset);
                PatchSystem.Instance.SetTerrain(m_buildVisiblePatchData.staticTerrain);
                m_buildVisiblePatchData.UseGpuTerrain();
                m_buildVisiblePatchData.GPUTerrain.UsePVS = true;
              //  m_buildVisiblePatchData.GPUTerrain.SetCulledPatchData();
            }
            if (GUILayout.Button("使用传统渲染"))
            {
                m_buildVisiblePatchData.GPUTerrain.UsePVS = false;
            }
            if (GUILayout.Button("读取patch new文件"))
            {
                PatchAsset patchAsset = DeserializePatchInfoNew("Assets/GPUDrivenTerrain/Patch/PatchNew.bytes");
            }
            
            GUILayout.EndHorizontal();
            
        }
    }
}


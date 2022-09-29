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


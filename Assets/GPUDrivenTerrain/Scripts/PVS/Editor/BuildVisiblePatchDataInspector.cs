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
            patchAsset.GenPatchDict();
            Debug.Log("DeserializePatchInfo end");
            return patchAsset;
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
                SerializePatchInfo(m_buildVisiblePatchData.bakePatch.patchAsset, Path.Combine(wholeDir, "Patch.bytes"));
                
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
            if (GUILayout.Button("读取Patch文件"))
            {
                PatchAsset patchAsset = DeserializePatchInfo("Assets/GPUDrivenTerrain/Patch/Patch.bytes");

            }
            GUILayout.EndHorizontal();
            
        }
    }
}


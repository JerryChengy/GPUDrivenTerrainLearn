using System;
using System.Collections;
using System.Collections.Generic;
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

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("生成VisiblePatchInfo")) {
                m_buildVisiblePatchData.Build();
            }
            if (GUILayout.Button("清理环境")) {
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
        }
    }
}


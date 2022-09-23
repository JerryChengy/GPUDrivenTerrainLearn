using System;
using System.Collections;
using System.Collections.Generic;
using GPUDrivenTerrainLearn;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace PVS
{
    /// <summary>
    /// 生成VisiblePatchList的主体类
    /// </summary>
    [ExecuteInEditMode]
    public class BuildVisiblePatchData : MonoBehaviour
    {
        public Terrain staticTerrain;
        public GameObject dynamicTerrain;
        public Texture2D heightMap;
        public TerrainAsset terrainAsset;
        private static bool isBuilding = false;
        public Camera sampleCamera;
        public void Start()
        {
        }

        public  void Build()
        {
            if (isBuilding)
            {
                return;
            }

            isBuilding = true;
            PrepareForBuild();
            
        }

        public void Clear()
        {
            isBuilding = false;
            staticTerrain.gameObject.SetActive(true);
            dynamicTerrain.SetActive(false);
            if (dynamicTerrain.GetComponent<GPUTerrain>())
            {
                DestroyImmediate(dynamicTerrain.GetComponent<GPUTerrain>());
            }
           
            CameraSetting.Restore(sampleCamera);
        }

        /// <summary>
        /// 正式build之前需要做些准备工作: 将Unity原有地形隐藏掉; 将GPUTerrain组件激活; 设置相机到合适的参数;
        /// </summary>
        private void PrepareForBuild()
        {
            //设置相机到合适的参数，用来做GPU光栅化 Occlusion
            CameraSetting.Init(sampleCamera);
            //将Unity原有地形隐藏
            //staticTerrain.gameObject.SetActive(false);
            
            //使用GPUTerrain
            dynamicTerrain.SetActive(true);
            if (dynamicTerrain.GetComponent<GPUTerrain>())
            {
                DestroyImmediate(dynamicTerrain.GetComponent<GPUTerrain>());
            }
            if (!dynamicTerrain.GetComponent<GPUTerrain>())
            {
                var gpuTerrain =  dynamicTerrain.AddComponent<GPUTerrain>();
                gpuTerrain.terrainAsset = AssetDatabase.LoadAssetAtPath<TerrainAsset>("Assets/GPUDrivenTerrain/Terrain.asset");
                gpuTerrain.isFrustumCullEnabled = false;
                gpuTerrain.isHizOcclusionCullingEnabled = false;
                gpuTerrain.hizDepthBias = 0.2f;
                gpuTerrain.boundsHeightRedundance = 4;
                gpuTerrain.distanceEvaluation = 1.2f;
                gpuTerrain.seamLess = true;
            }

            dynamicTerrain.transform.position = staticTerrain.transform.position +
                                                new Vector3(terrainAsset.worldSize.x / 2, 0,
                                                    terrainAsset.worldSize.z / 2);
            //生成相机采样点
            PatchInfo.Init(sampleCamera);
            CameraSample.Init((int)terrainAsset.worldSize.x, staticTerrain,sampleCamera);
          //  StartCoroutine(CameraSample.CameraSampleTraverse(sampleCamera));
        }
       
    }
}


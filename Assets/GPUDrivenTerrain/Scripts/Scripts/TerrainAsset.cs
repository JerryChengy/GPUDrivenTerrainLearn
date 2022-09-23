using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace GPUDrivenTerrainLearn{

    [CreateAssetMenu(menuName = "GPUDrivenTerrainLearn/TerrainAsset")]
    public class TerrainAsset : ScriptableObject
    {
        //Sector的大小，也是LOD0的Node大小
        public const int SECTOR_SIZE = 64;

        [SerializeField]
        private Vector3 _worldSize = new Vector3(2048,1024,2048);
        
        [SerializeField]
        private Texture2D _albedoMap;

        [SerializeField]
        private Texture2D _heightMap;

        [SerializeField]
        private Texture2D _normalMap;

        [SerializeField]
        private Texture2D[] _minMaxHeightMaps;

        [SerializeField]
        private Texture2D[] _quadTreeMaps;

        [SerializeField]
        private ComputeShader _terrainCompute;

        private RenderTexture _quadTreeMap;
        private RenderTexture _minMaxHeightMap;

        private Material _boundsDebugMaterial;

        public int MaxLodNodeCount
        {
            get
            {
                int lod0Count = (int)_worldSize.x / SECTOR_SIZE;
                int lodiCount = lod0Count;
                for (int i = 0; i < MaxLod; i++)
                {
                    lodiCount /= 2;
                }

                return lodiCount;
            }
        }

        //从0开始索引
        public int MaxLod
        {
            get
            {
                int lod0Count = (int)_worldSize.x / SECTOR_SIZE;
                int maxLod = (int)math.log2(lod0Count / 8) + 1;//从0开始索引
                return maxLod;
            }
        }
        public int MaxNodeId
        {
            get
            {
                int nodeCount = 0;
                int lod0Count = (int)_worldSize.x / SECTOR_SIZE;
                nodeCount += lod0Count * lod0Count;
                int lodiCount = lod0Count;
                for (int i = 0; i < MaxLod; i++)
                {
                    lodiCount *= 2;
                    nodeCount += lodiCount * lodiCount;
                }

                return nodeCount - 1;
            }
        }
        public Vector3 worldSize{
            get{
                return _worldSize;
            }
        }
        
        public Texture2D albedoMap{
            get{
                return _albedoMap;
            }
        }

        public Texture2D heightMap{
            get{
                return _heightMap;
            }
        }

        public Texture2D normalMap{
            get{
                return _normalMap;
            }
        }

        public RenderTexture quadTreeMap{
            get{
                if(!_quadTreeMap){
                    _quadTreeMap = TextureUtility.CreateRenderTextureWithMipTextures(_quadTreeMaps,RenderTextureFormat.R16);
                }
                return _quadTreeMap;
            }
        }

        public RenderTexture minMaxHeightMap{
            get{
                if(!_minMaxHeightMap){
                    _minMaxHeightMap = TextureUtility.CreateRenderTextureWithMipTextures(_minMaxHeightMaps,RenderTextureFormat.RG32);
                }
                return _minMaxHeightMap;
            }
        }

        public Material boundsDebugMaterial{
            get{
                if(!_boundsDebugMaterial){
                    _boundsDebugMaterial = new Material(Shader.Find("GPUTerrainLearn/BoundsDebug"));
                }
                return _boundsDebugMaterial;
            }
        }

        public ComputeShader computeShader{
            get{
                return _terrainCompute;
            }
        }

        private static Mesh _patchMesh;

        public static Mesh patchMesh{
            get{
                if(!_patchMesh){
                    _patchMesh = MeshUtility.CreatePlaneMesh(16);
                }
                return _patchMesh;
            }
        }


        private static Mesh _unitCubeMesh;

        public static Mesh unitCubeMesh{
            get{
                if(!_unitCubeMesh){
                    _unitCubeMesh = MeshUtility.CreateCube(1);
                }
                return _unitCubeMesh;
            }
        }
    }
}

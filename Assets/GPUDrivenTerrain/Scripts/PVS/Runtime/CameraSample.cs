using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;

namespace PVS
{
    //同时只会有一个sample camera，这里的静态参数在Runtime时有些会用到，故取名时避开了"Bake"
    public static class CameraSample
    {
        //采样空间间隔(x, z)
        private static int s_sampleTileSize = 32;
        //起始采样点(x, z)
        private static Vector2 s_sampleInitPos = Vector2.zero;
        //Y方向采样次数(y)
        private static int s_sampleHeightNum = 2;
        //采样高度步长(y)
        private static float s_sampleHeightStep = 10;
        //采样相机初始高度
        private static float s_sampleInitHeight = 3;
        //所有采样点
        private static List<Vector3> m_samplePointList = new List<Vector3>();
        //

        private static int s_samplePointIndex = 0;

        private static Camera s_sampleCamera;

        public static int SampleHeightNum
        {
            get { return s_sampleHeightNum; }
        }

        public static float SampleInitHeight
        {
            get { return s_sampleInitHeight; }
        }
        public static float SampleHeightStep
        {
            get { return s_sampleHeightStep; }
        }
        public static int SampleTileSize
        {
            get { return s_sampleTileSize; }
        }
#if UNITY_EDITOR

        public static void Init(int mapSize, Terrain terrain, Camera  camera)
        {
            InitSamplePointList(mapSize, terrain);
            s_sampleCamera = camera;
            s_sampleCamera.transform.rotation = quaternion.identity;
        }
        private static void InitSamplePointList(int mapSize, Terrain terrain)
        {
            m_samplePointList.Clear();
            s_samplePointIndex = 0;
            
            int sampleCountXZ = mapSize / s_sampleTileSize;
            
            for (int x = 0; x < sampleCountXZ; x++)
            {
                for (int z = 0; z < sampleCountXZ; z++)
                {
                    Vector3 pos = new Vector3(x * s_sampleTileSize + s_sampleTileSize / 2, 0,
                        z * s_sampleTileSize + s_sampleTileSize / 2);
                    float terrainHeight = terrain.SampleHeight(pos);
                    pos.y = terrainHeight + s_sampleInitHeight;
                    m_samplePointList.Add(pos);
                    for (int i = 0; i < s_sampleHeightNum; i++)
                    {
                        pos.y += s_sampleHeightStep;
                        m_samplePointList.Add(pos);
                    }
                }
            }   
        }

        public static string ProgressInfo(string sampleInfo)
        {
            return sampleInfo + ": " + s_samplePointIndex + "/" + m_samplePointList.Count;
        }
        public static float Progress()
        {
            if (m_samplePointList.Count == 0)
            {
                return 0;
            }
            return (float)s_samplePointIndex / m_samplePointList.Count;
        }
        public static int SampleOneByOne()
        {
            if (s_sampleCamera == null)
            {
                return -1;
            }
            if (s_samplePointIndex == m_samplePointList.Count)
            {
                return -1;
            }
            s_sampleCamera.transform.position = m_samplePointList[s_samplePointIndex++%m_samplePointList.Count];
            return s_samplePointIndex - 1;
        }
                
#endif
    }
    
}


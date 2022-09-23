using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PVS
{
    public class CameraSample
    {
        //采样空间间隔(x, z)
        private static int s_sampleTileSize = 32;
        //起始采样点(x, z)
        private static Vector2 s_sampleInitPos = Vector2.zero;
        //Y方向采样次数(y)
        private static int s_sampleHeightNum = 2;
        //采样高度步长(y)
        private static float s_sampleHeightStep = 10;
        //所有采样点
        private static List<Vector3> m_samplePointList = new List<Vector3>();

        public static void InitSamplePointList(int mapSize, Terrain terrain)
        {
            m_samplePointList.Clear();
            
            int sampleCountXZ = (mapSize + s_sampleTileSize) / s_sampleTileSize;
            
            for (int x = 0; x < sampleCountXZ; x++)
            {
                for (int z = 0; z < sampleCountXZ; z++)
                {
                    Vector3 pos = new Vector3(x * s_sampleTileSize + s_sampleTileSize / 2, 0,
                        z * s_sampleTileSize + s_sampleTileSize / 2);
                    float terrainHeight = terrain.SampleHeight(pos);
                    pos.y = terrainHeight;
                    m_samplePointList.Add(pos);
                }
            }   
        }

        public static void CameraSampleTraverse()
        {
            
        }
    }
    
}


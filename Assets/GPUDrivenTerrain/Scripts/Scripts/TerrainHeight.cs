using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUDrivenTerrainLearn
{
    public class TerrainHeight
    {
        private static byte[] heights;
        private static int xMax;
        private static int zMax;
        private static float yMax;
        private static int resolution;

        public static Vector3 TerrainPos
        {
            get;
            set;
        }
        public static void LoadHeightBytes(byte[] data)
        {
            float[] headData = new float[3];
            System.Buffer.BlockCopy(data, 0, headData, 0, sizeof(float)*3);
            xMax = (int)headData[0];
            zMax = (int)headData[1];
            yMax = headData[2];
            heights = data;
            resolution = (int)Mathf.Sqrt((data.Length - sizeof(float)*3)/2);
        }
        private static float SampleHeightMapData(int x, int y)
        {
            int headDataLen = sizeof(float) * 3;
            int idx = y * resolution * 2 + x * 2;
            byte h = heights[headDataLen + idx];
            byte l = heights[headDataLen + idx + 1];
            return h + l / 255f;
        }
        
        public static bool GetHeightInterpolated(Vector3 pos, ref float h)
        {
            if (pos.x - TerrainPos.x < 0 ||  pos.x - TerrainPos.x >= xMax)
            {
                return false;
            }
            if (pos.z - TerrainPos.z < 0 ||  pos.z - TerrainPos.z >= zMax)
            {
                return false;
            }
            float val = GetInterpolatedHeightVal(pos);
            h = val + TerrainPos.y;
            return true;
        }
        private static float GetInterpolatedHeightVal(Vector3 pos)
        {
            Vector3 localPos = pos - TerrainPos;
            var local_x = Mathf.Clamp01(localPos.x / xMax) * resolution;
            var local_y = Mathf.Clamp01(localPos.z / zMax) * resolution;
            int x = Mathf.FloorToInt(local_x);
            int y = Mathf.FloorToInt(local_y);
            float tx = local_x - x;
            float ty = local_y - y;
            float y00 = SampleHeightMapData(x, y);
            float y10 = SampleHeightMapData(x + 1, y);
            float y01 = SampleHeightMapData(x, y + 1);
            float y11 = SampleHeightMapData(x + 1, y + 1);
            return Mathf.Lerp(Mathf.Lerp(y00, y10, tx), Mathf.Lerp(y01, y11, tx), ty);
        }
    }

}
